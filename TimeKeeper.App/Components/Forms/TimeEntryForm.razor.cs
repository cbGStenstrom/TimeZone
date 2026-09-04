using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore.Infrastructure;
using TimeKeeper.App.Api.Services;
using TimeKeeper.Domain.Models;
using TimeKeeper.Domain.Services.Interfaces;

namespace TimeKeeper.App.Components.Forms
{
    public partial class TimeEntryForm : ComponentBase
    {
        #region injected services

        [Inject]
        public SessionService? SessionSvc{ get; set; }

        [Inject]
        public ILaborerService? UserSvc { get; set; }

        [Inject]
        public IWorkItemService? WorkItemSvc { get; set; }

        #endregion injected services

        #region parameters

        [Parameter]
        public WorkItem? WorkItem { get; set; }

        [Parameter]
        public EventCallback WorkItemChanged { get; set; }

        #endregion parameters

        #region properties

        TimeEntry? SelectedTimeEntry { get; set; }

        #endregion properties

        #region events

        [Parameter]
        public EventCallback<WorkItem> OnTimeEntryChanged { get; set; }

        #endregion events

        #region data
        #endregion data

        #region lifecycle

        protected override Task OnInitializedAsync()
        {
            this.InitializeNewTimeEntry();
            return base.OnInitializedAsync();
        }

        #endregion lifecycle

        #region event handlers

        public async void btnAdd_OnClick()
        {
            if (this.SelectedTimeEntry != null)
            {
                this.WorkItem?.TimeEntries.Add(this.SelectedTimeEntry);
                this.InitializeNewTimeEntry();

                await this.Form_OnChange();

                base.StateHasChanged();
            }
        }

        public async Task Form_OnChange()
        {
            if (OnTimeEntryChanged.HasDelegate)
            {
                await this.OnTimeEntryChanged.InvokeAsync();
            }
        }

        #endregion event handlers

        #region private

        void InitializeNewTimeEntry()
        {
            if(SessionSvc != null && SessionSvc.User != null && this.WorkItem != null)
            {
                this.SelectedTimeEntry = new TimeEntry()
                {
                    CreatedBy = SessionSvc.User.Username,
                    LaborerId = SessionSvc.User.Id,
                    UpdatedBy = SessionSvc.User.Username,
                    WorkItemId = this.WorkItem.Id,
                };

                this.SelectedTimeEntry.TakeSnapshot();
            }
        }

        #endregion private

        #region public
        #endregion public
    }
}
