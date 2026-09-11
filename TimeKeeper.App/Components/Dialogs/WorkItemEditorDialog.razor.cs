using Microsoft.AspNetCore.Components;
using TimeKeeper.App.Components.Forms;
using TimeKeeper.Domain.Models;

namespace TimeKeeper.App.Components.Dialogs;

public partial class WorkItemEditorDialog : CbComponentBase
{
    #region dependencies
    #endregion dependencies

    #region parameters

    [Parameter]
    public WorkItem? WorkItem { get; set; }

    #endregion parameters

    #region properties

    private WorkItemForm? WorkItemFormRef { get; set; }

    private bool SaveDisabled { get; set; }

    #endregion properties

    #region fields
    #endregion fields

    #region computed

    private string SaveButtonText => WorkItem?.IsNew == true ? "Create Work Item" : "Save Changes";

    #endregion computed

    #region lifecycle

    protected override void OnInitialized()
    {
        SaveDisabled = false;
    }

    #endregion lifecycle

    #region event handlers

    async Task btnSave_OnClick()
    {
        ArgumentNullException.ThrowIfNull(WorkItem);

        if (WorkItemFormRef == null || WorkItemFormRef.Validate() == false)
        {
            return;
        }

        if (WorkItem.IsNew)
        {
            WorkItem created =
                await WorkItemSvc!.AddWorkItem(WorkItem)
                ?? throw new Exception("Failed to create Work Item.");

            DialogSvc!.Close(created);
        }
        else
        {
            bool success =
                await WorkItemSvc!.UpdateWorkItem(WorkItem);

            if (!success)
            {
                throw new Exception("Failed to save Work Item.");
            }

            DialogSvc!.Close(WorkItem);
        }
    }

    void btnCancel_OnClick()
    {
        DialogSvc!.Close(null);
    }

    Task WorkItemForm_OnChanged()
    {
        SaveDisabled = false;
        return Task.CompletedTask;
    }

    #endregion event handlers
}