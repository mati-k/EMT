using Avalonia.Controls;
using Avalonia.Media;
using CommunityToolkit.Mvvm.DependencyInjection;
using EMT.Models;
using EMT.Services;
using EMT.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EMT.Views
{
    /// <summary>
    /// Preview of the mission tree, drawn the same way the game lays it out.
    /// </summary>
    public partial class MissionTreeView : UserControl
    {
        private const double w = 104;
        private const double h = 122;
        private const double spaceHorizontal = 0;
        private const double spaceVertical = 30;

        private MissionTreeViewModel? _viewModel;

        public MissionTreeView()
        {
            InitializeComponent();
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);

            if (_viewModel != null)
                _viewModel.TreeChanged -= UpdateMissionTree;

            _viewModel = DataContext as MissionTreeViewModel;

            if (_viewModel != null)
                _viewModel.TreeChanged += UpdateMissionTree;

            UpdateMissionTree();
        }

        public void UpdateMissionTree()
        {
            MainCanvas.Children.Clear();
            if (_viewModel == null)
                return;

            MissionFileModel missionFile = _viewModel.MissionFile;
            Dictionary<string, (double X, double Y)> map = new Dictionary<string, (double X, double Y)>();
            double maxX = 0, maxY = 0;

            foreach (MissionBranchModel branch in missionFile.Branches)
            {
                if (branch.Missions.Count == 0 || !branch.IsActive)
                    continue;

                foreach (MissionModel mission in branch.Missions)
                {
                    double x = (branch.Slot - 1) * (spaceHorizontal + w);
                    double y = (mission.RealPosition - 1) * (spaceVertical + h);

                    MissionControl missionControl = new MissionControl { DataContext = mission };
                    Canvas.SetLeft(missionControl, x);
                    Canvas.SetTop(missionControl, y);
                    missionControl.PointerPressed += (sender, args) => _viewModel.SelectMission(mission);
                    MainCanvas.Children.Add(missionControl);

                    map.TryAdd(mission.Name, (x, y));
                    maxX = Math.Max(maxX, x + w);
                    maxY = Math.Max(maxY, y + h);
                }
            }

            DrawArrows(missionFile, map);

            // Canvas doesn't measure its children, give the scroll viewer the real extent
            MainCanvas.Width = maxX;
            MainCanvas.Height = maxY;
        }

        private void DrawArrows(MissionFileModel missionFile, Dictionary<string, (double X, double Y)> map)
        {
            foreach (MissionBranchModel branch in missionFile.Branches)
            {
                if (!branch.IsActive)
                    continue;

                foreach (MissionModel mission in branch.Missions)
                {
                    foreach (MissionModel required in mission.RequiredMissions)
                    {
                        if (!map.TryGetValue(required.Name, out var from))
                            continue;

                        MissionModel requiredMission = missionFile.Branches.Where(b => b.IsActive).SelectMany(b => b.Missions).First(m => m.Name == required.Name);
                        int horizontalDiff = branch.Slot - requiredMission.Branch!.Slot;
                        int verticalDiff = mission.RealPosition - requiredMission.RealPosition;

                        if (horizontalDiff < 0)
                        {
                            AddIcon("gfx_arrow_left_out", from.X + 4, from.Y + 122);
                            for (int i = 0; i > horizontalDiff + 1; i--)
                            {
                                AddIcon("gfx_arrow_horizontal_skip_slot", from.X + (i - 1) * w - 6, from.Y + 127);
                            }

                            AddIcon("gfx_arrow_left_in", from.X + (w * horizontalDiff) + 69, from.Y + 125 + (verticalDiff - 1) * (h + spaceVertical));
                            AddIcon("gfx_arrow_end", from.X + 61 + (w * horizontalDiff), from.Y + 141 + (verticalDiff - 1) * (h + spaceVertical));
                        }

                        else if (horizontalDiff > 0)
                        {
                            AddIcon("gfx_arrow_right_out", from.X + 60, from.Y + 122);
                            for (int i = 0; i < horizontalDiff - 1; i++)
                            {
                                AddIcon("gfx_arrow_horizontal_skip_slot", from.X + (i + 1) * w - 6, from.Y + 127);
                            }

                            AddIcon("gfx_arrow_right_in", from.X + (w * horizontalDiff) - 5, from.Y + 127 + (verticalDiff - 1) * (h + spaceVertical));
                            AddIcon("gfx_arrow_end", from.X + 15 + (w * horizontalDiff), from.Y + 141 + (verticalDiff - 1) * (h + spaceVertical));
                        }

                        else
                        {
                            AddIcon("gfx_arrow_verticall_tile", from.X + 46, from.Y + 121);

                            for (int i = 0; i < verticalDiff - 1; i++)
                            {
                                AddIcon("gfx_arrow_verticall_skip_tier", from.X + 46, from.Y + 121 + i * (h + spaceVertical));
                            }

                            if (verticalDiff > 1)
                                AddIcon("gfx_arrow_verticall_tile", from.X + 46, from.Y + 121 + (verticalDiff - 1) * (h + spaceVertical));

                            AddIcon("gfx_arrow_end", from.X + 38, from.Y + 141 + (verticalDiff - 1) * (h + spaceVertical));
                        }
                    }
                }
            }
        }

        private void AddIcon(string icon, double x, double y)
        {
            var bitmap = Ioc.Default.GetService<IGfxService>()?.GetGfxBitmap(icon);
            if (bitmap == null)
                return;

            Image arrow = new Image
            {
                Stretch = Stretch.None,
                Source = bitmap,
                ZIndex = -1,
            };
            Canvas.SetLeft(arrow, x);
            Canvas.SetTop(arrow, y);
            MainCanvas.Children.Add(arrow);
        }
    }
}
