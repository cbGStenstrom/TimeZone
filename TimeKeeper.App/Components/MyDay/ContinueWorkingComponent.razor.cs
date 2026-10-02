using Microsoft.AspNetCore.Components;
using TimeKeeper.App.Models;


namespace TimeKeeper.App.Components.MyDay;

public partial class ContinueWorkingComponent : ComponentBase
{
    #region injected services
    #endregion injected services

    #region parameters

    /// <summary>
    ///  Gets or sets the activity summaries for the day.
    /// </summary>
    /// <remarks>
    ///  Set by a parent component.</remarks>
    [Parameter]
    public List<MyDayActivitySummary> Activities { get; set; } = [];

    /// <summary>
    ///  Gets or sets the callback invoked when resume work is clicked for an activity summary.
    /// </summary>
    [Parameter]
    public EventCallback<MyDayActivitySummary> OnResumeWorkClicked { get; set; }

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
    ///  Invokes the resume-work callback for the specified activity summary when a handler is 
    ///  registered.
    /// </summary>
    /// <param name="summary">
    ///  The activity summary associated with the resume-work action.</param>
    /// <returns>
    ///  A task that represents the asynchronous operation.</returns>
    private async Task ResumeWork_Click(MyDayActivitySummary summary)
    {
        if (OnResumeWorkClicked.HasDelegate)
        {
            await OnResumeWorkClicked.InvokeAsync(summary);
        }
    }

    #endregion event handlers

    #region private
    #endregion private

    #region public
    #endregion public
}