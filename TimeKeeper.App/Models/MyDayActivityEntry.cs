namespace TimeKeeper.App.Models;

/// <summary>
///  Represents a daily activity record with work start and end times, total hours worked, and an 
///  accomplishment note.
/// </summary>
/// <remarks>
///  Supports partial entries by allowing work times and hours worked to be unspecified.</remarks>
public class MyDayActivityEntry
{
    /// <summary>
    ///  Gets or sets the accomplishment.
    /// </summary>
    public string Accomplishment { get; set; } = string.Empty;

    /// <summary>
    ///  Gets or sets the date and time when work ended.
    /// </summary>
    public DateTime? EndWork { get; set; }

    /// <summary>
    ///  Gets or sets the total hours worked.
    /// </summary>
    public double? HoursWorked { get; set; }

    /// <summary>
    ///  Gets or sets the date and time when work started.
    /// </summary>
    public DateTime? StartWork { get; set; }

    /// <summary>
    ///  Gets or sets the time entry ID.
    /// </summary>
    public int TimeEntryId { get; set; }
}