using Microsoft.AspNetCore.Components;
using TimeKeeper.App.Models;

namespace TimeKeeper.App.Components.MyDay;

public partial class ActivitiesInReviewComponent : ComponentBase
{
    [Parameter]
    public List<MyDayActivitySummary> Activities { get; set; } = [];

    [Parameter]
    public EventCallback<MyDayActivitySummary> OnResumeWorkClicked { get; set; }

    [Parameter]
    public EventCallback<MyDayActivitySummary> OnViewHistoryClicked { get; set; }
}
