using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;
using TimeKeeper.DataAccess.Entities;
using TimeKeeper.Domain.Models.DTOs;
using TimeKeeper.Domain.Services.Interfaces;
using TimeKeeper.Domain.Utilities;

namespace TimeKeeper.App.Components.Reports
{
    public partial class WorkSummaryComponent : ComponentBase
    {
        #region injected services

        [Inject]
        public DialogService DialogService { get; set; }
        
        [Inject]
        public IJSRuntime JSRuntime { get; set; }

        [Inject]
        public IWorkItemService? WorkItemSvc { get; set; }


        [Inject]
        public ITimeEntryService? TimeEntrySvc { get; set; }

        #endregion injected services

        #region parameters

        /// <summary>
        ///  Gets/Sets the lower bound of the date range being displayed
        /// </summary>
        [Parameter]
        public DateOnly DateRangeEnd { get; set; }

        /// <summary>
        /// Supports 2-way data binding
        /// </summary>
        [Parameter]
        public EventCallback<DateOnly> DateRangeEndChanged { get; set; }

        /// <summary>
        ///  Gets/Sets the upper bound of the date range being displayed
        /// </summary>
        [Parameter]
        public DateOnly DateRangeStart { get; set; }

        /// <summary>
        /// Supports 2-way binding
        /// </summary>
        [Parameter]
        public EventCallback<DateOnly> DateRangeStartChanged { get; set; }

        /// <summary>
        /// Gets/Sets the width of the component
        /// </summary>
        [Parameter]
        public string Width { get; set; } = "100%";

        #endregion parameters

        #region properties

        /// <summary>
        ///  Gets/Sets the number of hours worked that were billable.
        /// </summary>
        public double BillableHours { get; set; }

        /// <summary>
        ///  Gets or sets the collection of work item date range summaries that match the current filter criteria.
        /// </summary>
        List<WorkItemDateRangeSummaryDto> FilteredWorkItemDateRangeSummaries { get; set; } = [];

        /// <summary>
        ///  Gets/Sets the number of hours worked that were no-billable.
        /// </summary>
        public double NonBillableHours { get; set; }

        /// <summary>
        ///  Gets or sets the identifier of the currently selected work item.
        /// </summary>
        int? SelectedWorkItemId { get; set; }

        /// <summary>
        ///  Gets or sets the collection of filter expressions applied to time entry summaries.
        /// </summary>
        /// <remarks>
        ///  Each expression in the collection is used to filter time entries when generating
        ///  summaries. All filters are combined using a logical AND operation. Modifying this 
        ///  collection affects which time entries are included in summary results.
        /// </remarks>
        List<Expression<Func<TimeEntry, bool>>> SummaryFilter { get; set; } = [];

        /// <summary>
        /// Get/Sets the total number of items int he list are no longer open.
        /// </summary>
        public int TotalClosedWorkItems { get; set; }

        /// <summary>
        ///  Gets or sets the total number of work items that are currently in the review state.
        /// </summary>
        public int TotalInReviewWorkItems { get; set; }

        /// <summary>
        /// Gets/Sets the collection of TimeEntries associated with a selected WorkItem
        /// </summary>
        IEnumerable<Domain.Models.TimeEntry>? TimeEntries { get; set; }

        /// <summary>
        ///  Gets/Sets the total number of hours worked.
        /// </summary>
        public double TotalHours { get; set; }

        /// <summary>
        ///  List of DTO objects which contain the Total Minutes worked for each WorkItem within the 
        ///  specified date range.
        /// </summary>
        List<WorkItemDateRangeSummaryDto> WorkItemDateRangeSummaries { get; set; } = [];

        /// <summary>
        ///  Gets or sets a predicate used to filter work items based on their date range summary.
        /// </summary>
        /// <remarks>
        ///  Assign a function to specify custom filtering logic for work items. If the value is
        ///  null, no filtering is applied and all work items are included.
        /// </remarks>
        Func<WorkItemDateRangeSummaryDto, bool>? WorkItemFilter { get; set; }

        #endregion properties

        #region events

        /// <summary>
        ///  Gets or sets the callback that is invoked when a work item is clicked.
        /// </summary>
        /// <remarks>
        ///  Use this event to handle user interactions with individual work items. The callback
        ///  receives the identifier of the clicked work item as its argument.
        /// </remarks>
        [Parameter]
        public EventCallback<int> OnWorkItemClick { get; set; }

        #endregion events

        #region data

        /// <summary>
        /// Retreives the WorkitemSummary data for the selected date range
        /// </summary>
        /// <returns></returns>
        async Task SetSummaries(Func<WorkItemDateRangeSummaryDto, bool> filter = null)
        {
            DateTime startDate  = this.DateRangeStart.ToDateTime(new TimeOnly(0, 0, 0));
            DateTime endDate    = this.DateRangeEnd.ToDateTime(new TimeOnly(23, 59, 59));

            this.FilteredWorkItemDateRangeSummaries = this.WorkItemDateRangeSummaries = 
                await WorkItemSvc!.GetWorkItemSummaryByDateRange(startDate, endDate) ?? [];

            // Set the global filter
            //
            this.WorkItemFilter = filter ?? this.WorkItemFilter;

            // If there are filters defined then apply them.
            //
            if (this.WorkItemFilter != null)
            {
                this.FilteredWorkItemDateRangeSummaries = [.. this.FilteredWorkItemDateRangeSummaries.Where(this.WorkItemFilter)];
            }
        }

        ///// <summary>
        /////  Retrieves and updates the list of time entries associated with the currently selected 
        /////  work item, applying the current summary filter.
        ///// </summary>
        ///// <returns>
        /////  A task that represents the asynchronous operation.
        ///// </returns>
        //async Task GetTimeEntriesForSelectedWorkItemId()
        //{
        //    this.TimeEntries = await TimeEntrySvc!.GetFilteredTimeEntries(this.SummaryFilter) ?? [];
        //}

        #endregion data

        #region lifecycle

        protected async override Task OnParametersSetAsync()
        {
            this.TimeEntries = [];
            await this.SetSummaries();
            await this.CalculateTotals();
        }

        #endregion lifecycle

        #region event handlers

        /// <summary>
        ///  Handles the click event for the Close Summary button, clearing the selected work item 
        ///  and updating the component state.
        /// </summary>
        /// <remarks>
        ///  Call this method in response to a user action that should dismiss or reset the current 
        ///  work item selection. This method triggers a UI update to reflect the cleared selection.
        /// </remarks>
        void btnCloseSummary_OnClick()
        {
            this.SelectedWorkItemId = null;
            base.StateHasChanged();
        }

        /// <summary>
        ///  Handles a summary card click event and updates the displayed time entry summaries based 
        ///  on the specified filter.
        /// </summary>
        /// <remarks>
        ///  This method resets the current time entries and applies the selected filter to update 
        ///  the summary view. The filter determines which time entries are included in the summary, 
        ///  such as all entries, only billable entries, or entries in review. If an unrecognized 
        ///  value is provided, no filter is applied and the summary is not updated.
        /// </remarks>
        /// <param name="args">
        ///  A string indicating the summary filter to apply. Valid values are "TOTAL", "BILLABLE", 
        ///  "NON-BILLABLE", "CLOSED", and "IN_REVIEW". The comparison is case-insensitive.
        /// </param>
        async void SummaryCard_OnClick(string args)
        {
            this.TimeEntries = null;
            this.InitializeSummaryFilter();

            switch(args.ToUpper())
            {
                case "TOTAL":
                    this.InitializeWorkItemFilter();
                    await this.SetSummaries();
                    await this.CalculateTotals();
                    break;
                case "BILLABLE":
                    await this.SetSummaries(e => e.IsBillable);
                    break;
                case "NON-BILLABLE":
                    await this.SetSummaries(e => !e.IsBillable);
                    break;
                case "CLOSED":
                    await this.SetSummaries(e => !e.IsOpen);
                    break;
                case "IN_REVIEW":
                    await this.SetSummaries(e => e.IsOpen && e.IsInReview);
                    break;
            }

            this.SelectedWorkItemId = null;
            base.StateHasChanged();
        }

        /// <summary>
        ///  Handles the click event for a work item by selecting it, updating related filters, and 
        ///  loading associated time entries.
        /// </summary>
        /// <remarks> 
        ///  If an event handler is registered with <see cref="OnWorkItemClick"/>, it is invoked
        ///  asynchronously with the selected work item ID. This method also triggers a UI state 
        ///  update after processing.
        /// </remarks>
        /// <param name="workitemId">
        ///  The unique identifier of the work item that was clicked.
        /// </param>
        async void WorkItem_OnClick(int workitemId)
        {
            this.SelectedWorkItemId = workitemId;
            //this.InitializeSummaryFilter();

            //await this.GetTimeEntriesForSelectedWorkItemId();

            if (this.OnWorkItemClick.HasDelegate)
            {
                await this.OnWorkItemClick.InvokeAsync(workitemId);
            }

            base.StateHasChanged();
        }

        #endregion event handlers

        #region private

        /// <summary>
        ///  Initializes the filter used to summarize time entries based on the current date range 
        ///  and selected work item.
        /// </summary>
        /// <remarks>
        ///  This method updates the SummaryFilter property to include criteria for the specified
        ///  date range and selected work item. It should be called whenever the date range or 
        ///  selected work item changes to ensure the filter remains accurate.
        /// </remarks>
        void InitializeSummaryFilter()
        {
            List<Expression<Func<TimeEntry, bool>>> result = [];
            DateTime startDate = this.DateRangeStart.ToDateTime(new TimeOnly(0, 0, 0)).ConvertLocalToUtc();
            DateTime endDate   = this.DateRangeEnd.ToDateTime(new TimeOnly(23, 59, 59)).ConvertLocalToUtc();

            result.Add(e => e.CreatedDate >= startDate);
            result.Add(e => e.CreatedDate <= endDate);
            result.Add(e => e.WorkItemId == this.SelectedWorkItemId);

            this.SummaryFilter = result;
        }

        /// <summary>
        ///  Resets the work item filter to its default state.
        /// </summary>
        /// <remarks>
        ///  Call this method to clear any existing work item filter and restore the default
        ///  filtering behavior. After calling this method, no filter will be applied to work items 
        ///  until a new filter is set.
        /// </remarks>
        void InitializeWorkItemFilter()
        {
            this.WorkItemFilter = null;
        }

        /// <summary>
        ///  Calculates the Total, Billable and NonBillable hours.
        /// </summary>
        Task CalculateTotals()
        {
            // Total hours
            //
            int totalMinutes = this.WorkItemDateRangeSummaries.Sum(e => e.TotalMinutesWorked);
            this.TotalHours = DateTimeUtils.MinutesToHours(totalMinutes);

            // Billable Hours
            //
            totalMinutes    = this.WorkItemDateRangeSummaries.Where(e=>e.IsBillable).Sum(e => e.TotalMinutesWorked);
            this.BillableHours  = DateTimeUtils.MinutesToHours(totalMinutes);

            // NonBillable Hours
            //
            totalMinutes = this.WorkItemDateRangeSummaries.Where(e => !e.IsBillable).Sum(e => e.TotalMinutesWorked);
            this.NonBillableHours = DateTimeUtils.MinutesToHours(totalMinutes);

            // TotalClosed Item count
            //
            this.TotalClosedWorkItems = this.WorkItemDateRangeSummaries.Where(e => !e.IsOpen).Count();

            // TotalInReview Item Count
            //
            this.TotalInReviewWorkItems= this.WorkItemDateRangeSummaries.Where(e => e.IsOpen && e.IsInReview).Count();
            return Task.CompletedTask;
        }

        /// <summary>
        ///  Returns the inline Style to be applied to the Grid of WorkItem results.
        /// </summary>
        /// <returns></returns>
        string GetInlineCssStyle()
        {
            //return $"width:{this.Width};";
            return $"flex-grow:1;";
        }

        /// <summary>
        ///  Returns the CssClass to be applied to individual items in the Grid of WorkItem results.
        /// </summary>
        /// <param name="summary"></param>
        /// <returns></returns>
        string GetItemCssClass(WorkItemDateRangeSummaryDto summary)
        {
            string cssOpacity = summary.IsOpen ? "" : "cb-opacity-4";
            string cssBillable = summary.IsBillable ? "cb-color-green" : "";

            return $"cb-item {cssOpacity} {cssBillable}";
        }

        /// <summary>
        ///  Returns the name of the status icon that represents the current state of the specified 
        ///  work item summary.   
        /// </summary>
        /// <remarks>
        ///  The returned icon name can be used to select a visual indicator in a user interface
        ///  based on the work item's status.
        /// </remarks>
        /// <param name="summary">
        ///  A summary object containing the status information for a work item. Cannot be null.
        /// </param>
        /// <returns>
        ///  A string containing the name of the icon to display for the work item's status. Returns
        ///  "integration_instructions" if the item is open and not in review, "content_paste_search" 
        ///  if the item is in review, or "inventory" if the item is closed.
        /// </returns>
        string GetStatusIcon(WorkItemDateRangeSummaryDto summary)
        {
            string result = "integration_instructions";

            if(!summary.IsOpen)
            {
                result = "inventory";
            }
            else if (summary.IsInReview)
            {
                result = "content_paste_search";
            }

            return result;
        }

        /// <summary>
        /// Returns the total hours of the currently selected set of workitems.
        /// </summary>
        /// <returns></returns>
        string GetTotalHours()
        {
            int totalMinutes  = this.WorkItemDateRangeSummaries.Sum(e => e.TotalMinutesWorked);
            double totalHours = DateTimeUtils.MinutesToHours(totalMinutes);

            return $"Total Hours: {totalHours.ToString("F2")} hrs";
        }

        #endregion private

        #region public
        #endregion public
    }
}