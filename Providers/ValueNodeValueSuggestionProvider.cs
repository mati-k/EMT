using AutoCompleteTextBox.Editors;
using EMT.CWToolsImplementation;
using EMT.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMT.Providers
{
    public class ValueNodeValueSuggestionProvider : ISuggestionProvider
    {
        private int limit = 10;
        public List<ValueNodeSuggestion> Suggestions { get; set; } = new List<ValueNodeSuggestion>();

        private static readonly ValueNodeValueSuggestionProvider _instance = new ValueNodeValueSuggestionProvider();
        public static ValueNodeValueSuggestionProvider Instance
        {
            get { return _instance; }
        }

        public IEnumerable GetSuggestions(string filter)
        {
            if (String.IsNullOrWhiteSpace(filter) || Suggestions.Count == 0)
                return null;

            return Suggestions.Where(suggestion => suggestion.Key.Contains(filter, StringComparison.CurrentCultureIgnoreCase) || suggestion.Localisation.Contains(filter, StringComparison.CurrentCultureIgnoreCase)).Take(limit);
        }
    }
}
