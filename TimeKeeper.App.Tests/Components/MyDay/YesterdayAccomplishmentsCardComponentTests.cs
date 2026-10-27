using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Radzen;
using Radzen.Blazor;
using TimeKeeper.App.Components.Dialogs;
using TimeKeeper.App.Components.MyDay;
using TimeKeeper.App.Models;
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
        using (var context = CreateContext())
        {
            var component = context.Render<YesterdayAccomplishmentsCardComponent>(parameters =>
            {
                if (explicitEmpty) parameters.Add(item => item.Activities, []);
            });
            Assert.Equal("Yesterday's Accomplishments", component.Find("h4").TextContent.Trim());
            Assert.Contains("No accomplishments recorded for the previous workday.", component.Markup);
            Assert.Single(component.FindComponents<RadzenCard>());
        }
    }

    [Fact]
    public void PopulatedEntriesRenderRequiredMetadataAndAggregatedRowsThenReturnToEmpty()
    {
        using (var context = CreateContext())
        {
            var entries = Entries();
            var summaries = YesterdayAccomplishmentsSummaryBuilder.Build(entries);
            var component = context.Render<YesterdayAccomplishmentsCardComponent>(parameters =>
                parameters.Add(item => item.Activities, summaries));
            Assert.Equal(4, component.FindComponents<RadzenButton>().Count);
            Assert.Contains("APP-17", component.Markup);
            Assert.Contains("OPS-29", component.Markup);
            Assert.Contains("Application", component.Markup);
            Assert.Contains("Operations", component.Markup);
            Assert.Contains("Feature", component.Markup);
            Assert.Contains("2 hours", component.Markup);
            Assert.All(component.FindComponents<RadzenIcon>(), icon => Assert.Equal("build", icon.Instance.Icon));
            Assert.DoesNotContain("Completed analysis", component.Markup);
            component.Render(parameters => parameters.Add(item => item.Activities, []));
            Assert.Contains("No accomplishments recorded for the previous workday.", component.Markup);
            Assert.Empty(component.FindComponents<RadzenButton>());
        }
    }

    [Fact]
    public void ResumeClickDeliversSelectedSummaryWithoutOpeningDialogAndIsSafeWithoutCallback()
    {
        using (var context = CreateContext())
        {
            var summaries = YesterdayAccomplishmentsSummaryBuilder.Build(Entries());
            MyDayActivitySummary? resumed = null;
            int opens = 0;
            context.Services.GetRequiredService<DialogService>().OnOpen += (_, _, _, _) => opens++;
            var component = context.Render<YesterdayAccomplishmentsCardComponent>(parameters => parameters
                .Add(item => item.Activities, summaries)
                .Add(item => item.OnResumeWorkClicked, summary => resumed = summary));
            var buttons = component.FindComponents<RadzenButton>();
            Assert.Equal(ButtonStyle.Success, buttons[2].Instance.ButtonStyle);
            Assert.Equal(Variant.Text, buttons[2].Instance.Variant);
            Assert.Equal(ButtonSize.Small, buttons[2].Instance.Size);
            buttons[2].Find("button").Click();
            Assert.Same(summaries[1], resumed);
            Assert.Equal(0, opens);
            var noCallback = context.Render<YesterdayAccomplishmentsCardComponent>(parameters =>
                parameters.Add(item => item.Activities, summaries));
            noCallback.Find("button").Click();
        }
    }

    [Fact]
    public async Task SelectionOpensOnlySelectedSessionsWithDayContextAndNeverResumes()
    {
        using (var context = CreateContext())
        {
            var summaries = YesterdayAccomplishmentsSummaryBuilder.Build(Entries());
            var day = new DateTime(2026, 10, 23);
            int resumes = 0;
            string? title = null;
            Dictionary<string, object?>? arguments = null;
            var dialogs = context.Services.GetRequiredService<DialogService>();
            dialogs.OnOpen += (text, type, values, options) =>
            {
                title = text;
                arguments = values;
                Assert.Equal(typeof(YesterdayAccomplishmentsDialog), type);
            };
            var host = context.Render<RadzenDialog>();
            var component = context.Render<YesterdayAccomplishmentsCardComponent>(parameters => parameters
                .Add(item => item.Activities, summaries).Add(item => item.Workday, day)
                .Add(item => item.OnResumeWorkClicked, _ => resumes++));
            component.FindComponents<RadzenButton>()[1].Find("button").Click();
            Assert.Equal(new[] { "Latest note", "Completed analysis" }, host.FindAll("li").Select(item => item.TextContent));
            Assert.Contains("APP-17", title);
            Assert.Contains("Oct 23, 2026", title);
            Assert.NotNull(arguments);
            Assert.Same(summaries[0].Entries, arguments[nameof(YesterdayAccomplishmentsDialog.Entries)]);
            await component.InvokeAsync(() => dialogs.Close());
            component.FindComponents<RadzenButton>()[3].Find("button").Click();
            Assert.Contains("OPS-29", title);
            Assert.Same(summaries[1].Entries, arguments[nameof(YesterdayAccomplishmentsDialog.Entries)]);
            await component.InvokeAsync(() => dialogs.Close());
            Assert.Equal(0, resumes);
            Assert.Equal("Completed analysis", summaries[0].Entries[1].Accomplishment);
        }
    }

    [Fact]
    public void DialogCombinesNewestFirstRichNotesSkipsBlankAndDismissesWithoutMutation()
    {
        using (var context = CreateContext())
        {
            var entries = YesterdayAccomplishmentsSummaryBuilder.Build(Entries())[0].Entries;
            var before = entries.Select(entry => (entry.TimeEntryId, entry.Accomplishment, entry.StartWork)).ToArray();
            int closes = 0;
            context.Services.GetRequiredService<DialogService>().OnClose += _ => closes++;
            var pending = context.Services.GetRequiredService<DialogService>().OpenAsync<YesterdayAccomplishmentsDialog>("Test");
            var component = context.Render<YesterdayAccomplishmentsDialog>(parameters =>
                parameters.Add(item => item.Entries, entries.AsEnumerable().Reverse()));
            Assert.Equal(new[] { "Latest note", "Completed analysis" }, component.FindAll("li").Select(item => item.TextContent));
            component.Find("button").Click();
            Assert.Equal(1, closes);
            Assert.Equal(before, entries.Select(entry => (entry.TimeEntryId, entry.Accomplishment, entry.StartWork)).ToArray());
            component.Render(parameters => parameters.Add(item => item.Entries,
                [new() { Accomplishment = " " }]));
            Assert.Empty(component.FindAll("li"));
            Assert.Contains("No accomplishments recorded for this work item.", component.Markup);
        }
    }

    [Fact]
    public void RenderedOrderMatchesTypeGroupsAndIdentifiersWithSafeMissingMetadata()
    {
        using (var context = CreateContext())
        {
            WorkItem Item(int id, string number, int typeId, string name) => new()
            {
                Id = id, ActivityNumber = number, Project = new() { Key = "APP" },
                Type = new() { Id = typeId, Name = name }
            };
            WorkItem[] items = [Item(3, "2", 2, "Feature"), Item(2, "1", 2, "Feature"),
                Item(1, "9", 1, "Bug"), new() { Id = 4 }];
            var summaries = YesterdayAccomplishmentsSummaryBuilder.Build(items.Select(item =>
                new TimeEntry { WorkItemId = item.Id, WorkItem = item }));
            var component = context.Render<YesterdayAccomplishmentsCardComponent>(parameters =>
                parameters.Add(item => item.Activities, summaries));
            Assert.Equal(new[] { "##", "APP-9", "APP-1", "APP-2" }, component.FindComponents<RadzenButton>()
                .Where(button => button.Instance.Icon != "play_arrow").Select(button => button.Instance.Text));
            Assert.Equal(string.Empty, component.FindComponents<RadzenIcon>()[0].Instance.Icon);
        }
    }
    private static BunitContext CreateContext()
    {
        var context = new BunitContext();
        context.Services.AddScoped<DialogService>();
        context.JSInterop.Setup<string>("Radzen.openDialog", _ => true).SetResult(string.Empty);
        context.JSInterop.SetupVoid("Radzen.closeDialog", _ => true).SetVoidResult();

        return context;
    }

    private static TimeEntry[] Entries()
    {
        var first = new WorkItem { Id = 1, Project = new() { Key = "APP", ShortName = "Application" }, ActivityNumber = "17", Type = new() { Id = 1, Name = "Feature", Icon = "build" } };
        var second = new WorkItem { Id = 2, Project = new() { Key = "OPS", ShortName = "Operations" }, ActivityNumber = "29", Type = first.Type };
        var day = new DateTime(2026, 10, 23);
        return
        [
            new() { Id = 1, WorkItemId = 1, WorkItem = first, StartWork = day.AddHours(9), EndWork = day.AddHours(10), Accomplishment = "Completed analysis" },
            new() { Id = 2, WorkItemId = 1, WorkItem = first, StartWork = day.AddHours(11), EndWork = day.AddHours(12), Accomplishment = "<p>Latest note</p>" },
            new() { Id = 3, WorkItemId = 2, WorkItem = second, StartWork = day.AddHours(10), Accomplishment = " " }
        ];
    }
}
