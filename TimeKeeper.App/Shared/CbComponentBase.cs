using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;
using System.Text.Json;
using TimeKeeper.App.Api.Services;
using TimeKeeper.App.Shared.Layout;
using TimeKeeper.Domain.Services.Interfaces;

namespace TimeKeeper.App.Components;

public class CbComponentBase : ComponentBase
{
    #region injected Services

    /// <summary>
    ///  Gets or sets the dialog service used to display modal dialogs within the component.
    /// </summary>
    /// <remarks>
    ///  This property is typically injected by the framework and provides methods for showing
    ///  dialogs, alerts, or prompts to the user. Assigning a custom implementation allows for 
    ///  overriding dialog behavior.
    /// </remarks>
    [Inject]
    public DialogService DialogSvc { get; set; } = default!;

    /// <summary>
    /// Provides access to JavaScript interop functionality.
    /// </summary>
    [Inject]
    public IJSRuntime JSRuntime { get; set; } = default!;

    /// <summary>
    /// Provides access to laborer-related operations through dependency injection.
    /// </summary>
    [Inject]
    public ILaborerService LaborerService { get; private set; } = default!;

    /// <summary>
    /// Provides an instance of the NavigationManager for managing URI navigation and state.
    /// </summary>
    [Inject]
    public NavigationManager NavigationManager { get; set; } = default!;

    /// <summary>
    /// Provides access to the notification service instance.
    /// </summary>
    [Inject]
    public NotificationService NotificationSvc { get; set; } = default!;

    /// <summary>
    /// Provides access to project-related operations.
    /// </summary>
    [Inject]
    public IProjectService ProjectService { get; set; } = default!;

    /// <summary>
    /// Provides access to the current session service instance.
    /// </summary>
    [Inject]
    public SessionService SessionService { get; set; } = default!;

    /// <summary>
    /// Provides access to the time entry service instance.
    /// </summary>
    [Inject]
    public ITimeEntryService TimeEntrySvc { get; set; } = default!;

    /// <summary>
    /// Provides access to work item operations through dependency injection.
    /// </summary>
    [Inject]
    public IWorkItemService WorkItemSvc { get; set; } = default!;

    #endregion injected services

    #region parameters

    /// <summary>
    ///  Gets or sets the layout information for the current component as provided by the 
    ///  cascading parameter.
    /// </summary>
    /// <remarks>
    ///  This property receives its value from an ancestor component that provides the 'Layout' 
    ///  cascading parameter. It enables child components to access or modify layout-related 
    ///  settings or state shared across the component hierarchy.
    /// </remarks>
    [CascadingParameter(Name = "Layout")]
    public AppLayout? Layout { get; set; }

    #endregion parameters

    #region properties

    /// <summary>
    ///  Backing field for the DialogSettings poperty.
    /// </summary>
    DialogSettings? _dialogSettings = null;

    /// <summary>
    ///  Gets or sets the settings used to configure dialog behavior.
    /// </summary>
    /// <remarks>
    ///  Setting this property updates the dialog configuration and triggers an asynchronous
    ///  save of the current state. Changes take effect immediately for subsequent dialog 
    ///  operations.
    /// </remarks>
    public DialogSettings? DialogSettings
    {
        get { return this._dialogSettings; }
        set
        {
            this._dialogSettings = value;
            InvokeAsync(SaveStateAsync);
        }
    }

    #endregion properties

    #region public methods

    /// <summary>
    ///  Creates a new DialogOptions instance configured with the current dialog settings and specified 
    ///  resizable and draggable options.
    /// </summary>
    /// <param name="width"></param>
    /// <param name="isResizable">
    ///  A value indicating whether the dialog can be resized. Set to <see langword="true"/> to allow 
    ///  resizing; otherwise, <see langword="false"/>.
    /// </param>
    /// <param name="isDraggable">
    ///  A value indicating whether the dialog can be moved by dragging. Set to <see langword="true"/> 
    ///  to allow dragging; otherwise, <see langword="false"/>.
    /// </param>
    /// <returns>
    ///  A DialogOptions object containing the dialog's position, size, and the specified resizable 
    ///  and draggable settings.
    /// </returns>
    public DialogOptions GetDialogOptions(string width, string height, bool isResizable = true, bool isDraggable = true)
    {
        this.DialogSettings = new DialogSettings()
        {
            Height = height,
            Width = width,
            Left = $"calc(100vw - ({width} / 2))",
            Top = $"calc(100vh - ({height} / 2)"
        };

        var options = new DialogOptions()
        {
            Left        = this.DialogSettings?.Left,
            Top         = this.DialogSettings?.Top,
            Width       = this.DialogSettings?.Width,
            Height      = this.DialogSettings?.Height,
            Resizable   = isResizable,
            Draggable   = isDraggable,
            Resize      = this.Dialog_OnResize,
            Drag        = this.Dialog_OnDrag
        };

        return options;
    }

    /// <summary>
    ///  Asynchronously loads dialog settings from the browser's local storage.
    /// </summary>
    /// <remarks>
    ///  If dialog settings are present in local storage under the key "DialogSettings", they
    ///  are deserialized and loaded. If no settings are found, the current dialog settings 
    ///  remain unchanged.
    /// </remarks>
    /// <returns>
    ///  A task that represents the asynchronous load operation.
    /// </returns>
    public async Task LoadStateAsync()
    {
        await Task.CompletedTask;

        var result = await JSRuntime!.InvokeAsync<string>("window.localStorage.getItem", "DialogSettings");
        if (!string.IsNullOrEmpty(result))
        {
            this._dialogSettings = JsonSerializer.Deserialize<DialogSettings>(result);
        }
    }

    /// <summary>
    ///  Asynchronously saves the current dialog settings to the browser's local storage.
    /// </summary>
    /// <remarks>
    ///  This method serializes the current dialog settings and stores them under the key
    ///  "DialogSettings" in the browser's local storage. The operation completes when the data 
    ///  has been written. This method should be called from a Blazor component or service with 
    ///  access to the JSRuntime.
    /// </remarks>
    /// <returns>
    ///  A task that represents the asynchronous save operation.
    /// </returns>
    public async Task SaveStateAsync()
    {
        await Task.CompletedTask;

        await JSRuntime!.InvokeVoidAsync("window.localStorage.setItem", "DialogSettings", JsonSerializer.Serialize<DialogSettings>(this.DialogSettings));
    }

    #endregion public methods

    #region event handlers

    /// <summary>
    ///  Handles a drag event by updating the dialog's position based on the specified point.
    /// </summary>
    /// <remarks>
    ///  This method updates the dialog's position and persists the new state asynchronously. It also
    ///  logs the updated position to the browser console for debugging purposes.
    /// </remarks>
    /// <param name="point">
    ///  The new position of the dialog, represented as a <see cref="System.Drawing.Point"/> with 
    ///  X and Y coordinates in pixels.
    /// </param>
    public void Dialog_OnDrag(System.Drawing.Point point)
    {
        //this.JSRuntime!.InvokeVoidAsync("eval", $"console.log('Dialog drag. Left:{point.X}, Top:{point.Y}')");

        if (this.DialogSettings == null)
        {
            this.DialogSettings = new DialogSettings();
        }

        this.DialogSettings.Left = $"{point.X}px";
        this.DialogSettings.Top = $"{point.Y}px";

        InvokeAsync(SaveStateAsync);
    }

    /// <summary>
    ///  Handles a resize event by updating the dialog's dimensions to match the specified size.
    /// </summary>
    /// <remarks>
    ///  This method updates the dialog's width and height settings and persists the new state
    ///  asynchronously. It also logs the resize event to the browser console for debugging purposes.
    /// </remarks>
    /// <param name="size">
    ///  The new size of the dialog, specifying the width and height in pixels.
    /// </param>
    public void Dialog_OnResize(System.Drawing.Size size)
    {
        //this.JSRuntime!.InvokeVoidAsync("eval", $"console.log('Dialog resize. Width:{size.Width}, Height:{size.Height}')");

        if (this.DialogSettings == null)
        {
            this.DialogSettings = new DialogSettings();
        }

        this.DialogSettings.Width = $"{size.Width}px";
        this.DialogSettings.Height = $"{size.Height}px";

        InvokeAsync(SaveStateAsync);
    }

    #endregion event handlers

}


public class DialogSettings
{
    public string Left { get; set; } = string.Empty;
    public string Top { get; set; } = string.Empty;
    public string Width { get; set; } = string.Empty;
    public string Height { get; set; } = string.Empty;
}