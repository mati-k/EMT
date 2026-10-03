using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using EMT.Models;
using EMT.ViewModels;

namespace EMT.Views
{
    public partial class EditorView : UserControl
    {
        private const double DragThreshold = 6;

        private EditorViewModel ViewModel => (EditorViewModel)DataContext!;

        // Drag only happens inside this tree, so the dragged item is kept here instead of in the drag data
        private object? _dragged;
        private PointerPressedEventArgs? _pressedArgs;
        private Point _pressedPoint;

        public EditorView()
        {
            InitializeComponent();

            BranchTree.AddHandler(PointerPressedEvent, Tree_PointerPressed, RoutingStrategies.Tunnel, handledEventsToo: true);
            BranchTree.AddHandler(PointerMovedEvent, Tree_PointerMoved, RoutingStrategies.Tunnel, handledEventsToo: true);
            BranchTree.AddHandler(PointerReleasedEvent, (_, _) => _pressedArgs = null, RoutingStrategies.Tunnel, handledEventsToo: true);
            DragDrop.AddDragOverHandler(BranchTree, Tree_DragOver);
            DragDrop.AddDropHandler(BranchTree, Tree_Drop);
        }

        private void AddMission_Click(object? sender, RoutedEventArgs e)
        {
            if ((sender as Control)?.DataContext is MissionBranchModel branch)
                ViewModel.AddMission(branch);
        }

        private void RemoveBranch_Click(object? sender, RoutedEventArgs e)
        {
            if ((sender as Control)?.DataContext is MissionBranchModel branch)
                ViewModel.RemoveBranch(branch);
        }

        private void RemoveMission_Click(object? sender, RoutedEventArgs e)
        {
            if ((sender as Control)?.DataContext is MissionModel mission)
                ViewModel.RemoveMission(mission);
        }

        private void Tree_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            _pressedArgs = null;
            if (!e.GetCurrentPoint(BranchTree).Properties.IsLeftButtonPressed || e.Source is not Visual source)
                return;

            // Let checkboxes and expanders work normally
            if (source.FindAncestorOfType<ToggleButton>(includeSelf: true) != null)
                return;

            var item = source.FindAncestorOfType<TreeViewItem>(includeSelf: true);
            if (item?.DataContext is MissionModel or MissionBranchModel)
            {
                _dragged = item.DataContext;
                _pressedArgs = e;
                _pressedPoint = e.GetPosition(BranchTree);
            }
        }

        private async void Tree_PointerMoved(object? sender, PointerEventArgs e)
        {
            if (_pressedArgs == null)
                return;

            Point delta = e.GetPosition(BranchTree) - _pressedPoint;
            if (System.Math.Abs(delta.X) < DragThreshold && System.Math.Abs(delta.Y) < DragThreshold)
                return;

            var pressedArgs = _pressedArgs;
            _pressedArgs = null;

            var data = new DataTransfer();
            data.Add(DataTransferItem.CreateText(_dragged?.ToString() ?? ""));
            await DragDrop.DoDragDropAsync(pressedArgs, data, DragDropEffects.Move);
            _dragged = null;
        }

        private void Tree_DragOver(object? sender, DragEventArgs e)
        {
            e.DragEffects = GetDropAction(e) != null ? DragDropEffects.Move : DragDropEffects.None;
        }

        private void Tree_Drop(object? sender, DragEventArgs e)
        {
            GetDropAction(e)?.Invoke();
            _dragged = null;
        }

        /// <summary>
        /// Works out what dropping the dragged item at the current position would do, null if not allowed.
        /// Dropping on upper half of a row inserts before it, lower half after it.
        /// </summary>
        private System.Action? GetDropAction(DragEventArgs e)
        {
            if (_dragged == null || e.Source is not Visual source)
                return null;

            var item = source.FindAncestorOfType<TreeViewItem>(includeSelf: true);
            object? target = item?.DataContext;

            if (target == null)
            {
                // Empty space below the tree, move branch to the end
                if (_dragged is MissionBranchModel draggedBranch)
                    return () => ViewModel.MoveBranch(draggedBranch, ViewModel.MissionFile.Branches.Count);

                return null;
            }

            if (target == _dragged)
                return null;

            bool after = IsLowerHalf(e, source, target);

            switch (_dragged, target)
            {
                case (MissionModel mission, MissionModel targetMission) when targetMission.Branch != null:
                    int missionIndex = targetMission.Branch.Missions.IndexOf(targetMission) + (after ? 1 : 0);
                    return () => ViewModel.MoveMission(mission, targetMission.Branch, missionIndex);

                case (MissionModel mission, MissionBranchModel targetBranch) when mission.Branch != targetBranch:
                    return () => ViewModel.MoveMission(mission, targetBranch, targetBranch.Missions.Count);

                case (MissionBranchModel branch, MissionBranchModel targetBranch):
                    int branchIndex = ViewModel.MissionFile.Branches.IndexOf(targetBranch) + (after ? 1 : 0);
                    return () => ViewModel.MoveBranch(branch, branchIndex);

                default:
                    return null;
            }
        }

        /// <summary>
        /// Checks position against the row itself, not the whole TreeViewItem which includes its children.
        /// </summary>
        private static bool IsLowerHalf(DragEventArgs e, Visual source, object target)
        {
            Control? row = source as Control ?? source.FindAncestorOfType<Control>();
            // Climb to the item template root, which sits directly in the header presenter
            while (row?.GetVisualParent() is Control parent && parent.DataContext == target
                && parent is not ContentPresenter && parent is not TreeViewItem)
            {
                row = parent;
            }

            if (row == null)
                return false;

            return e.GetPosition(row).Y > row.Bounds.Height / 2;
        }
    }
}
