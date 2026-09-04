using Microsoft.AspNetCore.Components;
using TimeKeeper.App.Api.Services;
using TimeKeeper.Domain.Models;
using TimeKeeper.Domain.Services.Interfaces;

namespace TimeKeeper.App.Components.Pages;

public class CbPageBase : CbComponentBase
{
    #region injected services

    //[Inject]
    //public DialogService? DialogSvc { get; set; }

    //[Inject]
    //public IJSRuntime JSRuntime { get; set; }

    //[Inject]
    //public ILaborerService? LaborerService { get; private set; }

    [Inject]
    public NavigationManager NavigationMgr { get; set; } = default!;

    [Inject]
    public IProjectService ProjectSvc { get; set; } = default!;

    [Inject]
    public SessionService SessionSvc { get; set; } = default!;

    //[Inject]
    //public ITimeEntryService? TimeEntrySvc { get; set; }

    //[Inject]
    //public IWorkItemService? WorkItemSvc { get; set; }

    #endregion injected services

    #region parameters

    /// <summary>
    ///  Gets or sets the currently active time entry for the component.
    /// </summary>
    [CascadingParameter(Name = "ActiveTimeEntry")]
    public TimeEntry? ActiveTimeEntry { get; set; }

    ///// <summary>
    /////  Gets or sets the layout information for the current component as provided by the 
    /////  cascading parameter.
    ///// </summary>
    ///// <remarks>
    /////  This property receives its value from an ancestor component that provides the 'Layout' 
    /////  cascading parameter. It enables child components to access or modify layout-related 
    /////  settings or state shared across the component hierarchy.
    ///// </remarks>
    //[CascadingParameter(Name ="Layout")]
    //public AppLayout? Layout { get; set; }

    #endregion parameters

    #region data-bound parameters
    #endregion data-bound parameters

    #region properties
    #endregion properties

    #region events
    #endregion events

    #region data
    #endregion data

    #region lifecycle

    protected override Task OnInitializedAsync()
    {
        this.CheckForLogin();
        return base.OnInitializedAsync();
    }

    #endregion lifecycle

    #region event handlers
    #endregion event handlers

    #region private

    /// <summary>
    /// Evaluates that the user has successfully logged in and redirects them to Login if not.
    /// </summary>
    void CheckForLogin()
    {
        if (SessionSvc == null || SessionSvc.User == null)
        {
            if(NavigationMgr != null)
            {
                NavigationMgr.NavigateTo("/login");
            }
        }
    }

    #endregion private

    #region public
    #endregion public


}
