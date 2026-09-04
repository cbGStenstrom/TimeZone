using Microsoft.AspNetCore.Components;
using TimeKeeper.Domain.Models;
using TimeKeeper.Domain.Utilities;

namespace TimeKeeper.App.Components.Reports
{
    public partial class WorkItemEntriesItemComponent : CbComponentBase
    {
        #region injected services
        #endregion injected services

        #region parameters

        /// <summary>
        ///  Gets or sets the <see cref="TimeEntry"> model to be displayed or edited by the component.
        /// </summary>
        [Parameter]
        public TimeEntry? Model { get; set; }

        #endregion parameters

        #region data-bound parameters
        #endregion data-bound parameters

        #region properties

        bool IsInEditMode { get; set; } = false;

        #endregion properties

        #region events
        #endregion events

        #region data

        /// <summary>
        ///  Asynchronously saves the current model by updating the associated time entry for the 
        ///  current user.
        /// </summary>
        /// <remarks>
        ///  This method does not perform any action if the model or the current user is null. The
        ///  model's state is snapshotted after a successful update.
        /// </remarks>
        /// <returns>
        ///  A task that represents the asynchronous save operation.
        /// </returns>
        async Task SaveTimeEntry()
        {
            if (this.Model != null && base.SessionService?.User != null)
            {
                _ = await base.TimeEntrySvc!.UpdateTimeEntry(this.Model, base.SessionService.User);
                this.Model?.TakeSnapshot();
            }
        }

        #endregion data

        #region lifecycle
        #endregion lifecycle

        #region event handlers

        void btnCancelEdit_OnClick()
        {
            this.IsInEditMode = false;
            this.Model?.RevertToSnapshot();
            base.StateHasChanged();
        }

        async Task btnSave_OnClick()
        {
            await this.SaveTimeEntry();
            this.IsInEditMode = false;
            base.StateHasChanged();
        }

        #endregion event handlers

        #region private

        /// <summary>
        ///  Formats the start and end work dates of the specified time entry as a human-readable date 
        ///  range string.
        /// </summary>
        /// <param name="entry">
        ///  The time entry containing the start and end work dates to display.
        /// </param>
        /// <returns>
        ///  A string representing the date range in the format "StartDate to EndDate". If either date 
        ///  is not set, "N/A" is used in its place.
        /// </returns>
        string GetDateRangeDisplay(TimeEntry entry)
        {
            string start = entry.StartWork.HasValue ? entry.StartWork.Value.ToString("ddd, MMM dd, yyyy hh:mm tt") : "N/A";
            string end = entry.EndWork.HasValue ? entry.EndWork.Value.ToString("ddd, MMM dd, yyyy hh:mm tt") : "N/A";

            string result = $"{start} - {end}";
            return result;
        }

        /// <summary>
        ///  Returns a formatted string representing the total hours worked for the specified time entry.
        /// </summary>
        /// <param name="entry">
        ///  The time entry containing the start and end work times to calculate hours worked.
        /// </param>
        /// <returns>
        ///  A string displaying the number of hours worked in the format "0.00 hrs". Returns "N/A" if 
        ///  the start or end time is not specified or if the calculated hours are negative.
        /// </returns>
        string GetHoursWorkedDisplay(TimeEntry entry)
        {

            double hoursWorked = !entry.StartWork.HasValue || !entry.EndWork.HasValue ? -1 :
                                  DateTimeUtils.HoursBetween(entry.StartWork.Value, entry.EndWork.Value);

            if (hoursWorked < 0)
                return "N/A";

            var minutes = !entry.StartWork.HasValue || !entry.EndWork.HasValue ? -1 : 
                          DateTimeUtils.MinutesBetween(entry.StartWork.Value, entry.EndWork.Value);
            var hrs  = (int)minutes / 60;
            var mins = (int)minutes % 60;

            string displayHrs  = hrs <= 0 ? string.Empty : $"{hrs} hrs";
            string displayMins = $"{mins} mins";
            string displayTime = !String.IsNullOrEmpty(displayHrs) ? $"({displayHrs}, {displayMins})" : $"({displayMins})";

            string result = $"{hoursWorked.ToString("0.00")} hrs {displayTime}";
            return result;
        }

        #endregion private

        #region public
        #endregion public
    }
}
