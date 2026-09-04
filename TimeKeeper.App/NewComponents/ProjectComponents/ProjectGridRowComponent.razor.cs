using Microsoft.AspNetCore.Components;
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
    public bool IsEditable { get; set; } = false;

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

    [Parameter]
    public EventCallback<int> OnProjectAdded { get; set; }

    [Parameter]
    public EventCallback<Project> OnProjectChange { get; set; }

    [Parameter]
    public EventCallback<int> OnProjectDeleted { get; set; }

    [Parameter]
    public EventCallback<Project> OnProjectUpdated { get; set; }

    #endregion events

    #region data

    void AddProject()
    {
    }

    /// <summary>
    ///  Deletes the current project asynchronously.
    /// </summary>
    /// <remarks>
    ///  This method performs the deletion operation only if a project is currently loaded. If no
    ///  project is available, the method returns <see langword="null"/> without performing any 
    ///  action.
    /// </remarks>
    /// <returns>
    ///  A <see cref="Project"/> object representing the deleted project if the operation succeeds; 
    ///  otherwise, <see langword="null"/> if there is no project to delete.
    /// </returns>
    async Task<Project?> DeleteProject()
    {
        if (this.ProjectModel == null) return null;

        Project? result = await base.ProjectService!.DeleteProject(this.ProjectModel.Id);
        return result;
    }

    /// <summary>
    ///  Updates the current project using the associated project model.
    /// </summary>
    /// <returns>
    ///  A task that represents the asynchronous operation. The task result is <see langword="true"/> 
    ///  if the project was updated successfully; otherwise, <see langword="false"/>.
    /// </returns>
    async Task<bool> UpdateProject()
    {
        ArgumentNullException.ThrowIfNull(base.SessionService?.User);
        ArgumentNullException.ThrowIfNull(this.ProjectModel);

        bool result = await base.ProjectService!.UpdateProject(this.ProjectModel, base.SessionService.User);
        return result;
    }

    #endregion data

    #region lifecycle
    #endregion lifecycle

    #region event handlers

    /// <summary>
    ///  Handles the Cancel button click event by disabling edit mode and updating the component 
    ///  state.
    /// </summary>
    /// <remarks>
    ///  Call this method in response to a user action that should exit edit mode. After execution,
    ///  the component will no longer be editable and any UI bound to the edit state will be refreshed.
    /// </remarks>
    void btnCancel_OnClick()
    {
        this.ProjectModel?.RevertToSnapshot();
        this.IsEditable = false;
        base.StateHasChanged();
    }

    void btnDelete_OnClick()
    {

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
    ///  Handles the save operation for the project and displays a confirmation or error message to 
    ///  the user.
    /// </summary>
    /// <remarks>
    ///  If the save operation is successful, a success alert is shown and the project is marked as
    ///  non-editable. If an error occurs, an error alert is displayed with the exception message. 
    ///  If an OnProjectSaved event handler is assigned, it is invoked after a successful save.
    /// </remarks>
    /// <returns>
    ///  A task that represents the asynchronous save operation.
    /// </returns>
    async Task btnSave_OnClick()
    {
        ArgumentNullException.ThrowIfNull(this.ProjectModel);

        try
        {
            // only update if the model has been changed
            //
            if (!this.ProjectModel.IsDirty())
            {
                await base.DialogSvc!.Alert("No changes were made to the project.");
            }
            else
            {
                await this.UpdateProject();
                await base.DialogSvc!.Alert("Project saved", "Success");

                if (this.OnProjectUpdated.HasDelegate) await this.OnProjectUpdated.InvokeAsync(this.ProjectModel);
            }

            this.IsEditable = false;
            base.StateHasChanged();
        }
        catch (Exception ex)
        {
            while (ex.InnerException != null) { ex = ex.InnerException; }
            await base.DialogSvc!.Alert(ex.Message, "Error");
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

    #endregion private

    #region public
    #endregion public
}