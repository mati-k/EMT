using EMT.Exceptions;
using EMT.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EMT.Helpers.Script
{
    /// <summary>
    /// Produces mission file text by rearranging the original text instead of regenerating it.
    /// Every branch and mission is a chunk of the original text (including comments right above it),
    /// chunks are put in the new order and only fields changed in the tool are patched in place.
    /// Everything the tool doesn't edit (triggers, effects, comments, formatting) is copied as is.
    /// </summary>
    public class MissionFileWriter
    {
        /// <summary>
        /// Part of text belonging to a branch or mission. Leading blank lines are kept apart: an item keeps
        /// its original spacing, except the one that was first, which had none and gets the usual separator.
        /// </summary>
        private record Chunk(int Start, int BodyStart, int End, bool WasFirst);

        /// <summary>
        /// Layout of a block containing items (root with branches, branch with missions).
        /// </summary>
        private class Container
        {
            public int PrefixEnd;
            public int SuffixStart;
            public int RegionEnd;
            public string Glue = "";
            public string Separator = "";
            public int ItemCount;
        }

        private readonly MissionFileModel _file;
        private readonly string _text;
        private readonly string _nl;
        private readonly Dictionary<ScriptNode, Chunk> _chunks = [];
        private readonly Dictionary<ScriptNode, Container> _containers = [];
        private readonly string _missionIndent;
        private readonly string _indentUnit;

        private MissionFileWriter(MissionFileModel file)
        {
            _file = file;
            _text = file.File.Text;
            _nl = file.File.NewLine;

            List<ScriptNode> branchNodes = file.Root.Children!.Where(MissionFileModel.IsBranchNode).ToList();
            _containers[file.Root] = Analyze(file.Root.Children!, MissionFileModel.IsBranchNode, 0, _text.Length);

            foreach (ScriptNode branch in branchNodes)
            {
                int regionStart = LineEndAfter(branch.OpenBrace + 1);
                int regionEnd = LineStartIfIndented(branch.CloseBrace);
                _containers[branch] = Analyze(branch.Children!, MissionBranchModel.IsMissionNode, regionStart, regionEnd);
            }

            ScriptNode? anyMission = branchNodes.SelectMany(branch => branch.Children!).FirstOrDefault(MissionBranchModel.IsMissionNode);
            _missionIndent = anyMission != null ? IndentOf(anyMission.Start) : "\t";
            _indentUnit = _missionIndent.StartsWith(' ') ? _missionIndent : "\t";
        }

        public static string Render(MissionFileModel file)
        {
            Validate(file);
            return new MissionFileWriter(file).Render();
        }

        private static void Validate(MissionFileModel file)
        {
            // Broken keys would make the file invalid for the game
            List<string> nameProblems = ScriptNames.FileProblems(file);
            if (nameProblems.Count > 0)
                throw new InvalidNameException(nameProblems);

            foreach (MissionBranchModel branch in file.Branches)
            {
                if (branch.Slot <= 0 && (branch.Source == null || branch.Slot != branch.SavedSlot))
                    throw new WrongPositionException($"Slot must be greater than 0, branch: {branch.Name}");

                foreach (MissionModel mission in branch.Missions)
                {
                    bool isNew = mission.Saved == null;
                    if (string.IsNullOrWhiteSpace(mission.Icon) && (isNew || mission.Icon != mission.Saved!.Icon))
                        throw new IconException(mission.Name);

                    if (mission.Position <= 0 && (isNew || mission.Position != mission.Saved!.Position))
                        throw new WrongPositionException($"Position must be greater than 0, mission: {mission.Name}");
                }
            }
        }

        private string Render()
        {
            Container root = _containers[_file.Root];
            StringBuilder output = new StringBuilder();

            output.Append(_text, 0, root.PrefixEnd).Append(root.Glue);
            AppendItems(output, root, _file.Branches.Select(branch => (branch.Source, (Func<Chunk?, string>)(chunk => RenderBranch(branch, chunk)))));
            output.Append(_text, root.SuffixStart, _text.Length - root.SuffixStart);

            return output.ToString();
        }

        /// <summary>
        /// Writes items in their new order, each separated the way it was in the file, or like its new neighbours.
        /// </summary>
        private void AppendItems(StringBuilder output, Container container, IEnumerable<(ScriptNode? Source, Func<Chunk?, string> Render)> items)
        {
            bool first = true;
            foreach (var (source, render) in items)
            {
                Chunk? chunk = source != null && _chunks.TryGetValue(source, out Chunk? found) ? found : null;

                if (!first)
                    output.Append(chunk != null && !chunk.WasFirst ? _text[chunk.Start..chunk.BodyStart] : container.Separator);
                else if (container.ItemCount == 0)
                    output.Append(_nl);

                output.Append(render(chunk));
                first = false;
            }
        }

        private string RenderBranch(MissionBranchModel branch, Chunk? chunk)
        {
            if (chunk == null || branch.Source == null)
                return NewBranch(branch);

            ScriptNode node = branch.Source;
            Container missions = _containers[node];

            List<Patch> patches = [];
            if (branch.Name != branch.SavedName)
                patches.Add(new Patch(node.NameStart, node.NameEnd, branch.Name));
            if (branch.Slot != branch.SavedSlot)
                patches.Add(SetValue(node, "slot", branch.Slot.ToString()));

            StringBuilder output = new StringBuilder();
            output.Append(ApplyPatches(chunk.BodyStart, missions.PrefixEnd, patches)).Append(missions.Glue);
            AppendItems(output, missions, branch.Missions.Select(mission => (mission.Source, (Func<Chunk?, string>)(chunk => RenderMission(mission, chunk)))));
            output.Append(_text, missions.SuffixStart, chunk.End - missions.SuffixStart);

            return output.ToString();
        }

        private string RenderMission(MissionModel mission, Chunk? chunk)
        {
            if (chunk == null || mission.Source == null || mission.Saved == null)
                return NewMission(mission, _missionIndent);

            ScriptNode node = mission.Source;
            MissionSnapshot saved = mission.Saved;
            List<Patch> patches = [];

            if (mission.Name != saved.Name)
                patches.Add(new Patch(node.NameStart, node.NameEnd, mission.Name));
            if (mission.Icon != saved.Icon)
                patches.Add(SetValue(node, "icon", mission.Icon));
            if (mission.Position != saved.Position)
                patches.Add(SetValue(node, "position", mission.Position.ToString()));

            List<string> required = mission.RequiredMissionNames();
            if (!required.SequenceEqual(saved.RequiredMissions))
            {
                string list = required.Count == 0 ? "{ }" : "{ " + string.Join(" ", required) + " }";
                if (node.Child("required_missions") is { IsGroup: true } existing)
                    patches.Add(new Patch(existing.OpenBrace, existing.CloseBrace + 1, list));
                else if (required.Count > 0)
                    patches.Add(InsertEntry(node, "required_missions = " + list));
            }

            return ApplyPatches(chunk.BodyStart, chunk.End, patches);
        }

        private string NewMission(MissionModel mission, string indent)
        {
            string inner = indent + _indentUnit;
            StringBuilder output = new StringBuilder();

            output.Append(indent).Append(mission.Name).Append(" = {").Append(_nl);
            output.Append(inner).Append("icon = ").Append(FormatValue(mission.Icon, false)).Append(_nl);
            output.Append(inner).Append("position = ").Append(mission.Position).Append(_nl);

            List<string> required = mission.RequiredMissionNames();
            if (required.Count > 0)
                output.Append(inner).Append("required_missions = { ").Append(string.Join(" ", required)).Append(" }").Append(_nl);

            output.Append(inner).Append("trigger = {").Append(_nl).Append(inner).Append('}').Append(_nl);
            output.Append(inner).Append("effect = {").Append(_nl).Append(inner).Append('}').Append(_nl);
            output.Append(indent).Append('}').Append(_nl);

            return output.ToString();
        }

        private string NewBranch(MissionBranchModel branch)
        {
            string unit = _indentUnit;
            StringBuilder output = new StringBuilder();

            output.Append(branch.Name).Append(" = {").Append(_nl);
            output.Append(unit).Append("slot = ").Append(branch.Slot).Append(_nl);
            output.Append(unit).Append("generic = no").Append(_nl);
            output.Append(unit).Append("ai = yes").Append(_nl);
            output.Append(unit).Append("has_country_shield = yes").Append(_nl);
            output.Append(unit).Append("potential = {").Append(_nl).Append(unit).Append('}').Append(_nl);

            foreach (MissionModel mission in branch.Missions)
            {
                output.Append(_nl);
                if (mission.Source != null && _chunks.TryGetValue(mission.Source, out Chunk? chunk))
                    output.Append(RenderMission(mission, chunk));
                else
                    output.Append(NewMission(mission, unit));
            }

            output.Append('}').Append(_nl);
            return output.ToString();
        }

        private record Patch(int Start, int End, string Text);

        /// <summary>
        /// Replaces value of a simple entry, or adds the entry if it isn't there.
        /// </summary>
        private Patch SetValue(ScriptNode node, string key, string value)
        {
            if (node.Child(key) is { IsGroup: false } existing)
                return new Patch(existing.ValueStart, existing.ValueEnd, FormatValue(value, existing.IsQuoted));

            return InsertEntry(node, key + " = " + FormatValue(value, false));
        }

        /// <summary>
        /// New entry goes on its own line right after the opening brace, indented like the other entries.
        /// </summary>
        private Patch InsertEntry(ScriptNode node, string entry)
        {
            int afterBrace = node.OpenBrace + 1;
            int lineEnd = LineEndAfter(afterBrace);

            // Block written on one line, keep it that way
            if (lineEnd == afterBrace)
                return new Patch(afterBrace, afterBrace, " " + entry);

            string indent = node.Children!.Count > 0 ? IndentOf(node.Children[0].Start) : IndentOf(node.Start) + _indentUnit;
            return new Patch(lineEnd, lineEnd, indent + entry + _nl);
        }

        private string ApplyPatches(int start, int end, List<Patch> patches)
        {
            StringBuilder output = new StringBuilder();
            int position = start;

            // Stable order keeps several insertions at the same place in the order they were added
            foreach (Patch patch in patches.OrderBy(patch => patch.Start))
            {
                output.Append(_text, position, patch.Start - position).Append(patch.Text);
                position = patch.End;
            }

            output.Append(_text, position, end - position);
            return output.ToString();
        }

        private static string FormatValue(string value, bool quoted)
        {
            bool needsQuotes = value.Length == 0 || value.Any(c => char.IsWhiteSpace(c) || c is '{' or '}' or '=' or '#' or '<' or '>');
            return quoted || needsQuotes ? "\"" + value + "\"" : value;
        }

        /// <summary>
        /// Splits a block into prefix, item chunks, glue and suffix. An item's chunk starts right after the
        /// previous item, so comments and spacing between two items move with the second one.
        /// The first item after non-item content only takes the comment lines directly above it.
        /// </summary>
        private Container Analyze(List<ScriptNode> children, Func<ScriptNode, bool> isItem, int regionStart, int regionEnd)
        {
            var container = new Container() { RegionEnd = regionEnd };
            int previousEnd = regionStart;
            bool previousWasItem = false;
            int? firstChunkStart = null;
            int? lastChunkEnd = null;
            StringBuilder glue = new StringBuilder();
            List<string> separators = [];

            foreach (ScriptNode child in children)
            {
                int childEnd = LineEndAfter(child.End);

                if (isItem(child))
                {
                    int start = previousWasItem ? previousEnd : AttachedCommentsStart(LineStartIfIndented(child.Start), previousEnd);

                    // Non-item content between items stays in the block, before the items
                    if (lastChunkEnd != null && !previousWasItem)
                        glue.Append(_text, lastChunkEnd.Value, start - lastChunkEnd.Value);

                    var chunk = new Chunk(start, SkipBlankLines(start, child.Start), childEnd, WasFirst: firstChunkStart == null);
                    _chunks[child] = chunk;
                    if (firstChunkStart != null)
                        separators.Add(_text[chunk.Start..chunk.BodyStart]);

                    firstChunkStart ??= start;
                    lastChunkEnd = childEnd;
                    container.ItemCount++;
                    previousWasItem = true;
                }
                else
                {
                    previousWasItem = false;
                }

                previousEnd = childEnd;
            }

            container.PrefixEnd = firstChunkStart ?? regionEnd;
            container.SuffixStart = lastChunkEnd ?? regionEnd;
            container.Glue = glue.ToString();

            // New neighbours are spaced like most items in this block, by default with a blank line
            container.Separator = separators.Count > 0
                ? separators.GroupBy(separator => separator).OrderByDescending(group => group.Count()).First().Key
                : _nl;

            return container;
        }

        /// <summary>
        /// Position after the end of line, if only whitespace or a comment follows <paramref name="position"/> on it.
        /// </summary>
        private int LineEndAfter(int position)
        {
            int i = position;
            while (i < _text.Length && _text[i] is ' ' or '\t' or '\r')
                i++;

            if (i < _text.Length && _text[i] == '#')
            {
                while (i < _text.Length && _text[i] != '\n')
                    i++;
            }

            if (i >= _text.Length)
                return _text.Length;

            return _text[i] == '\n' ? i + 1 : position;
        }

        /// <summary>
        /// Start of line if only indentation precedes <paramref name="position"/>.
        /// </summary>
        private int LineStartIfIndented(int position)
        {
            int i = position;
            while (i > 0 && _text[i - 1] is ' ' or '\t')
                i--;

            return i == 0 || _text[i - 1] == '\n' ? i : position;
        }

        private string IndentOf(int position)
        {
            int start = LineStartIfIndented(position);
            return _text[start..position];
        }

        /// <summary>
        /// Moves start up over comment lines directly above (no blank line in between), not past <paramref name="limit"/>.
        /// </summary>
        private int AttachedCommentsStart(int lineStart, int limit)
        {
            int start = lineStart;
            while (start > limit && start > 0)
            {
                int previousLineStart = start >= 2 ? _text.LastIndexOf('\n', start - 2) + 1 : 0;
                if (previousLineStart < limit)
                    break;

                string line = _text[previousLineStart..start].Trim();
                if (!line.StartsWith('#'))
                    break;

                start = previousLineStart;
            }

            return start;
        }

        /// <summary>
        /// Skips whitespace-only lines from <paramref name="start"/>, stopping at the line with content.
        /// </summary>
        private int SkipBlankLines(int start, int limit)
        {
            int position = start;
            while (position < limit)
            {
                int lineEnd = _text.IndexOf('\n', position);
                if (lineEnd < 0 || lineEnd >= limit || !string.IsNullOrWhiteSpace(_text[position..lineEnd]))
                    break;

                position = lineEnd + 1;
            }

            return position;
        }
    }
}
