using Microsoft.AspNetCore.Components;
using System.Text;

namespace TimeKeeper.App.Components.Layout;
/// <summary>
///  A Blazor component that manages the layout of a form element with an optional label.
///  Supports dynamic visibility, custom CSS classes, and inline styles for both the wrapper
///  and label. Provides flexible configuration for label text or template, position (Top, Bottom,
///  Left, Right), horizontal justification (Left, Center, Right), and vertical alignment (Top,
///  Center, Bottom). Also allows setting dimensions such as label width, overall width, and gap
///  between elements. Designed for creating consistent, customizable UI layouts.
/// </summary>
public partial class FormElementLayoutComponent : ComponentBase
{
        public enum LabelAign
        {
            Unset,
            Top,
            Center,
            Bottom
        }

        public enum LabelPositions
        {
            Unset,
            Bottom,
            Left,
            Right,
            Top
        }

        public enum LabelJustifies
        {
            Unset,
            Center,
            Left,
            Right
        }

        #region injected services
        #endregion injected services

        #region parameters

        /// <summary>
        /// Gets/Sets the template that defines the UI control to be rendered inside the layout.
        /// </summary>
        [Parameter]
        public RenderFragment? ControlTemplate { get; set; }

        /// <summary>
        /// Gets/Sets additional CSS classes applied to the control wrapper element.
        /// </summary>
        [Parameter]
        public string? ControlCssClass { get; set; }

        /// <summary>
        /// Gets/Sets CSS classes to be applied to the component wrapper.
        /// </summary>
        [Parameter]
        public string? CssClass { get; set; }

        /// <summary>
        /// Gets/Sets the spacing (CSS gap) between the label and the control. Default is "1rem".
        /// </summary>
        [Parameter]
        public string? Gap { get; set; } = "1rem";

        /// <summary>
        /// Gets/Sets the unique identifier applied to the wrapper div element.
        /// </summary>
        [Parameter]
        public string ID { get; set; } = "";

        /// <summary>
        /// Gets/Sets the flag which inidcates whether the component is visible. Default is true.
        /// </summary>
        [Parameter]
        public bool IsVisible { get; set; } = true;

        /// <summary>
        /// Gets/Sets text displayed in the label if no label template is provided.
        /// </summary>
        [Parameter]
        public string? Label { get; set; }

        /// <summary>
        /// Gets/Sets the vertical alignment of the label relative to the control (Top, Center, Bottom).
        /// </summary>
        [Parameter]
        public LabelAign LabelVertAlignment { get; set; } = LabelAign.Unset;

        /// <summary>
        /// Gets/Sets additional CSS classes applied to the label element.
        /// </summary>
        [Parameter]
        public string? LabelCssClass { get; set; }

        /// <summary>
        /// Gets/Sets the horizontal justification of the label (Left, Center, Right).
        /// </summary>
        [Parameter]
        public LabelJustifies LabelJustify { get; set; } = LabelJustifies.Left;

        /// <summary>
        /// Gets/Sets the position of the label relative to the control (Top, Bottom, Left, Right).
        /// </summary>
        [Parameter]
        public LabelPositions LabelPosition { get; set; } = LabelPositions.Left;

        /// <summary>
        /// Gets/Sets custom template for rendering the label content instead of plain text.
        /// </summary>
        [Parameter]
        public RenderFragment? LabelTemplate { get; set; }

        /// <summary>
        /// Gets/Sets width of the label element. Default is "20rem".
        /// </summary>
        [Parameter]
        public string? LabelWidth { get; set; } = "20rem";

        [Parameter]
        public string? Style { get; set; }

        /// <summary>
        /// Gets/Sets overall width of the wrapper element. Default is "100%".
        /// </summary>
        [Parameter]
        public string? Width { get; set; } = "100%";

        #endregion parameters

        #region properties

        protected bool EnableValidation { get; set; } = false;

        #endregion properties

        #region fields

        string controlWrapperCssClass = string.Empty;
        string labelCssClass = string.Empty;
        string labelInlineStyle = string.Empty;
        string wrapperCssClass = string.Empty;
        string wrapperInlineStyle = string.Empty;

        bool _isFirstLoad = true;

        #endregion fields

        #region events
        #endregion events

        #region data
        #endregion data

        #region lifecycle

        protected override async Task OnParametersSetAsync()
        {
            // Replace Task.WaitAll with await Task.WhenAll for browser compatibility
            await Task.WhenAll(
                this.SetControlWrapperCssClassList(),
                this.SetLabelCssClassList(),
                this.SetLabelInlineStyle(),
                this.SetWrapperCssClassList(),
                this.SetWrapperInlineStyle()
            );
        }

        protected override async Task OnInitializedAsync()
        {
            if (this._isFirstLoad)
            {
                this._isFirstLoad = false;
            }
        }

        #endregion lifecycle

        #region event handlers
        #endregion event handlers

        #region private

        /// <summary>
        ///  Sets the value to be applied to the Class attribute of the Div element that wraps the 
        ///  ControlTemplate
        /// </summary>
        /// <returns></returns>
        Task SetControlWrapperCssClassList()
        {
            return Task.Run(() =>
            {
                this.controlWrapperCssClass = $"cb-control-wrapper {this.ControlCssClass} k-w-full";
            });
        }

        /// <summary>
        /// Sets the value to be applied to the Class attribute of the Label element
        /// </summary>
        /// <returns></returns>
        Task SetLabelCssClassList()
        {
            return Task.Run(() =>
            {
                labelCssClass = $"cb-label {this.LabelCssClass}";
            });
        }

        /// <summary>
        ///  Sets the value to be applied to the Style attribute of the Label element
        /// </summary>
        /// <returns></returns>
        Task SetLabelInlineStyle()
        {
            return Task.Run(() =>
            {
                string labelWidth = $"width:{this.LabelWidth};";

                string result = $"{labelWidth}";
                labelInlineStyle = result;
            });
        }

        /// <summary>
        ///  Sets the value to be applied to the Class attribute of the Wrapper DIV element
        /// </summary>
        /// <returns></returns>
        Task SetWrapperCssClassList()
        {
            return Task.Run(() =>
            {
                StringBuilder ret = new StringBuilder($"cb-form-element-wrapper {this.CssClass}");

                switch (this.LabelPosition)
                {
                    case LabelPositions.Bottom:
                        ret.Append(" cb-label-position-bottom");
                        break;
                    case LabelPositions.Top:
                        ret.Append(" cb-label-position-top");
                        break;
                    case LabelPositions.Left:
                        ret.Append(" cb-label-position-left");
                        break;
                    case LabelPositions.Right:
                        ret.Append(" cb-label-position-right");
                        break;
                }

                switch (this.LabelJustify)
                {
                    case LabelJustifies.Center:
                        //ret.Append(" cb-label-justify-center");
                        ret.Append(" k-justify-items-center");
                        break;
                    case LabelJustifies.Left:
                        //ret.Append(" cb-label-justify-left");
                        ret.Append(" k-justify-items-left");
                        break;
                    case LabelJustifies.Right:
                        //ret.Append(" cb-label-justify-right");
                        ret.Append(" k-justify-items-right");
                        break;
                }

                switch (this.LabelVertAlignment)
                {
                    case LabelAign.Center:
                        ret.Append(" k-align-items-center");
                        break;
                    case LabelAign.Top:
                        ret.Append(" k-align-items-start");
                        break;
                    case LabelAign.Bottom:
                        ret.Append(" k-align-items-end");
                        break;
                }

                wrapperCssClass = ret.ToString();
            });
        }

        /// <summary>
        ///  Sets the value to be applied to the Style attribute of the Wrapper DIV element
        /// </summary>
        /// <returns></returns>
        Task SetWrapperInlineStyle()
        {
            return Task.Run(() =>
            {
                string gapStyle = $"gap:{this.Gap};";
                string labelWidth = $"width:{this.Width};";

                string result = $"{this.Style} {gapStyle} {labelWidth}";
                wrapperInlineStyle = result;
            });
        }

        #endregion private

        #region public

        /// <summary>
        ///  Represents configuration options for customizing layout and label alignment settings.
        /// </summary>
        /// <remarks>
        ///  Use this class to specify layout parameters such as gap size, label alignment, label
        ///  justification, label position, and width settings. These options can be used to control 
        ///  the appearance and alignment of UI components that support customizable layout and labeling.
        /// </remarks>
        public class Options
        {
            /// <summary>
            ///  Gets or sets the spacing between elements, typically specified as a CSS length value.
            /// </summary>
            public string Gap { get; set; } = ".5rem";

            /// <summary>
            ///  Gets or sets the unique identifier for the object.
            /// </summary>
            public string ID { get; set; }

            /// <summary>
            ///  Gets or sets the vertical alignment of the label within its container.
            /// </summary>
            public LabelAign LabelVertAlignment { get; set; } = LabelAign.Center;

            /// <summary>
            ///  Gets or sets the alignment used to justify label text within its container.
            /// </summary>
            public LabelJustifies LabelJustify { get; set; } = LabelJustifies.Left;

            /// <summary>
            ///  Gets or sets the position of the label relative to its associated control.
            /// </summary>
            /// <remarks>
            ///  Use this property to specify where the label should appear in relation to the control, 
            ///  such as to the left, right, above, or below. The default value is Left.
            /// </remarks>
            public LabelPositions LabelPosition { get; set; } = LabelPositions.Left;

            /// <summary>
            ///  Gets or sets the CSS width value applied to the label element.
            /// </summary>
            /// <remarks>
            ///  The value should be a valid CSS width, such as "20rem", "100px", or a percentage. If
            ///  not set, a default width may be applied by the component.
            /// </remarks>
            public string? LabelWidth { get; set; } = "20rem";

            /// <summary>
            ///  Gets or sets the width of the element as a CSS value.
            /// </summary>
            public string? Width { get; set; } = "100%";
        }

        #endregion public

        #region protected/overridable methods

        protected virtual dynamic? GetValidationFor()
        {
            return null;
        }

        #endregion protected/overridable methods
}