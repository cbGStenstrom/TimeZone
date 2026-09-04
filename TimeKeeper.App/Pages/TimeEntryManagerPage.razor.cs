using Microsoft.AspNetCore.Components;
using TimeKeeper.App.Api.Enums;
using TimeKeeper.App.Api.Services;
using TimeKeeper.App.Components;
using TimeKeeper.App.Components.Dialogs;
using TimeKeeper.App.Components.Forms;
using TimeKeeper.App.Components.Pages;
using TimeKeeper.Domain.Models;
using TimeKeeper.Domain.Services.Interfaces;

namespace TimeKeeper.App.Pages
{
    public partial class TimeEntryManagerPage : CbPageBase
    {
        #region injected services

        //[Inject]
        //public SessionService? SessionService { get; set; }

        //[Inject]
        //public ITimeEntryService? TimeEntrySvc { get; set; }

        [Inject]
        public ILaborerService? UserSvc { get; set; }

        //[Inject]
        //public IWorkItemService? WorkItemSvc { get; set; }

        #endregion injected services

        #region parameters
        #endregion parameters

        #region properties

        TimeEntryEditorDialog dlgTimeEntryEditor { get; set; }

        TimeEntryFilter Filter { get; set; }

        IEnumerable<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();

        IEnumerable<WorkItem> WorkItems { get; set; } = new List<WorkItem>();

        #endregion properties

        #region events
        #endregion events

        #region data

        async Task LoadWorkItems()
        {
            if(this.WorkItemSvc != null)
            {
                WorkItems = await this.WorkItemSvc.GetFilteredWorkItems();
            }
        }

        async Task LoadTimeEntries()
        {
            if(this.SessionService  != null && this.SessionService.User != null)
            {
                var filter = this.Filter?.GetFilter() ?? null;
                TimeEntries = await this.TimeEntrySvc!.GetFilteredTimeEntries(filter) ?? new List<TimeEntry>();
            }
        }

        /// <summary>
        ///  Asynchronously saves a new time entry for the current user.
        /// </summary>
        /// <param name="timeEntryModel">
        ///  The time entry data to be saved. Must not be null.
        /// </param>
        /// <returns>
        ///  A task that represents the asynchronous save operation.
        /// </returns>
        async Task SaveNewTimeEntry(TimeEntry timeEntryModel)
        {
            if(this.TimeEntrySvc != null && this.SessionSvc?.User != null)
            {
                var result = this.TimeEntrySvc.CreateNewTimeEntry(timeEntryModel, this.SessionSvc.User);
            }
        }

        /// <summary>
        ///  Updates an existing time entry with the specified data asynchronously.
        /// </summary>
        /// <param name="timeEntryModel">
        ///  The time entry model containing the updated data to apply. Cannot be null.
        /// </param>
        /// <returns>
        ///  A task that represents the asynchronous update operation.
        /// </returns>
        async Task UpdateTimeEntry(TimeEntry timeEntryModel)
        {
            if (this.TimeEntrySvc != null && this.SessionSvc?.User != null)
            {
                var result = await this.TimeEntrySvc.UpdateTimeEntry(timeEntryModel, this.SessionSvc.User);
            }
        }

        #endregion data

        #region lifecycle

        protected override async Task OnInitializedAsync()
        {
            await this.LoadWorkItems();
            await this.LoadTimeEntries();
        }

        #endregion lifecycle

        #region event handlers

        /// <summary>
        ///  Opens a dialog for creating a new time entry and initializes it with the current start 
        ///  time.
        /// </summary>
        /// <remarks>
        ///  The dialog is pre-populated with a new time entry whose start time is set to the current 
        ///  date and time. When the user saves the entry, the provided callback is invoked. The dialog 
        ///  is configured with custom size and modal options.
        /// </remarks>
        /// <returns>
        ///  A task that represents the asynchronous operation of opening the time entry dialog.
        /// </returns>
        async Task btnAddTimeEntry_OnClick()
        {
            var dialogOnSaveCallback = EventCallback.Factory.Create<TimeEntry>(this, Dialog_OnTimeEntryAdded);
            await this.dlgTimeEntryEditor.Open(new TimeEntry() { StartWork = DateTime.Now }, "New Time Entry", dialogOnSaveCallback, TimeEntrySaveActions.InsertTimeEntry);
        }

        /// <summary>
        ///  Handles the event, raised by the NEw Time Entry dialog box when the Save button is clicked.
        /// </summary>
        /// <param name="timeEntry">
        ///  The time entry to add and persist. Cannot be null.
        /// </param>
        /// <returns>
        ///  A task that represents the asynchronous operation.
        /// </returns>
        async Task Dialog_OnTimeEntryAdded(TimeEntry timeEntry)
        {
            this.dlgTimeEntryEditor.Close();

            // if this an Active TimeEntry (EndWork == null) set it as the globally accessible TimeEntry
            //
            if (timeEntry?.EndWork != null) base.Layout!.SetActiveTimeEntry(timeEntry);

            await this.LoadTimeEntries();
            base.StateHasChanged();
            await base.SaveStateAsync();
        }

        /// <summary>
        /// Handles the event raised when the Apply Filter button has been clicked
        /// </summary>
        async void Filter_OnClick()
        {
            await this.LoadTimeEntries();
            base.StateHasChanged();
        }

        /// <summary>
        /// Handles the event raised when the Delete button on a TimeEntry row has been clicked
        /// </summary>
        /// <param name="model"></param>
        async void Row_OnDeleteButtonClick(TimeEntry model)
        {
            // Reload the list of Time entries siince one has potentially been deleted.
            //
            await this.LoadTimeEntries();
            base.StateHasChanged();
        }

        /// <summary>
        /// Handles the event raised when the Save button on a TimeEntry row has been clicked
        /// </summary>
        void Row_OnSaveButtonClick()
        {
            this.CalculateTotalHours();
            base.StateHasChanged();
        }

        async void Row_OnTimeEntrySaved(TimeEntry timeEntryModel)
        {
            // if this an Active TimeEntry (EndWork == null) set it as the globally accessible TimeEntry
            //
            if (timeEntryModel?.EndWork != null) base.Layout!.SetActiveTimeEntry(timeEntryModel);

            await this.LoadTimeEntries();
            base.StateHasChanged();
        }

        #endregion event handlers

        #region private

        double CalculateTotalHours()
        {
            double result = 0;
            int totalMinutes = 0;

            foreach(var entry in this.TimeEntries)
            {
                if (entry.EndWork != null && entry.StartWork != null)
                {
                    TimeSpan interval = entry.EndWork.Value - entry.StartWork.Value;
                    totalMinutes += (int)interval.TotalMinutes;
                }
            }

            int hours = (int)(totalMinutes / 60);
            int minutes = totalMinutes - (60 * hours);

            double quarterHour = 0;

            if (minutes > 8 && minutes < 22)
            {
                quarterHour = .25;
            }
            else if (minutes > 21 && minutes < 37)
            {
                quarterHour = .5;
            }
            else if (minutes > 36 && minutes < 51)
            {
                quarterHour = .75;
            }
            else if (minutes > 50)
            {
                hours++;
            }

            result = hours + quarterHour;
            return result;
        }

        #endregion private

        #region public
        #endregion public
    }

    class WorkItemFilter 
    {
        public string? WorkItemTitle { get; set; }

    }

}
