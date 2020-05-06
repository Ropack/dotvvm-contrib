using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;

namespace DotVVM.Contrib
{
    /// <summary>
    /// Renders a client side repeater with virtualization support.
    /// </summary>
    [ControlMarkupOptions(AllowContent = false, DefaultContentProperty = nameof(ItemTemplate))]
    public sealed class VirtualizedRepeater : ItemsControl
    {
        private const string ClassSeparator = " ";

        private EmptyData? emptyDataContainer;
        private DotvvmControl? clientSideTemplate;

        public VirtualizedRepeater(bool allowImplicitLifecycleRequirements = true)
        {
            if (allowImplicitLifecycleRequirements)
            {
                SetValue(Internal.IsNamingContainerProperty, true);
                if (GetType() == typeof(Repeater))
                    LifecycleRequirements &= ~(ControlLifecycleRequirements.InvokeMissingInit | ControlLifecycleRequirements.InvokeMissingLoad);
            }
        }


        /// <summary>
        /// Occurs after the page commands are executed.
        /// </summary>
        protected override void OnPreRender(IDotvvmRequestContext context)
        {
            context.ResourceManager.AddRequiredResource("dotvvm.contrib.VirtualizedRepeater");
            context.ResourceManager.AddRequiredResource("virtualizedForeach");

            SetChildren(context, renderClientTemplate: !RenderOnServer, memoizeReferences: false);
            base.OnPreRender(context);
        }

        /// <summary>
        /// Gets or sets the template which will be displayed when the DataSource is empty.
        /// </summary>
        [MarkupOptions(AllowBinding = false, MappingMode = MappingMode.InnerElement)]
        public ITemplate? EmptyDataTemplate
        {
            get => (ITemplate?) GetValue(EmptyDataTemplateProperty);
            set => SetValue(EmptyDataTemplateProperty, value);
        }

        public static readonly DotvvmProperty EmptyDataTemplateProperty =
            DotvvmProperty.Register<ITemplate?, VirtualizedRepeater>(t => t.EmptyDataTemplate, null);

        /// <summary>
        /// Gets or sets the template for each Repeater item.
        /// </summary>
        [MarkupOptions(AllowBinding = false, MappingMode = MappingMode.InnerElement, Required = true)]
        [ControlPropertyBindingDataContextChange(nameof(DataSource))]
        [CollectionElementDataContextChange(1)]
        public ITemplate ItemTemplate
        {
            get => (ITemplate) GetValue(ItemTemplateProperty)!;
            set => SetValue(ItemTemplateProperty, value);
        }

        public static readonly DotvvmProperty ItemTemplateProperty =
            DotvvmProperty.Register<ITemplate, VirtualizedRepeater>(t => t.ItemTemplate);

        /// <summary>
        /// Gets or sets the template containing the elements that separate items.
        /// </summary>
        [MarkupOptions(AllowBinding = false, MappingMode = MappingMode.InnerElement)]
        public ITemplate? SeparatorTemplate
        {
            get => (ITemplate?) GetValue(SeparatorTemplateProperty);
            set => SetValue(SeparatorTemplateProperty, value);
        }

        public static readonly DotvvmProperty SeparatorTemplateProperty =
            DotvvmProperty.Register<ITemplate?, VirtualizedRepeater>(t => t.SeparatorTemplate, null);

        /// <summary>
        /// Gets or sets the name of the tag that wraps the Repeater.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public string WrapperTagName
        {
            get => (string) GetValue(WrapperTagNameProperty)!;
            set => SetValue(WrapperTagNameProperty, value);
        }

        public static readonly DotvvmProperty WrapperTagNameProperty =
            DotvvmProperty.Register<string, VirtualizedRepeater>(t => t.WrapperTagName, "div");

        /// <summary>
        /// Gets or sets if the repeater should use inline template (the default, traditional way of doing things) or if it should use Knockout's named template (with the template in &lt;script> tag).
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public bool RenderAsNamedTemplate
        {
            get => (bool) GetValue(RenderAsNamedTemplateProperty)!;
            set => SetValue(RenderAsNamedTemplateProperty, value);
        }

        public static readonly DotvvmProperty RenderAsNamedTemplateProperty =
            DotvvmProperty.Register<bool, VirtualizedRepeater>(nameof(RenderAsNamedTemplate), defaultValue: false);

        /// <summary>
        /// Gets or sets a direction in which are elements displayed. Default orientation is <c>Orientation.Vertical</c>.
        /// </summary>
        public OrientationMode Orientation
        {
            get => (OrientationMode) GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        public static readonly DotvvmProperty OrientationProperty
            = DotvvmProperty.Register<OrientationMode, VirtualizedRepeater>(c => c.Orientation, OrientationMode.Vertical);

        /// <summary>
        /// Gets or sets the size of one element in pixels.
        /// </summary>
        [MarkupOptions(Required = true)]
        public int ElementSize
        {
            get => (int) GetValue(ElementSizeProperty);
            set => SetValue(ElementSizeProperty, value);
        }

        public static readonly DotvvmProperty ElementSizeProperty
            = DotvvmProperty.Register<int, VirtualizedRepeater>(c => c.ElementSize, 100);

        protected override bool RendersHtmlTag => true;

        /// <summary>
        /// Occurs after the viewmodel is applied to the page and before the commands are executed.
        /// </summary>
        protected override void OnLoad(IDotvvmRequestContext context)
        {
            if (context.IsPostBack)
            {
                SetChildren(context, renderClientTemplate: false, memoizeReferences: true);
            }

            base.OnLoad(context);
        }

        protected override void RenderBeginTag(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            TagName = WrapperTagName;

            RenderContainerAttributes(writer);
            writer.RenderBeginTag(TagName);

            var (bindingName, bindingValue) = GetForeachKnockoutBindingGroup();
            var (optionsBindingName, optionsBindingValue) = GetForeachOptionsKnockoutBindingGroup();

            writer.AddKnockoutDataBind(bindingName, bindingValue);
            writer.AddKnockoutDataBind(optionsBindingName, optionsBindingValue);


            writer.RenderBeginTag(TagName);
        }

        private void RenderContainerAttributes(IHtmlWriter writer)
        {
            writer.AddAttribute("class", "virtualized-repeater-container", true, ClassSeparator);
            switch (Orientation)
            {
                case OrientationMode.Vertical:
                    writer.AddAttribute("class", "virtualized-repeater-vertical-container", true, ClassSeparator);
                    break;
                case OrientationMode.Horizontal:
                    writer.AddAttribute("class", "virtualized-repeater-horizontal-container", true, ClassSeparator);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(Orientation), $"Unknown value {Orientation} for property {nameof(Orientation)}.");
            }
        }

        private (string bindingName, KnockoutBindingGroup bindingValue) GetForeachOptionsKnockoutBindingGroup()
        {
            var value = new KnockoutBindingGroup();

            value.Add("elementSize", ElementSize.ToString());
            value.Add("orientation", $"'{Orientation.ToString().ToLower()}'");

            return ("virtualized-foreach-options", value);
        }

        private (string bindingName, KnockoutBindingGroup bindingValue) GetForeachKnockoutBindingGroup()
        {
            var value = new KnockoutBindingGroup();

            var javascriptDataSourceExpression = GetForeachDataBindExpression().GetKnockoutBindingExpression(this);
            value.Add("data", javascriptDataSourceExpression);

            return ("virtualized-foreach", value);
        }

        /// <summary>
        /// Renders the contents inside the control begin and end tags.
        /// </summary>
        protected override void RenderContents(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            RenderContentAttributes(writer);
            clientSideTemplate!.Render(writer, context);
        }

        private void RenderContentAttributes(IHtmlWriter writer)
        {
            switch (Orientation)
            {
                case OrientationMode.Vertical:
                    writer.AddAttribute("style", $"height: {ElementSize}px;");
                    break;
                case OrientationMode.Horizontal:
                    writer.AddAttribute("style", $"width: {ElementSize}px;");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(Orientation), $"Unknown value {Orientation} for property {nameof(Orientation)}.");
            }
        }

        protected override void RenderEndTag(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            base.RenderEndTag(writer, context);

            writer.RenderEndTag();

            emptyDataContainer?.Render(writer, context);
        }

        private DotvvmControl GetEmptyItem(IDotvvmRequestContext context)
        {
            if (emptyDataContainer == null)
            {
                var dataSourceBinding = GetDataSourceBinding();
                emptyDataContainer = new EmptyData();
                emptyDataContainer.SetValue(EmptyData.RenderWrapperTagProperty, true);
                emptyDataContainer.SetValue(EmptyData.WrapperTagNameProperty, GetValueRaw(WrapperTagNameProperty));
                emptyDataContainer.SetValue(EmptyData.VisibleProperty, GetValueRaw(VisibleProperty));
                emptyDataContainer.SetBinding(DataSourceProperty, dataSourceBinding);
                EmptyDataTemplate!.BuildContent(context, emptyDataContainer);
            }

            return emptyDataContainer;
        }

        private ConditionalWeakTable<object, DataItemContainer> childrenCache = new ConditionalWeakTable<object, DataItemContainer>();

        private DotvvmControl GetItem(IDotvvmRequestContext context, object? item = null, int? index = null, bool allowMemoizationRetrive = false,
            bool allowMemoizationStore = false)
        {
            if (allowMemoizationRetrive && item != null && childrenCache.TryGetValue(item, out var container2) && container2.Parent == null)
            {
                Debug.Assert(item == container2.GetValueRaw(DataContextProperty));
                SetUpServerItem(context, item, (int) index!, container2);
                return container2;
            }

            var container = new DataItemContainer();
            container.SetDataContextTypeFromDataSource(GetBinding(DataSourceProperty)!);
            if (item == null && index == null)
            {
                SetUpClientItem(container);
            }
            else
            {
                SetUpServerItem(context, item!, (int) index!, container);
            }

            ItemTemplate.BuildContent(context, container);

            // write it to the cache after the content is build. If it would be before that, exception could be suppressed
            if (allowMemoizationStore && item != null)
            {
                // this GetValue call adds the value without raising exception when the value is already added...
                childrenCache.GetValue(item, _ => container);
            }

            return container;
        }

        private DotvvmControl GetSeparator(IDotvvmRequestContext context)
        {
            var placeholder = new PlaceHolder();
            placeholder.SetDataContextType(this.GetDataContextType());
            SeparatorTemplate!.BuildContent(context, placeholder);
            return placeholder;
        }

        /// <summary>
        /// Performs the data-binding and builds the controls inside the <see cref="Repeater"/>.
        /// </summary>
        private void SetChildren(IDotvvmRequestContext context, bool renderClientTemplate, bool memoizeReferences)
        {
            Children.Clear();
            emptyDataContainer = null;
            clientSideTemplate = null;

            if (DataSource != null)
            {
                var index = 0;
                foreach (var item in GetIEnumerableFromDataSource()!)
                {
                    if (SeparatorTemplate != null && index > 0)
                    {
                        Children.Add(GetSeparator(context));
                    }

                    Children.Add(GetItem(context, item, index,
                        allowMemoizationRetrive: context.IsPostBack && !memoizeReferences, // on GET request we are not initializing the Repeater twice
                        allowMemoizationStore: memoizeReferences
                    ));
                    index++;
                }
            }

            if (renderClientTemplate)
            {
                if (SeparatorTemplate != null)
                {
                    Children.Add(GetSeparator(context));
                }

                Children.Add(clientSideTemplate = GetItem(context));
            }

            if (EmptyDataTemplate != null)
            {
                Children.Add(GetEmptyItem(context));
            }
        }

        private void SetUpClientItem(DataItemContainer container)
        {
            container.DataContext = null;
            container.SetValue(Internal.PathFragmentProperty, GetPathFragmentExpression() + "/[$index]");
            container.SetValue(Internal.ClientIDFragmentProperty, GetValueRaw(Internal.CurrentIndexBindingProperty));
        }

        private void SetUpServerItem(IDotvvmRequestContext context, object item, int index, DataItemContainer container)
        {
            container.DataItemIndex = index;
            container.DataContext = item;
            container.SetValue(Internal.PathFragmentProperty, GetPathFragmentExpression() + "/[" + index + "]");
            container.ID = index.ToString();
        }

        public enum OrientationMode
        {
            Vertical = 1,
            Horizontal = 2
        }
    }
}