using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using System.Data;
using TimeKeeper.App.Api.Services;
using TimeKeeper.App.Components.Dialogs;
using TimeKeeper.Domain.Models;

namespace TimeKeeper.App.Components.WorkItemComponents
{
    public partial class WorkItemTypeGridComponent : CbComponentBase
    {
        #region injected services

        [Inject]
        protected SessionService SessionSvc { get; set; } = default!;

        #endregion injected services

        #region parameters

        [Parameter]
        public IEnumerable<WorkItemType> WorkItemTypes { get; set; } = [];

        #endregion parameters

        #region properties
        #endregion properties

        #region fields

        protected WorkItemType? EditableDataItem;

        protected RadzenDataGrid<WorkItemType> DataGrid = default!;

        #endregion fields

        #region events

        [Parameter]
        public EventCallback<WorkItemType> OnDelete { get; set; }

        [Parameter]
        public EventCallback<WorkItemType> OnEnable { get; set; }

        [Parameter]
        public EventCallback<WorkItemType> OnUpdate { get; set; }

        #endregion events

        #region data
        #endregion data

        #region lifecycle
        #endregion lifecycle

        #region event handlers

        /// <summary>
        ///  Cancels editing for the specified work item type dataItem in the data grid.
        /// </summary>
        /// <param name="dataItem">The work item type dataItem to cancel editing for.</param>
        private void btnCancel_OnClick(WorkItemType dataItem)
        {
            this.EditableDataItem = null;
            this.DataGrid.CancelEditRow(dataItem);
        }

        /// <summary>
        ///  Handles deletion of an activity type by reloading current data, confirming the action, and 
        ///  either disabling referenced types or deleting unreferenced types.
        /// </summary>
        /// <remarks>
        ///  If the selected type is no longer present, the operation exits after clearing a matching
        ///  draft. When the type has associated activities, it is disabled instead of deleted after 
        ///  confirmation. After successful changes, data is reloaded and a matching draft is cleared.
        /// </remarks>
        /// <param name="dataItem">
        ///  The activity type selected for deletion.</param>
        /// <returns>
        ///  A task that represents the asynchronous click-handling operation.</returns>
        private async Task btnDelete_OnClick(WorkItemType dataItem)
        {
            if (this.OnDelete.HasDelegate)
            {
                await this.OnDelete.InvokeAsync(dataItem);
            }
        }

        /// <summary>
        ///  Handles the edit action for an activity type by creating a draft when it is enabled, or 
        ///  confirming and enabling it when it is disabled.
        /// </summary>
        /// <remarks>
        ///  When the dataItem is disabled, prompts for confirmation before enabling and then reloads the
        ///  data.</remarks>
        /// <param name="dataItem">
        ///  The activity type dataItem to edit or enable.</param>
        /// <returns>
        ///  A task that represents the asynchronous operation.</returns>
        private async Task btnEdit_OnClick(WorkItemType dataItem)
        {
            if (dataItem.IsEnabled)
            {
                if(this.EditableDataItem != null)
                {
                    this.DataGrid?.CancelEditRow(this.EditableDataItem);
                    this.EditableDataItem = null;
                }

                this.DataGrid?.EditRow(dataItem);
                this.EditableDataItem = dataItem;
                return;
            }

            // WHAT: Confirm enabling a disabled type, then enable and reload.
            // WHY: This is a safety measure to prevent accidental enabling of a type that may have been
            //  disabled for a reason.
            if (await Confirm("Enable this Activity Type?") == true)
            {
                if(this.OnEnable.HasDelegate)
                {
                    await this.OnEnable.InvokeAsync(dataItem);
                }
            }
        }

        /// <summary>
        ///  Invokes the update callback for the specified work item type when a handler is assigned.
        /// </summary>
        /// <remarks>
        ///  If no update callback is assigned, the operation completes without invoking a handler.
        /// </remarks>
        /// <param name="datatem">
        ///  The work item type to pass to the update callback.</param>
        /// <returns>
        ///  A task that represents the asynchronous update click operation.</returns>
        private async Task btnUpdate_OnClick(WorkItemType datatem)
        {
            if(this.OnUpdate.HasDelegate)
            {
                await this.OnUpdate.InvokeAsync(datatem);
            }
        }

        #endregion event handlers

        #region private

        /// <summary>
        ///  Displays a confirmation dialog for an activity type action.
        /// </summary>
        /// <param name="message">
        ///  The confirmation message shown in the dialog.</param>
        /// <returns>
        ///  A task that resolves to <see langword="true"/> when confirmed, <see langword="false"/> 
        ///  when declined, or <see langword="null"/> when the dialog is dismissed.</returns>
        private Task<bool?> Confirm(string message)
        {
            return base.DialogSvc.Confirm(message, "Activity Type", new ConfirmOptions { OkButtonText = "Yes", CancelButtonText = "No" });
        }

        #endregion private

        #region public
        #endregion public
    }
}