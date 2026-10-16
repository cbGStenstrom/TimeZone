namespace TimeKeeper.Domain.Models.DTOs;

/// <summary>
///  Contains work item identity, status, and minutes worked within a date range.
/// </summary>
public partial class WorkItemDateRangeSummaryDto
{
    #region properties

    /// <summary>
    ///  Gets or sets the activity number as stored, including leading zeroes.
    /// </summary>
    public string? ActivityNumber { get; set; }

    /// <summary>
    ///  Gets the project and activity prefix followed by the original work item title.
    /// </summary>
    /// <remarks>
    ///  Returns the original title when either identifier is missing or whitespace.
    /// </remarks>
    public string DisplayTitle =>
        string.IsNullOrWhiteSpace(this.ProjectKey) || string.IsNullOrWhiteSpace(this.ActivityNumber)
            ? this.WorkItemTitle
            : $"{this.ProjectKey}-{this.ActivityNumber} {this.WorkItemTitle}";

    /// <summary>
    ///  Gets or sets whether the work item is billable.
    /// </summary>
    public bool IsBillable { get; set; } = true;

    /// <summary>
    ///  Gets or sets whether the work item is in review.
    /// </summary>
    public bool IsInReview { get; set; } = false;

    /// <summary>
    ///  Gets or sets whether the work item is open.
    /// </summary>
    public bool IsOpen { get; set; } = true;

    /// <summary>
    ///  Gets or sets the associated project's key.
    /// </summary>
    public string? ProjectKey { get; set; }

    /// <summary>
    ///  Gets or sets the total minutes worked in the selected range.
    /// </summary>
    public int TotalMinutesWorked { get; set; } = 0;

    /// <summary>
    ///  Gets or sets the work item identifier.
    /// </summary>
    public int WorkItemID { get; set; } = 0;

    /// <summary>
    ///  Gets or sets the original title used for sorting and existing consumers.
    /// </summary>
    public string WorkItemTitle { get; set; } = string.Empty;

    #endregion properties
}
