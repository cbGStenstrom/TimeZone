using Microsoft.AspNetCore.Components;
using TimeKeeper.App.Api.Enums;
using TimeKeeper.Domain.Models;

namespace TimeKeeper.App.Components.Dialogs;

/// <summary>
///  Represents a dialog component for editing time entry records, providing functionality to open 
///  the editor with a specified time entry and handle save operations.
/// </summary>
/// <remarks>
///  This component is typically used within a Blazor application to allow users to create or modify 
///  time entries in a modal dialog. The dialog can be opened programmatically and supports registering 
///  a callback to handle save actions. The dialog's appearance and behavior can be customized through 
///  parameters when opening.
/// </remarks>
public partial class TimeEntryEditorDialog : CbComponentBase
{
    #region injected services
    #endregion injected services

    #region parameters

    [Parameter]
    public EventCallback<TimeEntry> OnSaveCallback { get; set; }

    #endregion parameters

    #region properties
    #endregion properties

    #region events
    #endregion events

    #region data
    #endregion data

    #region lifecycle
    #endregion lifecycle

    #region event handlers
    #endregion event handlers

    #region private
    #endregion private

    #region public

    public void Close()
    {
        this.DialogSvc!.Close();
    }

    /// <summary>
    ///  Opens the time entry editor dialog with the specified time entry and title, and registers 
    ///  a callback to be invoked when the entry is saved.
    /// </summary>
    /// <param name="timeEntryModel">
    ///  The time entry to be loaded into the editor. If null, the dialog is not opened.
    /// </param>
    /// <param name="title">
    ///  The title to display on the time entry editor dialog.
    /// </param>
    /// <param name="onSaveCallback">
    ///  A callback that is invoked when the user saves the time entry. The callback receives 
    ///  the saved time entry as its argument.
    /// </param>
    /// <returns>
    ///  A task that represents the asynchronous operation of opening the dialog.
    /// </returns>
    public async Task Open(TimeEntry? timeEntryModel, string title, EventCallback<TimeEntry> onSaveCallback, 
        TimeEntrySaveActions saveAction = TimeEntrySaveActions.Update)
    {
        if (timeEntryModel == null) return;

        // Open a Dialog, loaded with the TimeEntryEditorComponent. Create the options and parametes
        // to pass along to it.
        //
        var dialogOptions = base.GetDialogOptions("56rem", "38.9rem", true, true);
        var parameters    = new Dictionary<string, object>();
            parameters.Add("LoadedTimeEntry", timeEntryModel);
            parameters.Add("OnSaveClicked", onSaveCallback);
            //parameters.Add("SaveAction", saveAction);
            parameters.Add("ShowDialogButtons", true);

        await this.DialogSvc.OpenAsync<TimeEntryEditorComponent>(title,
            parameters,
            dialogOptions);
    }

    #endregion public
}