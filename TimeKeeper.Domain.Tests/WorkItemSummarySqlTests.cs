using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using TimeKeeper.DataAccess.Entities;
using TimeKeeper.Domain.Queries;
using TimeKeeper.Domain.Utilities;
using Xunit;

namespace TimeKeeper.Domain.Tests;

public class WorkItemSummarySqlTests
{
    [SqlServerFact]
    public async Task SummaryProjectsIdentifiersPreservesAggregationAndDoesNotWrite()
    {
        await using var database = await IsolatedDatabase.Create();
        await using var context = database.Context();
        await context.Database.EnsureCreatedAsync();

        const string title = "Refresh Design of the System Bar in A&A";
        var type = new WorkItemType { Name = "Summary fixture", IsEnabled = true };
        var projects = new[]
        {
            new Project { Key = "JCAM", LongName = "First", ShortName = "First" },
            new Project { Key = "OTHER", LongName = "Second", ShortName = "Second" },
            new Project { Key = null, LongName = "Legacy", ShortName = "Legacy" }
        };
        var items = projects.Select((project, index) => new WorkItem
        {
            Project = project,
            Type = type,
            ActivityNumber = index == 0 ? "5721" : "0007",
            Title = title,
            IsBillable = index != 1,
            IsOpen = true,
            IsInReview = index == 1
        }).ToArray();
        var laborer = new Laborer
        {
            FirstName = "Fixture",
            Email = "fixture@example.invalid",
            Username = "summary-fixture",
            Password = "synthetic-fixture"
        };
        context.WorkItems.AddRange(items);
        context.Laborers.Add(laborer);
        await context.SaveChangesAsync();
        // IsOpen has a database default; explicitly update one row to exercise closed summaries.
        items[0].IsOpen = false;
        await context.SaveChangesAsync();

        var start = new DateTime(2026, 10, 12, 0, 0, 0, DateTimeKind.Local);
        var utcStart = start.ConvertLocalToUtc();
        context.TimeEntries.AddRange(
            Entry(items[0], utcStart.AddHours(9), 30),
            Entry(items[0], utcStart.AddHours(10), 45),
            Entry(items[1], utcStart.AddHours(11), 20),
            Entry(items[2], utcStart.AddHours(12), 10),
            Entry(items[0], utcStart.AddDays(-1).AddHours(9), 120),
            Entry(items[1], utcStart.AddDays(1).AddHours(9), 180));
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        string before = await Snapshot(context);
        var handler = new WorkItemQueryHandler(context);
        var summaries = await handler.Handle(
            new GetWorkItemSummaryForDateRange(start, start.AddDays(1).AddTicks(-1)), default);

        Assert.Equal(3, summaries.Count);
        for (int index = 0; index < items.Length; index++)
        {
            var summary = Assert.Single(summaries, value => value.WorkItemID == items[index].Id);
            Assert.Equal(projects[index].Key, summary.ProjectKey);
            Assert.Equal(items[index].ActivityNumber, summary.ActivityNumber);
            Assert.Equal(title, summary.WorkItemTitle);
            Assert.Equal(items[index].IsOpen, summary.IsOpen);
            Assert.Equal(items[index].IsBillable, summary.IsBillable);
            Assert.Equal(items[index].IsInReview, summary.IsInReview);
            Assert.Equal(new[] { 75, 20, 10 }[index], summary.TotalMinutesWorked);
        }
        Assert.Equal("JCAM-5721 " + title, summaries.Single(s => s.WorkItemID == items[0].Id).DisplayTitle);
        Assert.Equal("OTHER-0007 " + title, summaries.Single(s => s.WorkItemID == items[1].Id).DisplayTitle);
        Assert.Equal(title, summaries.Single(s => s.WorkItemID == items[2].Id).DisplayTitle);
        Assert.Empty(context.ChangeTracker.Entries());
        Assert.Equal(before, await Snapshot(context));

        var empty = await handler.Handle(
            new GetWorkItemSummaryForDateRange(start.AddDays(5), start.AddDays(6)), default);
        Assert.Empty(empty);

        TimeEntry Entry(WorkItem item, DateTime entryStart, int minutes) => new()
        {
            WorkItem = item,
            Laborer = laborer,
            StartWork = entryStart,
            EndWork = entryStart.AddMinutes(minutes),
            Accomplishment = "Disposable summary fixture"
        };
    }

    private static async Task<string> Snapshot(TimeKeeperDbContext context)
    {
        // Scalar snapshots include all mapped values without serializing navigation cycles.
        var projects = await context.Projects.AsNoTracking().OrderBy(p => p.Id).ToListAsync();
        var items = await context.WorkItems.AsNoTracking().OrderBy(w => w.Id).ToListAsync();
        var entries = await context.TimeEntries.AsNoTracking().OrderBy(t => t.Id).ToListAsync();
        return JsonSerializer.Serialize(new
        {
            Projects = projects.Select(p => context.Entry(p).CurrentValues.ToObject()),
            WorkItems = items.Select(w => context.Entry(w).CurrentValues.ToObject()),
            TimeEntries = entries.Select(t => context.Entry(t).CurrentValues.ToObject())
        });
    }

    // The existing type-test fixture is private. Keep this fixture local and disposable too.
    private sealed class IsolatedDatabase : IAsyncDisposable
    {
        private string Master { get; }
        private string Name { get; } = "TimeKeeper_SummaryTests_" + Guid.NewGuid().ToString("N");
        private string Target { get; }

        private IsolatedDatabase()
        {
            var builder = new SqlConnectionStringBuilder(TestDatabaseConfiguration.TestConnectionString)
            {
                InitialCatalog = "master"
            };
            this.Master = builder.ConnectionString;
            builder.InitialCatalog = this.Name;
            this.Target = builder.ConnectionString;
        }

        public static async Task<IsolatedDatabase> Create()
        {
            var database = new IsolatedDatabase();
            await using var connection = new SqlConnection(database.Master);
            await connection.OpenAsync();
            await using var command = new SqlCommand($"CREATE DATABASE [{database.Name}]", connection);
            await command.ExecuteNonQueryAsync();
            return database;
        }

        public TimeKeeperDbContext Context() =>
            new(new DbContextOptionsBuilder<TimeKeeperDbContext>().UseSqlServer(this.Target).Options);

        public async ValueTask DisposeAsync()
        {
            SqlConnection.ClearAllPools();
            await using var connection = new SqlConnection(this.Master);
            await connection.OpenAsync();
            await using var command = new SqlCommand(
                $"ALTER DATABASE [{this.Name}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{this.Name}]",
                connection);
            await command.ExecuteNonQueryAsync();
        }
    }
}
