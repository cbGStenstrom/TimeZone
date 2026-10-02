using Microsoft.AspNetCore.Components;
using Radzen;
using System.Linq.Expressions;
using TimeKeeper.App.Api.Enums;
using TimeKeeper.App.Components.Dialogs;
using TimeKeeper.App.Components.Pages;
using TimeKeeper.App.Services.Interfaces;
using TimeKeeper.Domain.Models;

namespace TimeKeeper.App.Pages;

public partial class WorkItemMgrPage : CbPageBase
{
    #region injected services

    /// <summary>
    ///  Gets or sets the service used to launch work operations.
    /// </summary>
    [Inject]
    public IWorkLauncherService WorkLauncherSvc { get; set; } = default!;

    #endregion injected services

    #region properties

    /// <summary>
    ///  Gets or sets the collection of work items used as the data source.
    /// </summary>
    List<WorkItem> DataSource { get; set; } = [];

    /// <summary>
    ///  Gets the filtered query of work items.
    /// </summary>
    IQueryable<WorkItem> FilteredQuery { get { return this.GetFilteredQuery(); } }

    /// <summary>
    ///  Gets a queryable sequence of work items from the underlying data source.
    /// </summary>
    /// <remarks>
    ///  Provides an <see cref="IQueryable{T}"/> view over <c>DataSource</c> for LINQ query
    ///  composition.</remarks>
    IQueryable<WorkItem> QueryableWorkItems => this.DataSource.AsQueryable();

    /// <summary>
    ///  Gets or sets the time entry editor dialog.  
    /// </summary>
    TimeEntryEditorDialog? dlgTimeEntryEditor { get; set; }

    #endregion properties

    #region fields

    /// <summary>
    ///  Stores predicate expressions used to filter work items.
    /// </summary>
    /// <remarks>
    ///  Contains expression trees that can be consumed by LINQ providers.</remarks>
    private List<Expression<Func<Domain.Models.WorkItem, bool>>> _filter = [];

    /// <summary>
    ///  The current search term.
    /// </summary>
    private string? _searchTerm;

    #endregion fields

    #region data

    /// <summary>
    ///  Asynchronously loads filtered work items and assigns them to the data source.
    /// </summary>
    /// <remarks>
    ///  Assigns an empty collection when no work items are returned.</remarks>
    /// <returns>
    ///  A task that represents the asynchronous operation.</returns>
    private async Task InitializeDataSource()
    {
        List<WorkItem> stageData = await base.WorkItemSvc.GetFilteredWorkItems();
        this.DataSource = stageData ?? [];
    }

    #endregion data

    #region lifecycle

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await this.InitializeDataSource();
    }

    #endregion lifecycle

    #region event handlers

    /// <summary>
    ///  Attempts to delete a work item after verifying it has no time entries and receiving 
    ///  user confirmation.
    /// </summary>
    /// <remarks>
    ///  Deletion is blocked when the work item has associated time entries.</remarks>
    /// <param name="workItem">
    ///  The work item to delete.</param>
    /// <returns>
    ///  A task that represents the asynchronous delete workflow.</returns>
    async Task btnDeleteWorkItem_OnClick(WorkItem workItem)
    {
        // WHAT: FEAT-013 do not allow WorkItems with TimeEntries to be deleted.
        int entryCount = await TimeEntrySvc.GetTimeEntryCountForWorkItem(workItem.Id);

        if (entryCount > 0)
        {
            string errMsg = $"'{workItem.Title}' contains {entryCount} time entr" +
                            $"{(entryCount == 1 ? "y" : "ies")} and cannot be deleted." +
                            $"Consider archiving or closing the WorkItem instead.";
            await base.DialogSvc.Alert(errMsg, "Delete Not Allowed");
            return;
        }

        bool? confirmed =
            await DialogSvc!.Confirm(
                $"Delete '{workItem.Title}'?",
                "Delete Work Item",
                new ConfirmOptions()
                {
                    OkButtonText = "Delete",
                    CancelButtonText = "Cancel"
                });

        if (confirmed != true)
        {
            return;
        }

        await WorkItemSvc.DeleteWorkItem(workItem.Id);

        await InitializeDataSource();
    }

    /// <summary>
    ///  Opens the specified work item in the editor and refreshes the data source if editing 
    ///  returns an updated item.
    /// </summary>
    /// <param name="workItem">
    ///  The work item to open for editing.</param>
    /// <returns>
    ///  A task that completes when the edit operation and any required data source refresh 
    ///  finish.</returns>
    async Task btnEditWorkItem_OnClick(WorkItem workItem)
    {
        WorkItem? result = await this.OpenWorkItemInEditor(workItem);

        if (result != null)
        {
            await InitializeDataSource();
        }
    }

    /// <summary>
    ///  Handles the click event for creating a new work item.
    /// </summary>
    /// <returns>
    ///  A task that represents the asynchronous operation.</returns>
    async Task btnNewWorkItem_OnClick()
    {
        WorkItem? savedWorkItem = await this.OpenWorkItemInEditor(new WorkItem());

        if (savedWorkItem != null)
        {
            await this.InitializeDataSource();
        }
    }

    /// <summary>
    /// Starts work on the selected work item, resolving any currently active time entry first.
    /// </summary>
    /// <remarks>If no active time entry exists, a start-work dialog is opened. If the active entry matches
    /// the selected work item, the existing entry is opened. If a different entry is active, confirmation is requested
    /// before stopping it and starting the selected work item.</remarks>
    /// <param name="workItem">The work item selected to begin work on.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    async Task btnStartWork_OnClick(WorkItem workItem)
    {
        await WorkLauncherSvc!.StartWork(
            workItem,
            dlgTimeEntryEditor!,
            SessionService!.User!,
            DialogSvc!,
            EventCallback.Factory.Create<TimeEntry>(this, TimeEntry_OnCreated));
    }

    /// <summary>
    ///  Opens the double-clicked work item in the editor and reloads the data source when changes 
    ///  are saved.
    /// </summary>
    /// <remarks>
    ///  Throws <see cref="ArgumentNullException"/> when the event data or its work item is null.
    /// </remarks>
    /// <param name="rowArgs">
    ///  Provides row mouse event data for the double-clicked row, including the associated work 
    ///  item.</param>
    /// <returns>
    ///  A task that represents the asynchronous operation.</returns>
    async Task GridRow_OnDoubleClick(DataGridRowMouseEventArgs<WorkItem> rowArgs)
    {
        ArgumentNullException.ThrowIfNull(rowArgs?.Data);

        WorkItem workItem = rowArgs.Data;
        WorkItem? savedWorkItem = await this.OpenWorkItemInEditor(workItem);

        if (savedWorkItem != null)
        {
            await this.InitializeDataSource();
        }
    }

    /// <summary>
    ///  Updates the current work item filter criteria.
    /// </summary>
    /// <param name="filter">
    ///  The new filter criteria.</param>
    void OnWorkItemFilterChange(List<Expression<Func<Domain.Models.WorkItem, bool>>>? filter)
    {
        this._filter.Clear();

        if (filter is not null)
        {
            this._filter.AddRange(filter);
        }
    }

    /// <summary>
    ///  Clears all work item filter criteria and resets the search term.
    /// </summary>
    void OnWorkItemFilterClear()
    {
        this._filter.Clear();
        this._searchTerm = null;
    }

    /// <summary>
    ///  Handles creation of a time entry by reinitializing the data source and requesting a UI 
    ///  refresh.
    /// </summary>
    /// <param name="entry">
    ///  The newly created time entry.</param>
    /// <returns>
    ///  A task that represents the asynchronous refresh operation after a time entry is created.
    /// </returns>
    async Task TimeEntry_OnCreated(TimeEntry entry)
    {
        await InitializeDataSource();

        if (this.Layout is not null)
        {
            await this.Layout.Refresh();
        }

        StateHasChanged();
    }

    /// <summary>
    ///  Updates the current search term from the input change event.
    /// </summary>
    /// <param name="args">
    ///  Contains the updated input value.</param>
    /// <returns>
    ///  A task that represents the asynchronous operation.</returns>
    async Task txSearchTerm_OnInput(ChangeEventArgs args)
    {
        this._searchTerm = args.Value?.ToString();
    }

    #endregion event handlers

    #region methods

    /// <summary>
    ///  Builds a query of work items filtered by all configured predicates and, when provided, 
    ///  a case-insensitive title search term.
    /// </summary>
    /// <returns>
    ///  An <see cref="IQueryable{T}"/> of <see cref="WorkItem"/> that includes only items 
    ///  matching every filter and the optional title search criterion.</returns>
    IQueryable <WorkItem> GetFilteredQuery()
    {
        // WHAT: FEAT-001 if no filters or search term are provided, return the unfiltered query.
        if (string.IsNullOrWhiteSpace(this._searchTerm) && (this._filter.Count == 0))
        {
            return this.QueryableWorkItems;
        }

        // WHAT: FEAT-001 apply all filters and the optional search term to the query.
        IQueryable<WorkItem> query = this.QueryableWorkItems;

        // WHAT: FEAT-001 add all filter expressions to the query.
        foreach (Expression<Func<WorkItem, bool>> filterItem in this._filter)
        {
            query = query.Where(filterItem);
        }

        // WHAT: FEAT-001 apply the optional search term to the query.
        if (!string.IsNullOrWhiteSpace(this._searchTerm))
        {
            string searchTerm = this._searchTerm;

            query = query.Where(workItem =>
                workItem.Title.Contains(
                    searchTerm,
                    StringComparison.OrdinalIgnoreCase) 
                || 
                (workItem.ActivityNumber != null &&
                workItem.ActivityNumber.Contains(
                    searchTerm,
                    StringComparison.OrdinalIgnoreCase))
                ||
                (workItem.Project != null &&
                 workItem.Project.Key != null &&
                 workItem.Project.Key.Contains(
                     searchTerm,
                     StringComparison.OrdinalIgnoreCase))
            );
        }

        return query;
    }

    /// <summary>
    ///  Opens the time entry editor for an existing time entry and initializes the edit callback.   
    /// </summary>
    /// <param name="activeEntry">
    ///  The existing time entry to open for editing.</param>
    /// <returns>
    ///  A task that represents the asynchronous operation.</returns>
    async Task OpenExistingTimeEntry(TimeEntry activeEntry)
    {
        if (dlgTimeEntryEditor == null)
        {
            return;
        }

        var callback = EventCallback.Factory.Create<TimeEntry>(this, TimeEntry_OnCreated);

        await dlgTimeEntryEditor.Open(
            activeEntry,
            $"Edit Work - {activeEntry.WorkItem?.DisplayIdentifier}",
            callback,
            TimeEntrySaveActions.ContinueLater);
    }

    /// <summary>
    /// Opens the time entry editor to start work on the specified work item.
    /// </summary>
    /// <param name="workItem">The work item to associate with the new start-work time entry.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    async Task OpenStartWorkDialog(WorkItem workItem)
    {
        if (dlgTimeEntryEditor == null)
        {
            return;
        }

        TimeEntry model = new()
        {
            LaborerId = this.SessionService!.User!.Id,
            WorkItemId = workItem.Id,
            WorkItem = workItem,
            StartWork = DateTime.Now
        };

        var callback = EventCallback.Factory.Create<TimeEntry>(this, TimeEntry_OnCreated);

        await dlgTimeEntryEditor.Open(model, $"Start Work - {workItem.DisplayIdentifier}", callback, TimeEntrySaveActions.StartWork);
    }

    /// <summary>
    ///  Opens the work item editor dialog for the specified work item.
    /// </summary>
    /// <param name="workItem">
    ///  The work item to create or edit.</param>
    /// <returns>
    ///  A task that represents the asynchronous operation. The task result contains the saved 
    ///  work item, or <see langword="null"/> if the dialog is canceled.</returns>
    async Task<WorkItem?> OpenWorkItemInEditor(WorkItem workItem)
    {
        string dialogTitle = workItem.IsNew ? "New Work Item" : $"Edit Work Item: {workItem.Title}";

        Dictionary<string, object> parameters = new() { { "WorkItem", workItem } };

        DialogOptions dlgOptions = new DialogOptions()
        {
            Width = "900px",
            Resizable = true,
            Draggable = true
        };

        WorkItem? savedWorkItem = await DialogSvc!.OpenAsync<WorkItemEditorDialog>(dialogTitle, parameters, dlgOptions);

        return savedWorkItem;
    }

    /// <summary>
    ///  Sets the end time of the active time entry to the current local time and persists the update 
    ///  for the current user.
    /// </summary>
    /// <param name="activeEntry">
    ///  The active time entry to stop and update.</param>
    /// <returns>
    ///  A task that represents the asynchronous operation.</returns>
    async Task StopActiveTimeEntry(TimeEntry activeEntry)
    {
        activeEntry.EndWork = DateTime.Now;
        await this.TimeEntrySvc!.UpdateTimeEntry(activeEntry, this.SessionService!.User!);
    }

    #endregion methods
}
