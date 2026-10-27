using Microsoft.AspNetCore.Components;
using Radzen;
using TimeKeeper.App.Models;

namespace TimeKeeper.App.Components.Dialogs;

/// <summary>
///  Presents historical accomplishment content without editing or starting work.
/// </summary>
public partial class YesterdayAccomplishmentsDialog : ComponentBase
{
    #region parameters

    /// <summary>
    ///  Gets or sets sessions for the selected work item and workday only.
    /// </summary>
    [Parameter]
    public IEnumerable<MyDayActivityEntry> Entries { get; set; } = [];

    #endregion parameters

    #region dependencies

    /// <summary>
    ///  Gets or sets the service used to dismiss the dialog.
    /// </summary>
    [Inject]
    private DialogService DialogSvc { get; set; } = default!;

    #endregion dependencies

    #region event handlers

    /// <summary>
    ///  Dismisses the dialog without changing any supplied data.
    /// </summary>
    private void Close_OnClick()
    {
        this.DialogSvc.Close();
    }

    #endregion event handlers

    #region private methods

    private string GetDurationDisplay(MyDayActivityEntry entry)
    {
        string result = string.Empty;

        TimeSpan? duration = (entry.EndWork - entry.StartWork);
        if (duration.HasValue)
        {
            if (duration.Value.TotalHours < 1)
            {
                result = $"{duration.Value.Minutes}m";
            }
            else
            {
                result = $"{duration.Value.Hours}h {duration.Value.Minutes}m";
            }
        }

        return result;
    }

    private string GetTimeSpanDisplay(MyDayActivityEntry entry)
    {
        string result = string.Empty;

        result = $"{entry.StartWork:h:mm tt} - {entry.EndWork:h:mm tt}";


        return result;
    }

    #endregion private methods
}
