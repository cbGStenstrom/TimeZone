using Microsoft.AspNetCore.Components;
using TimeKeeper.App.Api.Enums;
using TimeKeeper.Domain.Models;

namespace TimeKeeper.App.Components;

public partial class ProjectGridRowComponent :CbComponentBase
{
    #region injected services
    #endregion injected services

    #region parameters

    /// <summary>
    ///  Gets or sets a value indicating whether the component's content can be edited by the user.
    /// </summary>
    [Parameter]
    public bool IsEditable { get; set; }

    /// <summary>
    ///  Gets or sets the project data model to be managed/displayed by the component.
    /// </summary>
    [Parameter]
    public Project? ProjectModel { get; set; }

    /// <summary>
    ///  DO NOT USE. Provided to allow two-way data binding. To capture the OnChange event use
    ///  <seealso cref="OnProjectChange"/>
    /// </summary>
    [Parameter]
    public EventCallback<Project> ProjectModelChanged { get; set; }

    #endregion parameters

    #region properties
    #endregion properties

    #region events

    /// <summary>
    ///  Gets or sets the callback invoked when the Add Project action is triggered.
    /// </summary>
    [Parameter]
    public EventCallback<Project> OnAddProjectClick { get; set; }

    /// <summary>
    ///  Gets or sets the callback invoked when the project is changed.
    /// </summary>
    [Parameter]
    public EventCallback<Project> OnProjectChange { get; set; }

    /// <summary>
    ///  Gets or sets the callback invoked when the Delete Project action is triggered.
    /// </summary>
    [Parameter]
    public EventCallback<Project> OnDeleteProjectClick { get; set; }

    /// <summary>
    ///  Gets or sets the callback invoked when the Save Project action is triggered.
    /// </summary>
    [Parameter]
    public EventCallback<Project> OnSaveProjectClick { get; set; }

    #endregion events

    #region lifecycle
    #endregion lifecycle

    #region event handlers

    /// <summary>
    ///  Cancels the current edit operation. For unsaved projects, invokes the deletion callback 
    ///  when assigned; for existing projects, restores the snapshot, exits edit mode, and refreshes 
    ///  the component state.
    /// </summary>
    /// <remarks>
    ///  Throws an exception if <c>ProjectModel</c> is <see langword="null" />.</remarks>
    /// <returns>
    ///  A task that represents the asynchronous operation.</returns>
    async Task btnCancel_OnClick()
    {
        ArgumentNullException.ThrowIfNull(this.ProjectModel);

        // WHAT: If the project is new and unsaved, we invoke the deletion callback to notify that
        //  it should be removed from the list.
        // WHY: This is necessary because the project has not been persisted yet, and we want to ensure
        //  that the UI reflects the removal of the unsaved project.
        if (this.ProjectModel.IsNew)
        {
            if (this.OnDeleteProjectClick.HasDelegate)
            {
                await this.OnDeleteProjectClick.InvokeAsync(this.ProjectModel);
            }

            return;
        }

        this.ProjectModel.RevertToSnapshot();
        this.IsEditable = false;

        base.StateHasChanged();
    }

    /// <summary>
    ///  Deletes the current project and raises the project-deleted callback when a handler is assigned.
    /// </summary>
    /// <remarks>
    ///  Invokes <c>OnDeleteProjectClick</c> with the current project identifier, or <c>0</c> when the
    ///     project model is unavailable.</remarks>
    /// <returns>
    ///  A task that represents the asynchronous delete and callback invocation operation.</returns>
    async Task btnDelete_OnClick()
    {
        await this.OnDeleteProjectClick.InvokeAsync(this.ProjectModel);
    }

    /// <summary>
    ///  Enables edit mode for the current component.
    /// </summary>
    /// <remarks>
    ///  Call this method to allow the user to modify the component's content. This method triggers 
    ///  a UI refresh to reflect the editable state.
    /// </remarks>
    void btnEdit_OnClick()
    {
        this.IsEditable = true;
        base.StateHasChanged();
    }

    /// <summary>
    ///  Saves the current project when changes are detected, creating a new project or updating an 
    ///  existing one, notifies the user of the result, and raises the project-updated callback when 
    ///  applicable.   
    /// </summary>
    /// <remarks> 
    ///  Throws when the project model is null. If no changes are detected, shows an informational
    ///  alert instead of saving. Always exits edit mode and refreshes component state; on failure, 
    ///  shows the innermost error message.</remarks>
    /// <returns>
    ///  A task that represents the asynchronous save operation.</returns>
    async Task btnSave_OnClick()
    {
        ArgumentNullException.ThrowIfNull(this.ProjectModel);

        try
        {
            // WHAT: Validate the project before proceeding with save operation.
            if (!(await this.ValidateProject())) return;

            // WHAT: If the project is not new and has no changes, show an informational alert
            //  instead of saving.
            if (!this.ProjectModel.IsNew && !this.ProjectModel.IsDirty())
            {
                await base.DialogSvc!.Alert("No changes were made to the project.", AlertTitles.Information);
            }
            else
            {
                await this.OnSaveProjectClick.InvokeAsync(this.ProjectModel);
            }

            this.IsEditable = false;
        }
        catch (Exception ex)
        {
            while (ex.InnerException != null)
            {
                ex = ex.InnerException;
            }

            await base.DialogSvc!.Alert(ex.Message, AlertTitles.Error);
        }
    }

    #endregion event handlers

    #region private

    /// <summary>
    ///  Returns the CSS class string for a column based on its editability.
    /// </summary>
    /// <param name="isEditableCol">
    ///  true to indicate that the column is editable; otherwise, false. The default is true.
    /// </param>
    /// <returns>
    ///  A string containing the CSS class for the column. Returns "cb-column cb-editable" if the 
    ///  column is editable; otherwise, returns "cb-column".
    /// </returns>
    string GetColumnCssClass(bool isEditableCol = true)
    {
        string result = (this.IsEditable && isEditableCol) ? "cb-column cb-editable" : "cb-column";
        return result;
    }

    /// <summary>
    ///  Validates that the current project model exists and includes required project identifier 
    ///  fields.
    /// </summary>
    /// <remarks>
    ///  Displays a validation alert before returning <see langword="false"/> when a required field 
    ///  is missing. Throws <see cref="ArgumentNullException"/> when <c>ProjectModel</c> is <see
    /// langword="null"/>.</remarks>
    /// <returns>
    ///  A task that resolves to <see langword="true"/> when the project key and short name are 
    ///  provided; otherwise, <see langword="false"/>.</returns>
    async Task<bool> ValidateProject()
    {
        ArgumentNullException.ThrowIfNull(this.ProjectModel);

        if (string.IsNullOrWhiteSpace(this.ProjectModel.Key))
        {
            await base.DialogSvc.Alert(
                "Project Acronym is required.",
                "Validation Error");

            return false;
        }

        if (string.IsNullOrWhiteSpace(this.ProjectModel.ShortName))
        {
            await base.DialogSvc.Alert(
                "Project Short Name is required.",
                "Validation Error");

            return false;
        }

        return true;
    }

    #endregion private

    #region public
    #endregion public
}