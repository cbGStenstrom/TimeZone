using Microsoft.AspNetCore.Components;
using Radzen;
using TimeKeeper.App.Components.Dialogs;
using TimeKeeper.App.Models;

namespace TimeKeeper.App.Components.MyDay;

/// <summary>
///  Displays previous-workday summaries with independent review and resume actions.
/// </summary>
public partial class YesterdayAccomplishmentsCardComponent : ComponentBase
{
    #region parameters

    /// <summary>
    ///  Gets or sets prepared, ordered activity summaries.
    /// </summary>
    [Parameter]
    public IEnumerable<MyDayActivitySummary> Activities { get; set; } = [];

    /// <summary>
    ///  Gets or sets the existing resume callback.
    /// </summary>
    [Parameter]
    public EventCallback<MyDayActivitySummary> OnResumeWorkClicked { get; set; }

    /// <summary>
    ///  Gets or sets the selected local workday.
    /// </summary>
    [Parameter]
    public DateTime Workday { get; set; }

    #endregion parameters

    #region dependencies

    /// <summary>
    ///  Gets or sets the shared Radzen dialog service.
    /// </summary>
    [Inject]
    private DialogService DialogSvc { get; set; } = default!;

    #endregion dependencies

    #region event handlers

    /// <summary>
    ///  Opens read-only accomplishments for the selected summary and day.
    /// </summary>
    /// <param name="summary">Selected work-item summary.</param>
    /// <returns>The dialog completion task.</returns>
    private async Task Activity_OnSelected(MyDayActivitySummary summary)
    {
        string title    = $"{summary.ActivityIdentifier} — {this.Workday:dddd, MMM dd, yyyy}";
        var parameters  = new Dictionary<string, object?>();
            parameters.Add(nameof(YesterdayAccomplishmentsDialog.Entries), summary.Entries);
        var options = new DialogOptions();
            options.Draggable = true;

        await this.DialogSvc.OpenAsync<YesterdayAccomplishmentsDialog>(title, parameters, options);
    }

    #endregion event handlers

    #region private

    private string GetDisplayName(MyDayActivitySummary activity)
    {
        return $"{activity.ActivityIdentifier} {activity.ActivityTitle}";
    }

    #endregion private
}
