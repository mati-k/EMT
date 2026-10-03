using System;
using System.Collections.Generic;

namespace EMT.Helpers.Script
{
    public class ScriptParseException : Exception
    {
        public ScriptParseException(string message, string text, int offset) : base($"{message} (line {LineOf(text, offset)})")
        {
        }

        private static int LineOf(string text, int offset)
        {
            int line = 1;
            for (int i = 0; i < offset && i < text.Length; i++)
            {
                if (text[i] == '\n')
                    line++;
            }

            return line;
        }
    }

    /// <summary>
    /// Parses Paradox script, recording where each entry is in the text. Comments are skipped,
    /// they stay in the text and move together with the entries around them.
    /// </summary>
    public class ScriptParser
    {
        private enum TokenKind { Word, String, Operator, Open, Close }

        private record Token(TokenKind Kind, string Text, int Start, int End);

        private readonly string _text;
        private readonly List<Token> _tokens;
        private int _position;

        private ScriptParser(string text)
        {
            _text = text;
            _tokens = Tokenize(text);
        }

        /// <summary>
        /// Parses whole text, returning root group spanning it.
        /// </summary>
        public static ScriptNode Parse(string text)
        {
            var parser = new ScriptParser(text);
            var root = new ScriptNode() { Children = [], OpenBrace = -1, CloseBrace = text.Length };
            parser.ParseBlock(root, isRoot: true);
            return root;
        }

        private Token? Peek() => _position < _tokens.Count ? _tokens[_position] : null;
        private Token Next() => _tokens[_position++];

        private void ParseBlock(ScriptNode group, bool isRoot)
        {
            while (true)
            {
                Token? token = Peek();

                if (token == null)
                {
                    if (!isRoot)
                        throw new ScriptParseException($"Missing closing brace for '{group.Name}'", _text, group.OpenBrace);
                    return;
                }

                if (token.Kind == TokenKind.Close)
                {
                    if (isRoot)
                        throw new ScriptParseException("Unexpected closing brace", _text, token.Start);

                    Next();
                    group.CloseBrace = token.Start;
                    return;
                }

                group.Children!.Add(ParseEntry());
            }
        }

        private ScriptNode ParseEntry()
        {
            Token first = Next();

            if (first.Kind == TokenKind.Open)
            {
                // Anonymous block, e.g. inside lists of blocks
                var anonymous = new ScriptNode() { Children = [], OpenBrace = first.Start };
                ParseBlock(anonymous, isRoot: false);
                return anonymous;
            }

            if (first.Kind == TokenKind.Operator)
                throw new ScriptParseException($"Unexpected '{first.Text}'", _text, first.Start);

            Token? op = Peek();
            if (op == null || op.Kind != TokenKind.Operator)
            {
                return new ScriptNode()
                {
                    Name = first.Text, NameStart = first.Start, NameEnd = first.End, IsBare = true,
                    IsQuoted = first.Kind == TokenKind.String, ValueStart = first.Start, ValueEnd = first.End,
                };
            }

            Next();
            Token value = Peek() ?? throw new ScriptParseException($"Missing value for '{first.Text}'", _text, op.Start);
            Next();

            // e.g. color = rgb { 1 2 3 }
            if (value.Kind == TokenKind.Word && Peek()?.Kind == TokenKind.Open)
                value = Next();

            switch (value.Kind)
            {
                case TokenKind.Open:
                    var group = new ScriptNode()
                    {
                        Name = first.Text, NameStart = first.Start, NameEnd = first.End, Operator = op.Text,
                        Children = [], OpenBrace = value.Start,
                    };
                    ParseBlock(group, isRoot: false);
                    return group;

                case TokenKind.Word:
                case TokenKind.String:
                    return new ScriptNode()
                    {
                        Name = first.Text, NameStart = first.Start, NameEnd = first.End, Operator = op.Text,
                        Value = value.Text, IsQuoted = value.Kind == TokenKind.String, ValueStart = value.Start, ValueEnd = value.End,
                    };

                default:
                    throw new ScriptParseException($"Unexpected '{value.Text}' after '{first.Text} {op.Text}'", _text, value.Start);
            }
        }

        private static bool IsOperatorChar(char c) => c is '=' or '<' or '>' or '!' or '?';

        private List<Token> Tokenize(string text)
        {
            List<Token> tokens = [];
            int i = 0;

            while (i < text.Length)
            {
                char c = text[i];

                if (char.IsWhiteSpace(c) || c == '﻿')
                {
                    i++;
                }
                else if (c == '#')
                {
                    while (i < text.Length && text[i] != '\n')
                        i++;
                }
                else if (c == '"')
                {
                    int start = i;
                    i++;
                    while (i < text.Length && text[i] != '"')
                    {
                        // Escaped quote doesn't end the string
                        if (text[i] == '\\' && i + 1 < text.Length)
                            i++;
                        i++;
                    }

                    if (i >= text.Length)
                        throw new ScriptParseException("Unclosed quote", text, start);

                    i++;
                    tokens.Add(new Token(TokenKind.String, text[(start + 1)..(i - 1)], start, i));
                }
                else if (c == '{')
                {
                    tokens.Add(new Token(TokenKind.Open, "{", i, i + 1));
                    i++;
                }
                else if (c == '}')
                {
                    tokens.Add(new Token(TokenKind.Close, "}", i, i + 1));
                    i++;
                }
                else if (IsOperatorChar(c) && (c is '=' or '<' or '>' || (i + 1 < text.Length && text[i + 1] == '=')))
                {
                    int start = i;
                    while (i < text.Length && IsOperatorChar(text[i]))
                        i++;

                    tokens.Add(new Token(TokenKind.Operator, text[start..i], start, i));
                }
                else
                {
                    int start = i;
                    while (i < text.Length)
                    {
                        char w = text[i];
                        if (char.IsWhiteSpace(w) || w is '{' or '}' or '#' or '"' or '=' or '<' or '>')
                            break;
                        if (w is '!' or '?' && i + 1 < text.Length && text[i + 1] == '=')
                            break;
                        i++;
                    }

                    tokens.Add(new Token(TokenKind.Word, text[start..i], start, i));
                }
            }

            return tokens;
        }
    }
}
