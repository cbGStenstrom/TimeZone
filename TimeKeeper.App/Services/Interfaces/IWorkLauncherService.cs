using Microsoft.AspNetCore.Components;
using Radzen;
using TimeKeeper.App.Components.Dialogs;
using TimeKeeper.Domain.Models;

namespace TimeKeeper.App.Services.Interfaces;

public interface IWorkLauncherService
{
    /// <summary>
    ///  Starts a work-entry operation for the specified work item and user using the provided dialog 
    ///  components.    
    /// </summary>
    /// <param name="workItem">
    ///  Work item to associate with the time entry.</param>
    /// <param name="dialog">
    ///  Editor dialog used to collect or update time entry details.</param>
    /// <param name="user">
    ///  Laborer performing the work.</param>
    /// <param name="dialogService">
    ///  Dialog service used to show and manage dialogs.</param>
    /// <param name="callback">
    ///  Callback invoked with the resulting time entry.</param>
    /// <returns>
    ///  A task that represents the asynchronous operation.</returns>
    Task StartWork( WorkItem workItem, TimeEntryEditorDialog dialog, Laborer user, DialogService dialogService, EventCallback<TimeEntry> callback);
}