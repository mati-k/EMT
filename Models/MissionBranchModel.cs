using Caliburn.Micro;
using Pdoxcl2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMT.Models
{
    public class MissionBranchModel : PropertyChangedBase, IParadoxRead, IParadoxWrite
    {
        private string _name;
        private int _slot = 1;
        private bool _generic = false;
        private bool _ai = true;
        private bool _countryShield = true;
        private NodeModel _potential = new GroupNodeModel() { Name = "potential" };
        private bool _isActive = true;

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                NotifyOfPropertyChange(() => Name);
            }
        }
        public int Slot
        {
            get { return _slot; }
            set
            {
                _slot = value;
                NotifyOfPropertyChange(() => Slot);
            }
        }
        public bool Generic
        {
            get { return _generic; }
            set
            {
                _generic = value;
                NotifyOfPropertyChange(() => Generic);
            }
        }
        public bool AI
        {
            get { return _ai; }
            set
            {
                _ai = value;
                NotifyOfPropertyChange(() => AI);
            }
        }
        public bool CountryShield
        {
            get { return _countryShield; }
            set
            {
                _countryShield = value;
                NotifyOfPropertyChange(() => CountryShield);
            }
        }
        public NodeModel Potential
        {
            get { return _potential; }
            set 
            { 
                _potential = value;
                NotifyOfPropertyChange(() => Potential);
            }
        }
        public BindableCollection<MissionModel> Missions { get; set; } = new BindableCollection<MissionModel>();
        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                _isActive = value;
                NotifyOfPropertyChange(() => IsActive);
            }
        }

        public void TokenCallback(ParadoxParser parser, string token)
        {
            switch (token)
            {
                case "slot": Slot = parser.ReadInt32(); break;
                case "generic": Generic = parser.ReadBool(); break;
                case "ai": AI = parser.ReadBool(); break;
                case "potential": Potential = parser.Parse(new GroupNodeModel() { Name = "potential" }); break;
                case "has_country_shield": CountryShield = parser.ReadBool(); break;
                default: Missions.Add(parser.Parse(new MissionModel() { Name = token })); break;
            }
        }

        public void Write(ParadoxStreamWriter writer)
        {
            writer.WriteLine("slot", Slot.ToString(), ValueWrite.LeadingTabs);
            writer.WriteLine("generic", BoolToString(Generic), ValueWrite.LeadingTabs);
            writer.WriteLine("ai", BoolToString(AI), ValueWrite.LeadingTabs);
            writer.WriteLine("has_country_shield", BoolToString(CountryShield), ValueWrite.LeadingTabs);

            Potential.Write(writer);
            writer.WriteLine();

            foreach (MissionModel mission in Missions)
            {
                writer.WriteLine(mission.Name + " = {", ValueWrite.LeadingTabs);
                mission.Write(writer);
                writer.WriteLine("}", ValueWrite.LeadingTabs);

                if (mission != Missions.Last())
                    writer.WriteLine();
            }
        }

        private string BoolToString(bool b)
        {
            return b ? "yes" : "no";
        }
    }
}
