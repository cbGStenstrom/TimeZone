using Microsoft.AspNetCore.Components;
using System.Linq.Expressions;
using TimeKeeper.App.Components.Forms;
using TimeKeeper.Domain.Models;

namespace TimeKeeper.App.Components;

public partial class TimeEntryStartWorkComponent : CbComponentBase
{
    #region injected services
    #endregion injected services

    #region parameters

    /// <summary>
    ///  Gets or sets the identifier of the time entry to be displayed or edited.
    /// </summary>
    [Parameter]
    public int? TimeEntryId { get; set; }

    #endregion parameters

    #region properties

    int? SelectedWorkItemId { get; set; }

    public TimeEntry? SelectedTimeEntry { get; set; }

    List<Expression<Func<DataAccess.Entities.WorkItem, bool>>>? WorkItemFilterExpression { get; set; }

    WorkItemLookupFilter? WorkItemLookupFilterComponent { get; set; }

    List<WorkItem>? WorkItemList { get; set; }

    #endregion properties

    #region events

    /// <summary>
    ///  Gets or sets the callback that is invoked when a new time entry is created.
    /// </summary>
    /// <remarks>
    ///  The callback receives the identifier of the newly created time entry as its argument. 
    ///  Assign this property to handle post-creation logic, such as updating UI elements or 
    ///  persisting additional data.
    /// </remarks>
    [Parameter]
    public EventCallback<int> OnTimeEntryCreated { get; set; }

    #endregion events

    #region data

    /// <summary>
    ///  Asynchronously loads the time entry with the specified identifier and sets it as the 
    ///  selected time entry.
    /// </summary>
    /// <param name="timeEntryId">
    ///  The unique identifier of the time entry to load.
    /// </param>
    /// <returns>
    ///  A task that represents the asynchronous load operation.
    /// </returns>
    async Task LoadSelectedTimeEntry(int timeEntryId)
    {
        this.SelectedTimeEntry = await base.TimeEntrySvc!.GetTimeEntryById(timeEntryId);
    }

    /// <summary>
    /// Loads the variable which is bound to the WorkItems drop down list.
    /// </summary>
    /// <returns></returns>
    async Task LoadWorkItems()
    {
        WorkItemList = (await base.WorkItemSvc!.GetFilteredWorkItems(this.WorkItemFilterExpression)).OrderBy(e => e.Title).ToList();
    }

    /// <summary>
    ///  Creates a new time entry for the current user and selected work item, if both are available.
    /// </summary>
    /// <remarks>
    ///  This method does not create a time entry if there is no authenticated user or if no work item
    ///  is selected. The created time entry is assigned to the SelectedTimeEntry property upon success.
    /// </remarks>
    async Task CreateTimeEntry()
    {
        Laborer? user = base.SessionService!.User;

        if (user != null && this.SelectedWorkItemId.HasValue)
        {
            TimeEntry model = new TimeEntry
            {
                LaborerId = user.Id,
                WorkItemId = this.SelectedWorkItemId.Value,
            };

            this.SelectedTimeEntry = await base.TimeEntrySvc!.StartWorkOnWorkItem(model, user);
        }
    }

    #endregion data

    #region lifecycle

    protected override async Task OnInitializedAsync()
    {
        this.WorkItemFilterExpression = WorkItemLookupFilterComponent?.GetFilter() ?? [];
        await this.LoadWorkItems();
        if (this.TimeEntryId.HasValue)
        {
            await this.LoadSelectedTimeEntry(this.TimeEntryId.Value);
        }
    }

    #endregion lifecycle

    #region event handlers

    /// <summary>
    ///  Handles the btnChange_OnClick button click event by closing the dialog.
    /// </summary>
    void btnCancel_OnClick()
    {
        base.DialogSvc!.Close(); 
    }

    /// <summary>
    ///  Handles the click event for the Start Now button by creating a new time entry and notifying 
    ///  listeners when the entry is created.
    /// </summary>
    /// <remarks>
    ///  If a time entry is successfully created and an event handler is registered, the method
    ///  invokes the OnTimeEntryCreated event with the new entry's identifier. The dialog is then 
    ///  closed and the component state is updated.
    /// </remarks>
    /// <returns>
    ///  A task that represents the asynchronous operation.
    /// </returns>
    async Task btnStartNow_OnClick()
    {
        await this.CreateTimeEntry();
        if(this.OnTimeEntryCreated.HasDelegate && this.SelectedTimeEntry != null)
        {
            await this.OnTimeEntryCreated.InvokeAsync(this.SelectedTimeEntry.Id);
        }
        base.DialogSvc!.Close();
        base.StateHasChanged();
    }

    /// <summary>
    ///  Handles the event raised when the user changes one of the filter parameters.
    /// </summary>
    /// <param name="filter"></param>
    async void LookupFilter_OnChange(List<Expression<Func<DataAccess.Entities.WorkItem, bool>>> filter)
    {
        this.WorkItemFilterExpression = filter;
        await this.LoadWorkItems();
        this.StateHasChanged();
    }

    #endregion event handlers

    #region private
    #endregion private

    #region public
    #endregion public
}