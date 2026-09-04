using System.Linq.Expressions;
using System.Text;
using Microsoft.AspNetCore.Components;
using TimeKeeper.App.Api.Models;
using TimeKeeper.Domain.Models;
using TimeKeeper.Domain.Services.Interfaces;
using TimeKeeper.Domain.Utilities;

namespace TimeKeeper.App.Components.Reports
{
    /// <summary>
    ///  Summarizes TimeEntries for a given date by Workitem.
    /// </summary>
    public partial class WorkItemByDate : ComponentBase
    {
        #region injected services

        [Inject]
        public IWorkItemService? WorkItemSvc { get; set; }


        [Inject]
        public ITimeEntryService? TimeEntrySvc{ get; set; }

        #endregion injected services

        #region parameters

        /// <summary>
        /// Gets/Sets the date to filter the report on.
        /// </summary>
        [Parameter]
        public DateOnly SelectedDate { get; set; } = DateTime.Now.ToDateOnly();

        #endregion parameters

        #region properties

        /// <summary>
        ///  Gets/Sets the accomplishments associated with the selected Workitem
        /// </summary>
        string Accomplishments { get; set; } = string.Empty;

        /// <summary>
        /// Gets the list of workitems and accumulated hours within the Selected Date range.
        /// </summary>
        Dictionary<int, WorkItemHoursDTO> HoursByWorkItem { get; } = new Dictionary<int, WorkItemHoursDTO>();

        /// <summary>
        /// Gets/Sets a reference to a WorkItem selected from the list.
        /// </summary>
        WorkItem? SelectedWorkItem { get; set; }

        List<TimeEntry> TimeEntries { get; } = new List<TimeEntry>();

        #endregion properties

        #region events
        #endregion events

        #region data

        /// <summary>
        /// Returns the list of Time Entries which fall within the SelectedDate 
        /// </summary>
        /// <returns></returns>
        async Task LoadTimeEntries()
        {
            var filter = new List<Expression<Func<DataAccess.Entities.TimeEntry, bool>>>();
            filter.Add(timeEntry => timeEntry.StartWork >= this.SelectedDate.GetStartOfDay());
            filter.Add(timeEntry => timeEntry.EndWork  <= this.SelectedDate.GetEndOfDay());

            IEnumerable<TimeEntry> timeEntries = await this.TimeEntrySvc!.GetFilteredTimeEntries(filter);

            this.TimeEntries.Clear();
            this.TimeEntries.AddRange(timeEntries);
        }
        
        /// <summary>
        /// Loads the WorkItem Summaries associated with each of it's associated TimeEntries 
        /// </summary>
        /// <param name="workitem"></param>
        /// <returns></returns>
        Task LoadWorkitemAccomplishments(WorkItem workitem)
        {
            StringBuilder result = new StringBuilder();

            foreach (var item in this.TimeEntries.OrderBy(e=>e.StartWork))
            {
                if (item.WorkItemId == workitem.Id)
                {
                    result.AppendLine($"<div class=\"cb-timestamp\">{item.StartWork?.ToString("T")} - {item.EndWork?.ToString("T")}</div>");
                        
                    if (!string.IsNullOrEmpty(item.Accomplishment))
                    {
                        result.AppendLine(item.Accomplishment);
                        result.AppendLine("<hr/>");
                    }
                }
            }

            this.Accomplishments = result.ToString();

            return Task.CompletedTask;
        }

        #endregion data

        #region lifecycle

        protected override async Task OnInitializedAsync()
        {
            await this.InitializeHoursByWorkItem();
        }

        #endregion lifecycle

        #region event handlers

        /// <summary>
        ///  Handles the event raised when the user selectes a new date from the DatePicker
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public async Task DatePicker_OnChange(DateTime? dateTime)
        {
            await this.InitializeHoursByWorkItem();
            this.Accomplishments = "";
            this.SelectedWorkItem = null;
            base.StateHasChanged();
        }

        /// <summary>
        /// Handles the event raised when the user clicks on a work item.
        /// </summary>
        /// <param name="workitem"></param>
        /// <returns></returns>
        public async void Item_OnClick(WorkItem workitem)
        {
            this.SelectedWorkItem = workitem;
            await this.LoadWorkitemAccomplishments(workitem);
            base.StateHasChanged();
        }

        #endregion event handlers

        #region private

        /// <summary>
        /// Returns the total hours (rounded to nearest quarter hour) for the selected date. Re-
        /// calculate this rather than simply adding up the total hours for each workitem, as adding
        /// the individual item's rounded value could leave time uncounted. For example if I work 
        /// for 20 minutes 3 times, each work item will be worth .25 hours, totalling to 45 minutes. 
        /// But I really would have worked 60 minutes, 1 hour. We want to make sure that for a daily 
        /// total we count ALL the time THEN round up or down.
        /// </summary>
        double CalculateTotalHours()
        {
            // Get total number of minutes worked ...
            //
            int totalMinutes = 0;

            foreach(TimeEntry model in this.TimeEntries)
            {
                if (model.EndWork.HasValue && model.StartWork.HasValue)
                {
                    totalMinutes += (int)(model.EndWork.Value - model.StartWork.Value).TotalMinutes;
                }
            }

            return DateTimeUtils.MinutesToHours(totalMinutes);

            //int hours = (int)(totalMinutes / 60);
            //int minutes = totalMinutes - (60 * hours);

            //double quarterHour = 0;

            //if (minutes > 8 && minutes < 22)
            //{
            //    quarterHour = .25;
            //}
            //else if (minutes > 21 && minutes < 37)
            //{
            //    quarterHour = .5;
            //}
            //else if (minutes > 36 && minutes < 51)
            //{
            //    quarterHour = .75;
            //}
            //else if (minutes > 50)
            //{
            //    hours++;
            //}

            //return hours + quarterHour;

        }

        /// <summary>
        ///  Organizaes, groups and calculates hours worked for the TimeEntry data associated with 
        ///  the SelectedDate.
        /// </summary>
        /// <returns></returns>
        async Task InitializeHoursByWorkItem()
        {
            await this.LoadTimeEntries();

            // Group the Entries by their workitem Id
            //
            var entryGroupQuery = from entries in this.TimeEntries 
                        group entries by entries.WorkItemId into entryGroup
                        orderby entryGroup.Key
                        select entryGroup;

            HoursByWorkItem.Clear();

            // For each workitem group sum the total hours worked on each time entry
            //
            foreach (var entryGroup in entryGroupQuery)
            {
                WorkItem? workItem = null;
                double hoursWorked = 0;

                foreach (var entry in entryGroup)
                {
                    if (entry.WorkItem != null)
                    {
                        workItem = entry.WorkItem;
                        hoursWorked += entry.HoursWorked ?? 0;
                    }
                }

                if (workItem != null)
                {
                    WorkItemHoursDTO dto = new WorkItemHoursDTO(workItem, hoursWorked);
                    HoursByWorkItem.Add(workItem.Id, dto);
                }
            }
        }

        #endregion private

        #region public
        #endregion public
    }
}
