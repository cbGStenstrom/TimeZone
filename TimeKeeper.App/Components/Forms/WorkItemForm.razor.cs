using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore.Infrastructure;
using TimeKeeper.Domain.Models;
using TimeKeeper.Domain.Services;
using TimeKeeper.Domain.Services.Interfaces;

namespace TimeKeeper.App.Components.Forms
{
    public partial class WorkItemForm : ComponentBase
    {
        #region injected services

        [Inject]
        public IProjectService? ProjectSvc { get; set; }

        #endregion injected services

        #region parameters

        [Parameter]
        public WorkItem? WorkItem { get; set; }

        [Parameter]
        public EventCallback WorkItemChanged { get; set; }

        #endregion parameters

        #region properties

        List<Project> ProjectList { get; set; } = new List<Project>();

        bool ShowValidation { get; set; } = false;

        IEnumerable<TimeKeeper.Domain.Enums.WorkItemType> WorkItemTypes { get; } = Enum.GetValues<TimeKeeper.Domain.Enums.WorkItemType>();

        #endregion properties

        #region events

        [Parameter]
        public EventCallback<WorkItem> OnWorkItemChanged { get; set; }

        #endregion events

        #region data

        async Task PopulateProjectList()
        {
            List<Project> data = (this.ProjectSvc != null) ? await this.ProjectSvc.GetProjects() : new List<Project>();
            this.ProjectList = data;
        }

        #endregion data

        #region lifecycle

        protected async override Task OnInitializedAsync()
        {
            await this.PopulateProjectList();
        }

        #endregion lifecycle

        #region event handlers

        public async Task Form_OnChange()
        {
            if (OnWorkItemChanged.HasDelegate)
            {
                await this.OnWorkItemChanged.InvokeAsync();
            }
        }

        #endregion event handlers

        #region private
        #endregion private

        #region public

        /// <summary>
        /// Validates the form, ensuring that the required WorkItemType field has been set. If
        /// invalid, sets a flag causing the validation message to be displayed.
        /// </summary>
        /// <returns>True if the form is valid; otherwise false.</returns>
        public bool Validate()
        {
            bool isValid = this.WorkItem?.WorkItemType != null;

            this.ShowValidation = !isValid;
            base.StateHasChanged();

            return isValid;
        }

        #endregion public
    }
}