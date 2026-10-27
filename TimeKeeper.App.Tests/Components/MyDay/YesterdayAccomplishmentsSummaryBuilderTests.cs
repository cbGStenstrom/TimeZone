using TimeKeeper.App.Models;
using TimeKeeper.Domain.Models;
using TimeKeeper.Domain.Utilities;
using Xunit;
using Entity = TimeKeeper.DataAccess.Entities.TimeEntry;

namespace TimeKeeper.App.Tests.Components.MyDay;

public class YesterdayAccomplishmentsSummaryBuilderTests
{
    [Theory]
    [InlineData("2026-10-26", "2026-10-23")]
    [InlineData("2026-10-27", "2026-10-26")]
    [InlineData("2026-10-28", "2026-10-27")]
    [InlineData("2026-10-29", "2026-10-28")]
    [InlineData("2026-10-30", "2026-10-29")]
    [InlineData("2027-03-01", "2027-02-26")]
    [InlineData("2027-01-01", "2026-12-31")]
    [InlineData("2026-10-24", "2026-10-23")]
    [InlineData("2026-10-25", "2026-10-24")]
    public void PreviousWorkdayUsesFixedLocalDates(string today, string expected)
    {
        Assert.Equal(DateTime.Parse(expected), YesterdayAccomplishmentsSummaryBuilder.GetPreviousWorkday(DateTime.Parse(today).AddHours(17)));
    }

    [Theory]
    [InlineData("2026-10-23")]
    [InlineData("2026-03-08")]
    [InlineData("2026-11-01")]
    public void PredicateScopesStartWorkAndLaborerAcrossSeparateLocalMidnights(string date)
    {
        var day = DateTime.Parse(date);
        var start = day.ConvertLocalToUtc();
        var end = day.AddDays(1).ConvertLocalToUtc();
        var predicate = YesterdayAccomplishmentsSummaryBuilder.CreateFilter(day, 7).Compile();
        Assert.True(predicate(new Entity { LaborerId = 7, StartWork = start, CreatedDate = start.AddYears(-1) }));
        Assert.True(predicate(new Entity { LaborerId = 7, StartWork = end.AddTicks(-1) }));
        Assert.False(predicate(new Entity { LaborerId = 7, StartWork = start.AddTicks(-1) }));
        Assert.False(predicate(new Entity { LaborerId = 7, StartWork = end }));
        Assert.False(predicate(new Entity { LaborerId = 8, StartWork = start }));
        Assert.False(predicate(new Entity { LaborerId = 7, StartWork = null, CreatedDate = start }));
    }

    [Fact]
    public void ProjectionPreservesSessionsAndSourcesGroupsByIdentityAndSortsMetadataAndTies()
    {
        var day = new DateTime(2026, 10, 23);
        WorkItem Item(int id, string number, int typeId, string name) => new()
        {
            Id = id, ActivityNumber = number, Project = new() { Key = "APP" },
            Type = new() { Id = typeId, Name = name }
        };
        TimeEntry Entry(int id, WorkItem? item, int minute = 15) => new()
        {
            Id = id, WorkItemId = item?.Id ?? 99, WorkItem = item,
            StartWork = day, EndWork = day.AddMinutes(minute), Accomplishment = "Same content"
        };
        var entries = new[]
        {
            Entry(5, Item(3, "2", 2, "Feature")),
            Entry(4, Item(2, "1", 2, "Feature")),
            Entry(3, Item(1, "1", 2, "Feature")),
            Entry(2, Item(1, "1", 2, "Feature")),
            Entry(1, Item(4, "9", 1, "Bug")),
            Entry(6, null),
            Entry(7, new WorkItem { Id = 5 }),
            Entry(8, Item(6, "3", 3, "Feature"))
        };
        entries[1].EndWork = null;
        var before = entries.Select(entry => (entry.Id, entry.WorkItem, entry.Accomplishment, entry.StartWork, entry.EndWork)).ToArray();
        var summaries = YesterdayAccomplishmentsSummaryBuilder.Build(entries);
        Assert.Equal(new[] { 5, 4, 1, 2, 3, 6 }, summaries.Select(summary => summary.WorkItemId));
        var grouped = summaries.Single(summary => summary.WorkItemId == 1);
        Assert.Equal(.5, grouped.TotalHoursWorked);
        Assert.Equal(new[] { 2, 3 }, grouped.Entries.Select(entry => entry.TimeEntryId));
        Assert.Equal(0, summaries.Single(summary => summary.WorkItemId == 2).TotalHoursWorked);
        Assert.Equal(before, entries.Select(entry => (entry.Id, entry.WorkItem, entry.Accomplishment, entry.StartWork, entry.EndWork)).ToArray());
    }
}
