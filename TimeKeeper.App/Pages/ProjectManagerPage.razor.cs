using System.ComponentModel;
using TimeKeeper.App.Api.Enums;
using TimeKeeper.App.Components.Pages;
using TimeKeeper.Domain.Models;

namespace TimeKeeper.App.Pages;

public partial class ProjectManagerPage : CbPageBase
{

    #region dependencies
    #endregion dependencies

    #region properties

    /// <summary>
    ///  Gets or sets the collection of projects currently loaded in the application.
    /// </summary>
    List<Project> ProjectListData { get; set; } = [];

    #endregion properties

    #region data

    /// <summary>
    ///  Deletes the specified project.
    /// </summary>
    /// <param name="project">
    ///  The project to delete.</param>
    /// <returns>
    ///  A task that represents the asynchronous operation.</returns>
    async Task DeleteProject(Project project)
    {
        ArgumentNullException.ThrowIfNull(project);
        _ = await base.ProjectService!.DeleteProject(project.Id);
    }

    /// <summary>
    ///  Retrieves a list of projects, sorted according to the current sort column and direction.
    /// </summary>
    /// <remarks>
    ///  The sorting is determined by the values of the SortColumn and SortDirection properties. 
    ///  The method always returns a non-null list, which may be empty.
    /// </remarks>
    /// <returns>
    ///  A task that represents the asynchronous operation. The task result contains a list of 
    ///  projects sorted by the specified column and direction. The list will be empty if no 
    ///  projects are available.
    /// </returns>
    async Task<List<Project>> GetProjectList()
    {
        List<Project> result = await base.ProjectSvc.GetProjects() ?? [];
        return result;
    }

    /// <summary>
    ///  Saves a project by adding it when it is new or updating it when it already exists.
    /// </summary>
    /// <remarks>
    ///  Throws <see cref="ArgumentNullException"/> when the current user or <paramref name="project"/> 
    ///  is <see langword="null"/>.</remarks>
    /// <param name="project">
    ///  The project to save.</param>
    /// <returns>
    ///  <see langword="true"/> when a new project is added; otherwise, the result of the update 
    ///  operation.</returns>
    async Task<bool> SaveProject(Project project)
    {
        ArgumentNullException.ThrowIfNull(base.SessionService?.User);
        ArgumentNullException.ThrowIfNull(project);

        if (project.IsNew)
        {
            if(this.DuplicateProjectKeyExist(project))
            {
                throw new InvalidOperationException($"A project with the acronym '{project.Key}' already exists.");
            }

            await base.ProjectService.AddProject(project, base.SessionService.User);
            return true;

        }
        else
        {
            return await base.ProjectService!.UpdateProject(project, base.SessionService.User);
        }
    }

    #endregion data

    #region lifecycle

    /// <summary>
    ///  Initializes component data asynchronously when the component is first initialized.
    /// </summary>
    /// <returns>A task that represents the asynchronous initialization operation.</returns>
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await this.InitializeData();
    }

    #endregion lifecycle

    #region event handlers

    /// <summary>
    ///  Adds the project to the beginning of the project list when it is marked as new.
    /// </summary>
    /// <param name="project">
    ///  The project to evaluate and add.</param>
    /// <returns>
    ///  A task that represents the asynchronous operation.</returns>
    async Task ProjectGridComponent_OnAddProjectClick(Project project)
    {
        if(project.IsNew)
        {
            // WHAT: Checks if there is already an unsaved project in the list before adding a new one.
            // WHY: To prevent multiple unsaved projects from being added to the list, which could
            //  lead to confusion or data loss.
            bool hasUnsavedProject = ProjectListData.Any(p => p.IsNew);
            if (hasUnsavedProject) { return; }

            // WHAT: Inserts the new project at the beginning of the list.
            this.ProjectListData.Insert(0, project);
        }
    }
    
    /// <summary>
    ///  Deletes the specified project and reloads the data.
    /// </summary>
    /// <param name="project">
    ///  The project to delete.</param>
    /// <returns>
    ///  A task that represents the asynchronous operation.</returns>
    async Task ProjectGridComponent_OnDeleteProjectClick(Project project)
    {
        try
        {
            // WHAT: If the project is new, it is removed from the list without calling the delete
            //  service.
            // WHY: New projects have not been persisted to the database, so there is no need to
            //  call the delete service.
            if (project.IsNew)
            {
                this.ProjectListData.Remove(project);
                return;
            }

            await this.DeleteProject(project);
            await this.InitializeData();
        }
        catch (Exception ex)
        {
            await base.DialogSvc.Alert($"An unexpected error occurred: {ex.Message}", AlertTitles.Error);
        }
    }   

    /// <summary>
    ///  Saves the updated project and reloads component data.
    /// </summary>
    /// <param name="project">
    ///  The updated project to save.</param>
    /// <returns>
    ///  A task that represents the asynchronous save and reload operation.</returns>
    async Task ProjectGridComponent_OnSaveProjectClick(Project project)
    {
        try
        {
            // WHAT: Selects the appropriate success message based on whether the project is new
            //  or existing.
            string successMsg = project.IsNew ? "Project created" : "Project saved";

            await this.SaveProject(project);
            await base.DialogSvc!.Alert(successMsg, AlertTitles.Success);
            await this.InitializeData();
        }
        catch (InvalidOperationException ex)
        {
            await base.DialogSvc.Alert(ex.Message, AlertTitles.Error);
        }
        catch(Exception ex)
        {
            await base.DialogSvc.Alert($"An unexpected error occurred: {ex.Message}", AlertTitles.Error);
        }
    }

    #endregion event handlers

    #region methods

    /// <summary>
    ///  Asynchronously initializes the data required by the component.
    /// </summary>
    /// <returns>
    ///  A task that represents the asynchronous initialization operation.
    /// </returns>
    async Task InitializeData()
    {
        this.ProjectListData = await this.GetProjectList();
    }

    /// <summary>
    ///  Determines whether a project with the same key already exists in the current project list.
    /// </summary>
    /// <remarks>
    ///  Returns <see langword="false"/> when <c>ProjectListData</c> is <see langword="null"/>.
    /// </remarks>
    /// <param name="project">
    ///  The project whose key is checked for duplicates.</param>
    /// <returns>
    ///  <see langword="true"/> if a project with a matching key exists in <c>ProjectListData</c>; 
    ///  otherwise, <see langword="false"/>.</returns>
    bool DuplicateProjectKeyExist(Project project) 
    {
        bool result = false;
        if (this.ProjectListData != null)
        {
            result = this.ProjectListData.Any(g => g.Key == project.Key && g.Id != project.Id);
        }
        return result;
    }

    #endregion methods
}
