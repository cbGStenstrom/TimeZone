using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;
using System.Text.Json;
using TimeKeeper.App.Api.Enums;
using TimeKeeper.App.Components.Dialogs;
using TimeKeeper.Domain.Models;

namespace TimeKeeper.App.Components;

public partial class TimeEntryStatusComponent : CbComponentBase
{
    #region constants
    #endregion constants

    #region parameters

    /// <summary>
    ///  Gets or sets the currently active time entry.
    /// </summary>
    [Parameter]
    public TimeEntry? ActiveTimeEntry { get; set; }

    /// <summary>
    ///  Gets or sets the callback that is invoked when the active time entry changes.
    /// </summary>
    /// <remarks>
    ///  Use this property to handle changes to the active time entry in the parent component. The
    ///  callback receives the new active <see cref="TimeEntry"/> as its argument.
    /// </remarks>
    [Parameter]
    public EventCallback<TimeEntry> ActiveTimeEntryChanged { get; set; }

    /// <summary>
    ///  Gets or sets the identifier of the currently active time entry, if any.
    /// </summary>
    /// <remarks>
    ///  This is bound to the TimeEntryId parameter of the TimeEntryStartWorkComponent
    ///  for passing the Id of the new TimeEntry back to the host page.
    /// </remarks>
    [Parameter]
    public int? ActiveTimeEntryId { get; set; }

    /// <summary>
    ///  Gets or sets the CSS gap value applied between child elements.
    /// </summary>
    /// <remarks>
    ///  The value should be a valid CSS length (for example, "1rem", "8px", or "0.5em"). This
    ///  property controls the spacing between items in a flex or grid layout.
    /// </remarks>
    [Parameter]
    public string Gap { get; set; } = "1rem";

    /// <summary>
    ///  Gets or sets the orientation of the status display.
    /// </summary>
    /// <remarks>
    ///  Use this property to specify whether the status elements are arranged horizontally or
    ///  vertically. The default orientation is horizontal.
    /// </remarks>
    [Parameter]
    public Radzen.Orientation StatusOrientation { get; set; } = Radzen.Orientation.Horizontal;

    /// <summary>
    ///  Occurs when a new time entry is created.
    /// </summary>
    /// <remarks>
    ///  This event is triggered when the user creates a new time entry through the time entry editor
    ///  dialog. The event callback provides the identifier of the newly created time entry.
    /// </remarks>
    [Parameter]
    public EventCallback<int> OnTimeEntryCreated { get; set; }

    #endregion parameters

    #region properties

    /// <summary>
    ///  Gets or sets the reference to the dialog component used for editing time entry details.
    /// </summary>
    private TimeEntryEditorDialog? dlgTimeEntryEditor { get; set; }

    /// <summary>
    ///  Gets or Sets flag indicating whether searators should be rendered to the screen.
    /// </summary>
    private bool ShowSeparators { get; set; } = false;

    /// <summary>
    ///  Gets or sets the alignment of items within the status area.
    /// </summary>
    Radzen.AlignItems StatusAlignItems { get; set; } = AlignItems.Center;

    #endregion properties

    #region fields

    /// <summary>
    ///  Flag indicating that the page has been loaded FALSE or not TRUE
    /// </summary>
    private bool _isFirstLoad = true;

    #endregion fields

    #region events

    /// <summary>
    /// Callback invoked when the time entry value changes.
    /// </summary>
    [Parameter]
    public EventCallback OnTimeEntryChanged { get; set; }

    #endregion events

    #region data

    /// <summary>
    ///  Asynchronously loads the currently active time entry and updates the ActiveTimeEntry 
    ///  property.
    /// </summary>
    /// <returns>
    ///  A task that represents the asynchronous load operation.
    /// </returns>
    private async Task LoadActiveTimeEntry()
    {
        this.ActiveTimeEntry = await base.TimeEntrySvc.GetActiveTimeEntry();
    }

    #endregion data

    #region lifecycle

    /// <summary>
    /// Asynchronously handles logic when component parameters are set.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    protected override async Task OnParametersSetAsync()
    {
        // On the first time the component is loaded, load an Active TimeEntry, if one exists.
        //
        if (this._isFirstLoad)
        {
            await this.LoadActiveTimeEntry();
            this.ConfigureLayout();
            this._isFirstLoad = false;
        }
    }

    #endregion lifecycle

    #region event handlers

    /// <summary>
    ///  Handles the click event for the End Work button, opening the End Work dialog with the 
    ///  selected action.
    /// </summary>
    /// <remarks>
    ///  The dialog allows the user to complete an end work action based on the selected value. The
    ///  dialog is configured with options for resizing and dragging, and uses settings for its 
    ///  dimensions and position if available.
    /// </remarks>
    /// <param name="args">
    ///  The split button item that was clicked, containing the selected action value.
    /// </param>
    /// <returns>
    ///  A task that represents the asynchronous operation.
    /// </returns>
    private async Task btnEndWork_OnClick()
    {
        if (this.dlgTimeEntryEditor == null) return;

        var dialogOnSaveCallback = EventCallback.Factory.Create<TimeEntry>(this, Dialog_OnTimeEntryEnded);
        await this.dlgTimeEntryEditor.Open(this.ActiveTimeEntry, "Edit Time Entry", dialogOnSaveCallback, TimeEntrySaveActions.ContinueLater);
    }

    /// <summary>
    /// Handles the click event for the Start Work button by opening the time entry editor dialog.
    /// </summary>
    /// <remarks>The dialog allows the user to edit or create a time entry associated with a specific
    /// order. The dialog is opened with predefined size and position options, and is not resizable or
    /// draggable.</remarks>
    /// <returns>A task that represents the asynchronous operation of opening the dialog.</returns>
    private async Task btnStartWork_OnClick()
    {
        if (this.dlgTimeEntryEditor == null) return;

        var dialogOnSaveCallback = EventCallback.Factory.Create<TimeEntry>(this, Dialog_OnTimeEntryCreated);
        await this.dlgTimeEntryEditor.Open(new TimeEntry(), "Start Time Entry", dialogOnSaveCallback, TimeEntrySaveActions.StartWork);

    }

    /// <summary>
    ///  Handles the event when a new time entry is created and updates the active time entry 
    ///  accordingly.
    /// </summary>
    /// <remarks>
    ///  This method updates the current active time entry and refreshes the component state to
    ///  reflect the changes. It is intended to be used as an event handler for time entry creation 
    ///  events.
    /// </remarks>
    /// <param name="activeTimeEntryId">
    ///  The identifier of the newly created active time entry to be loaded and displayed.
    /// </param>
    private async void Dialog_OnTimeEntryCreated(TimeEntry activeTimeEntry)
    {
        this.dlgTimeEntryEditor?.Close();

        await this.ActiveTimeEntryChanged.InvokeAsync(this.ActiveTimeEntry);
        await this.OnTimeEntryChanged.InvokeAsync();
    }

    /// <summary>
    ///  Handles the completion of a time entry by ending the active time entry and updating the 
    ///  component state.
    /// </summary>
    /// <param name="endedTimeEntryId">
    ///  The identifier of the time entry that has ended.
    /// </param>
    private async void Dialog_OnTimeEntryEnded()
    {
        this.dlgTimeEntryEditor?.Close();

        await this.ActiveTimeEntryChanged.InvokeAsync();
        await this.OnTimeEntryChanged.InvokeAsync();
    }

    #endregion event handlers

    #region private

    /// <summary>
    ///  Configures the layout alignment of the status indicator based on the current orientation.
    /// </summary>
    /// <remarks>
    ///  This method sets the alignment of the status indicator to center when the orientation is 
    ///  horizontal, or to start when the orientation is vertical. It should be called whenever 
    ///  the orientation changes to ensure the layout remains consistent.
    /// </remarks>
    private void ConfigureLayout()
    {
        if (this.StatusOrientation == Radzen.Orientation.Horizontal)
        {
            this.StatusAlignItems = AlignItems.Center;
            this.ShowSeparators = true;
        }
        else
        {
            this.StatusAlignItems = AlignItems.Start;
            this.ShowSeparators = false;
        }
    }

    #endregion private

}