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
            object source = dropInfo.Data;
            object target = dropInfo.TargetItem;

            if (source == null || target == null || IsSelfOrChild(source, target))
                return;

            if (source is MissionBranchModel && target is MissionModel)
                return;

            dropInfo.Effects = System.Windows.DragDropEffects.Move;
            var isTreeViewItem = dropInfo.InsertPosition.HasFlag(RelativeInsertPosition.TargetItemCenter) && dropInfo.VisualTargetItem is TreeViewItem;

            if (source is MissionModel && target is MissionModel)
            {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;

            }

            else if (source is MissionModel && target is MissionBranchModel)
            {
                dropInfo.DropTargetAdorner = (isTreeViewItem) ? DropTargetAdorners.Highlight : DropTargetAdorners.Insert;

            }

            else if (source is MissionBranchModel && target is MissionBranchModel)
            {
                if (!isTreeViewItem)
                    dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;

            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            int insertIndex = GetInsertIndex(dropInfo);

            object source = dropInfo.Data;
            object target = dropInfo.TargetItem;

            if (source is MissionModel && target is MissionModel)
            {
                var dest = dropInfo.TargetCollection as BindableCollection<MissionModel>;
                
                if (dest == (source as MissionModel).Branch.Missions)
                {
                    int currentIndex = dest.IndexOf(source as MissionModel);
                    if (currentIndex < insertIndex)
                        insertIndex--;

                    if (insertIndex == dest.Count)
                        insertIndex--;

                    dest.Move(dest.IndexOf(source as MissionModel), insertIndex);
                }

                else
                {
                    if (dest == null)
                        dest = (target as MissionModel).Branch.Missions;

                    (source as MissionModel).Branch.Missions.Remove(source as MissionModel);
                    dest.Insert(insertIndex, source as MissionModel);
                }
            }

            else if (source is MissionModel && target is MissionBranchModel)
            {
                var sourceMission = source as MissionModel;
                if (target != sourceMission.Branch)
                {
                    sourceMission.Branch.Missions.Remove(sourceMission);
                    (target as MissionBranchModel).Missions.Add(sourceMission);
                }
            }

            else if (source is MissionBranchModel && target is MissionBranchModel)
            {
                var branches = (source as MissionBranchModel).MissionFile.Branches;

                if (dropInfo.TargetCollection is BindableCollection<MissionBranchModel>) //insert before
                {
                    if (branches.IndexOf(source as MissionBranchModel) < insertIndex)
                        insertIndex--;

                    branches.Move(branches.IndexOf(source as MissionBranchModel), insertIndex);
                }

                else
                {
                    insertIndex = branches.IndexOf(target as MissionBranchModel);

                    int currentIndex = branches.IndexOf(source as MissionBranchModel);
                    if (currentIndex < insertIndex)
                        insertIndex--;

                    if (insertIndex == branches.Count)
                        insertIndex--;

                    branches.Move(currentIndex, insertIndex);
                }
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

        private bool IsSelfOrChild(object source, object target)
        {
            if (source == target)
                return true;

            if (target is MissionBranchModel && source is MissionModel)
            {
                MissionBranchModel targetBranch = target as MissionBranchModel;
                MissionModel sourceMission = target as MissionModel;

                if (targetBranch.Missions.Contains(sourceMission))
                    return true;
            }

            return false;
        }
    }
}
