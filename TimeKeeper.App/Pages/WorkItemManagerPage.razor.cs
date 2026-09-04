using System.Linq.Expressions;
using TimeKeeper.App.Components.Forms;
using TimeKeeper.App.Components.Pages;
using TimeKeeper.Domain.Enums;
using TimeKeeper.Domain.Models;

namespace TimeKeeper.App.Pages
{
    public partial class WorkItemManagerPage : CbPageBase
    {
        #region injected services
        #endregion injected services

        #region parameters
        #endregion parameters

        #region properties

        bool DeleteWorkItemButtonClicked { get; set; } = false;

        bool EditTimeEntriesButtonClicked { get; set; } = false;

        List<WorkItem> FilteredWorkItemList { get; set; } = [];

        IsBillableOptions FilterIsBillable { get; set; } = IsBillableOptions.All;

        IsOpenOptions FilterIsOpen { get; set; } = IsOpenOptions.Open;

        string? FilterTitle { get; set; }

        string? LookupKeyword { get; set; }

        List<Project> ProjectList { get; set; } = [];

        bool SaveIsDisabled { get; set; } = true;

        int? SelectedProjectId { get; set; }

        IList<WorkItem>? SelectedWorkItems { get; set; }

        bool ShowWorkItemEditor { get; set; } = false;

        bool ShowTimeEntryEditor { get; set; } = false;


        WorkItemForm? WorkItemFormRef { get; set; }

        List<WorkItem> WorkItemList { get; set; } = [];

        #endregion properties

        #region events
        #endregion events

        #region data

        /// <summary>
        /// Permanently deletes the <paramref name="model"/> argument from the database.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        async Task<WorkItem?> DeleteWorkItem(WorkItem model)
        {
            WorkItem? result = await this.WorkItemSvc.DeleteWorkItem(model.Id);
            return result;
        }


        async Task PopulateProjectList()
        {
            this.ProjectList = await this.ProjectSvc!.GetProjects() ?? [];
        }

        /// <summary>
        /// Populates the lisst of WorkItems that will be bound to the DataGrid.
        /// </summary>
        /// <returns></returns>
        async Task PopulateWorkItemList(List<Expression<Func<DataAccess.Entities.WorkItem, bool>>>? filter = null)
        {
            filter ??= [];

            this.ApplyIsBillingFilter(ref filter);  
            this.ApplyIsOpenFilter(ref filter);
            this.ApplyTitleFilter(ref filter);
            this.ApplyProjectIdFilter(ref filter);

            List<WorkItem> data = [.. (await WorkItemSvc.GetFilteredWorkItems()).OrderBy(e => e.Title)];
            this.WorkItemList = data;

            // initialize the FilteredWorkItemList to be the same as the WorkItemList. This is the
            // list that will actually be bound to the UI and will be filtered down as the user
            // interacts with the filter options and search box.
            this.FilteredWorkItemList = data;
        }

        /// <summary>
        /// Saves new and modified existing WorkItem objects
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        async Task<bool> SaveWorkItem(WorkItem? model)
        {
            bool success = false;

            if(this.WorkItemSvc != null && model != null)
            {
                if (model.IsNew)
                {
                    model.CreatedBy = model.UpdatedBy = this.SessionSvc?.User?.Username ?? "Not Available";
                    model = await this.WorkItemSvc.AddWorkItem(model);
                    model?.TakeSnapshot();
                    success = true;
                }
                else
                {
                    if (await this.WorkItemSvc.UpdateWorkItem(this.SelectedWorkItems[0]))
                    {
                        success = true;
                    }
                }
            }

            return success;
        }

        #endregion data

        #region lifecycle

        protected override async Task OnInitializedAsync()
        {
            await this.PopulateProjectList();
            //await this.PopulateWorkItemList();
            await this.InitializeDataSource();
        }

        #endregion lifecycle

        #region event handlers

        /// <summary>
        /// Handles when a user clicks on the Cancel save button.
        /// </summary>
        void btnCancelSave_OnClick()
        {
            this.SelectedWorkItems?[0]?.RevertToSnapshot();
            this.SelectedWorkItems = null;

            this.ShowTimeEntryEditor = false;
            this.ShowWorkItemEditor = false;
            this.SaveIsDisabled = true;

            base.StateHasChanged();

        }

        /// <summary>
        /// Handles when a user clicks on the Delete Workitem button in a grid row. A click here 
        /// will also bubble up through the gridRow_OnSelect event handler as well.
        /// </summary>
        void btnDeleteWorkItem_OnCLick()
        {
            this.EditTimeEntriesButtonClicked = false;
            this.DeleteWorkItemButtonClicked = true;

        }

        /// <summary>
        /// Handles when a user clicks on the New Work Item button
        /// </summary>
        void btnNewWorkItem_OnClick()
        {
            this.SelectedWorkItems = new List<WorkItem>() { new WorkItem() };

            this.ShowTimeEntryEditor = false;
            this.ShowWorkItemEditor = true;

            base.StateHasChanged();
        }

        /// <summary>
        /// Handles when a user clicks on the Save button.
        /// </summary>
        async void btnSave_OnClick()
        {
            if (this.SelectedWorkItems != null && this.SelectedWorkItems[0] != null)
            {
                // Do not allow saving until the required WorkItemType field has been set.
                //
                if (this.ShowWorkItemEditor && this.WorkItemFormRef != null && !this.WorkItemFormRef.Validate())
                {
                    return;
                }

                if (await this.SaveWorkItem(this.SelectedWorkItems[0]))
                {
                    this.SelectedWorkItems = null;

                    this.ShowTimeEntryEditor = false;
                    this.ShowWorkItemEditor = false;
                    this.SaveIsDisabled = true;

                    // After successful save, reload the WorkItem list
                    //
                    await PopulateWorkItemList();
                }
                else
                {
                    this.SelectedWorkItems[0].RevertToSnapshot();
                }

                base.StateHasChanged();
            }
        }

        /// <summary>
        /// Handles when a user changes the IsBillabel filter option.
        /// </summary>
        async void FilterIsBillable_OnChange(object value)
        {
            await this.PopulateWorkItemList();
            this.StateHasChanged();
        }

        /// <summary>
        /// Handles when a user changes the IsBillabel filter option.
        /// </summary>
        async void FilterIsOpen_OnChange(object value)
        {
            await this.PopulateWorkItemList();
            this.StateHasChanged();
        }

        /// <summary>
        /// Handles when a user changes the IsBillabel filter option.
        /// </summary>
        async void FilterProjectId_OnChange(object value)
        {
            await this.PopulateWorkItemList();
            this.StateHasChanged();
        }

        /// <summary>
        /// Handles when a user clicks on a row in the grid item. Clicks on buttons or other row 
        /// contents will bubble through here as well. 
        /// </summary>
        /// <param name="item"></param>
        async void gridRow_OnSelect(WorkItem item)
        {
            // If there is a selected item and it has unsaved changes then revert it to it's original values
            //
            if(this.SelectedWorkItems != null && this.SelectedWorkItems.Count > 0 && this.SelectedWorkItems[0] != null)
            {
                this.SelectedWorkItems[0].RevertToSnapshot();
                this.SaveIsDisabled = true;
            }

            // If the user clicked on theEdit TimeEntries button
            //
            if (this.EditTimeEntriesButtonClicked)
            {
                this.ShowWorkItemEditor = false;
                this.ShowTimeEntryEditor = true;
                this.EditTimeEntriesButtonClicked = false;
            }

            // If the user clicked on the Delete WorkItem 
            //
            else if (this.DeleteWorkItemButtonClicked)
            {
                this.DeleteWorkItemButtonClicked = false;

                if (this.SelectedWorkItems != null && this.SelectedWorkItems.Count > 0 && this.SelectedWorkItems[0] != null)
                {
                    // Capture the ID of the WorkItem before it is deleted
                    //
                    int idToRemove = this.SelectedWorkItems[0].Id;

                    // Delete the record from the database
                    //
                    await this.DeleteWorkItem(this.SelectedWorkItems[0]);

                    // After successful deletion, reload the WorkItem list
                    //
                    await PopulateWorkItemList();

                    // Deselect the selected WorkItem
                    //
                    this.SelectedWorkItems = null;
                    this.ShowTimeEntryEditor = false;
                    this.ShowWorkItemEditor = false;
                    this.SaveIsDisabled = true;

                    base.StateHasChanged();
                }
            }

            // Otherwise if the user clicked on the Edit WorkItem button or the row itself.
            //
            else
            {
                this.ShowWorkItemEditor = true;
                this.ShowTimeEntryEditor = false;
            }

            base.StateHasChanged();
        }

        /// <summary>
        /// Handles when the OnChange event is raiswd from the WorkItemEditor. This is raised whenever 
        /// an of the fields within the WorkItem Editor form have been modified. 
        /// </summary>
        void WorkItemEditor_OnChanged()
        {
            // if the selected item Is Not dirty then disable the save button. If the selected item
            // is undefined or IS dirty,then enable the Save button.
            //
            this.SaveIsDisabled = !this.SelectedWorkItems[0]?.IsDirty() ?? false;
        }

        #endregion event handlers

        #region private

        void ApplyIsBillingFilter(ref List<Expression<Func<DataAccess.Entities.WorkItem, bool>>> filter)
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

        void ApplyIsOpenFilter(ref List<Expression<Func<DataAccess.Entities.WorkItem, bool>>> filter)
        {
            switch(this.FilterIsOpen)
            {
                case IsOpenOptions.All:
                    break;
                case IsOpenOptions.InReview:
                    filter.Add(e => e.IsInReview);
                    break;
                case IsOpenOptions.Open:
                    filter.Add(e => e.IsOpen);
                    filter.Add(e => !e.IsInReview);
                    break;
                case IsOpenOptions.Closed:
                    filter.Add(e => !e.IsOpen);
                    break;
            }
        }

        void ApplyProjectIdFilter(ref List<Expression<Func<DataAccess.Entities.WorkItem, bool>>> filter)
        {
            if (this.SelectedProjectId.HasValue)
            {
                filter.Add(e => e.ProjectId == this.SelectedProjectId.Value);
            }
        }

        void ApplyTitleFilter(ref List<Expression<Func<DataAccess.Entities.WorkItem, bool>>> filter)
        {
            if (!string.IsNullOrWhiteSpace(FilterTitle))
            {
                var titleFilter = FilterTitle.ToUpper();
                filter.Add(e => e.Title != null &&
                                e.Title.ToUpper().Contains(titleFilter));
            }
        }

        #endregion private

        #region public
        #endregion public

        #region new code

        #region fields

        private string? _searchTerm;

        #endregion fields

        #region properties

        private List<WorkItem> DataSource { get; set; } = [];

        private IQueryable<WorkItem> FilteredQuery
        {
            get
            {
                return string.IsNullOrWhiteSpace(_searchTerm) 
                    ? this.QueryableWorkItems 
                    : this.QueryableWorkItems.Where(e => 
                            e.Title != null && 
                            e.Title.Contains(_searchTerm, StringComparison.OrdinalIgnoreCase));
            }
        }

        private IQueryable<WorkItem> QueryableWorkItems
        {
            get { return this.DataSource.AsQueryable(); }
        }

        #endregion properties

        #region data

        private async Task InitializeDataSource()
        {
            this.DataSource = [.. (await WorkItemSvc.GetFilteredWorkItems()).OrderBy(e => e.Title)];
        }

        private async Task ReloadDataSource()
        {
            await this.InitializeDataSource();
        }

        #endregion data

        #region event handlers

        private void OnLookupKeywordChange(string searchTerm)
        {
            List<WorkItem> data = [.. this.WorkItemList.Where(e => e.Title != null && e.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))];
            this.FilteredWorkItemList = data;
        }

        private async Task OnWorkItemLookupFilterChange(List<Expression<Func<DataAccess.Entities.WorkItem, bool>>> filter)
        {
            await this.PopulateWorkItemList(filter);

        }

        #endregion event handlers

        #region private methods



        #endregion private methods

        #endregion new code
    }
}
