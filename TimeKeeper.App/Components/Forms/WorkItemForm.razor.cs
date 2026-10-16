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

        [Inject] public IWorkItemTypeService TypeSvc { get; set; } = default!;
        List<WorkItemType> WorkItemTypes { get; set; } = [];
        bool RetainsDisabledType => WorkItem?.IsNew == false && WorkItem.Type?.IsEnabled == false && WorkItem.WorkItemType == WorkItem.Type.Id;

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

        protected override async Task OnParametersSetAsync()
        {
            await RefreshWorkItemTypes();
        }

        private async Task RefreshWorkItemTypes()
        {
            // Reused forms and open dropdowns must reflect current type management changes.
            var allTypes = await TypeSvc.GetTypes();
            WorkItemTypes = allTypes.Where(t => t.IsEnabled).ToList();
            if (WorkItem?.IsNew == false)
                WorkItem.Type = allTypes.SingleOrDefault(t => t.Id == WorkItem.WorkItemType);
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

        public void Rollback()
        {
            this.WorkItem?.RevertToSnapshot();
        }

        public bool Validate()
        {
            bool isValid = true;

            if (WorkItem?.ProjectId == null)
            {
                isValid = false;
            }

            if (WorkItem?.WorkItemType == null)
            {
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(WorkItem?.ActivityNumber))
            {
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(WorkItem?.Title))
            {
                isValid = false;
            }

            this.ShowValidation = !isValid;

            StateHasChanged();

            return isValid;
        }

        #endregion public
    }
}
