using Caliburn.Micro;
using Pdoxcl2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EMT.SharedData;
using EMT.Exceptions;

namespace EMT.Models
{
    public class MissionModel : PropertyChangedBase, IParadoxRead, IParadoxWrite
    {
        private string _name;
        private int _position = 1;
        private int _realPosition;
        private string _title = "";
        private string _description = "";
        private string _icon;
        private NodeModel _provincesToHighlight = new GroupNodeModel() { Name = "provinces_to_highlight" };
        private NodeModel _trigger = new GroupNodeModel() { Name = "trigger" };
        private NodeModel _effect = new GroupNodeModel() { Name = "effect" };

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                NotifyOfPropertyChange(() => Name);
                NotifyOfPropertyChange(() => TitleOrName);
            }
        }
        public int Position
        {
            get { return _position; }
            set
            {
                _position = value;
                NotifyOfPropertyChange(() => Position);
            }
        }
        public string Icon
        {
            get { return _icon; }
            set
            {
                _icon = value;
                NotifyOfPropertyChange(() => Icon);
                NotifyOfPropertyChange(() => IconPath);
            }
        }
        public BindableCollection<MissionModel> RequiredMissions { get; set; } = new BindableCollection<MissionModel>();
        public NodeModel ProvincesToHighlight
        {
            get { return _provincesToHighlight; }
            set
            {
                _provincesToHighlight = value;
                NotifyOfPropertyChange(() => ProvincesToHighlight);
            }
        }
        public NodeModel Trigger
        {
            get { return _trigger; }
            set
            {
                _trigger = value;
                NotifyOfPropertyChange(() => Trigger);
            }
        }
        public NodeModel Effect
        {
            get { return _effect; }
            set
            {
                _effect = value;
                NotifyOfPropertyChange(() => Effect);
            }
        }
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                NotifyOfPropertyChange(() => Title);
                NotifyOfPropertyChange(() => TitleOrName);
            }
        }
        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                NotifyOfPropertyChange(() => Description);
            }
        }
        public string IconPath
        {
            get
            {
                if (!String.IsNullOrWhiteSpace(Icon) && GfxStorage.Instance.GfxFiles.ContainsKey(Icon))
                    return GfxStorage.Instance.GfxFiles[Icon];
                return "";
            }
        }
        public string TitleOrName
        {
            get
            {
                if (String.IsNullOrWhiteSpace(Title))
                    return Name;
                return Title;
            }
        }
        public int RealPosition
        {
            get { return _realPosition; }
            set
            {
                _realPosition = value;
                NotifyOfPropertyChange(() => RealPosition);
            }
        }

        public MissionBranchModel Branch { get; set; }

        public MissionModel (MissionBranchModel branch)
        {
            this.Branch = branch;
        }

        public MissionModel()
        {

        }

        public void TokenCallback(ParadoxParser parser, string token)
        {
            try
            {
                switch (token)
                {
                    case "position": Position = parser.ReadInt32(); break;
                    case "icon": Icon = parser.ReadString(); break;
                    case "required_missions": RequiredMissions = new BindableCollection<MissionModel>(parser.ReadStringList().Select(name => new MissionModel() { Name = name })); break;
                    case "provinces_to_highlight": ProvincesToHighlight = parser.Parse(new GroupNodeModel() { Name = "provinces_to_highlight" }); break;
                    case "trigger": Trigger = parser.Parse(new GroupNodeModel() { Name = "trigger" }); break;
                    case "effect": Effect = parser.Parse(new GroupNodeModel() { Name = "effect" }); break;
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Mission exception, mission: {Name}  , token: {token} \n{e}");
            }
        }

        public void Write(ParadoxStreamWriter writer)
        {
            if (String.IsNullOrWhiteSpace(Icon))
                throw new IconException(Name);
            writer.WriteLine("icon", Icon, ValueWrite.LeadingTabs);

            if (Position <= 0)
                throw new WrongPositionException(string.Format("Position must be greater than 0, mission: {0}", Name));
            writer.WriteLine("position", Position.ToString(), ValueWrite.LeadingTabs);

            RequiredMissions = new BindableCollection<MissionModel>(RequiredMissions.Where(mission => !String.IsNullOrWhiteSpace(mission.Name)));
            if (RequiredMissions != null && RequiredMissions.Count > 0)
            {
                if (RequiredMissions.Count > 1)
                {
                    writer.WriteLine("required_missions = {", ValueWrite.LeadingTabs);
                    foreach (MissionModel required in RequiredMissions)
                    {
                        writer.WriteLine(required.Name, ValueWrite.LeadingTabs);
                    }
                    writer.WriteLine("}", ValueWrite.LeadingTabs);
                }

                else
                {
                    writer.WriteLine("required_missions = { " + RequiredMissions[0].Name + " } ", ValueWrite.LeadingTabs);
                }
            }

            if (ProvincesToHighlight != null)
            {
                ProvincesToHighlight.Write(writer);
                writer.WriteLine();

            }

            if (Trigger == null)
                Trigger = new GroupNodeModel() { Name = "trigger" };
            Trigger.Write(writer);
            writer.WriteLine();

            if (Effect == null)
                Effect = new GroupNodeModel() { Name = "effect" };
            Effect.Write(writer);
        }
    }
}
