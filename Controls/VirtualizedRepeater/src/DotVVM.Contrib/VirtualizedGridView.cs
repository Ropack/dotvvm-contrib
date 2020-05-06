using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Binding.Properties;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Utils;

namespace DotVVM.Contrib
{
    [ControlMarkupOptions(AllowContent = false, DefaultContentProperty = nameof(Columns))]
    public sealed class VirtualizedGridView : ItemsControl
    {
        private EmptyData? emptyDataContainer;
        private HtmlGenericControl? head;

        public VirtualizedGridView() : base("div")
        {
            SetValue(Internal.IsNamingContainerProperty, true);

            Columns = new List<GridViewColumn>();

            if (GetType() == typeof(GridView))
                LifecycleRequirements &= ~(ControlLifecycleRequirements.InvokeMissingInit | ControlLifecycleRequirements.InvokeMissingLoad);
        }

        /// <summary>
        /// Gets or sets the template which will be displayed when the DataSource is empty.
        /// </summary>
        [MarkupOptions(MappingMode = MappingMode.InnerElement)]
        public ITemplate? EmptyDataTemplate
        {
            get => (ITemplate?) GetValue(EmptyDataTemplateProperty);
            set => SetValue(EmptyDataTemplateProperty, value);
        }

        public static readonly DotvvmProperty EmptyDataTemplateProperty =
            DotvvmProperty.Register<ITemplate?, VirtualizedGridView>(t => t.EmptyDataTemplate, null);


        /// <summary>
        /// Gets or sets a collection of columns that will be placed inside the grid.
        /// </summary>
        [MarkupOptions(AllowBinding = false, MappingMode = MappingMode.InnerElement)]
        [ControlPropertyBindingDataContextChange("DataSource")]
        [CollectionElementDataContextChange(1)]
        public List<GridViewColumn>? Columns
        {
            get => (List<GridViewColumn>?) GetValue(ColumnsProperty);
            set => SetValue(ColumnsProperty, value);
        }

        public static readonly DotvvmProperty ColumnsProperty =
            DotvvmProperty.Register<List<GridViewColumn>, VirtualizedGridView>(c => c.Columns);

        public int RowHeight
        {
            get => (int)GetValue(RowHeightProperty);
            set => SetValue(RowHeightProperty, value);
        }
        public static readonly DotvvmProperty RowHeightProperty
            = DotvvmProperty.Register<int, VirtualizedGridView>(c => c.RowHeight, 100);


        /// <summary>
        /// Gets or sets whether the header row should be displayed when the grid is empty.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public bool ShowHeaderWhenNoData
        {
            get { return (bool) GetValue(ShowHeaderWhenNoDataProperty)!; }
            set { SetValue(ShowHeaderWhenNoDataProperty, value); }
        }

        public static readonly DotvvmProperty ShowHeaderWhenNoDataProperty =
            DotvvmProperty.Register<bool, VirtualizedGridView>(t => t.ShowHeaderWhenNoData, false);


        protected override void OnLoad(IDotvvmRequestContext context)
        {
            DataBind(context);
            base.OnLoad(context);
        }

        protected override void OnPreRender(IDotvvmRequestContext context)
        {
            context.ResourceManager.AddRequiredResource("dotvvm.contrib.VirtualizedGridView");
            context.ResourceManager.AddRequiredResource("virtualizedForeach");

            DataBind(context);
            base.OnPreRender(context);
        }

        private void DataBind(IDotvvmRequestContext context)
        {
            Children.Clear();
            emptyDataContainer = null;
            head = null;

            var dataSourceBinding = GetDataSourceBinding();
            var dataSource = DataSource;

            CreateHeaderRow();

            var index = 0;
            if (dataSource != null)
            {
                foreach (var item in GetIEnumerableFromDataSource()!)
                {
                    // create row
                    var placeholder = new DataItemContainer {DataItemIndex = index};
                    placeholder.SetDataContextTypeFromDataSource(dataSourceBinding);
                    placeholder.DataContext = item;
                    placeholder.SetValue(Internal.PathFragmentProperty, GetPathFragmentExpression() + "/[" + index + "]");
                    placeholder.ID = index.ToString();
                    Children.Add(placeholder);
                    CreateRowWithCells(context, placeholder);

                    index++;
                }

            }

            // add empty item
            if (EmptyDataTemplate != null)
            {
                emptyDataContainer = new EmptyData();
                emptyDataContainer.SetValue(VisibleProperty, GetValueRaw(VisibleProperty));
                emptyDataContainer.SetBinding(DataSourceProperty, dataSourceBinding);
                EmptyDataTemplate.BuildContent(context, emptyDataContainer);
                Children.Add(emptyDataContainer);
            }
        }

        private void CreateHeaderRow()
        {
            head = new HtmlGenericControl("div");
            head.CssClasses.Add("virtualized-gridview-header", true);
            head.Attributes["style"] = "width: fit-content;";

            Children.Add(head);

            var headerRow = new HtmlGenericControl("div");
            headerRow.CssClasses.Add("virtualized-gridview-row", true);
            headerRow.Attributes["style"] = "display: flex;";
            head.Children.Add(headerRow);
            foreach (var column in Columns!)
            {
                var cell = new HtmlGenericControl("div");
                cell.CssClasses.Add("virtualized-gridview-cell", true);
                SetCellAttributes(column, cell, true);
                headerRow.Children.Add(cell);

                Literal literal = new Literal();
                literal.SetValue(Literal.TextProperty, column.GetValueRaw(GridViewColumn.HeaderTextProperty));
                cell.Children.Add(literal);
            }
        }

        private static void SetCellAttributes(GridViewColumn column, HtmlGenericControl cell, bool isHeaderCell)
        {
            if (!string.IsNullOrEmpty(column.Width))
            {
                cell.Attributes["style"] = "width: " + column.Width;
            }

            if (!isHeaderCell)
            {
                var cssClassBinding = column.GetValueBinding(GridViewColumn.CssClassProperty);
                if (cssClassBinding != null)
                {
                    cell.Attributes["class"] = cssClassBinding;
                }
                else if (!string.IsNullOrWhiteSpace(column.CssClass))
                {
                    cell.Attributes["class"] = column.CssClass;
                }
            }
            else
            {
                if (column.IsPropertySet(GridViewColumn.VisibleProperty)) cell.SetValue(TableUtils.ColumnVisibleProperty, GridViewColumn.VisibleProperty.GetValue(column));
                if (column.IsPropertySet(GridViewColumn.HeaderCssClassProperty)) // transfer all bindings (even StaticValue), because column has wrong DataContext for them
                {
                    cell.Attributes["class"] = column.GetValueRaw(GridViewColumn.HeaderCssClassProperty);
                }
            }
        }

        private void CreateRowWithCells(IDotvvmRequestContext context, DataItemContainer placeholder)
        {
            var row = CreateRow(placeholder);

            // create cells
            foreach (var column in Columns!)
            {
                var cell = new HtmlGenericControl("div");
                cell.CssClasses.Add("virtualized-gridview-cell", true);
                cell.SetValue(Internal.DataContextTypeProperty, column.GetValueRaw(Internal.DataContextTypeProperty));
                SetCellAttributes(column, cell, false);
                row.Children.Add(cell);

                column.CreateControls(context, cell);
            }
        }

        private HtmlGenericControl CreateRow(DataItemContainer placeholder)
        {
            var row = new HtmlGenericControl("div");
            row.Attributes["style"] = $"display: flex; height: {RowHeight}px;";
            row.CssClasses.Add("virtualized-gridview-row", true);
            placeholder.Children.Add(row);
            return row;
        }

        protected override void RenderContents(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            // render the header
            head?.Render(writer, context);

            // render body
            writer.AddKnockoutDataBind("virtualized-foreach","dotvvm.evaluator.getDataSourceItems($gridViewDataSet)");

            var (optionsBindingName, optionsBindingValue) = GetForeachOptionsKnockoutBindingGroup();
            writer.AddKnockoutDataBind(optionsBindingName, optionsBindingValue);

            writer.AddAttribute("class", "virtualized-gridview-body", true, ",");
            writer.AddAttribute("style", "width: fit-content;");
            writer.RenderBeginTag("div");
            
            // render contents
            var placeholder = new DataItemContainer {DataContext = null};
            placeholder.SetDataContextTypeFromDataSource(GetBinding(DataSourceProperty));
            placeholder.SetValue(Internal.PathFragmentProperty, GetPathFragmentExpression() + "/[$index]");
            placeholder.SetValue(Internal.ClientIDFragmentProperty, GetValueRaw(Internal.CurrentIndexBindingProperty));
            Children.Add(placeholder);
            CreateRowWithCells(context, placeholder);
            placeholder.Render(writer, context);

            writer.RenderEndTag();
        }

        protected override void RenderBeginTag(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            if (!ShowHeaderWhenNoData)
            {
                writer.WriteKnockoutDataBindComment("if",
                    GetForeachDataBindExpression().GetProperty<DataSourceLengthBinding>().Binding.CastTo<IValueBinding>().GetKnockoutBindingExpression(this));
            }

            base.RenderBeginTag(writer, context);
        }

        protected override void RenderEndTag(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            base.RenderEndTag(writer, context);

            if (!ShowHeaderWhenNoData)
            {
                writer.WriteKnockoutDataBindEndComment();
            }

            emptyDataContainer?.Render(writer, context);
        }

        private (string bindingName, KnockoutBindingGroup bindingValue) GetForeachOptionsKnockoutBindingGroup()
        {
            var value = new KnockoutBindingGroup();

            value.Add("elementSize", RowHeight.ToString());
            value.Add("orientation", "'vertical'");

            return ("virtualized-foreach-options", value);
        }

        protected override void AddAttributesToRender(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            writer.AddKnockoutDataBind("withGridViewDataSet", GetDataSourceBinding().GetKnockoutBindingExpression(this));
            base.AddAttributesToRender(writer, context);
        }

        public override IEnumerable<DotvvmBindableObject> GetLogicalChildren()
        {
            return base.GetLogicalChildren().Concat(Columns);
        }
    }
}