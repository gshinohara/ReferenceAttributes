using Eto.Forms;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using System.Collections.Generic;
using System.Linq;

namespace ReferenceAttributes.Utility
{
    internal class LayerSelector : Dialog<bool>
    {
        public List<Layer> Layers { get; } = new List<Layer>();

        public LayerSelector(LayerTable layers, bool multiple)
        {
            Title = multiple ? "Select Layers" : "Select Layer";
            Resizable = false;
            Padding = 5;

            DynamicLayout layout = new DynamicLayout
            {
                Padding = 10
            };

            TreeGridView treeGridView = new TreeGridView
            {
                AllowMultipleSelection = multiple,
            };
            GridColumn column = new GridColumn
            {
                HeaderText = "Layers",
                Editable = false,
                DataCell = new TextBoxCell(0)
            };
            treeGridView.Columns.Add(column);

            TreeGridItemCollection items = new TreeGridItemCollection();
            foreach(Layer layer in layers.OrderBy(l => l.SortIndex))
            {
                TreeGridItem item = new TreeGridItem
                {
                    Values = new string[] { layer.Name },
                    Tag = layer,
                    Expanded = true,
                };

                if (items.LastOrDefault() is TreeGridItem lastItem && lastItem.Tag is Layer parentLayer && layer.IsChildOf(parentLayer))
                {
                    TreeGridItem currentItem = lastItem;
                    while (true)
                    {
                        ITreeGridItem ichilditem = currentItem.Children.FirstOrDefault(_item => _item is TreeGridItem _childItem && _childItem.Tag is Layer _childLayer && layer.IsChildOf(_childLayer));
                        if (ichilditem is TreeGridItem childItem)
                            currentItem = childItem;
                        else
                            break;
                    }
                    currentItem.Children.Add(item);
                }
                else
                    items.Add(item);
            }

            treeGridView.DataStore = items;

            Button btn_ok = new Button { Text = "OK" };
            btn_ok.Click += (sender, e) =>
            {
                foreach (object obj in treeGridView.SelectedItems)
                {
                    if (obj is TreeGridItem item && item.Tag is Layer layer)
                        Layers.Add(layer);
                }
                Close(true);
            };
            Button btn_cancel = new Button { Text = "Cancel" };
            btn_cancel.Click += (sender, e) =>
            {
                Close(false);
            };

            layout.AddSeparateRow(treeGridView);
            layout.Add(null);
            layout.AddSeparateRow(null, btn_ok, btn_cancel, null);
            layout.Add(null);

            Content = layout;

            KeyDown += (sender, e) =>
            {
                if (e.Key == Keys.Escape)
                    Close(false);
            };
        }
    }
}