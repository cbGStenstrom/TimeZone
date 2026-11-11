using Microsoft.AspNetCore.Components;
using TimeKeeper.Domain.Models;

namespace TimeKeeper.App.Components.MyDay;

/// <summary>
///  Represents a UI card component for the current activity and its active time entry.
/// </summary>
public partial class CurrentActivityCardComponent : ComponentBase
{

    #region dependencies
    #endregion dependencies

    #region parameters

    /// <summary>
    ///  Gets or sets the active time entry.
    /// </summary>
    [Parameter]
    public TimeEntry? ActiveTimeEntry { get; set; }

    /// <summary>
    /// Gets or sets the width of the component.
    /// </summary>
    [Parameter]
    public string Width { get; set; } = "100%";

    #endregion parameters

    #region events

    [Parameter]
    public EventCallback<TimeEntry?> OnStopActivityClick { get; set; }

    #endregion events

    #region event handlers

    private async Task btnStop_OnClick(TimeEntry? timeEntry)
    {
        await OnStopActivityClick.InvokeAsync(timeEntry);
    }

    #endregion event handlers

}