using Microsoft.AspNetCore.Components;
using System.Linq.Expressions;
using TimeKeeper.Domain.Models;
using TimeKeeper.Domain.Services.Interfaces;
using TimeKeeper.Domain.Utilities;
namespace TimeKeeper.App.Components.Reports;

public partial class WorkItemEntriesComponent : ComponentBase
{
    #region injected services

    [Inject]
    public IWorkItemService? WorkItemSvc { get; set; }

    [Inject]
    public ITimeEntryService? TimeEntrySvc { get; set; }

    #endregion injected services

    #region parameters

    /// <summary>
    ///  Gets or sets the end date for the operation or range.
    /// </summary>
    [Parameter]
    public DateTime EndDate { get; set; }

    /// <summary>
    ///  Gets or sets the start date for the operation or event.
    /// </summary>
    [Parameter]
    public DateTime StartDate { get; set; }

    /// <summary>
    ///  Gets or sets the unique identifier of the work item associated with this component.
    /// </summary>
    [Parameter]
    public int WorkitemId { get; set; }

    #endregion parameters

    #region data-bound parameters
    #endregion data-bound parameters

    #region properties

    /// <summary>
    ///  Gets or sets the collection of time entries associated with the current context.
    /// </summary>
    IEnumerable<TimeEntry> TimeEntries { get; set; } = [];

    WorkItem? Workitem { get; set; }

    #endregion properties

    #region events
    #endregion events

    #region data

    /// <summary>
    ///  Asynchronously retrieves the work item with the specified identifier.
    /// </summary>
    /// <param name="workitemId">
    ///  The unique identifier of the work item to retrieve.
    /// </param>
    /// <returns>
    ///  A task that represents the asynchronous operation. The task result contains the <see cref="WorkItem"/> 
    ///  if found; otherwise, <see langword="null"/>.
    /// </returns>
    async Task<WorkItem?> GetWorkItem(int workitemId)
    {
        WorkItem? result = await this.WorkItemSvc!.GetWorkItemByID(workitemId);
        return result;
    }

    /// <summary>
    ///  Asynchronously retrieves all time entries associated with the specified work item that fall 
    ///  within the given date range.
    /// </summary>
    /// <param name="workitemId">
    ///  The unique identifier of the work item for which to retrieve time entries.
    /// </param>
    /// <param name="lowerBound">
    ///  The start of the date range. Only time entries with a start or end time on or after this 
    ///  date are included.
    /// </param>
    /// <param name="upperBound">
    ///  The end of the date range. Only time entries with a start or end time on or before this 
    ///  date are included.
    /// </param>
    /// <returns>
    ///  A task that represents the asynchronous operation. The task result contains a collection 
    ///  of time entries for the specified work item within the given date range. The collection is 
    ///  empty if no matching entries are found.
    /// </returns>
    async Task<IEnumerable<TimeEntry>> GetWorkItemTimeEntriesForDateRange(long workitemId, DateTime lowerBound, DateTime upperBound)
    {
        DateTime startDate = lowerBound.GetStartOfDay().ConvertLocalToUtc();
        DateTime endDate   = upperBound.GetEndOfDay().ConvertLocalToUtc();

        var filter = new List<Expression<Func<DataAccess.Entities.TimeEntry, bool>>>();
            filter.Add(entry => entry.WorkItemId == workitemId);
            filter.Add(entry => entry.StartWork >= startDate && entry.StartWork <= endDate);
            filter.Add(entry => entry.EndWork >= startDate && entry.EndWork <= endDate);

        IEnumerable<TimeEntry> results = await this.TimeEntrySvc!.GetFilteredTimeEntries(filter);
        return results.OrderByDescending(e=>e.StartWork); 
    }

    #endregion data

    #region lifecycle


    protected override async Task OnParametersSetAsync()
    {
        this.Workitem    = await this.GetWorkItem(this.WorkitemId);
        this.TimeEntries = await this.GetWorkItemTimeEntriesForDateRange(this.WorkitemId, this.StartDate, this.EndDate);
    }

    #endregion lifecycle

    #region event handlers
    #endregion event handlers

    #region private

    /// <summary>
    ///  Formats the start and end work dates of the specified time entry as a human-readable date 
    ///  range string.
    /// </summary>
    /// <param name="entry">
    ///  The time entry containing the start and end work dates to display.
    /// </param>
    /// <returns>
    ///  A string representing the date range in the format "StartDate to EndDate". If either date 
    ///  is not set, "N/A" is used in its place.
    /// </returns>
    string GetDateRangeDisplay(TimeEntry entry) 
    {
        string start = entry.StartWork.HasValue ? entry.StartWork.Value.ToString("ddd, MMM dd, yyyy hh:mm tt") : "N/A";
        string end   = entry.EndWork.HasValue ? entry.EndWork.Value.ToString("ddd, MMM dd, yyyy hh:mm tt") : "N/A";

        string result = $"{start} - {end}";
        return result;
    }

    /// <summary>
    ///  Returns a formatted string representing the total hours worked for the specified time entry.
    /// </summary>
    /// <param name="entry">
    ///  The time entry containing the start and end work times to calculate hours worked.
    /// </param>
    /// <returns>
    ///  A string displaying the number of hours worked in the format "0.00 hrs". Returns "N/A" if 
    ///  the start or end time is not specified or if the calculated hours are negative.
    /// </returns>
    string GetHoursWorkedDisplay(TimeEntry entry)
    {

        double hoursWorked = !entry.StartWork.HasValue || !entry.EndWork.HasValue ? -1 :
                              DateTimeUtils.HoursBetween(entry.StartWork.Value, entry.EndWork.Value);

        if (hoursWorked < 0)
            return "N/A";

        string result = $"{hoursWorked.ToString("0.00")} hrs";
        return result;
    }


    string GetWorkItemTitle()
    {
        if(this.TimeEntries is not null && this.TimeEntries.Count() > 0)
        {
            return this.Workitem?.Title ?? string.Empty;
        }
        else
        {
            return string.Empty;
        }
    }

    #endregion private

    #region public
    #endregion public
}
