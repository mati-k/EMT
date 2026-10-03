using System;
using System.IO;
using System.Text;

namespace EMT.Helpers.Script
{
    /// <summary>
    /// Text of a file together with how it was stored, so it can be written back the same way.
    /// </summary>
    public class TextFile
    {
        private static readonly Encoding Windows1252;
        private static readonly UTF8Encoding StrictUtf8 = new(false, throwOnInvalidBytes: true);

        static TextFile()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Windows1252 = Encoding.GetEncoding(1252);
        }

        public string Text { get; init; } = "";
        public Encoding Encoding { get; init; } = new UTF8Encoding(true);
        public string NewLine { get; init; } = "\r\n";

        /// <summary>
        /// Reads a script file. Game expects Windows-1252, but files with UTF-8 BOM
        /// or valid UTF-8 characters are kept as UTF-8.
        /// </summary>
        public static TextFile ReadScript(string path) => Read(File.ReadAllBytes(path), utf8ByDefault: false);

        /// <summary>
        /// Reads a localisation file, which the game expects as UTF-8 with BOM.
        /// </summary>
        public static TextFile ReadLocalisation(string path) => Read(File.ReadAllBytes(path), utf8ByDefault: true);

        public static TextFile Read(byte[] bytes, bool utf8ByDefault)
        {
            Encoding encoding;
            string text;

            if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
            {
                encoding = new UTF8Encoding(true);
                text = encoding.GetString(bytes, 3, bytes.Length - 3);
            }
            else if (TryDecodeUtf8(bytes, out string utf8Text) && (utf8ByDefault || ContainsNonAscii(utf8Text)))
            {
                encoding = new UTF8Encoding(false);
                text = utf8Text;
            }
            else if (utf8ByDefault)
            {
                // Broken UTF-8, decode leniently
                encoding = new UTF8Encoding(true);
                text = encoding.GetString(bytes);
            }
            else
            {
                encoding = Windows1252;
                text = Windows1252.GetString(bytes);
            }

            return new TextFile
            {
                Text = text,
                Encoding = encoding,
                NewLine = text.Contains("\r\n") || !text.Contains('\n') ? "\r\n" : "\n",
            };
        }

        public static TextFile NewLocalisation() => new() { Encoding = new UTF8Encoding(true) };
        public static TextFile NewScript() => new() { Encoding = Windows1252 };

        /// <summary>
        /// Writes text in the original encoding. Line endings are written as given, new text should use <see cref="NewLine"/>.
        /// </summary>
        public void Write(Stream stream, string text)
        {
            byte[] preamble = Encoding.GetPreamble();
            stream.Write(preamble, 0, preamble.Length);
            byte[] bytes = Encoding.GetBytes(text);
            stream.Write(bytes, 0, bytes.Length);
        }

        private static bool TryDecodeUtf8(byte[] bytes, out string text)
        {
            try
            {
                text = StrictUtf8.GetString(bytes);
                return true;
            }
            catch (DecoderFallbackException)
            {
                text = "";
                return false;
            }
        }

        private static bool ContainsNonAscii(string text)
        {
            foreach (char c in text)
            {
                if (c > 127)
                    return true;
            }

            return false;
        }
    }
}
