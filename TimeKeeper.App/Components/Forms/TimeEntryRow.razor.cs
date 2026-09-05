using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore.Infrastructure;
using TimeKeeper.App.Api.Enums;
using TimeKeeper.App.Api.Services;
using TimeKeeper.App.Components.Dialogs;
using TimeKeeper.Domain.Models;
using TimeKeeper.Domain.Services.Interfaces;

namespace TimeKeeper.App.Components.Forms
{
    public partial class TimeEntryRow : CbComponentBase
    {
        #region injected services

        [Inject]
        public SessionService? SessionSvc { get; set; }

        #endregion injected services

        #region parameters

        [Parameter]
        public TimeEntry? TimeEntryItem { get; set; }

        [Parameter]
        public EventCallback TimeEntryItemChanged { get; set; }

        [Parameter]
        public IEnumerable<WorkItem> WorkItems { get; set; }  = new List<WorkItem>();

        #endregion parameters

        #region properties

        /// <summary>
        ///  Gets or sets the reference to the dialog component used for editing time entry details.
        /// </summary>
        TimeEntryEditorDialog dlgTimeEntryEditor { get; set; }


        /// <summary>
        /// Gets/Sets the row to either be editable or not.
        /// </summary>
        bool IsEditable { get; set; } = false;

        /// <summary>
        /// Gets/Set a flag which will disable/enable the row
        /// </summary>
        bool IsDisabled
        {
            get
            {
                bool isDisabled = (!this.SelectedWorkItem?.IsOpen) ?? true;
                return isDisabled;
            }
        }

        /// <summary>
        /// Gets the List of WorkItems that are available in the DropDownList
        /// </summary>
        List<WorkItem> OpenWorkItems { get; } = new List<WorkItem>();

        /// <summary>
        /// Gets a reference to the Workitem selected in the drop down list.
        /// </summary>
        WorkItem? SelectedWorkItem
        {
            get { return this.WorkItems.FirstOrDefault(e=>e.Id == this.TimeEntryItem?.WorkItemId); }
        }

        #endregion properties

        #region events

        [Parameter]
        public EventCallback<TimeEntry> OnDeleteButtonClick { get; set; }

        [Parameter]
        public EventCallback<TimeEntry> OnSaveButtonClick { get; set; }

        [Parameter]
        public EventCallback<TimeEntry> OnTimeEntrySaved { get; set; }

        #endregion events

        #region data

        /// <summary>
        /// Permanently deletes the <paramref name="model"/> argument from the database.
        /// </summary>
        /// <returns></returns>
        async Task<Boolean> DeleteTimeEntry(TimeEntry model)
        {
            bool result = false;

            if (model == null) return result;

            if (this.TimeEntrySvc != null && this.SessionSvc != null && this.SessionSvc.User != null)
            {
                result = await this.TimeEntrySvc.DeleteTimeEntry(model, this.SessionSvc.User.Username);
            }

            return result;
        }

        /// <summary>
        /// Loads the WorkItems bound to the drop down list with open and available workitems
        /// </summary>
        void LoadOpenWorkItems()
        {
            OpenWorkItems.Clear();
            OpenWorkItems.AddRange(this.WorkItems.Where(e => e.IsOpen == true).ToList());
        }

        /// <summary>
        /// Updates the TimeEntry bound to this row object.
        /// </summary>
        async Task<TimeEntry?> UpdateTimeEntry()
        {
            TimeEntry? result = this.TimeEntryItem;

            if (this.TimeEntrySvc != null && this.SessionSvc != null && this.SessionSvc.User != null)
            {
                result = await this.TimeEntrySvc.UpdateTimeEntry(this.TimeEntryItem, this.SessionSvc.User);
            }

            return result;
        }

        #endregion data

        #region lifecycle

        protected override void OnInitialized()
        {
            this.LoadOpenWorkItems();
            base.OnInitialized();
        }

        #endregion lifecycle

        #region event handlers

        /// <summary>
        /// Handles the event raised when the user clicks on the btnChange_OnClick button.
        /// </summary>
        void btnCancel_OnClick()
        {
            this.IsEditable = false;
        }

        /// <summary>
        /// Handles the event raised when the user clicks on the Delete button.
        /// </summary>
        async void btnDelete_OnClick()
        {
            bool isSuccess = await this.DeleteTimeEntry(this.TimeEntryItem!);
            base.StateHasChanged();

            if (this.OnDeleteButtonClick.HasDelegate)
            {
                await this.OnDeleteButtonClick.InvokeAsync(TimeEntryItem);
            }
        }

        /// <summary>
        /// Handles the event raised when the user clicks on the Edit button.
        /// </summary>
        async Task btnEdit_OnClick()
        {
            var dialogOnSaveCallback = EventCallback.Factory.Create<TimeEntry>(this, Dialog_OnTimeEntrySaved);
            await this.dlgTimeEntryEditor.Open(this.TimeEntryItem, "Edit Time Entry", dialogOnSaveCallback, TimeEntrySaveActions.Update);
        }

        /// <summary>
        /// Handles the event raised when the user clicks on the btnSave_OnClick button.
        /// </summary>
        async void btnSave_OnClick()
        {
            this.TimeEntryItem = await this.UpdateTimeEntry();
            this.IsEditable = false;
            base.StateHasChanged();

            if(this.OnSaveButtonClick.HasDelegate)
            {
                await this.OnSaveButtonClick.InvokeAsync();
            }
        }

        /// <summary>
        ///  Handles the event when a time entry is saved in the dialog and notifies subscribers.
        /// </summary>
        /// <param name="timeEntryModel">
        ///  The time entry data that was saved by the user. Cannot be null.
        /// </param>
        /// <returns>
        ///  A task that represents the asynchronous operation.
        /// </returns>
        async Task Dialog_OnTimeEntrySaved(TimeEntry timeEntryModel)
        {
            this.dlgTimeEntryEditor.Close();

            if (this.OnTimeEntrySaved.HasDelegate)
            {
                await this.OnTimeEntrySaved.InvokeAsync(timeEntryModel);
            }
        }

        #endregion event handlers

        #region private

        /// <summary>
        /// Returns the <paramref name="dateTime"/> argument formatted for the row.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        string GetDateFormattedForDisplay(DateTime? dateTime)
        {
            string result = (dateTime.HasValue) ? dateTime.Value.ToString("h:mm:tt (MMM dd, yyyy)") : "";
            return result;
        }

        /// <summary>
        /// Returns the Title of the WorkItem associated with the <paramref name="workitemId"/> 
        /// argument.
        /// </summary>
        /// <param name="workitemId"></param>
        /// <returns></returns>
        string GetWorkItemTitle(int workitemId)
        {
            var workItem = this.WorkItems.FirstOrDefault(e=>e.Id == workitemId);
            return workItem?.Title ?? string.Empty;
        }

        #endregion private

        #region public
        #endregion public
    }
}
