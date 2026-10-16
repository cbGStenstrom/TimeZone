using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using TimeKeeper.App.Components.Dialogs;
using TimeKeeper.App.Components.Pages;
using TimeKeeper.Domain.Models;
using TimeKeeper.Domain.Services.Interfaces;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TimeKeeper.App.Pages;

public partial class WorkItemTypeManagerPage : CbPageBase
{
    #region dependencies

    [Inject]
    protected ILogger<WorkItemTypeManagerPage> Logger { get; set; } = default!;

    [Inject]
    public IWorkItemTypeService TypeService { get; set; } = default!;

    #endregion dependencies

    #region properties

    protected string? FilterText
    {
        get => this._filterText;
        set
        {
            if (this._filterText == value) return;
            this._filterText = value;
            this.FilterWorkItemTypes();
        }
    }

    /// <summary>
    ///  Gets the collection of work item types that are visible based on the current filter and 
    ///  sort settings.
    /// </summary>
    private IEnumerable<WorkItemType> VisibleRows { get; set; } = [];

    #endregion properties

    #region Fields

    private string? _filterText;


    /// <summary>
    ///Contains the collection of <see cref="WorkItemType"/> rows.
    /// </summary>
    private List<WorkItemType> _rows = [];

    #endregion Fields

    #region Data

    /// <summary>
    ///  Confirms and removes a work item type, or disables it when it is referenced by existing 
    ///  activities.
    /// </summary>
    /// <remarks>
    ///  When the type is referenced by activities, it is disabled instead of deleted to preserve
    ///  existing associations and history. If deletion fails, data is reloaded and the exception 
    ///  is rethrown.</remarks>
    /// <param name="model">
    ///  The work item type to evaluate and process for deletion or disabling.</param>
    /// <returns>
    ///  A task that represents the asynchronous operation.</returns>
    private async Task DeleteWorkitemType(WorkItemType model)
    {
        // WHAT: Determine if the work item type is referenced by any activities and prepare the
        //  appropriate confirmation message.
        // WHY: This check ensures that we do not delete a type that is still in use, and provides the
        //  user with the option to disable it instead, preserving existing associations and history.
        var referenced = model.ActivityCount > 0;

        // WHAT: Prepare the appropriate confirmation message based on whether the work item type
        //  is referenced by any activities.
        // WHY: This message informs the user of the consequences of their action, ensuring they
        //  make an informed decision.
        var message = referenced ? "This type has associated Activities and cannot be deleted. Disable it instead? Existing associations and history will be retained; it will be excluded from editable Type lists." : "Permanently delete this Activity Type?";
        if (await Confirm(message) != true) return;

        // WHAT: Perform the deletion or disabling of the work item type based on whether it is referenced.
        // WHY: If the type is referenced, we disable it to preserve existing associations. If it is not
        //  referenced, we delete it to remove it from the system. We also handle any exceptions that
        //  may occur during the deletion process, refreshing the data to ensure the user sees the
        //  most up-to-date information.
        if (referenced)
        {
            // WHAT: Disable the work item type.
            // WHY: This operation preserves existing associations and history while preventing the
            //  type from being used in new activities.
            await TypeService.SetEnabled(model.Id, false, SessionSvc.User!);
        }
        else
        {
            try
            {
                // WHAT: Delete the work item type.
                // WHY: This operation removes the work item type from the system when it is not
                //  referenced by any activities.
                await TypeService.Delete(model.Id);
            }
            catch
            {
                // Refresh stale counts; the next click must confirm the current action.
                await ReloadWorkitemTypes();
                throw;
            }
        }
    }

    /// <summary>
    ///  Enables the specified work item type for the current user.
    /// </summary>
    /// <remarks>
    ///  If enabling fails, work item types are reloaded to refresh stale counts before the exception
    ///  is rethrown.</remarks>
    /// <param name="model">
    ///  The work item type to enable.</param>
    /// <returns>
    ///  A task that represents the asynchronous enable operation.</returns>
    private async Task EnableWorkItemType(WorkItemType model)
    {
        try
        {
            await TypeService.SetEnabled(model.Id, true, SessionSvc.User!);
        }
        catch
        {
            // Refresh stale counts; the next click must confirm the current action.
            await ReloadWorkitemTypes();
            throw;
        }
    }

    private void FilterWorkItemTypes()
    {
        // WHAT: Filter the work item types by name or description (case-insensitive).
        var text = this.FilterText?.Trim();

        // WHAT: Filter the work item types based on the current filter text, if any.
        this.VisibleRows = string.IsNullOrEmpty(text)
            ? this._rows
            : [.. this._rows.Where(row =>
                row.Name.Contains(text, StringComparison.OrdinalIgnoreCase) ||
                row.Description?.Contains(text, StringComparison.OrdinalIgnoreCase) == true)];
    }

    /// <summary>
    ///  Asynchronously reloads the type _rows from the type service.
    /// </summary>
    /// <returns>
    ///  A task that represents the asynchronous reload operation.</returns>
    private async Task ReloadWorkitemTypes() 
    {
        this._rows = await this.TypeService.GetTypes();
        this.FilterWorkItemTypes();
    }

    /// <summary>
    ///  Saves the specified work item type draft for the current user.
    /// </summary>
    /// <remarks>
    ///  Returns without saving when <paramref name="dataModel"/> is <see langword="null"/>.</remarks>
    /// <param name="dataModel">
    ///  The work item type draft to save.</param>
    /// <returns>
    ///  A task that represents the asynchronous save operation.</returns>
    private async Task UpdateWorkItemType(WorkItemType dataModel)
    {
        // WHAT: Ensure that the data model is not null before proceeding with the update operation.
        // WHY: This check prevents null reference exceptions and ensures that the update logic only
        //  executes when a valid data model is provided.
        if (dataModel == null) return;

        // WHAT: Save the current draft and reload data.
        // WHY: This is the standard update operation for a draft, ensuring that changes are persisted
        //  and the UI reflects the latest state.
        await TypeService.Save(dataModel, SessionSvc.User!);
    }

    #endregion Data

    #region Lifecycle
    #endregion Lifecycle

    #region Event Handlers

    private async Task btnAdd_OnClick()
    {
        if (await DialogSvc.OpenAsync<WorkItemTypeAddDialog>("Add New Type") == true)
        {
            await ReloadWorkitemTypes();
        }
    }

    private async Task DataGrid_OnDelete(WorkItemType workItemTypeModel)
    {
        await this.DeleteWorkitemType(workItemTypeModel);
        await ReloadWorkitemTypes();
    }

    private async Task DataGrid_OnEnable(WorkItemType model)
    {
        try
        {
            await this.EnableWorkItemType(model);
            await ReloadWorkitemTypes();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error enabling work item type with ID {WorkItemTypeId}", model.Id);
            await this.DialogSvc.Alert("An error occurred while enabling the work item type.", "Save Failed");
        }
    }

    private async Task DataGrid_OnUpdate(WorkItemType workItemTypeModel)
    {
        try
        {
            await this.UpdateWorkItemType(workItemTypeModel);
            await ReloadWorkitemTypes();
        }
        catch(Exception ex)
        {
            Logger.LogError(ex, "Error updating work item type with ID {WorkItemTypeId}", workItemTypeModel.Id);
            await this.DialogSvc.Alert("An error occurred while updating the work item type.", "Save Failed");
        }
    }

    #endregion Event Handlers

    #region private methods

    /// <summary>
    ///  Displays a confirmation dialog for an activity type action.
    /// </summary>
    /// <param name="message">
    ///  The confirmation message shown in the dialog.</param>
    /// <returns>
    ///  A task that resolves to <see langword="true"/> when confirmed, <see langword="false"/> 
    ///  when declined, or <see langword="null"/> when the dialog is dismissed.</returns>
    private Task<bool?> Confirm(string message)
    {
        return DialogSvc.Confirm(message, "Activity Type", new ConfirmOptions { OkButtonText = "Yes", CancelButtonText = "No" });
    }

    /// <summary>
    ///  Asynchronously initializes the component and reloads data when a user session is available.
    /// </summary>
    /// <remarks>
    ///  Calls the base initialization logic before conditionally invoking reload when `SessionSvc.User` 
    ///  is not null.</remarks>
    /// <returns>
    ///  A task that represents the asynchronous initialization operation.</returns>
    protected override async Task OnInitializedAsync() 
    { 
        await base.OnInitializedAsync(); 
        if (SessionSvc.User != null) 
        {
            await ReloadWorkitemTypes(); 
        }
    }

    #endregion private methods
}
