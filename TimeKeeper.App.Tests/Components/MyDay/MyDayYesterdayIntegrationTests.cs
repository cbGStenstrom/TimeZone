using System.Linq.Expressions;
using System.Reflection;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Radzen;
using TimeKeeper.App.Api.Services;
using TimeKeeper.App.Components.Dialogs;
using TimeKeeper.App.Components.MyDay;
using TimeKeeper.App.Models;
using TimeKeeper.App.Pages;
using TimeKeeper.App.Services.Interfaces;
using TimeKeeper.App.Shared.Layout;
using TimeKeeper.Domain.Models;
using TimeKeeper.Domain.Services.Interfaces;
using TimeKeeper.Domain.Utilities;
using Xunit;
using Entity = TimeKeeper.DataAccess.Entities.TimeEntry;

namespace TimeKeeper.App.Tests.Components.MyDay;

public class MyDayYesterdayIntegrationTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task PageFiltersByWorkdayAndUserAndResumesThroughExistingLauncherAndRefreshCallback(bool signedIn)
    {
        using (var context = new BunitContext())
        {
            var day = YesterdayAccomplishmentsSummaryBuilder.GetPreviousWorkday(DateTime.Today);
            var user = new Laborer { Id = 7 };
            var item = new WorkItem { Id = 42, ActivityNumber = "42", Project = new() { Key = "APP" } };
            Entity[] entities =
            [
                new() { Id = 1, LaborerId = 7, WorkItemId = 42, StartWork = day.AddHours(9).ConvertLocalToUtc(), CreatedDate = day.AddYears(-1) },
                new() { Id = 2, LaborerId = 8, WorkItemId = 43, StartWork = day.AddHours(9).ConvertLocalToUtc() },
                new() { Id = 3, LaborerId = 7, WorkItemId = 44, StartWork = day.AddDays(1).ConvertLocalToUtc() }
            ];
            int queries = 0;
            var timeService = Proxy<ITimeEntryService>((method, args) =>
            {
                if (method.Name == nameof(ITimeEntryService.GetActiveTimeEntry)) return Task.FromResult<TimeEntry?>(null);
                if (method.Name == nameof(ITimeEntryService.GetFilteredTimeEntries))
                {
                    queries++;
                    var filters = Assert.IsType<List<Expression<Func<Entity, bool>>>>(args[0]);
                    IEnumerable<TimeEntry> result = entities.Where(entity => filters.All(filter => filter.Compile()(entity)))
                        .Select(entity => new TimeEntry { Id = entity.Id, LaborerId = entity.LaborerId, WorkItemId = entity.WorkItemId,
                            WorkItem = item, StartWork = entity.StartWork?.ConvertUtcToLocal(), Accomplishment = "Selected note" }).ToArray();
                    return Task.FromResult(result);
                }
                throw new InvalidOperationException("Unexpected write: " + method.Name);
            });
            var workService = Proxy<IWorkItemService>((method, _) => method.Name == nameof(IWorkItemService.GetFilteredWorkItems)
                ? Task.FromResult(new List<WorkItem>()) : throw new InvalidOperationException(method.Name));
            var launcher = new Launcher();
            context.Services.AddSingleton(timeService);
            context.Services.AddSingleton(workService);
            context.Services.AddSingleton(Proxy<IProjectService>((method, _) => throw new InvalidOperationException(method.Name)));
            context.Services.AddSingleton(Proxy<ILaborerService>((method, _) => throw new InvalidOperationException(method.Name)));
            context.Services.AddSingleton(new SessionService { User = signedIn ? user : null });
            context.Services.AddScoped<DialogService>();
            context.Services.AddScoped<NotificationService>();
            context.Services.AddSingleton<IWorkLauncherService>(launcher);

            var layout = context.Render<RefreshLayout>();
            var page = context.Render<CascadingValue<AppLayout>>(parameters => parameters
                .Add(component => component.Name, "Layout")
                .Add(component => component.Value, layout.Instance)
                .AddChildContent<MyDayPage>());
            var region = page.FindComponent<YesterdayAccomplishmentsCardComponent>();
            Assert.Single(page.FindComponents<YesterdayAccomplishmentsCardComponent>());
            Assert.Empty(page.FindComponents<YesterdayActivitySummaryComponent>());
            Assert.Equal(day, region.Instance.Workday);
            if (!signedIn)
            {
                Assert.Empty(region.Instance.Activities);
                Assert.Equal(1, queries); // Only the unchanged recent-activities query runs.
                Assert.Equal(0, launcher.Calls);
                return;
            }
            var summary = Assert.Single(region.Instance.Activities);
            Assert.Equal(42, summary.WorkItemId);
            Assert.Single(summary.Entries);
            region.Find("button").Click();
            Assert.Same(item, launcher.Item);
            Assert.Same(user, launcher.User);
            Assert.Same(page.FindComponent<TimeEntryEditorDialog>().Instance, launcher.Editor);
            Assert.Same(context.Services.GetRequiredService<DialogService>(), launcher.Dialogs);
            Assert.Equal(1, launcher.Calls);
            int before = queries;
            await page.InvokeAsync(() => launcher.Saved.InvokeAsync(new TimeEntry()));
            Assert.True(queries > before);
            Assert.Single(page.FindComponent<YesterdayAccomplishmentsCardComponent>().Instance.Activities);
        }
    }

    private static T Proxy<T>(Func<MethodInfo, object?[], object?> invoke) where T : class
    {
        var proxy = DispatchProxy.Create<T, ServiceProxy>();
        ((ServiceProxy)(object)proxy).Handler = invoke;
        return proxy;
    }

    public class ServiceProxy : DispatchProxy
    {
        public Func<MethodInfo, object?[], object?> Handler { get; set; } = default!;
        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args) =>
            this.Handler(targetMethod ?? throw new InvalidOperationException(), args ?? []);
    }

    public class RefreshLayout : AppLayout
    {
        protected override Task OnInitializedAsync() => Task.CompletedTask;
        protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder) { }
    }

    private sealed class Launcher : IWorkLauncherService
    {
        public int Calls { get; private set; }
        public DialogService? Dialogs { get; private set; }
        public TimeEntryEditorDialog? Editor { get; private set; }
        public WorkItem? Item { get; private set; }
        public EventCallback<TimeEntry> Saved { get; private set; }
        public Laborer? User { get; private set; }
        public Task StartWork(WorkItem workItem, TimeEntryEditorDialog dialog, Laborer user, DialogService dialogService, EventCallback<TimeEntry> callback)
        {
            this.Calls++;
            this.Item = workItem;
            this.Editor = dialog;
            this.User = user;
            this.Dialogs = dialogService;
            this.Saved = callback;
            return Task.CompletedTask;
        }
    }
}
