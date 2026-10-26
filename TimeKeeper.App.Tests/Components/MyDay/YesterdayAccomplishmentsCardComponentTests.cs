using Bunit;
using Radzen.Blazor;
using TimeKeeper.App.Components.MyDay;
using TimeKeeper.Domain.Models;
using Xunit;

namespace TimeKeeper.App.Tests.Components.MyDay;

public class YesterdayAccomplishmentsCardComponentTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void EmptyEntriesShowHeadingAndEmptyMessage(bool explicitEmpty)
    {
        using (var context = new BunitContext())
        {
            var component = context.Render<YesterdayAccomplishmentsCardComponent>(parameters =>
            {
                if (explicitEmpty) parameters.Add(item => item.TimeEntries, []);
            });

            Assert.Equal("Yesterday's Accomplishments", component.Find("h4").TextContent.Trim());
            Assert.Contains("No work recorded yesterday.", component.Markup);
            Assert.Single(component.FindComponents<RadzenCard>());
        }
    }

    [Fact]
    public void PopulatedEntriesRenderDistinctValuesInNestedCards()
    {
        using (var context = new BunitContext())
        {
            TimeEntry[] entries =
            [
                new() { WorkItem = new() { Project = new() { Key = "APP" }, ActivityNumber = "17", Title = "First task" }, Accomplishment = "Completed analysis" },
                new() { WorkItem = new() { Project = new() { Key = "OPS" }, ActivityNumber = "29", Title = "Second task" }, Accomplishment = "Fixed report" }
            ];
            var component = context.Render<YesterdayAccomplishmentsCardComponent>(parameters =>
                parameters.Add(item => item.TimeEntries, entries));

            var outerCard = component.FindComponent<RadzenCard>();
            var cards = outerCard.FindComponents<RadzenCard>();
            Assert.Equal(2, cards.Count);
            Assert.Equal("APP-17", cards[0].Find("strong").TextContent);
            Assert.Contains("First task", cards[0].Markup);
            Assert.Contains("Completed analysis", cards[0].Markup);
            Assert.Equal("OPS-29", cards[1].Find("strong").TextContent);
            Assert.Contains("Second task", cards[1].Markup);
            Assert.Contains("Fixed report", cards[1].Markup);
            Assert.DoesNotContain("No work recorded yesterday.", component.Markup);
        }
    }

    [Fact]
    public void ParameterUpdatesRenderAccomplishmentWithoutWorkItemThenReturnToEmpty()
    {
        using (var context = new BunitContext())
        {
            var component = context.Render<YesterdayAccomplishmentsCardComponent>();
            component.Render(parameters => parameters.Add(item => item.TimeEntries,
                [new TimeEntry { WorkItem = null, Accomplishment = "Unlinked accomplishment" }]));

            Assert.Contains("Unlinked accomplishment", component.Markup);
            Assert.Equal(string.Empty, component.Find("strong").TextContent);
            Assert.Equal(2, component.FindComponents<RadzenCard>().Count);
            Assert.DoesNotContain("No work recorded yesterday.", component.Markup);

            component.Render(parameters => parameters.Add(item => item.TimeEntries, []));

            Assert.Contains("No work recorded yesterday.", component.Markup);
            Assert.DoesNotContain("Unlinked accomplishment", component.Markup);
            Assert.Single(component.FindComponents<RadzenCard>());
        }
    }
}
