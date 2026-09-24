using Microsoft.AspNetCore.Components;
using System.Linq.Expressions;
using TimeKeeper.App.Api.Enums;
using TimeKeeper.App.Components.Dialogs;
using TimeKeeper.App.Components.Pages;
using TimeKeeper.App.Models;
using TimeKeeper.App.Services.Interfaces;
using TimeKeeper.Domain.Models;

namespace TimeKeeper.App.Pages;

public partial class MyDayPage : CbPageBase
{
    #region injected services

    /// <summary>
    ///  Gets or sets the service used to launch work items.
    /// </summary>
    /// <remarks>
    ///  Provided by dependency injection.</remarks>
    [Inject]
    public IWorkLauncherService WorkLauncherSvc { get; set; } = default!;

    #endregion injected services

    #region properties

    /// <summary>
    ///  Gets or sets the active time entry.
    /// </summary>
    /// <remarks>
    ///  Hides the inherited ActiveTimeEntry member.</remarks>
    new TimeEntry? ActiveTimeEntry { get; set; }

    /// <summary>
    ///  Gets or sets the date label text.
    /// </summary>
    /// <remarks>
    ///  Initialized to the current local date formatted as "dddd, dd MMMM yyyy".</remarks>
    string DateLabel { get; set; } = DateTime.Now.ToString("dddd, dd MMMM yyyy");

    /// <summary>
    ///  Gets or sets the time entry editor dialog.  
    /// </summary>
    TimeEntryEditorDialog? dlgTimeEntryEditor { get; set; }

    /// <summary>
    ///  Gets or sets the collection of recent activity summaries.
    /// </summary>
    List<MyDayActivitySummary> RecentActivities { get; set; } = [];

    /// <summary>
    ///  Gets or sets the collection of activity summaries recorded for the previous day.
    /// </summary>
    List<MyDayActivitySummary> YesterdayActivities { get; set; } = [];

    #endregion properties

    #region lifecycle

    protected override async Task OnInitializedAsync()
    {
        this.ActiveTimeEntry = await base.TimeEntrySvc!.GetActiveTimeEntry();
        await LoadYesterdayEntries();
        await LoadRecentActivities();
    }

    #endregion lifecycle

    #region event handlers

    /// <summary>
    ///  Resumes work for the specified activity summary by starting the work flow for its work item.
    /// </summary>
    /// <param name="summary">
    ///  The activity summary that contains the work item to resume.</param>
    /// <returns>
    ///  A task that represents the asynchronous operation.</returns>
    async Task ResumeWork_Click(MyDayActivitySummary summary)
    {
        await WorkLauncherSvc!.StartWork(
            summary.WorkItem,
            dlgTimeEntryEditor!,
            SessionService!.User!,
            DialogSvc!,
            EventCallback.Factory.Create<TimeEntry>(
                this,
                TimeEntry_OnSaved));
    }

    /// <summary>
    ///  Refreshes summary data after a time entry is saved.
    /// </summary>
    /// <remarks>
    ///  Updates the active time entry, reloads yesterday's entries, and requests a UI re-render.
    /// </remarks>
    /// <param name="entry">
    ///  Time entry that was saved.</param>
    /// <returns>
    ///  A task that represents the asynchronous refresh operation.</returns>
    async Task TimeEntry_OnSaved(TimeEntry entry)
    {
        // WHAT: Refresh the active time entry.
        // WHY: Because the user may have edited the active time entry, and we want to reflect
        // that change in the summary.
        this.ActiveTimeEntry = await TimeEntrySvc!.GetActiveTimeEntry();

        // WHAT: Refresh yesterday's activities
        // WHY: Because the user may have edited an entry from yesterday, and we want to reflect
        //  that change in the summary.
        await LoadYesterdayEntries();
        StateHasChanged();
    }

    /// <summary>
    ///  Navigates to the work item history page for the specified activity summary.
    /// </summary>
    /// <param name="summary">
    ///  Contains the work item identifier used to build the history route.</param>
    /// <returns>
    ///  A task that represents the asynchronous navigation operation.</returns>
    async Task ViewHistory_Click(MyDayActivitySummary summary)
    {
        NavigationMgr?.NavigateTo($"/WorkItemHistory/{summary.WorkItemId}");
    }

    #endregion event handlers

    #region methods

    /// <summary>
    ///  Loads recent activity summaries from time entries created in the last seven days, grouped 
    ///  by work item and ordered by most recent work.
    /// </summary>
    /// <remarks>
    ///  Populates `RecentActivities` with up to five work-item summaries, including total hours
    ///  worked, first start time, and last end time. Entries without an associated work item are 
    ///  excluded.</remarks>
    /// <returns>
    ///  A task that represents the asynchronous operation.</returns>
    private async Task LoadRecentActivities()
    {
        DateTime start = DateTime.Today.AddDays(-7);

        var filter = new List<Expression<Func<DataAccess.Entities.TimeEntry, bool>>> { e => e.CreatedDate >= start };

        IEnumerable<TimeEntry> entries = await base.TimeEntrySvc!.GetFilteredTimeEntries(filter) ?? [];

        RecentActivities = [.. entries
                .Where(e => e.WorkItem != null)
                .GroupBy(e => e.WorkItemId)
                .Select(g => { 
                    WorkItem workItem = g.First().WorkItem!;
                    return new MyDayActivitySummary {
                        WorkItem = workItem,
                        TotalHoursWorked = g.Sum(t => t.HoursWorked ?? 0),
                        FirstWorked = g.Min(t => t.StartWork) ?? DateTime.MinValue,
                        LastWorked = g.Max(t => t.EndWork) ?? DateTime.MinValue,
                        Entries = []
                    };
                })
                .OrderByDescending(a => a.LastWorked)
                .Take(5)];
    }

    /// <summary>
    ///  Loads yesterday's time entries, groups them by work item, and updates the activity summaries 
    ///  ordered by most recent work time.
    /// </summary>
    /// <remarks>
    ///  Uses yesterday's local date range from 12:00 AM through 11:59:59 PM.</remarks>
    /// <returns>
    ///  A task that represents the asynchronous operation.</returns>
    private async Task LoadYesterdayEntries()
    {
        DateTime yesterday = DateTime.Today.AddDays(-1);
        DateTime start = yesterday.Date;
        DateTime end = yesterday.Date.AddDays(1).AddSeconds(-1);

        var filter = new List<Expression<Func<DataAccess.Entities.TimeEntry, bool>>>{
                        e => e.CreatedDate >= start &&
                             e.CreatedDate <= end
                    };

        IEnumerable<TimeEntry> entries = await base.TimeEntrySvc!.GetFilteredTimeEntries(filter) ?? [];

        YesterdayActivities = [.. entries
                .Where(e => e.WorkItem != null)
                .GroupBy(e => e.WorkItemId)
                .Select(g =>
                {
                    WorkItem workItem = g.First().WorkItem!;

                    return new MyDayActivitySummary
                    {
                        WorkItem         = workItem,
                        TotalHoursWorked = g.Sum(t => t.HoursWorked ?? 0),
                        FirstWorked      = g.Min(t => t.StartWork) ?? DateTime.MinValue,
                        LastWorked       = g.Max(t => t.EndWork) ?? DateTime.MinValue,
                        Entries          = [.. g.OrderBy(t => t.StartWork)
                                            .Select(t => new MyDayActivityEntry {
                                                TimeEntryId = t.Id,
                                                StartWork = t.StartWork,
                                                EndWork = t.EndWork,
                                                HoursWorked = t.HoursWorked,
                                                Accomplishment = t.Accomplishment ?? string.Empty
                                            })]
                    };
                })
                .OrderByDescending(a => a.LastWorked)];
    }

    /// <summary>
    ///  Opens the time entry editor for an existing time entry and initializes the edit callback.   
    /// </summary>
    /// <param name="activeEntry">
    ///  The existing time entry to open for editing.</param>
    /// <returns>
    ///  A task that represents the asynchronous operation.</returns>
    async Task OpenExistingTimeEntry(TimeEntry activeEntry)
    {
        if (dlgTimeEntryEditor == null)
        {
            return;
        }

        var callback = EventCallback.Factory.Create<TimeEntry>(this, TimeEntry_OnSaved);

        await dlgTimeEntryEditor.Open(
            activeEntry,
            $"Edit Work - {activeEntry.WorkItem?.DisplayIdentifier}",
            callback,
            TimeEntrySaveActions.ContinueLater);
    }

    /// <summary>
    /// Opens the time entry editor to start work on the specified work item.
    /// </summary>
    /// <param name="workItem">The work item to associate with the new start-work time entry.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    async Task OpenStartWorkDialog(WorkItem workItem)
    {
        if (dlgTimeEntryEditor == null)
        {
            return;
        }

        TimeEntry model = new()
        {
            LaborerId = this.SessionService!.User!.Id,
            WorkItemId = workItem.Id,
            WorkItem = workItem,
            StartWork = DateTime.Now
        };

        var callback = EventCallback.Factory.Create<TimeEntry>(this, TimeEntry_OnSaved);

        await dlgTimeEntryEditor.Open(model, $"Start Work - {workItem.DisplayIdentifier}", callback, TimeEntrySaveActions.StartWork);
    }

    /// <summary>
    ///  Sets the end time of the active time entry to the current local time and persists the update 
    ///  for the current user.
    /// </summary>
    /// <param name="activeEntry">
    ///  The active time entry to stop and update.</param>
    /// <returns>
    ///  A task that represents the asynchronous operation.</returns>
    async Task StopActiveTimeEntry(TimeEntry activeEntry)
    {
        activeEntry.EndWork = DateTime.Now;
        await this.TimeEntrySvc!.UpdateTimeEntry(activeEntry, this.SessionService!.User!);
    }

    #endregion methods
}