using CommunityToolkit.Mvvm.ComponentModel;
using Pdoxcl2Sharp;
using System;
using System.Linq;

namespace EMT.Models
{
    public partial class ValueNodeModel : NodeModel
    {
        [ObservableProperty]
        private string _value = "";

        public override void TokenCallback(ParadoxParser parser, string token)
        {
        }

        public override bool SinglePath()
        {
            return true;
        }

        public override NodeModel Copy()
        {
            return new ValueNodeModel() { Name = this.Name, Value = this.Value };
        }

        public override void Write(ParadoxStreamWriter writer, ValueWrite valueWrite)
        {
            if (Name.Contains("name") || Name.Contains("has_dlc") || Value.Any(character => Char.IsWhiteSpace(character)))
                valueWrite = valueWrite | ValueWrite.Quoted;

            writer.Write(Name, Value, valueWrite);
        }
    }
}
