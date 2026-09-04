using Microsoft.AspNetCore.Components;
using Radzen.Blazor;
using System.Threading.Tasks;
using TimeKeeper.Domain.Models;

namespace TimeKeeper.App.Components
{
    public partial class TimeEntryEndWorkComponent : CbComponentBase
    {
        #region injected services
        #endregion injected services

        #region parameters

        /// <summary>
        ///  Gets or sets the currently active time entry.
        /// </summary>
        [Parameter]
        public TimeEntry? ActiveTimeEntry { get; set; }

        [Parameter]
        public EventCallback ActiveTimeEntryChanged { get; set; }

        /// <summary>
        ///  Gets or sets the identifier of the action to perform when work ends.
        /// </summary>
        [Parameter]
        public int? EndWorkAction { get; set; }

        #endregion parameters

        #region properties
        #endregion properties

        #region events

        /// <summary>
        ///  Gets or sets the callback that is invoked when a time entry ends.
        /// </summary>
        /// <remarks>
        ///  The callback receives the identifier of the time entry that has ended as its argument. 
        ///  Use this event to perform actions such as updating the UI or persisting data when a 
        ///  time entry is completed.
        /// </remarks>
        [Parameter]
        public EventCallback OnTimeEntryEnded { get; set; }

        #endregion events

        #region data

        /// <summary>
        ///  Ends the currently active time entry, marking it as completed. Optionally flags the 
        ///  entry for review based on the specified parameter.
        /// </summary>
        /// <param name="toBeReviewed">
        ///  Indicates whether the ended time entry should be flagged for review. Set to <see langword="true"/> 
        ///  to mark the entry as reviewable; otherwise, <see langword="false"/>.
        /// </param>
        /// <returns>
        ///  A task that represents the asynchronous operation of ending the active time entry.
        /// </returns>
        async Task EndActiveTimeEntry(bool toBeReviewed = false)
        {
            if(this.ActiveTimeEntry != null)
            {
                await base.TimeEntrySvc!.EndWorkOnWorkItem(Model: this.ActiveTimeEntry, Username: "gstenstrom", SubmitForReview: toBeReviewed);
                this.ActiveTimeEntry = null;
            }
        }

        #endregion data

        #region lifecycle

        protected override async Task OnInitializedAsync()
        {
            // Initialize the end datetime value
            //
            this.InitializeTimeEndEndWork();
        }

        #endregion lifecycle

        #region event handlers

        /// <summary>
        ///  Handles the Cancel button click event by closing the dialog.
        /// </summary>
        /// <remarks>
        ///  This method is typically invoked when the user clicks the Cancel button in a dialog.
        ///  After execution, the dialog is closed and no further action is taken.
        /// </remarks>
        void btnCancel_OnClick()
        {
            base.DialogSvc!.Close();
        }

        /// <summary>
        ///  Handles the click event for the End Work button, finalizing the active time entry and 
        ///  triggering related actions.
        /// </summary>
        /// <remarks>
        ///  This method finalizes the current time entry, invokes any registered callbacks for time 
        ///  entry changes and completion, and closes the dialog. It should be called in response 
        ///  to the user's request to end their work session.
        /// </remarks>
        /// <param name="args">
        ///  The split button item that was clicked, representing the user's selected action for 
        ///  ending work.
        /// </param>
        /// <returns>
        ///  A task that represents the asynchronous operation of ending the active time entry and 
        ///  updating the UI.
        /// </returns>
        async Task btnEndWork_OnClick(RadzenSplitButtonItem args)
        {
            bool toBeReviewed = args != null && args.Value == "1";

            await this.EndActiveTimeEntry(toBeReviewed);
            await this.ActiveTimeEntryChanged.InvokeAsync();

            if (this.OnTimeEntryEnded.HasDelegate)
            {
                await this.OnTimeEntryEnded.InvokeAsync();
            }

            base.DialogSvc!.Close();
            base.StateHasChanged();
        }

        #endregion event handlers

        #region private

        /// <summary>
        ///  Sets the end time of the active time entry to the current date and time.
        /// </summary>
        void InitializeTimeEndEndWork()
        {
            if (this.ActiveTimeEntry != null)
            {
                this.ActiveTimeEntry.EndWork = DateTime.Now;
            }
        }

        #endregion private

        #region public
        #endregion public
    }
}