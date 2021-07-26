using AutoCompleteTextBox.Editors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using System.Collections;
using EMT.Models;
using EMT.Converters;
using EMT.CWToolsImplementation;

namespace EMT.Providers
{
    public class SuggestionProvider : ISuggestionProvider
    {
        private int limit = 10;
        private List<string> suggestions = new List<string>();

        private static readonly SuggestionProvider _instance = new SuggestionProvider();
        public static SuggestionProvider Instance
        {
            get { return _instance; }
        }

        public void SelectNode(NodeModel node)
        {
            suggestions = new List<string>();

            if (node == null)
                return;

            if (node.Root.Name.Equals("effect"))
                suggestions = ConfigStorage.Instance.ValueRules["effect"].Keys.ToList();
            else
                suggestions = ConfigStorage.Instance.ValueRules["trigger"].Keys.ToList();
        }

        public IEnumerable GetSuggestions(string filter)
        {
            if (String.IsNullOrWhiteSpace(filter) || suggestions.Count == 0)
                return null;

            return suggestions.Where(effect => effect.Contains(filter)).Take(limit);
        }
    }
}
