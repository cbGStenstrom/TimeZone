using Microsoft.AspNetCore.Components;
using Radzen;
using System.Linq.Expressions;
using TimeKeeper.App.Components.Dialogs;
using TimeKeeper.App.Components.Pages;
using TimeKeeper.Domain.Models;

namespace TimeKeeper.App.Pages
{
    public partial class WorkItemMgrPage : CbPageBase
    {
        #region injected services
        #endregion injected services

        #region properties

        List<WorkItem> DataSource { get; set; } = [];

        IQueryable<WorkItem> FilteredQuery { get { return this.GetFilteredQuery(); } }

        IQueryable<WorkItem> QueryableWorkItems => this.DataSource.AsQueryable();


        #endregion properties

        #region fields

        private List<Expression<Func<Domain.Models.WorkItem, bool>>> _filter = [];

        private string? _searchTerm;

        #endregion fields

        #region data

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

        async Task btnDeleteWorkItem_OnClick(WorkItem workItem)
        {
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

            await WorkItemSvc!.DeleteWorkItem(workItem.Id);

            await InitializeDataSource();
        }


        async Task btnEditWorkItem_OnClick(WorkItem workItem)
        {
            WorkItem? result = await this.OpenWorkItemInEditor(workItem);

            if (result != null)
            {
                await InitializeDataSource();
            }
        }


        async Task btnNewWorkItem_OnClick()
        {
            WorkItem? savedWorkItem = await this.OpenWorkItemInEditor(new WorkItem());

            if (savedWorkItem != null)
            {
                await this.InitializeDataSource();
            }
        }

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

        void OnWorkItemFilterChange(List<Expression<Func<Domain.Models.WorkItem, bool>>>? filter)
        {
            this._filter.Clear();

            if (filter is not null)
            {
                this._filter.AddRange(filter);
            }
        }

        void OnWorkItemFilterClear()
        {
            this._filter.Clear();
            this._searchTerm = null;
        }

        async Task txSearchTerm_OnInput(ChangeEventArgs args)
        {
            this._searchTerm = args.Value?.ToString();
        }

        #endregion event handlers

        #region methods

        IQueryable <WorkItem> GetFilteredQuery()
        {
            IQueryable<WorkItem> query = this.QueryableWorkItems;

            foreach (Expression<Func<WorkItem, bool>> filterItem in this._filter)
            {
                query = query.Where(filterItem);
            }

            if (!string.IsNullOrWhiteSpace(this._searchTerm))
            {
                string searchTerm = this._searchTerm;

                query = query.Where(workItem =>
                    workItem.Title.Contains(
                        searchTerm,
                        StringComparison.OrdinalIgnoreCase));
            }

            return query;
        }

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

        #endregion methods
    }
}
