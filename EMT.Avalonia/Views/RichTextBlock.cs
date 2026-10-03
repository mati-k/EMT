using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Data;
using CommunityToolkit.Mvvm.DependencyInjection;
using EMT.Services;
using System;
using System.Collections.Generic;

namespace EMT.Views
{
    /// <summary>
    /// TextBlock rendering EU4 colour codes, e.g. "§Ygold§! text".
    /// </summary>
    public class RichTextBlock : TextBlock
    {
        public static readonly StyledProperty<string?> RichTextProperty =
            AvaloniaProperty.Register<RichTextBlock, string?>(nameof(RichText), defaultBindingMode: BindingMode.OneWay);

        public string? RichText
        {
            get => GetValue(RichTextProperty);
            set => SetValue(RichTextProperty, value);
        }

        protected override Type StyleKeyOverride => typeof(TextBlock);

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property != RichTextProperty || Inlines == null)
                return;

            Inlines.Clear();
            if (!string.IsNullOrEmpty(RichText))
            {
                var root = new Span();
                FormatText(root, RichText);
                Inlines.Add(root);
            }
        }

        private static void FormatText(Span root, string richText)
        {
            var gfxService = Ioc.Default.GetService<IGfxService>();

            string text = richText.Replace("\\n", Environment.NewLine);
            string[] formats = text.Split('§');

            Stack<char> formatCharacters = new Stack<char>();

            for (int i = 0; i < formats.Length; i++)
            {
                if (formats[i].Length == 0)
                    continue;

                Span span = new Span();
                root.Inlines.Add(span);

                if (i == 0)
                {
                    span.Inlines.Add(formats[i]);
                    continue;
                }

                if (formats[i][0] == '!' && formatCharacters.Count > 0)
                {
                    formatCharacters.Pop();
                }

                else
                {
                    formatCharacters.Push(formats[i][0]);
                }

                span.Inlines.Add(formats[i].Substring(1));

                if (formatCharacters.Count > 0 && gfxService?.GetColorForKey(formatCharacters.Peek()) is { } brush)
                {
                    span.Foreground = brush;
                }
            }
        }
    }
}
