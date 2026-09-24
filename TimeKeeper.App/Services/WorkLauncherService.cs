using Microsoft.AspNetCore.Components;
using Radzen;
using TimeKeeper.App.Api.Enums;
using TimeKeeper.App.Components.Dialogs;
using TimeKeeper.App.Services.Interfaces;
using TimeKeeper.Domain.Models;
using TimeKeeper.Domain.Services.Interfaces;

namespace TimeKeeper.App.Services;

public class WorkLauncherService : IWorkLauncherService
{
    #region fields

    /// <summary>
    /// Provides access to time entry operations.
    /// </summary>
    private readonly ITimeEntryService _timeEntryService;
    
    #endregion fields

    #region ctor

    public WorkLauncherService(ITimeEntryService timeEntryService)
    {
        _timeEntryService = timeEntryService;
    }

    #endregion ctor

    #region IWorkLauncherService Members

    /// <inheritdoc/>
    public async Task StartWork( WorkItem workItem, TimeEntryEditorDialog dialog, Laborer user, DialogService dialogService, EventCallback<TimeEntry> callback)
    {
        TimeEntry? activeEntry = await _timeEntryService.GetActiveTimeEntry();

        // WHAT: If no active entry exists then open the start dialog for the selected work item
        // WHY: This is the first time the user is starting work on a work item, so we need to create
        //  a new time entry for it.
        if (activeEntry == null)
        {
            await OpenStartDialog(workItem, dialog, user, callback);
            return;
        }

        // WHAT: If the active entry is for the same work item as the selected work item then open
        //  the edit dialog for the active entry .
        // WHY: The user is continuing work on the same work item, so we allow them to edit the
        //  existing time entry.
        if (activeEntry.WorkItemId == workItem.Id)
        {
            await dialog.Open(activeEntry, $"Edit Work - {workItem.DisplayIdentifier}", callback, TimeEntrySaveActions.ContinueLater);
            return;
        }

        // WHAT: If the active entry is for a different work item than the selected work item then
        //  confirm with the user if they want to stop the current activity and start the new one.
        bool? stopCurrent = await dialogService.Confirm(
                $"Current Activity:\r\n" +
                $"{activeEntry.WorkItem?.DisplayIdentifier}\r\n\r\n" +
                $"Selected Activity:\r\n" +
                $"{workItem.DisplayIdentifier}\r\n\r\n" +
                $"Stop the current activity and begin the selected one?",
                "Active Work Detected",
                new ConfirmOptions() {
                    OkButtonText = "Stop Current And Start New",
                    CancelButtonText = "Continue Current Activity"
                });

        if (stopCurrent != true) return;

        activeEntry.EndWork = DateTime.Now;

        await _timeEntryService.UpdateTimeEntry(activeEntry, user);
        await OpenStartDialog(workItem, dialog, user, callback);    
    }

    #endregion IWorkLauncherService Members

    #region private methods

    /// <summary>
    ///  Opens the start-work dialog with a new time entry initialized for the specified laborer 
    ///  and work item.  
    /// </summary>
    /// <param name="workItem">
    ///  The work item to associate with the new time entry.</param>
    /// <param name="dialog">
    ///  The dialog used to edit and submit the start-work time entry.</param>
    /// <param name="user">
    ///  The laborer starting work.</param>
    /// <param name="callback">
    ///  The callback invoked with the saved time entry.</param>
    /// <returns>
    ///  A task that represents the asynchronous dialog operation.</returns>
    private async Task OpenStartDialog(WorkItem workItem, TimeEntryEditorDialog dialog, Laborer user, EventCallback<TimeEntry> callback)
    {
        TimeEntry model = new() {
            LaborerId = user.Id,
            WorkItemId = workItem.Id,
            WorkItem = workItem,
            StartWork = DateTime.Now
        };

        await dialog.Open(model, $"Start Work - {workItem.DisplayIdentifier}", callback, TimeEntrySaveActions.StartWork);
    }

    #endregion private methods
}