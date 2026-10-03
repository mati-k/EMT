using EMT.Helpers.Script;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace EMT.Models
{
    /// <summary>
    /// Localisation file edited line by line: only values of changed keys are touched,
    /// everything else (header, comments, other entries, version numbers) stays as it was.
    /// </summary>
    public partial class Localisation
    {
        // " key:0 "value"" with optional version number and any indentation
        [GeneratedRegex(@"^(?<prefix>\s+(?<key>[^\s:#""]+):(?<version>\d*)\s*"")")]
        private static partial Regex EntryStart();

        [GeneratedRegex(@"^\s*l_[A-Za-z_]+:\s*(#.*)?$")]
        private static partial Regex LanguageHeader();

        private readonly TextFile _file;

        // Lines keep their "\r", so unchanged lines are written back exactly
        private readonly List<string> _lines;
        private readonly string _lineEnd;
        private bool _endsWithNewLine;

        // Key to index of its first line
        private readonly Dictionary<string, int> _index = new();

        public static Localisation Load(string path) => new Localisation(TextFile.ReadLocalisation(path));

        public Localisation(TextFile file)
        {
            _file = file;
            _lineEnd = file.NewLine == "\r\n" ? "\r" : "";
            _lines = file.Text.Split('\n').ToList();

            // Text ending with newline gives empty last element, which Save adds back
            _endsWithNewLine = _lines.Count > 1 && _lines[^1].Length == 0;
            if (_lines[^1].Length == 0)
                _lines.RemoveAt(_lines.Count - 1);

            for (int i = 0; i < _lines.Count; i++)
            {
                if (TryParseEntry(_lines[i], out string key, out _, out _, out _))
                    _index.TryAdd(key, i);
            }
        }

        public IEnumerable<string> Keys => _index.Keys;

        public bool TryGet(string key, out string value)
        {
            value = "";
            return _index.TryGetValue(key, out int line) && TryParseEntry(_lines[line], out _, out value, out _, out _);
        }

        /// <summary>
        /// Updates value in place, or appends a new entry if key isn't in the file yet.
        /// </summary>
        public void Set(string key, string value)
        {
            if (_index.TryGetValue(key, out int line) && TryParseEntry(_lines[line], out _, out string current, out string prefix, out string suffix))
            {
                if (current != value)
                    _lines[line] = prefix + value + suffix;
                return;
            }

            if (!_lines.Any(l => LanguageHeader().IsMatch(l)))
            {
                _lines.Insert(0, "l_english:" + _lineEnd);
                foreach (string existing in _index.Keys.ToList())
                    _index[existing]++;
            }

            // Previous last line needs its line end now
            if (!_endsWithNewLine && _lines.Count > 1 && !_lines[^1].EndsWith('\r'))
                _lines[^1] += _lineEnd;
            _endsWithNewLine = true;

            _lines.Add($" {key}:0 \"{value}\"" + _lineEnd);
            _index[key] = _lines.Count - 1;
        }

        /// <summary>
        /// Renames key in place, if the new key isn't used yet.
        /// </summary>
        public void Rename(string oldKey, string newKey)
        {
            if (oldKey == newKey || _index.ContainsKey(newKey) || !_index.TryGetValue(oldKey, out int line))
                return;

            Match match = EntryStart().Match(_lines[line]);
            Group keyGroup = match.Groups["key"];
            _lines[line] = _lines[line][..keyGroup.Index] + newKey + _lines[line][(keyGroup.Index + keyGroup.Length)..];

            _index.Remove(oldKey);
            _index[newKey] = line;
        }

        public void Save(Stream stream)
        {
            _file.Write(stream, string.Join("\n", _lines) + (_endsWithNewLine ? "\n" : ""));
        }

        /// <summary>
        /// Splits entry line into key, value, text before value (up to opening quote) and after it (closing quote and comment).
        /// </summary>
        private static bool TryParseEntry(string line, out string key, out string value, out string prefix, out string suffix)
        {
            key = value = prefix = suffix = "";

            Match match = EntryStart().Match(line);
            if (!match.Success)
                return false;

            prefix = match.Groups["prefix"].Value;
            int valueStart = prefix.Length;

            // Value ends at the first quote followed only by whitespace or a comment, so quotes inside values are fine
            for (int i = valueStart; i < line.Length; i++)
            {
                if (line[i] != '"')
                    continue;

                string rest = line[(i + 1)..].TrimStart();
                if (rest.Length == 0 || rest.StartsWith('#'))
                {
                    key = match.Groups["key"].Value;
                    value = line[valueStart..i];
                    suffix = line[i..];
                    return true;
                }
            }

            return false;
        }
    }
}
