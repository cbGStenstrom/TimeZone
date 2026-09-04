using Microsoft.AspNetCore.Components;
using System.Linq.Expressions;
using TimeKeeper.App.Components.Forms;
using TimeKeeper.Domain.Models;

namespace TimeKeeper.App.Components;

public partial class WorkItemListComponent : CbComponentBase
{
    #region injected services
    #endregion injected services

    #region parameters

    /// <summary>
    ///  Gets or sets a value indicating whether the component should be displayed in a dialog window.
    /// </summary>
    [Parameter]
    public bool OpenInDialog { get; set; } = false;

    /// <summary>
    ///  Gets or sets the currently selected work item.
    /// </summary>
    [Parameter]
    public WorkItem? SelectedWorkItem { get; set; }

    /// <summary>
    ///  Do not use. This is provided to support two-data binding on the SelectedWorkItem. Gets or 
    ///  sets the callback that is invoked when the selected work item changes.
    /// </summary>
    [Parameter]
    public EventCallback<WorkItem?> SelectedWorkItemChanged { get; set; }

    #endregion parameters

    #region properties

    /// <summary>
    ///  Gets or sets the identifier of the currently selected work item.
    /// </summary>
    int SelectedWorkItemId { get; set; }

    /// <summary>
    ///  Gets or sets the collection of filter expressions used to select work items.
    /// </summary>
    /// <remarks>
    ///  Each expression in the collection defines a predicate that a work item must satisfy to be
    ///  included in the results. All expressions are typically combined using a logical AND operation. 
    ///  Set this property to customize which work items are retrieved or processed based on specific 
    ///  criteria.
    /// </remarks>
    List<Expression<Func<DataAccess.Entities.WorkItem, bool>>>? WorkItemFilterExpression { get; set; }

    /// <summary>
    ///  Gets or sets the collection of work items associated with this instance.
    /// </summary>
    List<WorkItem>? WorkItemList { get; set; }

    /// <summary>
    ///  Gets or sets the filter component used to refine work item lookup results.
    /// </summary>
    WorkItemLookupFilter? WorkItemLookupFilterComponent { get; set; }

    #endregion properties

    #region events

    /// <summary>
    ///  Gets or sets the callback that is invoked when the associated work item changes.
    /// </summary>
    /// <remarks>
    ///  Use this property to handle changes to the work item, such as updates to its state or
    ///  properties. The callback receives the updated <see cref="WorkItem"/> as its argument.
    /// </remarks>
    [Parameter]
    public EventCallback<WorkItem> OnWorkItemChanged { get; set; }

    #endregion events

    #region data

    /// <summary>
    /// Loads the variable which is bound to the WorkItems drop down list.
    /// </summary>
    /// <returns></returns>
    async Task LoadWorkItems()
    {
        WorkItemList = [.. (await base.WorkItemSvc!.GetFilteredWorkItems(this.WorkItemFilterExpression)).OrderBy(e => e.Title)];
    }

    #endregion data

    #region lifecycle

    protected override async Task OnInitializedAsync()
    {
        await this.LoadWorkItems();
        this.InitializeSelectedWorkItem();
    }

    #endregion lifecycle

    #region event handlers

    /// <summary>
    ///  Handles the click event for the Close button, updating the selected work item and closing 
    ///  the dialog.
    /// </summary>
    /// <remarks> 
    ///  If a work item is selected and an event handler is registered, this method invokes the
    ///  OnWorkItemChanged event before closing the dialog. This ensures that any changes to the 
    ///  selected work item are communicated to event subscribers prior to dialog closure.
    /// </remarks>
    async void btnClose_OnClick()
    {
        this.UpdateSelectedWorkItem();

        if (this.OnWorkItemChanged.HasDelegate && this.SelectedWorkItem != null)
        {
            await this.OnWorkItemChanged.InvokeAsync(this.SelectedWorkItem);
        }

        base.DialogSvc!.Close(this.SelectedWorkItem);
    }

    /// <summary>
    ///  Handles the change event for the work item selection and notifies subscribers when the 
    ///  selected work item changes.
    /// </summary>
    /// <param name="args">
    ///  An object containing event data associated with the change event.
    /// </param>
    async void ddlWorkItem_OnChange(object args)
    {
        this.UpdateSelectedWorkItem();

        if (!this.OpenInDialog)
        {
            // Execute the two-way binding
            //
            if (this.SelectedWorkItemChanged.HasDelegate && this.SelectedWorkItem != null)
            {
                await this.SelectedWorkItemChanged.InvokeAsync(this.SelectedWorkItem);
            }

            // If the user has defined an additional handler, execute it.
            //
            if (this.OnWorkItemChanged.HasDelegate && this.SelectedWorkItem != null)
            {
                await this.OnWorkItemChanged.InvokeAsync(this.SelectedWorkItem);
            }
        }
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

    /// <summary>
    ///  Initializes the selected work item by updating the selected work item identifier to match 
    ///  the currently selected work item.
    /// </summary>
    /// <remarks>
    ///  Call this method to synchronize the selected work item identifier with the current selection.
    ///  This is typically used to ensure that dependent logic or UI elements reflect the correct 
    ///  work item state.
    /// </remarks>
    void InitializeSelectedWorkItem()
    {
        if (this.SelectedWorkItem != null)
        {
            this.SelectedWorkItemId = this.SelectedWorkItem.Id;
        }
    }

    /// <summary>
    ///  Updates the selected work item based on the current selected work item ID.
    /// </summary>
    /// <remarks>
    ///  If the selected work item ID is not zero and a matching work item exists in the work item
    ///  list, the selected work item is set to that item. Otherwise, the selected work item is set 
    ///  to null.
    /// </remarks>
    void UpdateSelectedWorkItem()
    {
        if (this.SelectedWorkItemId != 0)
        {
            this.SelectedWorkItem = this.WorkItemList?.FirstOrDefault(w => w.Id == this.SelectedWorkItemId);
        }
        else
        {
            this.SelectedWorkItem = null;
        }
    }

    #endregion private

    #region public
    #endregion public
}