using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using TimeKeeper.Domain.Models;
using TimeKeeper.Domain.Services;
using TimeKeeper.Domain.Services.Interfaces;
using TimeKeeper.Domain.Utilities;

namespace TimeKeeper.App.Components.Forms
{
    public partial class TimeEntryFilter : ComponentBase
    {
        #region injected services

        [Inject]
        public ITimeEntryService? TimeEntrySvc { get; set; }

        [Inject]
        public IWorkItemService? WorkItemSvc { get; set; }

        #endregion injected services

        #region parameters
        #endregion parameters

        #region properties

        DateTime? EndDate { get; set; }

        WorkItem? SelectedWorkItem { get; set; }
        DateTime? StartDate { get; set; }

        /// <summary>
        /// Gets the list of WorkItems that will be searchable in the dropdown
        /// </summary>
        List<WorkItem> WorkItemData { get; } = new List<WorkItem>();

        /// <summary>
        /// Gets/Sets the text to search the WorkItemTitle on.
        /// </summary>
        string? WorkItemSearchTerm { get; set; }

        #endregion properties

        #region events

        [Parameter]
        public EventCallback OnFilterClick { get; set; }

        #endregion events

        #region data

        /// <summary>
        /// Loads the WorkItems which are bound to the drop down filter.
        /// </summary>
        /// <returns></returns>
        async Task LoadWorkItemData()
        {
            var data = await this.WorkItemSvc!.GetFilteredWorkItems();
            if (data != null) 
            { 
                this.WorkItemData.Clear();

                foreach(var workItem in data.OrderByDescending(e=>e.CreatedDate).ToList())
                {
                    this.WorkItemData.Add(workItem);
                }
            }
        }

        #endregion data

        #region lifecycle

        protected override async Task OnInitializedAsync()
        {
            await this.InitializeFilter();
            //await this.LoadWorkItemData();
        }

        #endregion lifecycle

        #region event handlers

        /// <summary>
        /// Handles the onclick event of the Filter button. 
        /// </summary>
        void btnFilter_OnClick()
        {
            if(this.OnFilterClick.HasDelegate)
            {
                this.OnFilterClick.InvokeAsync();
            }
        }

        #endregion event handlers

        #region private

        Task InitializeFilter()
        {
            DateTime startOfDay = DateTime.Now.Date;
            DateTime endOfDay = DateTime.Now.Date.AddDays(1).AddTicks(-1);

            this.StartDate = startOfDay;
            this.EndDate = endOfDay;

            return Task.CompletedTask;
        }

        #endregion private

        #region public

        /// <summary>
        /// Assembles and Returns the filter expression if any is defined.
        /// </summary>
        /// <returns></returns>
        public List<Expression<Func<DataAccess.Entities.TimeEntry, bool>>>? GetFilter()
        {
            List<Expression<Func<DataAccess.Entities.TimeEntry, bool>>> result = null;

            if(this.SelectedWorkItem != null)
            {
                result = result ?? new List<Expression<Func<DataAccess.Entities.TimeEntry, bool>>>();
                result.Add(e => e.WorkItemId == this.SelectedWorkItem.Id);
            }

            if (this.StartDate.HasValue)
            {
                result = result ?? new List<Expression<Func<DataAccess.Entities.TimeEntry, bool>>>();
                result.Add(e=>e.StartWork >= this.StartDate.Value.ConvertLocalToUtc());
            }

            if (this.EndDate.HasValue)
            {
                result = result ?? new List<Expression<Func<DataAccess.Entities.TimeEntry, bool>>>();
                result.Add(e => e.EndWork <= this.EndDate.Value.ConvertLocalToUtc());
            }

            return result;
        }

        #endregion public
    }
}
