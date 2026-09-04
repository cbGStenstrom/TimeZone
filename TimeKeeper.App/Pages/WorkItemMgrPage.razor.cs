using Microsoft.AspNetCore.Components;
using System.Linq.Expressions;
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

        private void OnSearchTermChange(string searchTerm)
        {
            // placeholder
        }

        private void OnWorkItemFilterChange(List<Expression<Func<Domain.Models.WorkItem, bool>>>? filter)
        {
            this._filter.Clear();

            if(filter != null && this.FilteredQuery.Any())
            {
                this._filter.AddRange(filter);
            }
        }

        private void OnWorkItemFilterClear()
        {
            this._filter.Clear();
            this._searchTerm = null;
        }

        #endregion event handlers

        #region methods

        private IQueryable<WorkItem> GetFilteredQuery()
        {
            // If a search term is defined then add it to the filter.
            //
            if(!string.IsNullOrWhiteSpace(this._searchTerm))
            {
                this._filter.Add(e => e.Title.Contains(_searchTerm, StringComparison.OrdinalIgnoreCase));
            }

            // If the filter has not been defined eithe by the filter component in OnWOrkItemFilterChange
            // and/or by the addition of a search term, then just return the Full list of workitems.
            //
            if(this._filter.Count == 0)
            {
                return this.QueryableWorkItems;
            }

            // Otherwise add anything in the filter to the query
            //
            var query = this.QueryableWorkItems;

            foreach (var filterItem in this._filter)
            {
                query = query.Where(filterItem);
            }

            return query;

        }

        #endregion methods
    }
}
