using TimeKeeper.App.Api.Enums;
using TimeKeeper.App.Components.Pages;
using TimeKeeper.Domain.Models;

namespace TimeKeeper.App.Pages;

public partial class ProjectManagerPage : CbPageBase
{

    #region injected services
    #endregion injected services

    #region properties

    /// <summary>
    ///  Gets or sets the collection of projects currently loaded in the application.
    /// </summary>
    List<Project> ProjectListData { get; set; } = [];

    #endregion properties

    #region data

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
        List<Project> result = await base.ProjectSvc!.GetProjects() ?? [];
        return result;
    }

    #endregion data

    #region lifecycle

    protected override async Task OnInitializedAsync()
    {
        await this.InitializeData();
    }

    #endregion lifecycle

    #region event handlers

    /// <summary>
    ///  Handles the event when a project is updated in the project grid component.
    /// </summary>
    /// <param name="savedProject">
    ///  The project that was updated. Cannot be null.
    /// </param>
    /// <returns>
    ///  A task that represents the asynchronous operation.
    /// </returns>
    async Task ProjectGridComponent_OnProjectUpdated(Project savedProject)
    {
        await this.InitializeData();
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

    #endregion methods
}
