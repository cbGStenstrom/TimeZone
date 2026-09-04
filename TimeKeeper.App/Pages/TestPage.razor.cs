using Microsoft.AspNetCore.Components;
using System.Linq.Expressions;
using TimeKeeper.App.Components;
using TimeKeeper.App.Components.Pages;
using TimeKeeper.Domain.Models;

namespace TimeKeeper.App.Pages;

public partial class TestPage : CbPageBase
{
    #region injected services
    #endregion injected services

    #region properties

    WorkItem? SelectedWorkItem { get; set; }

    int SelectedWorkItemId { get; set; }

    #endregion properties

    #region data

    async Task<TimeEntry?> GetTimeEntryById(int? timeEntryId)
    {
        TimeEntry? result = timeEntryId.HasValue ? await base.TimeEntrySvc!.GetTimeEntryById(timeEntryId.Value) : null;
        return result;
    }

    async Task<WorkItem?> GetTimeEntriesForWorkItemDateRange(int workitemId, DateTime startDate, DateTime endDate)
    {
        var filter = new List<Expression<Func<TimeKeeper.DataAccess.Entities.WorkItem, bool>>>();
        filter.Add(e => e.Id == workitemId);
        filter.Add(e => e.TimeEntries.Any(te => te.StartWork >= startDate && te.EndWork <= endDate));

        var workItem = await base.WorkItemSvc!.GetWorkItemByID(workitemId);

        return workItem;
    }

    #endregion data

    #region lifecycle
    #endregion lifecycle

    #region event handlers

    async Task btnOpenEditor_OnClick()
    {

        TimeEntry model = new TimeEntry()
        {
            StartWork = DateTime.Now,
        };

        var dialogOptions = base.GetDialogOptions("56rem", "38.9rem", true, true);
        var parameters = new Dictionary<string, object>();
        parameters.Add("OpenInDialog", true);
        parameters.Add("LoadedTimeEntry", model);

        await this.DialogSvc!.OpenAsync<TimeEntryEditorComponent>($"Edit Time Entry",
            parameters,
            dialogOptions);
    }

    async void btnLoad_WorkItem()
    {
        DateTime startDate = new DateTime(2026, 1, 1);
        DateTime endDate   = new DateTime(2026, 12, 31);
        SelectedWorkItem = await GetTimeEntriesForWorkItemDateRange(SelectedWorkItemId, startDate, endDate);

        this.Layout?.Refresh();

        base.StateHasChanged();
    }

    #endregion event handlers

    #region methods
    #endregion methods
}
