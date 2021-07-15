using Caliburn.Micro;
using EMT.Models;
using GongSolutions.Wpf.DragDrop;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace EMT.Handlers
{
    public class DropTargetHandler : IDropTarget
    {
        public void DragOver(IDropInfo dropInfo)
        {
            NodeModel source = dropInfo.Data as NodeModel;
            NodeModel target = dropInfo.TargetItem as NodeModel;

            if (source != null && target != null && source.Root == target.Root)
            {
                dropInfo.Effects = System.Windows.DragDropEffects.Move;
                var isTreeViewItem = dropInfo.InsertPosition.HasFlag(RelativeInsertPosition.TargetItemCenter) && dropInfo.VisualTargetItem is TreeViewItem;
                dropInfo.DropTargetAdorner = (isTreeViewItem && (target is GroupNodeModel)) ? DropTargetAdorners.Highlight : DropTargetAdorners.Insert;
            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            NodeModel source = dropInfo.Data as NodeModel;
            NodeModel target = dropInfo.TargetItem as NodeModel;

            int insertIndex = GetInsertIndex(dropInfo);
            int index = source.Parent.Nodes.IndexOf(source);

            var desintation = dropInfo.TargetCollection as BindableCollection<NodeModel>;

            if (desintation != null)
            {
                if (source.Parent.Nodes == desintation)
                {
                    if (insertIndex == desintation.Count)
                        insertIndex--;

                    desintation.Move(index, insertIndex);
                }

                else
                {
                    source.Parent.Nodes.RemoveAt(index);
                    desintation.Insert(insertIndex, source);
                }
            }

            else
            {
                source.Parent.Nodes.RemoveAt(index);
                target.Parent.Nodes.Insert(insertIndex, source);
            }

            if (target is GroupNodeModel group && group.Nodes == desintation)
            {
                source.Parent = group;
            }

            else
            {
                source.Parent = target.Parent;
            }
        }

        protected int GetInsertIndex(IDropInfo dropInfo)
        {
            var insertIndex = dropInfo.UnfilteredInsertIndex;

            if (dropInfo.VisualTarget is ItemsControl itemsControl)
            {
                if (itemsControl.Items is IEditableCollectionView editableItems)
                {
                    var newItemPlaceholderPosition = editableItems.NewItemPlaceholderPosition;
                    if (newItemPlaceholderPosition == NewItemPlaceholderPosition.AtBeginning && insertIndex == 0)
                    {
                        ++insertIndex;
                    }
                    else if (newItemPlaceholderPosition == NewItemPlaceholderPosition.AtEnd && insertIndex == itemsControl.Items.Count)
                    {
                        --insertIndex;
                    }
                }
            }

            return insertIndex;
        }
    }
}
