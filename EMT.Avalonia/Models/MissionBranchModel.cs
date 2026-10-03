using CommunityToolkit.Mvvm.ComponentModel;
using EMT.Exceptions;
using Pdoxcl2Sharp;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace EMT.Models
{
    public partial class MissionBranchModel : ObservableObject, IParadoxRead, IParadoxWrite
    {
        [ObservableProperty]
        private string _name = "";

        [ObservableProperty]
        private int _slot = 1;

        [ObservableProperty]
        private bool _generic = false;

        [ObservableProperty]
        private bool _aI = true;

        [ObservableProperty]
        private bool _countryShield = true;

        [ObservableProperty]
        private GroupNodeModel _potential = (GroupNodeModel)DefaultPotential.Instance.Potential.Copy();

        [ObservableProperty]
        private GroupNodeModel? _potentialOnLoad;

        [ObservableProperty]
        private bool _isActive = true;

        public ObservableCollection<MissionModel> Missions { get; } = new ObservableCollection<MissionModel>();

        public MissionFileModel MissionFile { get; set; }

        public MissionBranchModel(MissionFileModel missionFile)
        {
            this.MissionFile = missionFile;
            Missions.CollectionChanged += Missions_CollectionChanged;
        }

        private void Missions_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // Keep back-reference correct whenever a mission is moved between branches
            if (e.NewItems != null)
            {
                foreach (MissionModel mission in e.NewItems)
                    mission.Branch = this;
            }
        }

        public void TokenCallback(ParadoxParser parser, string token)
        {
            try
            {
                switch (token)
                {
                    case "slot": Slot = parser.ReadInt32(); break;
                    case "generic": Generic = parser.ReadBool(); break;
                    case "ai": AI = parser.ReadBool(); break;
                    case "potential": Potential = parser.Parse(new GroupNodeModel() { Name = "potential" }); break;
                    case "potential_on_load": PotentialOnLoad = parser.Parse(new GroupNodeModel() { Name = "potential_on_load" }); break;
                    case "has_country_shield": CountryShield = parser.ReadBool(); break;
                    default: Missions.Add(parser.Parse(new MissionModel(this) { Name = token })); break;
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Mission branch exception, branch: {Name}  , token: {token} \n{e}");
            }
        }

        public void Write(ParadoxStreamWriter writer)
        {
            if (Slot <= 0)
                throw new WrongPositionException(string.Format("Slot must be greater than 0, branch: {0}", Name));

            writer.WriteLine("slot", Slot.ToString(), ValueWrite.LeadingTabs);
            writer.WriteLine("generic", BoolToString(Generic), ValueWrite.LeadingTabs);
            writer.WriteLine("ai", BoolToString(AI), ValueWrite.LeadingTabs);
            writer.WriteLine("has_country_shield", BoolToString(CountryShield), ValueWrite.LeadingTabs);

            Potential.Write(writer);
            if (PotentialOnLoad != null)
            {
                PotentialOnLoad.Write(writer);
            }

            writer.WriteLine();

            foreach (MissionModel mission in Missions)
            {
                if (String.IsNullOrWhiteSpace(mission.Name))
                    throw new MissionNameException(Name);

                writer.WriteLine(mission.Name + " = {", ValueWrite.LeadingTabs);
                mission.Write(writer);
                writer.WriteLine("}", ValueWrite.LeadingTabs);

                if (mission != Missions.Last())
                    writer.WriteLine();
            }
        }

        private static string BoolToString(bool b)
        {
            return b ? "yes" : "no";
        }
    }
}
