using Microsoft.AspNetCore.Components;
using TimeKeeper.App.Api.Enums;
using TimeKeeper.Domain.Models;

namespace TimeKeeper.App.Components;

public partial class ProjectGridComponent : CbComponentBase
{
    #region enums

    public enum ProjectGridColumns
    {
        Acronym,
        LongName,
        ShortName,
        UpdatedBy,
        UpdatedDate
    }

    #endregion enums

    #region injected services
    #endregion injected services

    #region constants

    public const string SortAscendingIcon = GoogleIcons.ArrowDropUp;
    public const string SortDescendingIcon = GoogleIcons.ArrowDropDown;

    #endregion constants

    #region fields

    bool _dataHasLoaded = false;

    #endregion fields

    #region parameters

    /// <summary>
    ///  Gets or sets the collection of project data to be processed or displayed.
    /// </summary>
    [Parameter]
    public List<Project>? ProjectData { get; set; }

    #endregion parameters

    #region properties

    /// <summary>
    ///  Gets or sets the icon name used for the Acronym header.
    /// </summary>
    string btnAcronymIcon { get; set; } = SortAscendingIcon;

    /// <summary>
    ///  Gets or sets the icon name used for the LongName header.
    /// </summary>
    string btnLongNameIcon { get; set; } = string.Empty;

    /// <summary>
    ///  Gets or sets the icon name used for the ShortName header.
    /// </summary>
    string btnShortNameIcon { get; set; } = string.Empty;

    /// <summary>
    ///  Gets or sets the icon name used for the UpdatedBy header.
    /// </summary>
    string btnUpdatedByIcon { get; set; } = string.Empty;

    /// <summary>
    ///  Gets or sets the icon name used for the UpdatedDate header.
    /// </summary>
    string btnUpdatedDateIcon { get; set; } = string.Empty;

    /// <summary>
    ///  Gets or sets the column currently selected in the project grid.
    /// </summary>
    ProjectGridColumns SelectedColumn { get; set; } = ProjectGridColumns.Acronym;

    /// <summary>
    ///  Gets or sets the function used to select the column by which projects are sorted.
    /// </summary>
    /// <remarks>
    ///  The function should return a string representing the sort key for a given project. Changing
    ///  this property allows customization of sorting behavior based on different project attributes.
    /// </remarks>
    Func<Project, object> SortColumn { get; set; } = (e => e?.Key ?? "Key");

    /// <summary>
    ///  Gets or sets the direction in which items are sorted.
    /// </summary>
    SortDirections SortDirection { get; set; } = SortDirections.Ascending;

    #endregion properties

    #region events

    [Parameter]
    public EventCallback<Project> OnProjectUpdated { get; set; }

    #endregion events

    #region data
    #endregion data

    #region lifecycle

    protected override async Task OnParametersSetAsync()
    {
        // Only run the following routine if the data has not been loaded yet.
        //
        if(!this._dataHasLoaded)
        { 
            // If data exists in the data variable then sort it and flag it as loaded.
            //
            if(this.ProjectData != null && this.ProjectData.Count > 0)
            {
                await this.ApplySortToDataSet();
                this._dataHasLoaded = true;
            }
        }
    }

    #endregion lifecycle

    #region event handlers

    /// <summary>
    ///  Handles the click event for the acronym column header, refreshing the project list and 
    ///  applying the default sort order.
    /// </summary>
    /// <returns>
    ///  A task that represents the asynchronous operation.
    /// </returns>
    async Task btnAcronymColHeader_OnClick()
    {
        this.SetSortCriteria(ProjectGridColumns.Acronym, e => e.Key ?? "Key");
        await this.ApplySortToDataSet();
        base.StateHasChanged();
    }

    /// <summary>
    ///  Handles the click event for the Long Name column header, retrieving the project list, 
    ///  applying sorting by the Long Name column, and refreshing the displayed data.
    /// </summary>
    /// <returns>
    ///  A task that represents the asynchronous operation.
    /// </returns>
    async Task btnLongNameColHeader_OnClick()
    {
        this.SetSortCriteria(ProjectGridColumns.LongName, e => e.LongName ?? "LongName");
        await this.ApplySortToDataSet();
        base.StateHasChanged();
    }

    /// <summary>
    ///  Handles the click event for the Short Name column header, refreshing the project list 
    ///  and applying sorting by the Short Name column.
    /// </summary>
    /// <remarks>
    ///  This method retrieves the latest list of projects, applies sorting based on the Short
    ///  Name column, and refreshes the component's state. Call this method in response to a 
    ///  user clicking the Short Name column header to update the displayed data accordingly.
    /// </remarks>
    /// <returns>
    ///  A task that represents the asynchronous operation.
    /// </returns>
    async Task btnShortNameColHeader_OnClick()
    {
        this.SetSortCriteria(ProjectGridColumns.ShortName, e => e.ShortName ?? "ShortName");
        await this.ApplySortToDataSet();
        base.StateHasChanged();
    }

    /// <summary>
    ///  Handles the click event for the 'Updated By' column header, retrieves the project list, 
    ///  applies sorting by the 'Updated By' field, and refreshes the displayed data.
    /// </summary>
    /// <returns>
    ///  A task that represents the asynchronous operation. The task completes when the data has 
    ///  been initialized and the UI has been updated.
    /// </returns>
    async Task btnUpdatedByColHeader_OnClick()
    {
        this.SetSortCriteria(ProjectGridColumns.UpdatedBy, e => e.UpdatedBy ?? "UpdatedBy");
        await this.ApplySortToDataSet();
        base.StateHasChanged();
    }

    /// <summary>
    ///  Handles the click event for the Updated Date column header, retrieves the project list, 
    ///  applies sorting by updated date, and refreshes the component state.
    /// </summary>
    /// <remarks>
    /// This method should be called in response to a user clicking the Updated Date column header 
    /// in the project grid. It ensures that the grid is sorted by the updated date and that the 
    /// displayed data is refreshed accordingly.
    ///</remarks>
    /// <returns>
    ///  A task that represents the asynchronous operation of handling the column header click and 
    ///  updating the project grid.
    /// </returns>
    async Task btnUpdatedDateColHeader_OnClick()
    {
        this.SetSortCriteria(ProjectGridColumns.UpdatedDate, e => e.UpdatedDate);
        await this.ApplySortToDataSet();
        base.StateHasChanged();
    }

    /// <summary>
    ///  Invokes the OnProjectUpdated event callback with the specified project when a project 
    ///  update occurs.
    /// </summary>
    /// <param name="projectModell">
    ///  The project model containing the updated project data to be passed to the event callback. 
    ///  Cannot be null.
    /// </param>
    /// <returns>
    ///  A task that represents the asynchronous operation.
    /// </returns>
    async Task ProjectGridRowComponent_OnProjectUpdated(Project projectModell)
    {
        if(this.OnProjectUpdated.HasDelegate)
        {
            this._dataHasLoaded = false;
            await this.OnProjectUpdated.InvokeAsync(projectModell);
        }
    }

    #endregion event handlers

    #region private

    /// <summary>
    ///  Applies the current sort Criteria to the project data set asynchronously.
    /// </summary>
    /// <remarks>
    ///  This method updates the ProjectData property by sorting it according to the SortColumn and
    ///  SortDirection properties. The operation is performed in-place and overwrites the existing 
    ///  data set. Ensure that ProjectData is not null before calling this method.
    /// </remarks>
    /// <returns>
    ///  A task that represents the asynchronous sort operation.
    /// </returns>
    async Task ApplySortToDataSet()
    {
        ArgumentNullException.ThrowIfNull(this.ProjectData);

        // sort the data
        //
        this.ProjectData = (this.SortDirection == SortDirections.Ascending) ?
                            [.. this.ProjectData.OrderBy(SortColumn)] :
                            [.. this.ProjectData.OrderByDescending(SortColumn)];
    }

    /// <summary>
    ///  Sets the column and sorting direction for the project grid, updating the sort icon and 
    ///  criteria accordingly.
    /// </summary>
    /// <remarks>
    ///  Calling this method updates the visual sort indicator for the selected column and changes 
    ///  the sorting direction. If the same column is selected consecutively, the sort direction 
    ///  alternates between ascending and descending.
    /// </remarks>
    /// <param name="selectedColumn">
    ///  The column to sort by in the project grid. Selecting the same column toggles the sort direction; 
    ///  selecting a different column resets the sort direction to ascending.
    /// </param>
    /// <param name="sortColumn">
    ///  A function that selects the property of the project to use for sorting. This determines 
    ///  the sort criteria for the selected column.
    /// </param>
    void SetSortCriteria(ProjectGridColumns selectedColumn, Func<Project, object> sortColumn)
    {
        if (this.SelectedColumn == selectedColumn)
        {
            this.SortDirection = (this.SortDirection == SortDirections.Ascending) ?
                                    SortDirections.Descending : SortDirections.Ascending;
        }
        else
        {
            this.SelectedColumn = selectedColumn;
            this.SortColumn = sortColumn;
            this.SortDirection = SortDirections.Ascending;
        }

        // set the up/down icon on the selected button
        //
        string icon = (this.SortDirection == SortDirections.Ascending) ? SortAscendingIcon : SortDescendingIcon;

        this.btnAcronymIcon = string.Empty;
        this.btnLongNameIcon = string.Empty;
        this.btnShortNameIcon = string.Empty;
        this.btnUpdatedByIcon = string.Empty;
        this.btnUpdatedDateIcon = string.Empty;

        switch (selectedColumn)
        {
            case ProjectGridColumns.Acronym:
                this.btnAcronymIcon = icon;
                break;
            case ProjectGridColumns.LongName:
                this.btnLongNameIcon = icon;
                break;
            case ProjectGridColumns.ShortName:
                this.btnShortNameIcon = icon;
                break;
            case ProjectGridColumns.UpdatedBy:
                this.btnUpdatedByIcon = icon;
                break;
            case ProjectGridColumns.UpdatedDate:
                this.btnUpdatedDateIcon = icon;
                break;
        }
    }

    #endregion private

    #region public
    #endregion public
}