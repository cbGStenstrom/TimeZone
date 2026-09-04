using Microsoft.AspNetCore.Components;
using TimeKeeper.App.Api.Services;

namespace TimeKeeper.App.Components.Layout
{
    public partial class MainLayout : LayoutComponentBase
    {
        #region injected services

        [Inject]
        public NavigationManager? NavigationManager { get; set; }

        [Inject]
        public SessionService? SessionService { get; set; }

        #endregion injected services

        #region parameters
        #endregion parameters

        #region data-bound parameters
        #endregion data-bound parameters

        #region fields
        
        bool sidebarExpanded = true;

        #endregion fields

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

        /// <summary>
        /// Handles the click event of the Dashboard button in the sidemenu.
        /// </summary>
        void btnDashboard_OnClick()
        {
            this.NavigationManager?.NavigateTo("/Dashboard");
        }

        /// <summary>
        /// Handles the click event of the Home button in the sidemenu.
        /// </summary>
        void btnHome_OnClick()
        {
            this.NavigationManager?.NavigateTo("/");
        }

        /// <summary>
        /// Handles the click event of the Log Out button in the sidemenu.
        /// </summary>
        private void lnkLogOut_OnClick()
        {
            this.SessionService?.Logout();
            this.CheckForLogin();
        }

        /// <summary>
        /// Handles the click event of the Projects button on the sidemenu.
        /// </summary>
        void lnkProjects_OnClick()
        {
            this.NavigationManager?.NavigateTo("/ProjectManager");
        }

        /// <summary>
        /// Handles the click event of the WorkItems button on the sidemenu.
        /// </summary>
        void lnkTimeEntries_OnClick()
        {
            this.NavigationManager?.NavigateTo("/TimeEntryManager");
        }

        /// <summary>
        /// Handles the click event of the WorkItems button on the sidemenu.
        /// </summary>
        void lnkWorkItems_OnClick()
        {
            this.NavigationManager?.NavigateTo("/WorkItemManager");
        }

        #endregion event handlers

        #region private

        /// <summary>
        /// Evaluates that the user has successfully logged in and redirects them to Login if not.
        /// </summary>
        void CheckForLogin()
        {
            if (SessionService == null || SessionService.User == null)
            {
                if (NavigationManager != null)
                {
                    NavigationManager.NavigateTo("/login");
                }
            }
        }

        #endregion private

        #region public
        #endregion public
    }
}
