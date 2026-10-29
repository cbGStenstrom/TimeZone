using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Radzen.Blazor;
using Radzen.Documents.Spreadsheet;
using TimeKeeper.App.Models;
using TimeKeeper.Domain.Models;


namespace TimeKeeper.App.Components.MyDay;

public partial class ContinueWorkingComponent : ComponentBase
{
    #region injected services

    [Inject]
    private ContextMenuService ContextMenuSvc { get; set; } = default!;

    #endregion injected services

    #region parameters

    /// <summary>
    ///  Gets or sets the summary summaries for the day.
    /// </summary>
    /// <remarks>
    ///  Set by a parent component.</remarks>
    [Parameter]
    public List<MyDayActivitySummary> Activities { get; set; } = [];

    /// <summary>
    ///  Gets or sets the callback invoked when resume work is clicked for an summary summary.
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
    ///  Invokes the resume-work callback for the specified summary summary when a handler is 
    ///  registered.
    /// </summary>
    /// <param name="summary">
    ///  The summary summary associated with the resume-work action.</param>
    /// <returns>
    ///  A task that represents the asynchronous operation.</returns>
    private async Task ResumeWork_OnClick(MyDayActivitySummary summary)
    {
        if (OnResumeWorkClicked.HasDelegate)
        {
            await OnResumeWorkClicked.InvokeAsync(summary);
        }
    }

    #endregion event handlers

    #region private

    private string CalculateDurationDisplay(MyDayActivitySummary activity)
    {
        TimeSpan duration = TimeSpan.FromHours(activity.TotalHoursWorked);
        return $"{(int)duration.TotalHours}h {duration.Minutes}m";
    }

    /// <summary>
    ///  Calculates a user-friendly string representing the time elapsed since the summary was last worked.
    /// </summary>
    /// <param name="activity">
    ///  The summary summary containing the last worked timestamp.</param>
    /// <returns>
    ///  "Yesterday" when the last worked date is the previous day; otherwise a string in the format 
    ///  "{hours}h {minutes}m ago" when the last worked time is earlier than the current time; 
    ///  otherwise an empty string.</returns>
    private string CalculateHowLongAgoDisplay(MyDayActivitySummary activity)
    {
        string result = string.Empty;

        // WHAT: Calculate the amount f time since the last worked entry for the summary summary.
        DateTime timestamp = DateTime.Now;
        DateTime lastWorked = activity.LastWorked;

        // WHAT: If the LastWorked value represents a yesterday, then simply return "Yesterday" as the display string.
        if (lastWorked.Date == timestamp.AddDays(-1).Date)
        {
            return "Yesterday";
        }

        // WHAT: If the current timestamp is greater than the last worked timestamp, calculate the
        //  difference and display it in a user-friendly format.
        if (timestamp > lastWorked)
        {
            TimeSpan diff = (timestamp - lastWorked);

            // WHAT: If the difference is greater than zero hours, then display the number of hours
            //  and minutes; otherwise, display only the number of minutes.
            string hoursAgo = (int)diff.TotalHours > 0 ? $"{(int)diff.TotalHours}h" : "";
            return $"{hoursAgo} {diff.Minutes}m ago";    
        }

        return result;
    }

    private bool IsCodeActivity(MyDayActivitySummary summary)
    {
        string[] codeTypes = ["bugfix", "feature", "chore", "techdebt"];
        return codeTypes.Contains(summary.WorkItem?.Type?.Name.ToLower());
    }

    private bool ShowReactivateMenuItem(MyDayActivitySummary summary)
    {
        bool result = false;
        bool isCodeActivity = this.IsCodeActivity(summary);

        result = (isCodeActivity && summary.WorkItem.IsInReview);

        return result;
    }

    private bool ShowSubmitPrMenuItem(MyDayActivitySummary summary)
    {
        bool result = false;
        bool isCodeActivity = this.IsCodeActivity(summary);

        result = (isCodeActivity && !summary.WorkItem.IsInReview);

        return result;
    }

    #endregion private

    #region public
    #endregion public
}