using System.Linq.Expressions;
using TimeKeeper.Domain.Models;
using TimeKeeper.Domain.Utilities;
using TimeEntryEntity = TimeKeeper.DataAccess.Entities.TimeEntry;

namespace TimeKeeper.App.Models;

/// <summary>
///  Selects and summarizes the previous workday without changing contributing records.
/// </summary>
public static class YesterdayAccomplishmentsSummaryBuilder
{
    #region data

    /// <summary>
    ///  Builds one summary per work-item identity, retaining every contributing session.
    /// </summary>
    /// <param name="entries">Qualifying local-time models returned by the filtered query.</param>
    /// <returns>Type-grouped summaries with newest-first sessions.</returns>
    public static List<MyDayActivitySummary> Build(IEnumerable<TimeEntry> entries)
    {
        return [.. entries.Where(entry => entry.WorkItem != null)
            .GroupBy(entry => entry.WorkItemId)
            .Select(group => new MyDayActivitySummary
            {
                WorkItem = group.Select(entry => entry.WorkItem).OfType<WorkItem>().First(),
                TotalHoursWorked = group.Sum(entry => entry.HoursWorked ?? 0),
                FirstWorked = group.Min(entry => entry.StartWork) ?? DateTime.MinValue,
                LastWorked = group.Max(entry => entry.StartWork) ?? DateTime.MinValue,
                Entries = [.. group.OrderByDescending(entry => entry.StartWork).ThenBy(entry => entry.Id)
                    .Select(entry => new MyDayActivityEntry
                    {
                        TimeEntryId = entry.Id,
                        StartWork = entry.StartWork,
                        EndWork = entry.EndWork,
                        HoursWorked = entry.HoursWorked,
                        Accomplishment = entry.Accomplishment ?? string.Empty
                    })]
            })
            .OrderBy(summary => summary.WorkItem.Type?.Name ?? string.Empty, StringComparer.Ordinal)
            .ThenBy(summary => summary.WorkItem.Type?.Id)
            .ThenBy(summary => summary.ActivityIdentifier, StringComparer.Ordinal)
            .ThenBy(summary => summary.WorkItemId)];
    }

    /// <summary>
    ///  Creates a laborer-scoped half-open UTC predicate from local day boundaries.
    /// </summary>
    /// <param name="day">Selected local workday.</param>
    /// <param name="laborerId">Current session laborer identity.</param>
    /// <returns>The entity predicate used by the existing query service.</returns>
    public static Expression<Func<TimeEntryEntity, bool>> CreateFilter(DateTime day, int laborerId)
    {
        // WHAT: Convert each midnight independently.
        // WHY: A local day can span 23 or 25 hours at a daylight-saving transition.
        DateTime start = day.Date.ConvertLocalToUtc();
        DateTime end = day.Date.AddDays(1).ConvertLocalToUtc();
        return entry => entry.LaborerId == laborerId && entry.StartWork >= start && entry.StartWork < end;
    }

    /// <summary>
    ///  Selects Friday on Monday and the preceding calendar date otherwise.
    /// </summary>
    /// <param name="today">Explicit local current date.</param>
    /// <returns>The previous workday, with the existing weekend fallback.</returns>
    public static DateTime GetPreviousWorkday(DateTime today)
    {
        return today.Date.AddDays(today.DayOfWeek == DayOfWeek.Monday ? -3 : -1);
    }

    #endregion data
}
