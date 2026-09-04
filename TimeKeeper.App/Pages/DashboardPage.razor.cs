using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;
using System.Linq.Expressions;
using TimeKeeper.App.Api.Models;
using TimeKeeper.App.Components.Pages;
using TimeKeeper.Domain.Services.Interfaces;
using TimeKeeper.Domain.Utilities;

namespace TimeKeeper.App.Pages
{
    public partial class DashboardPage : CbPageBase
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
        #endregion parameters

        #region properties

        /// <summary>
        /// Gets/Sets the label expressing the selected Date range.
        /// </summary>
        String DateLabel { get; set; } = string.Empty;

        /// <summary>
        /// Gets/Sets the date that represents the upper bounds of any date range lookups
        /// </summary>
        DateOnly DateRangeEnd { get; set; }

        /// <summary>
        /// Gets/Sets the collection of options available in the Date Range Drop Down List.
        /// </summary>
        Dictionary<string, string> DateRangeOptions { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets/Sets the date that represents the lower bounds of any date range lookups
        /// </summary>
        DateOnly DateRangeStart { get; set; }

        /// <summary>
        ///  Gets or sets a value indicating whether the right sidepanel is currently expanded.
        /// </summary>
        bool PanelIsExpanded = false;

        /// <summary>
        /// Get/Sets the Date selected in the Drop Down List box.
        /// </summary>
        DateTime SelectedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Get/Set flag indicating whether the user has chosen Work Day or Work Week date range.
        /// </summary>
        string SelectedDateRangeOption { get; set; } = "DAY";


        int SelectedWorkItemId { get; set; }

        /// <summary>
        /// Gets/Sets the collection of TimeEntries associated with a selected WorkItem
        /// </summary>
        IEnumerable<Domain.Models.TimeEntry>? TimeEntries { get; set; }

        #endregion properties

        #region events
        #endregion events

        #region data

        /// <summary>
        ///  Loads the DateRange options property that are loaded into the Drop Down List.
        /// </summary>
        /// <returns></returns>
        Task LoadDateRangeOptions()
        {
            this.DateRangeOptions = new Dictionary<string, string>{
                { "DAY", "Work Day"},
                { "WEEK", "Work Week"}
            };

            return Task.CompletedTask;
        }

        /// <summary>
        ///  Asynchronously loads the time entry history for a specified work item within the current 
        ///  date range.
        /// </summary>
        /// <remarks>
        ///  The date range is determined by the current values of the DateRangeStart and DateRangeEnd 
        ///  properties. Only time entries created within this range are included.
        /// </remarks>
        /// <param name="workitemId">
        ///  The unique identifier of the work item for which to retrieve time entry history.
        /// </param>
        /// <returns>
        ///  A task that represents the asynchronous operation.
        /// </returns>
        async Task LoadWorkitemHistoryForDateRange(long workitemId)
        {
            DateTime startDate = this.DateRangeStart.ToDateTime(new TimeOnly(0, 0, 0));
            DateTime endDate   = this.DateRangeEnd.ToDateTime(new TimeOnly(23, 59, 59));

            List<Expression<Func<TimeKeeper.DataAccess.Entities.TimeEntry, bool>>> filter = [];
            filter.Add(e => e.WorkItemId == workitemId && e.CreatedDate >= startDate && e.CreatedDate <= endDate);

            this.TimeEntries = await TimeEntrySvc!.GetFilteredTimeEntries(filter) ?? [];
        }

        #endregion data

        #region lifecycle

        protected override async Task OnInitializedAsync()
        {
            await this.LoadDateRangeOptions();
            await this.SetWorkDayDateRange();
        }

        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);
        }

        #endregion lifecycle

        #region event handlers

        async void DatePicker_OnChange()
        {
            await this.SetDateRange();
            base.StateHasChanged();
        }

        async void DateRangeOption_OnChange()
        {
            await this.SetDateRange();
            base.StateHasChanged();
        }

        async void WorkSummary_OnItemClick(int workitemId)
        {
            this.SelectedWorkItemId = workitemId;
            this.PanelIsExpanded    = true;

            base.StateHasChanged();
        }

        #endregion event handlers

        #region private

        async Task OpenSummaryDialog()
        {
            DialogSettings settings = new DialogSettings
            {
                Height = "300px",
                Width = "300px"
            };

            DialogOptions options = new DialogOptions
            {
                Resizable = false,
                Draggable = false,
                //Resize = OnResize,
                //Drag = OnDrag,
                Width = settings != null ? settings.Width : "700px",
                Height = settings != null ? settings.Height : "512px",
                Left = settings != null ? settings.Left : null,
                Top = settings != null ? settings.Top : null
            };

            await base.DialogSvc!.OpenAsync<WorkItemHistoryPage>("Title", null, options);
        }

        /// <summary>
        /// Decides whether to set the Date range by Day or Work Week.
        /// </summary>
        /// <returns></returns>
        async Task SetDateRange()
        {
            switch (this.SelectedDateRangeOption.ToUpper())
            {
                case "WEEK":
                    await SetWorkWeekDateRange();
                    break;
                default:
                    await SetWorkDayDateRange();
                    break;
            }
        }

        /// <summary>
        ///  Sets the Start and End dates of the Date Range to represent a single 24-hour day based 
        ///  on the selected date.
        /// </summary>
        Task SetWorkDayDateRange()
        {
            DateRangeStart = this.SelectedDate.ToDateOnly();
            DateRangeEnd = this.SelectedDate.ToDateOnly();

            DateLabel = $"{DateRangeStart:dddd, dd MMMM yyyy}";

            return Task.CompletedTask;
        }

        /// <summary>
        ///  Sets the Start and End dates of the Date Range to represent a billing week (Sat - Fri)
        ///  based on the selected date.
        /// </summary>
        Task SetWorkWeekDateRange()
        {
            // The work range starts on the Saturday prior to the SelectedDate
            // 

            DayOfWeek today = this.SelectedDate.DayOfWeek;

            switch (today)
            {
                case DayOfWeek.Saturday:
                    DateRangeStart = this.SelectedDate.ToDateOnly();
                    break;
                default:
                    int offset = (((int)today) + 1) * -1;
                    DateRangeStart = this.SelectedDate.AddDays(offset).ToDateOnly();
                    break;

            }

            DateRangeEnd = DateRangeStart.AddDays(6);

            DateLabel = $"{DateRangeStart:dddd, dd MMMM yyyy} -- {DateRangeEnd:dddd, dd MMMM yyyy}";

            return Task.CompletedTask;
        }

        #endregion private

        #region public
        #endregion public
    }
}
