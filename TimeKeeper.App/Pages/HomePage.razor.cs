using System.Linq.Expressions;
using TimeKeeper.App.Components.Forms;
using TimeKeeper.App.Components.Pages;
using TimeKeeper.Domain.Models;

namespace TimeKeeper.App.Pages
{
    public partial class HomePage : CbPageBase
    {
        #region injected services
        #endregion injected services

        #region parameters
        #endregion parameters

        #region properties 

        Dictionary<string, string> Actions { get; set; } = [];

        string SelectedActionKey { get; set; } = "PAUSE";

        Project? SelectedProject { get; set; }

        TimeEntry? SelectedTimeEntry { get; set; }

        List<Expression<Func<DataAccess.Entities.WorkItem, bool>>>? WorkItemFilter { get; set; }

        WorkItemLookupFilter? WorkItemLookupFilterComponent { get; set; }

        List<WorkItem>? WorkItemList { get; set; }

        Laborer? User { get; set; }

        #endregion properties

        #region events
        #endregion events

        #region data

        /// <summary>
        ///  Adds a new time entry associated with the selected Workitem.
        /// </summary>
        /// <returns></returns>
        async Task<TimeEntry> AddNewEntry()
        {
            TimeEntry result = new TimeEntry();

            if (this.TimeEntrySvc != null && SelectedTimeEntry != null && this.SessionSvc?.User != null)
            {
                result = await base.TimeEntrySvc.StartWorkOnWorkItem(this.SelectedTimeEntry, base.SessionSvc.User);
            }

            return result;
        }

        /// <summary>
        /// Completes a time entry, and closes the work item..  
        /// </summary>
        /// <returns></returns>
        async Task CloseTimeEntry()
        {
            if(this.TimeEntrySvc != null && SelectedTimeEntry != null && this.SessionSvc?.User != null)
            {
                await this.TimeEntrySvc.EndWorkOnWorkItem(SelectedTimeEntry, this.SessionSvc.User.Username, true);
            }
        }

        private async Task<Project?> GetProjectByID(int projectID)
        {
            Project? result = (this.ProjectSvc != null) ? await this.ProjectSvc.GetProjectByID(projectID) : null;
            return result;
        }

        /// <summary>
        ///  Initializes the set of available actions for the current instance.
        /// </summary>
        /// <remarks>
        ///  This method populates the Actions collection with predefined action keys and their
        ///  corresponding descriptions. It should be called before attempting to access or use the 
        ///  Actions collection to ensure all standard actions are available.
        /// </remarks>
        void InitializeActions()
        {
            this.Actions.Add("CLOSE", "Close Workitem");
            this.Actions.Add("PAUSE", "Continue Later");
            this.Actions.Add("REVIEW", "Submitted For Review");
        }

        /// <summary>
        ///  Loads the current active time entry for the signed-in user, if one exists.
        /// </summary>
        /// <remarks>
        ///  If the user does not have an active time entry (an entry without an end time), the 
        ///  method resets the form to allow creation of a new entry. This method should be called 
        ///  only when a user is signed in.
        ///  </remarks>
        /// <returns>
        ///  A task that represents the asynchronous operation. The task completes when the current 
        ///  time entry is loaded or the form is reset if no active entry exists.
        /// </returns>
        async Task LoadCurrentTimeEntryForUsername()
        {
            if(base.SessionSvc?.User != null)
            {
                List<Expression<Func<DataAccess.Entities.TimeEntry, bool>>> filter = new() { entry => entry.CreatedBy == this.SessionSvc.User.Username &&
                                                                                        entry.EndWork == null };
                IEnumerable<TimeEntry>? data = await base.TimeEntrySvc!.GetFilteredTimeEntries(filter);

                TimeEntry? selectedEntry = data.FirstOrDefault();

                // If the user does not have any entries that have not been ended then present them
                // with an option to create a new entry.
                //
                if(selectedEntry == null)
                    await this.ResetForm();
                else
                    SelectedTimeEntry = selectedEntry;
            }
        }

        /// <summary>
        /// Loads the variable which is bound to the WorkItems drop down list.
        /// </summary>
        /// <returns></returns>
        async Task LoadWorkItems()
        {
            //List<Expression<Func<DataAccess.Entities.WorkItem, bool>>> filter = new() { item => item.IsOpen == true };
            WorkItemList = (await base.WorkItemSvc!.GetFilteredWorkItems(this.WorkItemFilter)).OrderBy(e=>e.Title).ToList();
        }

        /// <summary>
        /// Loads the user reference
        /// </summary>
        /// <returns></returns>
        private Task LoadUser()
        {
            if(this.SessionSvc != null && this.SessionSvc.User != null)
            {
                this.User = this.SessionSvc.User;
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Resets the form to prepare for a new WorkItem selection.
        /// </summary>
        /// <returns></returns>
        Task ResetForm()
        {
            this.SelectedTimeEntry = new TimeEntry();

            // Set the accomplishment to an empty string in order to rebind the HTML Editor.
            // Apparently the Editor will not bind to a NULL value(?).
            //
            this.SelectedTimeEntry.Accomplishment = "";

            return Task.CompletedTask;
        }

        /// <summary>
        /// Set the EndWork timestamp for the SelectedTimeEntry
        /// </summary>
        /// <returns></returns>
        async Task SetEndForTimeEntry()
        {
            if (this.TimeEntrySvc != null && SelectedTimeEntry != null && this.SessionSvc?.User != null)
            {
                await this.TimeEntrySvc.EndWorkOnWorkItem(SelectedTimeEntry, this.SessionSvc.User.Username, false);
            }
        }

        /// <summary>
        ///  Marks the currently selected work item for review by setting its review status and 
        ///  updating it in the data store asynchronously.
        /// </summary>
        /// <remarks>
        ///  If no work item is selected or the work item service is unavailable, the method completes 
        ///  without making changes. The review status is indicated by setting the work item's 
        ///  IsInReview property to <see langword="true"/> before updating.
        /// </remarks>
        /// <returns>
        ///  A task that represents the asynchronous operation. The task completes when the work 
        ///  item has been updated for review.
        /// </returns>
        /// <exception cref="Exception">
        ///  Thrown if the work item update operation fails.
        /// </exception>
        async Task SetWorkItemForReview()
        {
            WorkItem? workItem = this.SelectedTimeEntry?.WorkItem;

            if(workItem is not null && base.WorkItemSvc is not null)
            {
                // Set the EndWork timestamp of the TimeEntry item.
                //
                await this.SetEndForTimeEntry();

                // Set the IsInReview flag to TRUE to indicate that this workitem in now in Review
                //
                workItem.IsInReview = true;

                if(!await base.WorkItemSvc.UpdateWorkItem(workItem))
                {
                    throw new Exception("Workitem Update failed");
                }
            }
        }

        #endregion data

        #region lifecycle

        protected override async Task OnInitializedAsync()
        {
            this.WorkItemFilter = this.WorkItemLookupFilterComponent?.GetFilter();
            this.InitializeActions();
            await this.LoadWorkItems();
            await this.LoadUser();
            await this.LoadCurrentTimeEntryForUsername();

            await base.OnInitializedAsync();
        }

        #endregion lifecycle

        #region event handlers

        /// <summary>
        /// Handles the event raised when the user clicks on the "Close Work Item" button.
        /// </summary>
        async void btnCloseItem_OnClick()
        {
            await this.CloseTimeEntry();
            this.WorkItemFilter = this.WorkItemLookupFilterComponent!.GetFilter();
            await this.LoadWorkItems();
            await this.ResetForm();
            base.StateHasChanged();
        }

        /// <summary>
        /// Handles the event raised when the user clicks on the "Continue Later" button.
        /// </summary>
        /// <returns></returns>
        async Task btnContinue_OnClick()
        {
            await SetEndForTimeEntry();

            // Since we closed the existing time entry we populate with whatever the next time entry
            // is for the user. Here, we should expect an empty/new TimeEntry
            //
            await this.LoadCurrentTimeEntryForUsername();
        }

        /// <summary>
        /// Handles the event raised when the user clicks on the "Start Time" button.
        /// </summary>
        /// <returns></returns>
        async Task btnStartTime_OnClick()
        {
            await AddNewEntry();

            // We have set the start time so now we need to reload it so that the user can close it.
            //
            await this.LoadCurrentTimeEntryForUsername();
            await base.Layout?.Refresh();
        }

        async Task btnSubmit_OnClick()
        {
            switch(this.SelectedActionKey)
            {
                case "PAUSE":
                    await SetEndForTimeEntry();
                    await this.LoadCurrentTimeEntryForUsername();
                    break;
                case "CLOSE":
                    await this.CloseTimeEntry();
                    this.WorkItemFilter = this.WorkItemLookupFilterComponent!.GetFilter();
                    await this.LoadWorkItems();
                    await this.ResetForm();
                    break;
                case "REVIEW":
                    await SetWorkItemForReview();
                    await this.LoadCurrentTimeEntryForUsername();
                    break;
            }

            await base.Layout?.Refresh();
        }

        /// <summary>
        /// Handles the event raised when the user changes one of the filter parameters.
        /// </summary>
        /// <param name="filter"></param>
        async void LookupFilter_OnChange(List<Expression<Func<DataAccess.Entities.WorkItem, bool>>> filter)
        {
            this.WorkItemFilter = filter;
            await this.LoadWorkItems();
            this.StateHasChanged();
        }


        async Task TimeEntryComponent_OnTimeEntryChanged()
        {
            await base.Layout!.Refresh();
            base.StateHasChanged();
        }

        #endregion event handlers

        #region private

        /// <summary>
        /// Gets the StartWork timestamp formatted for display.
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        string? GetFormattedDate(DateTime? timestamp)
        {
            string? result = (timestamp.HasValue) ? timestamp.Value.ToString("MMM dd, yyyy hh:mm:tt") : null;
            return result;
        }

        #endregion private

        #region public
        #endregion public
    }
}
