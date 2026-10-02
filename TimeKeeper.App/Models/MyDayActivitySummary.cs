using TimeKeeper.Domain.Models;

namespace TimeKeeper.App.Models;

/// <summary>
///  Represents aggregated work activity for a single work item over a selected period, including 
///  total time, activity range, and contributing entries.
/// </summary>
/// <remarks>
///  Includes convenience accessors for commonly used work item and project details derived from 
///  the summarized work item.</remarks>
public class MyDayActivitySummary
{
    /// <summary>
    ///  The WorkItem being summarized.
    /// </summary>
    public WorkItem WorkItem { get; set; } = null!;

    /// <summary>
    ///  Total hours worked on the WorkItem during the selected period.
    /// </summary>
    public double TotalHoursWorked { get; set; }

    /// <summary>
    ///  First time worked during the period.
    /// </summary>
    public DateTime FirstWorked { get; set; }

    /// <summary>
    ///  Most recent activity during the period.
    /// </summary>
    public DateTime LastWorked { get; set; }

    /// <summary>
    ///  Individual TimeEntries contributing to the summary.
    /// </summary>
    public List<MyDayActivityEntry> Entries { get; set; } = [];

    #region Convenience Properties

    /// <summary>
    ///  Gets the display identifier of the associated work item activity.
    /// </summary>
    public string ActivityIdentifier => WorkItem.DisplayIdentifier;
    
    /// <summary>
    ///  Gets the title of the associated work item activity.
    /// </summary>
    public string ActivityTitle => WorkItem.Title ?? string.Empty;
    
    /// <summary>
    ///  Gets the number of individual time entries contributing to the summary.
    /// </summary>
    public int EntryCount => Entries.Count;
    
    /// <summary>
    ///  Gets a value indicating whether the associated work item is billable.
    /// </summary>
    public bool IsBillable => WorkItem.IsBillable;
    
    /// <summary>
    ///  Gets a value indicating whether the associated work item is in review.
    /// </summary>
    public bool IsInReview => WorkItem.IsInReview;

    /// <summary>
    ///  Gets a user-friendly display string for the date in <c>LastWorked</c> based on how recent it is.
    /// </summary>
    public string LastWorkedDisplay
    {
        get
        {
            DateTime today = DateTime.Today;

            if (LastWorked.Date == today)
            {
                return "Today";
            }

            if (LastWorked.Date == today.AddDays(-1))
            {
                return "Yesterday";
            }

            if (LastWorked.Date > today.AddDays(-7))
            {
                return LastWorked.ToString("dddd");
            }

            return LastWorked.ToString("MMM dd");
        }
    }

    /// <summary>
    ///  Gets the key of the associated project.
    /// </summary>
    public string ProjectKey => WorkItem.Project?.Key ?? string.Empty;
    
    /// <summary>
    ///  Gets the name of the associated project.
    /// </summary>
    public string ProjectName => WorkItem.Project?.ShortName ?? string.Empty;
    
    /// <summary>
    ///  Gets the identifier of the associated work item.
    /// </summary>
    public int WorkItemId => WorkItem.Id;

    #endregion

    #region public methods


    #endregion public methods
}
