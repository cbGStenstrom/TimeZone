using Microsoft.AspNetCore.Components;

namespace TimeKeeper.App.Components.Reports
{
    public partial class WorkItemHistoryComponent : CbComponentBase
    {
        #region injected services
        #endregion injected services

        #region parameters
        [Parameter]
        public int? WorkItemId { get; set; }
        #endregion parameters

        #region properties
        private TimeKeeper.Domain.Models.WorkItem? SelectedWorkItem { get; set; }
        private List<TimeKeeper.Domain.Models.TimeEntry> Entries { get; set; } = [];
        #endregion properties

        #region events
        #endregion events

        #region data
        #endregion data

        #region lifecycle
        protected override async Task OnParametersSetAsync()
        {
            SelectedWorkItem = null;
            Entries = [];
            if (!WorkItemId.HasValue)
            {
                return;
            }

            int selectedId = WorkItemId.Value;
            var workItems = await WorkItemSvc.GetFilteredWorkItems([w => w.Id == selectedId]);
            SelectedWorkItem = workItems.FirstOrDefault();
            if (SelectedWorkItem == null)
            {
                return;
            }

            var entries = await TimeEntrySvc.GetFilteredTimeEntries([e => e.WorkItemId == selectedId]);
            Entries = [.. entries.OrderByDescending(e => e.StartWork).ThenByDescending(e => e.Id)];
        }
        #endregion lifecycle

        #region event handlers
        #endregion event handlers

        #region private
        #endregion private

        #region public
        #endregion public
    }
}
