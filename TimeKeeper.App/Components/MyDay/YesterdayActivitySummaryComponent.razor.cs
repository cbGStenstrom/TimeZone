using Microsoft.AspNetCore.Components;
using TimeKeeper.App.Models;

namespace TimeKeeper.App.Components.MyDay
{
    public partial class YesterdayActivitySummaryComponent : ComponentBase
    {
        #region injected services
        #endregion injected services

        #region parameters

        /// <summary>
        ///  Gets or sets the callback invoked when resume work is clicked.
        /// </summary>
        [Parameter]
        public EventCallback<MyDayActivitySummary> OnResumeWorkClicked { get; set; }

        /// <summary>
        ///  Gets or sets a callback that is invoked when a day activity history view is requested.
        /// </summary>
        /// <remarks>
        ///  Supply a delegate to handle the action in the parent component.</remarks>
        [Parameter]
        public EventCallback<MyDayActivitySummary> OnViewHistoryClicked { get; set; }

        /// <summary>
        ///  Gets or sets the activity summary for the day.
        /// </summary>
        [Parameter]
        public MyDayActivitySummary? Summary { get; set; }

        #endregion parameters

        #region properties
        #endregion properties

        #region events
        #endregion events

        #region data
        #endregion data

        #region lifecycle
        #endregion lifecycle

        #region event handlers

        /// <summary>
        ///  Invokes the resume-work callback with the current summary when a summary is available 
        ///  and a handler is assigned.
        /// </summary>
        /// <returns>
        ///  A task that represents the asynchronous operation.</returns>
        private async Task ResumeWork_Click()
        {
            if (Summary != null && OnResumeWorkClicked.HasDelegate)
            {
                await OnResumeWorkClicked.InvokeAsync(Summary);
            }
        }

        /// <summary>
        ///  Invokes the history view callback when a summary is available and a handler is assigned.
        /// </summary>
        /// <remarks>
        ///  Calls `OnViewHistoryClicked.InvokeAsync(Summary)` only when `Summary` is not `null` and 
        ///  `OnViewHistoryClicked` has a delegate.</remarks>
        /// <returns>
        ///  A task that represents the asynchronous operation.</returns>
        private async Task ViewHistory_Click()
        {
            if (Summary != null && OnViewHistoryClicked.HasDelegate)
            {
                await OnViewHistoryClicked.InvokeAsync(Summary);
            }
        }

        #endregion event handlers

        #region private
        #endregion private

        #region public
        #endregion public
    }
}