using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using TimeKeeper.Domain.Enums;
using TimeKeeper.Domain.Models;

namespace TimeKeeper.App.Components.Forms;


public partial class WorkItemLookupFilter : CbComponentBase
{
    #region injected services
    #endregion injected services

    #region parameters
    #endregion parameters

    #region properties

    /// <summary>
    /// Gets or sets the billable filter option.
    /// </summary>
    private IsBillableOptions FilterIsBillable { get; set; } = IsBillableOptions.All;

    /// <summary>
    /// Gets or sets the open/closed filter option.
    /// </summary>
    private IsOpenOptions FilterIsOpen { get; set; } = IsOpenOptions.Open;

    /// <summary>
    /// Gets or sets the list of projects available for filtering.
    /// </summary>
    private List<Project> ProjectList { get; set; } = [];

    /// <summary>
    /// Gets or sets the currently selected project ID for filtering.
    /// </summary>
    private int? SelectedProjectId { get; set; }

    /// <summary>
    /// Gets or sets the list of work items matching the current filter.
    /// </summary>
    private List<WorkItem> WorkItemList { get; set; } = [];

    #endregion properties

    #region events

    /// <summary>
    /// Event raised when the filter criteria change, providing the updated list of filter expressions.
    /// </summary>
    [Parameter]
    public EventCallback<List<Expression<Func<DataAccess.Entities.WorkItem, bool>>>> OnFilterChange { get; set; }

    [Parameter]
    public EventCallback OnFilterClear { get; set; }

    [Parameter]
    public EventCallback<List<Expression<Func<Domain.Models.WorkItem, bool>>>> OnFilterModelChange { get; set; }

    #endregion events

    #region data

    /// <summary>
    /// Populates the list of Projects that will be bound to the Project Filter drop down list.
    /// </summary>
    /// <returns></returns>
    private async Task PopulateProjectList()
    {
        this.ProjectList = (await this.ProjectService!.GetProjects()).OrderBy(e=>e.ShortName).ToList() ?? [];
    }

    #endregion data

    #region lifecycle

    /// <summary>
    /// Initializes the component asynchronously by loading the project list.
    /// </summary>
    protected async override Task OnInitializedAsync()
    {
        await this.PopulateProjectList();
    }

    #endregion lifecycle

    #region event handlers

    /// <summary>
    /// Handles when a user clicks on the Clear Filter button.
    /// </summary>
    private async Task ClearFilter_OnClick()
    {
        this.ClearFilter();

        var filter = this.GetFilter();
        await this.OnFilterChange.InvokeAsync(filter);

        var modelFilter = this.GetModelFilter();
        await this.OnFilterModelChange.InvokeAsync(modelFilter);

        await this.OnFilterClear.InvokeAsync();
    }

    /// <summary>
    /// Handles when a user changes the IsBillable filter option.
    /// </summary>
    /// <param name="value">The new filter value selected by the user.</param>
    private async Task FilterIsBillable_OnChange(object value)
    {
        var filter = this.GetFilter();
        await this.OnFilterChange.InvokeAsync(filter);

        var modelFilter = this.GetModelFilter();
        await this.OnFilterModelChange.InvokeAsync(modelFilter);
    }

    /// <summary>
    /// Handles when a user changes the IsOpen filter option.
    /// </summary>
    /// <param name="value">The new filter value selected by the user.</param>
    private async Task FilterIsOpen_OnChange(object value)
    {
        var filter = this.GetFilter();
        await this.OnFilterChange.InvokeAsync(filter);

        var modelFilter = this.GetModelFilter();
        await this.OnFilterModelChange.InvokeAsync(modelFilter);
    }

    /// <summary>
    /// Handles when a user changes the Project filter option.
    /// </summary>
    /// <param name="value">The new filter value selected by the user.</param>
    private async Task FilterProjectId_OnChange(object value)
    {
        var filter = this.GetFilter();
        await this.OnFilterChange.InvokeAsync(filter);

        var modelFilter = this.GetModelFilter();
        await this.OnFilterModelChange.InvokeAsync(modelFilter);
    }

    #endregion event handlers

    #region private

    /// <summary>
    /// Applies the billable filter expression to the provided filter list.
    /// </summary>
    /// <param name="filter">The filter list to append the billable expression to.</param>
    private void ApplyIsBillingFilter(ref List<Expression<Func<DataAccess.Entities.WorkItem, bool>>> filter)
    {
        if (this.FilterIsBillable == IsBillableOptions.Billable)
        {
            filter.Add(e => e.IsBillable);
        }
        else if (this.FilterIsBillable == IsBillableOptions.NonBillable)
        {
            filter.Add(e => !e.IsBillable);
        }
    }

    private void ApplyIsBillingFilter(ref List<Expression<Func<Domain.Models.WorkItem, bool>>> filter)
    {
        if (this.FilterIsBillable == IsBillableOptions.Billable)
        {
            filter.Add(e => e.IsBillable);
        }
        else if (this.FilterIsBillable == IsBillableOptions.NonBillable)
        {
            filter.Add(e => !e.IsBillable);
        }
    }

    /// <summary>
    /// Applies the open/closed filter expression to the provided filter list.
    /// </summary>
    /// <param name="filter">The filter list to append the open/closed expression to.</param>
    private void ApplyIsOpenFilter(ref List<Expression<Func<DataAccess.Entities.WorkItem, bool>>> filter)
    {
        if (this.FilterIsOpen == IsOpenOptions.Open)
        {
            filter.Add(e => e.IsOpen);
        }
        else if (this.FilterIsOpen == IsOpenOptions.Closed)
        {
            filter.Add(e => !e.IsOpen);
        }
    }

    private void ApplyIsOpenFilter(ref List<Expression<Func<Domain.Models.WorkItem, bool>>> filter)
    {
        if (this.FilterIsOpen == IsOpenOptions.Open)
        {
            filter.Add(e => e.IsOpen);
        }
        else if (this.FilterIsOpen == IsOpenOptions.Closed)
        {
            filter.Add(e => !e.IsOpen);
        }
    }

    /// <summary>
    /// Applies the project ID filter expression to the provided filter list.
    /// </summary>
    /// <param name="filter">The filter list to append the project ID expression to.</param>
    private void ApplyProjectIdFilter(ref List<Expression<Func<DataAccess.Entities.WorkItem, bool>>> filter)
    {
        if (this.SelectedProjectId.HasValue)
        {
            filter.Add(e => e.ProjectId == this.SelectedProjectId.Value);
        }
    }

    private void ApplyProjectIdFilter(ref List<Expression<Func<Domain.Models.WorkItem, bool>>> filter)
    {
        if (this.SelectedProjectId.HasValue)
        {
            filter.Add(e => e.ProjectId == this.SelectedProjectId.Value);
        }
    }

    /// <summary>
    /// Resets all filter properties to their default values.
    /// </summary>
    private void ClearFilter()
    {
        this.SelectedProjectId = null;
        this.FilterIsBillable = IsBillableOptions.All;
        this.FilterIsOpen = IsOpenOptions.All;
    }

    #endregion private

    #region public

    /// <summary>
    /// Builds and returns the current list of filter expressions based on the selected filter options.
    /// </summary>
    /// <returns>A list of filter expressions to apply when querying work items.</returns>
    public List<Expression<Func<DataAccess.Entities.WorkItem, bool>>> GetFilter()
    {
        List<Expression<Func<DataAccess.Entities.WorkItem, bool>>> result = [];
        this.ApplyIsBillingFilter(ref result);
        this.ApplyIsOpenFilter(ref result);
        this.ApplyProjectIdFilter(ref result);

        return result;
    }

    /// <summary>
    ///  Builds and returns the current list of filter expressions based on the selected filter options. 
    ///  To be applied against a Query/Queryable of Domain Models.
    /// </summary>
    /// <returns></returns>
    public List<Expression<Func<Domain.Models.WorkItem, bool>>> GetModelFilter()
    {
        List<Expression<Func<Domain.Models.WorkItem, bool>>> result = [];
        this.ApplyIsBillingFilter(ref result);
        this.ApplyIsOpenFilter(ref result);
        this.ApplyProjectIdFilter(ref result);

        return result;

    }

    #endregion public
}