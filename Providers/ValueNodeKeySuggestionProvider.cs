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
    public class ValueNodeKeySuggestionProvider : ISuggestionProvider
    {
        private int limit = 10;
        public List<ValueNodeSuggestion> Suggestions { get; set; } = new List<ValueNodeSuggestion>();

        private static readonly ValueNodeKeySuggestionProvider _instance = new ValueNodeKeySuggestionProvider();
        public static ValueNodeKeySuggestionProvider Instance
        {
            get { return _instance; }
        }

        public IEnumerable GetSuggestions(string filter)
        {
            if (String.IsNullOrWhiteSpace(filter) || Suggestions.Count == 0)
                return null;

            return Suggestions.Where(suggestion => suggestion.Key.Contains(filter, StringComparison.OrdinalIgnoreCase) || suggestion.Localisation.Contains(filter, StringComparison.CurrentCultureIgnoreCase)).Take(limit);
        }
    }
}
