using Bunit;
using Radzen;
using Radzen.Blazor;
using TimeKeeper.App.Components.MyDay;
using TimeKeeper.App.Models;
using Xunit;

namespace TimeKeeper.App.Tests.Components.MyDay;

public class ContinueWorkingComponentTests
{
    [Fact]
    public void ActivitiesRenderLabelsAndResumeButtonParameters()
    {
        using (var context = new BunitContext())
        {
            var activities = CreateActivities();
            var component = context.Render<ContinueWorkingComponent>(parameters =>
                parameters.Add(item => item.Activities, activities));

            Assert.Contains("Continue Working", component.Markup);
            Assert.Contains("APP-17 First task", component.Markup);
            Assert.Contains("APP-29 Second task", component.Markup);
            Assert.Contains(activities[0].LastWorked.ToString("MMM dd"),
                component.FindAll("div").Select(item => item.TextContent.Trim()));
            Assert.Single(component.FindComponents<RadzenCard>());
            Assert.NotEmpty(component.FindComponents<RadzenStack>());
            Assert.Equal(2, component.FindAll("button").Count);
            Assert.Collection(component.FindComponents<RadzenButton>(), button => AssertResumeButton(button.Instance), button => AssertResumeButton(button.Instance));
        }
    }

    [Fact]
    public void ClickingSelectedResumeControlDeliversThatActivityExactlyOnce()
    {
        using (var context = new BunitContext())
        {
            var activities = CreateActivities();
            var received = new List<MyDayActivitySummary>();
            var component = context.Render<ContinueWorkingComponent>(parameters => parameters
                .Add(item => item.Activities, activities)
                .Add(item => item.OnResumeWorkClicked, (MyDayActivitySummary summary) => received.Add(summary)));

            component.FindAll("button")[1].Click();

            Assert.Same(activities[1], Assert.Single(received));
        }
    }

    [Fact]
    public void ClickingWithoutCallbackIsSafeAndParameterUpdatesReplaceControls()
    {
        using (var context = new BunitContext())
        {
            var activities = CreateActivities();
            var component = context.Render<ContinueWorkingComponent>(parameters =>
                parameters.Add(item => item.Activities, activities));

            component.FindAll("button")[0].Click();
            Assert.Equal(2, component.FindAll("button").Count);

            component.Render(parameters => parameters.Add(item => item.Activities, [activities[1]]));

            Assert.DoesNotContain("APP-17 First task", component.Markup);
            Assert.Contains("APP-29 Second task", component.Markup);
            Assert.Single(component.FindAll("button"));

            component.Render(parameters => parameters.Add(item => item.Activities, []));

            Assert.DoesNotContain("Second task", component.Markup);
            Assert.Empty(component.FindComponents<RadzenButton>());
            Assert.Empty(component.FindAll("button"));
            Assert.Contains("Continue Working", component.Markup);
        }
    }

    private static void AssertResumeButton(RadzenButton button)
    {
        Assert.Equal("play_arrow", button.Icon);
        Assert.Equal(Variant.Text, button.Variant);
        Assert.Equal(ButtonStyle.Success, button.ButtonStyle);
        Assert.Equal(ButtonSize.Small, button.Size);
        Assert.True(button.Click.HasDelegate);
    }

    private static List<MyDayActivitySummary> CreateActivities() =>
    [
        new() { WorkItem = new() { Project = new() { Key = "APP" }, ActivityNumber = "17", Title = "First task" }, LastWorked = new DateTime(2001, 1, 2) },
        new() { WorkItem = new() { Project = new() { Key = "APP" }, ActivityNumber = "29", Title = "Second task" }, LastWorked = new DateTime(2001, 1, 3) }
    ];
}
