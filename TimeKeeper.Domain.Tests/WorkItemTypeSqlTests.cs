using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using TimeKeeper.DataAccess.Entities;
using TimeKeeper.Domain.Commands;
using TimeKeeper.Domain.Queries;
using TimeKeeper.Domain.Utilities.Mappers;
using Xunit;
namespace TimeKeeper.Domain.Tests;

// SQL integration tests use the configured TimeKeeperTestDB connection as the
// SQL Server target. Each test creates and removes only its own GUID-named database.
public sealed class SqlServerFactAttribute : FactAttribute
{
}

public class WorkItemTypeSqlTests
{
    private static readonly TimeKeeper.Domain.Models.Laborer User = new() { Username = "type-tests" };
    [SqlServerFact]
    public async Task FailedWritesCannotLeakIntoLaterOperations()
    {
        await using var db = await IsolatedDatabase.Create();
        var saves = new SaveCounter();
        await using var ctx = db.Context(saves);
        await ctx.Database.EnsureCreatedAsync();
        var handler = new WorkItemTypeCommandHandler(ctx);
        var type = new TimeKeeper.Domain.Models.WorkItemType { Name = "Original" };
        await handler.Handle(new SaveWorkItemType(type, User), default);
        saves.FailNext = true;
        type.Name = "Rejected";
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(new SaveWorkItemType(type, User), default));
        Assert.Empty(ctx.ChangeTracker.Entries());
        await handler.Handle(new SetWorkItemTypeEnabled(type.Id, false, User), default);
        Assert.Equal("Original", (await ctx.WorkItemTypes.AsNoTracking().SingleAsync()).Name);

        saves.FailNext = true;
        var rejected = new TimeKeeper.Domain.Models.WorkItemType { Name = "Rejected insert" };
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(new SaveWorkItemType(rejected, User), default));
        Assert.True(rejected.IsNew);
        await handler.Handle(new SetWorkItemTypeEnabled(type.Id, true, User), default);
        Assert.Equal(1, await ctx.WorkItemTypes.CountAsync());

        saves.FailNext = true;
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(new DeleteWorkItemType(type.Id), default));
        await handler.Handle(new SetWorkItemTypeEnabled(type.Id, false, User), default);
        Assert.Equal(1, await ctx.WorkItemTypes.CountAsync());
    }
    [SqlServerFact]
    public async Task CrudCountsAuditZeroKeyAndAssignmentRules()
    {
        await using var db = await IsolatedDatabase.Create();
        var saves = new SaveCounter();
        await using var ctx = db.Context(saves); await ctx.Database.EnsureCreatedAsync();
        await ctx.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT dbo.WorkItemTypes ON; INSERT dbo.WorkItemTypes(Id,Name,IsEnabled) VALUES(0,'Bugfix',1); SET IDENTITY_INSERT dbo.WorkItemTypes OFF;");
        var handler = new WorkItemTypeCommandHandler(ctx); var query = new WorkItemTypeQueryHandler(ctx);
        var zero = (await query.Handle(new(), default)).Single(); Assert.Equal(0, zero.Id); Assert.False(zero.IsNew); Assert.Equal(0, zero.ActivityCount);
        zero.CreatedBy = "Must not overwrite creation"; zero.CreatedDate = DateTime.MinValue;
        zero.Description = "Legacy"; Assert.True(await handler.Handle(new SaveWorkItemType(zero, User), default));
        var saved = await ctx.WorkItemTypes.AsNoTracking().SingleAsync(); var created = saved.CreatedDate; var creator = saved.CreatedBy; var updated = saved.UpdatedDate;
        Assert.NotEqual(zero.CreatedBy, creator); Assert.NotEqual(zero.CreatedDate, created); Assert.Equal(User.Username, saved.UpdatedBy);
        var calls = saves.Calls;
        Assert.False(await handler.Handle(new SaveWorkItemType(zero, User), default)); Assert.Equal(calls, saves.Calls);
        Assert.Equal(updated, (await ctx.WorkItemTypes.AsNoTracking().SingleAsync()).UpdatedDate);
        Assert.True(await handler.Handle(new SetWorkItemTypeEnabled(0, false, User), default));
        var disabled = await ctx.WorkItemTypes.AsNoTracking().SingleAsync(); Assert.Equal(created, disabled.CreatedDate); Assert.Equal(creator, disabled.CreatedBy);
        calls = saves.Calls;
        Assert.False(await handler.Handle(new SetWorkItemTypeEnabled(0, false, User), default)); Assert.Equal(calls, saves.Calls); Assert.Equal(disabled.UpdatedDate, (await ctx.WorkItemTypes.AsNoTracking().SingleAsync()).UpdatedDate);
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(new SaveWorkItemType(new() { Name = "bugFIX" }, User), default));
        var custom = new TimeKeeper.Domain.Models.WorkItemType { Name = "Custom", Icon = "event", Description = "optional", IsEnabled = false };
        await handler.Handle(new SaveWorkItemType(custom, User), default); Assert.True(custom.Id > 0);
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(new SaveWorkItemType(new() { Id = custom.Id, IsNew = false, Name = "BUGFIX" }, User), default));
        Assert.DoesNotContain(await query.Handle(new(true), default), t => t.Id == custom.Id);
        await handler.Handle(new SetWorkItemTypeEnabled(custom.Id, true, User), default);
        var project = new Project { LongName = "Tests", ShortName = "Tests" }; ctx.Projects.Add(project); await ctx.SaveChangesAsync();
        var workHandler = new WorkItemCommandHandler(ctx);
        var model = new TimeKeeper.Domain.Models.WorkItem { ProjectId = project.Id, Title = "History", WorkItemType = 0 };
        await Assert.ThrowsAsync<InvalidOperationException>(() => workHandler.Handle(new AddWorkItem(model), default));
        model.WorkItemType = custom.Id; model = await workHandler.Handle(new AddWorkItem(model), default);
        Assert.Equal(1, (await query.Handle(new(), default)).Single(t => t.Id == custom.Id).ActivityCount);
        await handler.Handle(new SetWorkItemTypeEnabled(custom.Id, false, User), default);
        model.Title = "Unrelated edit"; Assert.True(await workHandler.Handle(new UpdateWorkItem(model), default));
        Assert.Equal(custom.Id, model.Type!.Id);
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(new DeleteWorkItemType(custom.Id), default));
        await handler.Handle(new SetWorkItemTypeEnabled(0, true, User), default);
        model.WorkItemType = 0; await workHandler.Handle(new UpdateWorkItem(model), default);
        Assert.Equal(0, model.Type!.Id);
        var counts = await query.Handle(new(), default); Assert.Equal(1, counts.Single(t => t.Id == 0).ActivityCount); Assert.Equal(0, counts.Single(t => t.Id == custom.Id).ActivityCount);
        await handler.Handle(new DeleteWorkItemType(custom.Id), default);
        model.WorkItemType = 9999; await Assert.ThrowsAsync<InvalidOperationException>(() => workHandler.Handle(new UpdateWorkItem(model), default));
        await workHandler.Handle(new DeleteWorkItem(model.Id), default);
        Assert.Equal(0, (await query.Handle(new(), default)).Single(t => t.Id == 0).ActivityCount);
        await handler.Handle(new DeleteWorkItemType(0), default); Assert.Empty(await query.Handle(new(), default));
    }
    [SqlServerFact]
    public async Task RelationalConstraintsProtectAgainstStaleCountsAndDuplicateRaces()
    {
        await using var db = await IsolatedDatabase.Create(); await using var ctx = db.Context(); await ctx.Database.EnsureCreatedAsync();
        // EnsureCreated uses the EF model, which does not define defaults for these required flags.
        await ctx.Database.ExecuteSqlRawAsync("INSERT dbo.WorkItemTypes(Name,IsEnabled) VALUES('Unique',1); INSERT dbo.Projects(LongName,ShortName) VALUES('P','P'); INSERT dbo.WorkItems(ProjectID,Title,WorkitemType,IsBillable,IsInReview,IsComplete,IsDeployed) SELECT 1,'Existing',Id,0,0,0,0 FROM dbo.WorkItemTypes;");
        await Assert.ThrowsAsync<SqlException>(() => db.Execute("DELETE dbo.WorkItemTypes"));
        Assert.Equal(1, await ctx.WorkItems.CountAsync());
        var orphan = await Assert.ThrowsAsync<SqlException>(() => db.Execute("INSERT dbo.WorkItems(ProjectID,Title,WorkitemType,IsBillable,IsInReview,IsComplete,IsDeployed) VALUES(1,'Orphan',9999,0,0,0,0)"));
        Assert.Equal(547, orphan.Number);
        var missingType = await Assert.ThrowsAsync<SqlException>(() => db.Execute("INSERT dbo.WorkItems(ProjectID,Title,WorkitemType,IsBillable,IsInReview,IsComplete,IsDeployed) VALUES(1,'Null',NULL,0,0,0,0)"));
        Assert.Equal(515, missingType.Number);
        await Assert.ThrowsAsync<SqlException>(() => db.Execute("INSERT dbo.WorkItemTypes(Name,IsEnabled) VALUES('unique',1)"));
        await Assert.ThrowsAsync<SqlException>(() => db.Execute("INSERT dbo.WorkItemTypes(Name,IsEnabled) VALUES('   ',1)"));
        async Task<bool> Insert() { try { await db.Execute("INSERT dbo.WorkItemTypes(Name,IsEnabled) VALUES('Race',1)"); return true; } catch (SqlException) { return false; } }
        var results = await Task.WhenAll(Insert(), Insert()); Assert.Single(results, r => r);
    }
    [SqlServerFact]
    public async Task UpgradePreservesLegacyIdsEntriesAndMeaningsAndIsRepeatSafe()
    {
        await using var db = await IsolatedDatabase.Create();
        foreach (var table in new[] { "Projects", "Laborers", "WorkItems", "TimeEntries" })
        {
            var schema = File.ReadAllText(Path.Combine(RepositoryRoot(), "TimeKeeper.Data", "Tables", table + ".sql"));
            if (table == "WorkItems") schema = schema.Replace("    CONSTRAINT FK_WorkItems_WorkItemTypes FOREIGN KEY (WorkitemType) REFERENCES dbo.WorkItemTypes(Id),", "").Split("GO")[0];
            await db.Execute(schema);
        }
        await db.Execute("INSERT dbo.Projects(LongName,ShortName) VALUES('Legacy','Legacy'); INSERT dbo.Laborers(FirstName,Email,Username,Password) VALUES('Fixture','fixture@example.invalid','fixture','synthetic-fixture'); SET IDENTITY_INSERT dbo.WorkItems ON; INSERT dbo.WorkItems(ID,ProjectID,Title,WorkitemType) VALUES(10,1,'Zero',0),(11,1,'One',1),(12,1,'Two',2),(13,1,'Three',3),(14,1,'Four',4),(15,1,'Five',5); SET IDENTITY_INSERT dbo.WorkItems OFF; SET IDENTITY_INSERT dbo.TimeEntries ON; INSERT dbo.TimeEntries(ID,LaborerID,WorkItemID) VALUES(100,1,10),(101,1,15); SET IDENTITY_INSERT dbo.TimeEntries OFF;");
        var script = File.ReadAllText(Path.Combine(RepositoryRoot(), "TimeKeeper.Data", "Scripts", "018-WorkItemTypes.sql"));
        await db.Execute(script); await db.Execute(script);
        await db.Execute("ALTER TABLE dbo.WorkItems ADD CONSTRAINT FK_Test_Types FOREIGN KEY(WorkitemType) REFERENCES dbo.WorkItemTypes(Id)");
        await using var conn = db.Connection(); await conn.OpenAsync();
        await using var command = new SqlCommand("SELECT w.Id,t.Name FROM dbo.WorkItems w JOIN dbo.WorkItemTypes t ON w.WorkitemType=t.Id ORDER BY w.Id", conn);
        await using (var reader = await command.ExecuteReaderAsync())
        {
            string[] names = ["Bugfix", "Chore", "Feature", "Meeting", "PTO", "TechDebt"]; int i = 0;
            while (await reader.ReadAsync()) { Assert.Equal(10 + i, reader.GetInt32(0)); Assert.Equal(names[i++], reader.GetString(1)); }
            Assert.Equal(6, i);
        }
        command.CommandText = "SELECT COUNT(*) FROM dbo.TimeEntries WHERE (Id=100 AND WorkItemID=10) OR (Id=101 AND WorkItemID=15)";
        Assert.Equal(2, await command.ExecuteScalarAsync());
    }
    [SqlServerFact]
    public async Task FreshBootstrapAndUnknownLegacyDiagnostic()
    {
        var script = File.ReadAllText(Path.Combine(RepositoryRoot(), "TimeKeeper.Data", "Scripts", "018-WorkItemTypes.sql"));
        await using (var db = await IsolatedDatabase.Create()) { await db.Execute(script); await db.Execute(script); await using var conn = db.Connection(); await conn.OpenAsync(); await using var cmd = new SqlCommand("SELECT COUNT(*) FROM dbo.WorkItemTypes", conn); Assert.Equal(6, await cmd.ExecuteScalarAsync()); cmd.CommandText = "INSERT dbo.WorkItemTypes(Name,IsEnabled) OUTPUT INSERTED.Id VALUES('Beyond legacy',1)"; Assert.True((int)(await cmd.ExecuteScalarAsync())! > 5); }
        await using (var db = await IsolatedDatabase.Create())
        {
            await db.Execute("CREATE TABLE dbo.WorkItems(Id int PRIMARY KEY,WorkitemType int NOT NULL); INSERT dbo.WorkItems VALUES(42,99)");
            var error = await Assert.ThrowsAsync<SqlException>(() => db.Execute(script)); Assert.Equal(50018, error.Number);
            await using var conn = db.Connection(); await conn.OpenAsync(); await using var cmd = new SqlCommand("SELECT OBJECT_ID('dbo.WorkItemTypes')", conn); Assert.Equal(DBNull.Value, await cmd.ExecuteScalarAsync());
        }
    }
    private static string RepositoryRoot() { var dir = new DirectoryInfo(AppContext.BaseDirectory); while (dir != null && !File.Exists(Path.Combine(dir.FullName, "TimeKeeper.sln"))) dir = dir.Parent; return dir?.FullName ?? throw new InvalidOperationException("Repository root not found."); }
    private sealed class SaveCounter : SaveChangesInterceptor
    {
        public int Calls { get; private set; }
        public bool FailNext { get; set; }
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData data, InterceptionResult<int> result, CancellationToken ct = default)
        {
            Calls++;
            if (FailNext) { FailNext = false; throw new InvalidOperationException("Simulated write failure."); }
            return ValueTask.FromResult(result);
        }
    }
    private sealed class IsolatedDatabase : IAsyncDisposable
    {
        private readonly string master, target, name;
        private IsolatedDatabase(string configuration)
        {
            name = "TimeKeeper_TypeTests_" + Guid.NewGuid().ToString("N");
            var builder = new SqlConnectionStringBuilder(configuration) { InitialCatalog = "master" }; master = builder.ConnectionString;
            builder.InitialCatalog = name; target = builder.ConnectionString;
        }
        public static async Task<IsolatedDatabase> Create()
        {
            var db = new IsolatedDatabase(
                TestDatabaseConfiguration.TestConnectionString);

            await using var conn = new SqlConnection(db.master);
            await conn.OpenAsync();

            await using var cmd =
                new SqlCommand($"CREATE DATABASE [{db.name}]", conn);

            await cmd.ExecuteNonQueryAsync();

            return db;
        }
        public SqlConnection Connection() => new(target);
        public TimeKeeperDbContext Context(SaveCounter? counter = null) { var options = new DbContextOptionsBuilder<TimeKeeperDbContext>().UseSqlServer(target); if (counter != null) options.AddInterceptors(counter); return new(options.Options); }
        public async Task Execute(string sql) { await using var conn = Connection(); await conn.OpenAsync(); await using var cmd = new SqlCommand(sql, conn); await cmd.ExecuteNonQueryAsync(); }
        public async ValueTask DisposeAsync() { SqlConnection.ClearAllPools(); await using var conn = new SqlConnection(master); await conn.OpenAsync(); await using var cmd = new SqlCommand($"ALTER DATABASE [{name}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{name}]", conn); await cmd.ExecuteNonQueryAsync(); }
    }
}
