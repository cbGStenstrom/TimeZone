using Microsoft.AspNetCore.Components;
using Radzen;
using TimeKeeper.App.Api.Services;
using TimeKeeper.App.Components;
using TimeKeeper.Domain.Models;
using TimeKeeper.Domain.Services.Interfaces;

namespace TimeKeeper.App.Shared.Layout;

/// <summary>
///  Provides the main layout component for the application, managing navigation, session state, 
///  and rendering of shared UI elements such as the sidebar and header.
/// </summary>
/// <remarks>
///  AppLayout serves as the root layout for pages in the application, coordinating navigation 
///  actions and user session management through injected services. It handles common UI interactions, 
///  such as sidebar navigation and logout functionality, and ensures users are redirected to the 
///  login page if not authenticated. This component is typically used as the top-level layout in 
///  Blazor applications to provide a consistent structure and shared behavior across pages.
/// </remarks>
public partial class AppLayout : LayoutComponentBase
{
    #region injected services

    /// <summary>
    ///  Gets or sets the dialog service used to display modal dialogs within the component.
    /// </summary>
    /// <remarks>
    ///  This property is typically injected by the framework and provides methods for showing
    ///  dialogs, alerts, or prompts to the user. Assigning a custom implementation allows for 
    ///  overriding dialog behavior.
    /// </remarks>
    [Inject]
    public DialogService? DialogSvc { get; set; }

    /// <summary>
    ///  Gets or sets the <see cref="NavigationManager"/> service used for managing navigation and 
    ///  URI operations within the application.
    /// </summary>
    /// <remarks>
    ///  This property is typically injected by the Blazor framework to enable programmatic navigation
    ///  and access to the current URI. It may be <see langword="null"/> if not set by the dependency 
    ///  injection system.
    /// </remarks>
    [Inject]
    public NavigationManager? NavigationMgr { get; set; }

    /// <summary>
    ///  Gets or sets the session service used to manage user session state within the component.
    /// </summary>
    /// <remarks>
    ///  This property is typically injected by the dependency injection framework. Assigning a 
    ///  value manually is not recommended unless overriding the default service behavior.
    /// </remarks>
    [Inject]
    public SessionService? SessionSvc { get; set; }

    /// <summary>
    /// Gets or sets the service used to manage time entry operations.
    /// </summary>
    [Inject]
    public ITimeEntryService? TimeEntrySvc { get; set; }

    #endregion injected services

    #region properties

    /// <summary>
    ///  Gets or sets the currently active time entry.
    /// </summary>
    TimeEntry? ActiveTimeEntry { get; set; }

    #endregion properties

    #region data-bound parameters
    #endregion data-bound parameters

    #region fields

    /// <summary>
    ///  Gets or sets a value indicating whether the sidebar is expanded.
    /// </summary>
    bool LeftSidebarExpanded { get; set; } = true;

    bool RightSidebarExpanded { get; set; } = false;

    #endregion fields

    #region events
    #endregion events

    #region data

    /// <summary>
    ///  Initializes and retrieves the currently active time entry, if one exists.
    /// </summary>
    /// <returns>
    ///  A task that represents the asynchronous operation. The task result contains the active 
    ///  <see cref="TimeEntry"/> if one exists; otherwise, <see langword="null"/>.
    /// </returns>
    async Task<TimeEntry?> InitializeActiveEntry()
    {
        var result = await this.TimeEntrySvc!.GetActiveTimeEntry();
        return result;
    }

    #endregion data

    #region lifecycle

    protected override async Task OnInitializedAsync()
    {
        this.CheckForLogin();
        this.ActiveTimeEntry = await this.InitializeActiveEntry();
    }

    #endregion lifecycle

    #region event handlers

    /// <summary>
    /// Handles the click event of the Dashboard button in the sidemenu.
    /// </summary>
    void btnDashboard_OnClick()
    {
        this.NavigationMgr?.NavigateTo("/Dashboard");
    }

    /// <summary>
    /// Handles the click event of the Home button in the sidemenu.
    /// </summary>
    void btnHome_OnClick()
    {
        this.NavigationMgr?.NavigateTo("/");
    }

    async void btnStartWork_OnClick()
    {
        await this.DialogSvc!.OpenAsync<TimeEntryStartWorkComponent>($"Order",
               new Dictionary<string, object>() { { "TimeEntryId", 55016 } },
               new DialogOptions()
               {
                   Resizable = false,
                   Draggable = false,
                   Width = "700px",
                   Height = "512px",
                   Left = "100px",
                   Top = "100px"
               });
    }

    /// <summary>
    /// Handles the click event of the Log Out button in the sidemenu.
    /// </summary>
    private void lnkLogOut_OnClick()
    {
        this.SessionSvc?.Logout();
        this.CheckForLogin();
    }

    /// <summary>
    /// Handles the click event of the Projects button on the sidemenu.
    /// </summary>
    void lnkProjects_OnClick()
    {
        this.NavigationMgr?.NavigateTo("/ProjectManagement");
    }

    /// <summary>
    ///  Handles the click event for the Project Manager link and navigates to the Project Manager 
    ///  page.
    /// </summary>
    /// <remarks>
    ///  This method uses the Navigation Manager to perform client-side navigation. If the Navigation
    ///  Manager is not available, no navigation occurs.
    /// </remarks>
    void lnkProjectMgr_OnClick()
    {
        this.NavigationMgr?.NavigateTo("/ProjectManager");
    }

    /// <summary>
    /// Handles the click event of the Test Page button on the sidemenu.
    /// </summary>
    void lnkTestPage_OnClick()
    {
        this.NavigationMgr?.NavigateTo("/TestPage");
    }

    /// <summary>
    /// Handles the click event of the WorkItems button on the sidemenu.
    /// </summary>
    void lnkTimeEntries_OnClick()
    {
        this.NavigationMgr?.NavigateTo("/TimeEntryManager");
    }

    /// <summary>
    /// Handles the click event of the WorkItems button on the sidemenu.
    /// </summary>
    void lnkWorkItems_OnClick(bool isNew = false)
    {
        if (isNew)
        {
            this.NavigationMgr?.NavigateTo("/WorkItemManager");
        }
        else
        {
            this.NavigationMgr?.NavigateTo("/WorkItemManagerOld");
        }
    }

    /// <summary>
    ///  Handles updates to the active time entry when a change occurs.
    /// </summary>
    /// <returns>
    ///  A task that represents the asynchronous operation.
    /// </returns>
    async Task TimeEntryStatusComponent_OnTimeEntryChanged()
    {
        this.ActiveTimeEntry = await this.InitializeActiveEntry();
        base.StateHasChanged();
    }

    #endregion event handlers

    #region private

    /// <summary>
    /// Evaluates that the user has successfully logged in and redirects them to Login if not.
    /// </summary>
    void CheckForLogin()
    {
        if (SessionSvc == null || SessionSvc.User == null)
        {
            if (NavigationMgr != null)
            {
                NavigationMgr.NavigateTo("/login");
            }
        }
    }

    #endregion private

    #region public

    /// <summary>
    ///  Gets the currently active time entry, if one exists.
    /// </summary>
    /// <returns>
    ///  The active <see cref="TimeEntry"/> if one is in progress; otherwise, <see langword="null"/>.
    /// </returns>
    public TimeEntry? GetActiveTimeEntry()
    {
        return this.ActiveTimeEntry;
    }

    /// <summary>
    ///  Asynchronously refreshes the active time entry and updates the component state.
    /// </summary>
    /// <remarks>
    ///  Call this method to ensure the displayed active time entry reflects the latest data from 
    ///  the database. This method triggers a UI update after refreshing the data.
    /// </remarks>
    /// <returns>
    ///  A task that represents the asynchronous refresh operation.
    /// </returns>
    public async Task Refresh()
    {
        this.ActiveTimeEntry = await this.InitializeActiveEntry();
        base.StateHasChanged();
    }

    /// <summary>
    ///  Sets the specified time entry as the active entry and refreshes the current state asynchronously.
    /// </summary>
    /// <remarks>
    ///  This method initiates an asynchronous refresh operation but does not return a Task. Exceptions 
    ///  thrown during the refresh may not be observed. For scenarios where exception handling or awaiting
    ///  completion is required, consider refactoring to use an asynchronous pattern that returns a Task.
    /// </remarks>
    /// <param name="timeEntryModel">
    ///  The time entry to set as active. Can be null to clear the active entry.
    /// </param>
    public async void SetActiveTimeEntry(TimeEntry timeEntryModel)
    {
        this.ActiveTimeEntry = timeEntryModel;
        await this.Refresh();
    }

    #endregion public
}