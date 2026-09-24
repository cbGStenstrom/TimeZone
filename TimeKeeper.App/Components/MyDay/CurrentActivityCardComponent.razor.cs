using Microsoft.AspNetCore.Components;
using TimeKeeper.Domain.Models;

namespace TimeKeeper.App.Components.MyDay;

/// <summary>
///  Represents a UI card component for the current activity and its active time entry.
/// </summary>
public partial class CurrentActivityCardComponent : ComponentBase
{

    #region parameters

    /// <summary>
    ///  Gets or sets the active time entry.
    /// </summary>
    [Parameter]
    public TimeEntry? ActiveTimeEntry { get; set; }

    #endregion parameters

}