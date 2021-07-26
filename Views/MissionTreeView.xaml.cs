using EMT.Converters;
using EMT.Models;
using EMT.SharedData;
using EMT.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EMT.Views
{
    /// <summary>
    /// Interaction logic for MissionTreeView.xaml
    /// </summary>
    public partial class MissionTreeView : UserControl
    {
        private double w = 104;
        private double h = 122;
        private float spaceHorizontal = 0;
        private float spaceVertical = 30;

        public MissionTreeView()
        {
            InitializeComponent();
        }

        public void UpdateMissionTree()
        {
            MissionFileModel missionFile = (DataContext as MissionTreeViewModel).MissionFile;
            MainGrid.Children.Clear();

            UpdateMissionPositions(missionFile);
            
            Dictionary<string, Tuple<double, double>> map = new Dictionary<string, Tuple<double, double>>();

            foreach (MissionBranchModel branch in missionFile.Branches)
            {
                if (branch.Missions.Count == 0 || !branch.IsActive)
                    continue;

                foreach (MissionModel mission in branch.Missions)
                {
                    MissionControl missionControl = new MissionControl();

                    missionControl.HorizontalAlignment = HorizontalAlignment.Left;
                    missionControl.VerticalAlignment = VerticalAlignment.Top;
                    missionControl.Margin = new Thickness( ((branch.Slot - 1) * (spaceHorizontal + w)),  ((mission.RealPosition - 1) * (spaceVertical + h)), 0, 0);

                    missionControl.DataContext = mission;
                    missionControl.MouseDown += ((sender, args) => { (DataContext as MissionTreeViewModel).SelectMission(mission); });

                    MainGrid.Children.Add(missionControl);
                    
                    if (!map.ContainsKey(mission.Name))
                        map.Add(mission.Name, new Tuple<double, double>(missionControl.Margin.Left, missionControl.Margin.Top));
                }

                //DrawBranchBorder(branch);
            }

            DrawArrows(missionFile, map);
        }

        private void UpdateMissionPositions(MissionFileModel missionFile)
        {
            Dictionary<string, Tuple<MissionModel, bool>> missions = new Dictionary<string, Tuple<MissionModel, bool>>();
            missionFile.Branches.Where(b => b.IsActive).SelectMany(branch => branch.Missions).ToList()
                .ForEach(mission => {
                    if (!missions.ContainsKey(mission.Name))
                        missions.Add(mission.Name, new Tuple<MissionModel, bool>(mission, false));
                });
                
            foreach (string mission in missions.Keys.ToList())
            {
                if (!missions[mission].Item2)
                {
                    RecalculateRealPosition(missions[mission].Item1, missions);
                }
            }
        }

        private void RecalculateRealPosition(MissionModel mission, Dictionary<string, Tuple<MissionModel, bool>> missions)
        {
            if (mission.RequiredMissions.Count == 0)
            {
                mission.RealPosition = mission.Position;
            }

            else
            {
                int max = mission.Position;
                foreach (MissionModel required in mission.RequiredMissions)
                {
                    if (missions.ContainsKey(required.Name))
                    {
                        if (!missions[required.Name].Item2)
                            RecalculateRealPosition(missions[required.Name].Item1, missions);

                        max = Math.Max(max, missions[required.Name].Item1.RealPosition + 1);
                    }
                }

                mission.RealPosition = max;
            }

            missions[mission.Name] = new Tuple<MissionModel, bool>(missions[mission.Name].Item1, true);
        }
    
        private void DrawBranchBorder(MissionBranchModel branch)
        {
            int missionCount = branch.Missions.Max(mission => mission.RealPosition) - branch.Missions.Min(mission => mission.RealPosition) + 1;

            Rectangle branchRectangle = new Rectangle();
            branchRectangle.HorizontalAlignment = HorizontalAlignment.Left;
            branchRectangle.VerticalAlignment = VerticalAlignment.Top;
            branchRectangle.Margin = new Thickness(((branch.Slot - 1) * (spaceHorizontal + w)), ((branch.Missions.Min(mission => mission.RealPosition) - 1) * (spaceVertical + h)), 0, 0);
            branchRectangle.Width = w + 2;
            branchRectangle.Height = missionCount * h + (missionCount - 1) * spaceVertical;
            branchRectangle.Stroke = Brushes.Red;
            branchRectangle.StrokeThickness = 2;
            MainGrid.Children.Add(branchRectangle);
        }

        private void DrawArrows(MissionFileModel missionFile, Dictionary<string, Tuple<double, double>> map)
        {
            foreach (MissionBranchModel branch in missionFile.Branches)
            {
                if (!branch.IsActive)
                    continue;

                foreach (MissionModel mission in branch.Missions)
                {
                    if (mission.RequiredMissions == null || mission.RequiredMissions.Count == 0)
                        continue;

                    foreach (MissionModel required in mission.RequiredMissions)
                    {
                        if (!map.ContainsKey(required.Name))
                            continue;

                        int branchSlot = missionFile.Branches.Where(b => b.Missions.Where(m => m.Name == required.Name).Any()).First().Slot;
                        int horizontalDiff = branch.Slot - branchSlot;
                        int verticalDiff = mission.RealPosition - missionFile.Branches.SelectMany(b => b.Missions).Where(m => m.Name == required.Name).First().RealPosition;

                        if (horizontalDiff < 0)
                        {
                            AddIcon("gfx_arrow_left_out", map[required.Name].Item1 + 4, map[required.Name].Item2 + 122);
                            for (int i = 0; i > horizontalDiff + 1; i--)
                            {
                                AddIcon("gfx_arrow_horizontal_skip_slot", map[required.Name].Item1 + (i-1) * w - 6, map[required.Name].Item2 + 127);
                            }

                            AddIcon("gfx_arrow_left_in", map[required.Name].Item1 + (w * horizontalDiff) + 69, map[required.Name].Item2 + 125 + (verticalDiff - 1) * (h + spaceVertical));
                            AddIcon("gfx_arrow_end", map[required.Name].Item1 + 61 + (w * horizontalDiff), map[required.Name].Item2 + 141 + (verticalDiff-1) * (h + spaceVertical));
                        }

                        else if (horizontalDiff > 0)
                        {
                            AddIcon("gfx_arrow_right_out", map[required.Name].Item1 + 60, map[required.Name].Item2 + 122);
                            for (int i = 0; i < horizontalDiff - 1; i++)
                            {
                                AddIcon("gfx_arrow_horizontal_skip_slot", map[required.Name].Item1 + (i + 1) * w - 6, map[required.Name].Item2 + 127);
                            }

                            AddIcon("gfx_arrow_right_in", map[required.Name].Item1 + (w * horizontalDiff) - 5, map[required.Name].Item2 + 127 + (verticalDiff-1) * (h + spaceVertical));
                            AddIcon("gfx_arrow_end", map[required.Name].Item1 + 15 + (w * horizontalDiff), map[required.Name].Item2 + 141 + (verticalDiff-1) * (h + spaceVertical));
                        }

                        else
                        {
                            AddIcon("gfx_arrow_verticall_tile", map[required.Name].Item1 + 46, map[required.Name].Item2 + 121);

                            for (int i = 0; i < verticalDiff - 1; i++)
                            {
                                AddIcon("gfx_arrow_verticall_skip_tier", map[required.Name].Item1 + 46, map[required.Name].Item2 + 121 + i * (h+spaceVertical));
                            }

                            if (verticalDiff > 1)
                                AddIcon("gfx_arrow_verticall_tile", map[required.Name].Item1 + 46, map[required.Name].Item2 + 121 + (verticalDiff - 1) * (h + spaceVertical));

                            AddIcon("gfx_arrow_end", map[required.Name].Item1 + 38, map[required.Name].Item2 + 141 + (verticalDiff-1) * (h + spaceVertical));
                        }
                    }
                }
            }
        }

        private void AddIcon(string icon, double x, double y)
        {
            Image arrow = new Image();
            arrow.Stretch = Stretch.None;
            arrow.HorizontalAlignment = HorizontalAlignment.Left;
            arrow.VerticalAlignment = VerticalAlignment.Top;
            arrow.Source = DDSConverter.Convert(GfxStorage.Instance.GfxFiles[icon]);
            arrow.Margin = new Thickness(x, y, 0, 0);
            arrow.SetValue(Panel.ZIndexProperty, -1);
            MainGrid.Children.Add(arrow);
        }
    }
}
