using Microsoft.AspNetCore.Components;
using Microsoft.IdentityModel.Tokens;
using Radzen;
using TimeKeeper.App.Components.Dialogs;
using TimeKeeper.App.Models;

namespace TimeKeeper.App.Components.MyDay;

/// <summary>
///  Displays previous-workday summaries with independent review and resume actions.
/// </summary>
public partial class YesterdayAccomplishmentsCardComponent : ComponentBase
{

    #region dependencies

    /// <summary>
    ///  Gets or sets the shared Radzen dialog service.
    /// </summary>
    [Inject]
    private DialogService DialogSvc { get; set; } = default!;

    #endregion dependencies

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

    [Parameter]
    public string Width { get; set; } = "100%";

    /// <summary>
    ///  Gets or sets the selected local workday.
    /// </summary>
    [Parameter]
    public DateTime Workday { get; set; }

    #endregion parameters

    #region fields

    private string CardCssStyle = string.Empty;

    #endregion fields

    #region lifecycle

    protected override void OnInitialized()
    {
        base.OnInitialized();
        this.InitializeCardCssStyle();
    }

    #endregion lifecycle

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

    private double CalculateTotalTime()
    {
        double time = this.Activities?.Where(summary=> !summary.ExcludeFromRollup).Sum(summary => summary.TotalHoursWorked) ?? 0;
        return time;
    }

    private string GetDisplayName(MyDayActivitySummary activity)
    {
        return $"{activity.ActivityIdentifier} {activity.ActivityTitle}";
    }

    private void InitializeCardCssStyle()
    {
        this.CardCssStyle = $"width:{this.Width};";
    }

    #endregion private
}
