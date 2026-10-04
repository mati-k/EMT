using EMT.Helpers.Script;
using System;
using System.IO;

namespace EMT.Helpers
{
    /// <summary>
    /// Turns exceptions into messages telling the user where the problem is and what they can do about it.
    /// </summary>
    public static class ErrorText
    {
        /// <summary>
        /// Problem in a script file: file, line, column and the line itself.
        /// </summary>
        public static string ForScript(string path, string text, ScriptParseException e)
        {
            var (line, column, lineText) = Locate(text, e.Offset);
            return $"{path}\nLine {line}, column {column}: {e.Message}\n\n    {lineText}";
        }

        public static (int Line, int Column, string LineText) Locate(string text, int offset)
        {
            offset = Math.Clamp(offset, 0, text.Length);
            int line = 1;
            int lineStart = 0;

            for (int i = 0; i < offset; i++)
            {
                if (text[i] == '\n')
                {
                    line++;
                    lineStart = i + 1;
                }
            }

            int lineEnd = text.IndexOf('\n', lineStart);
            if (lineEnd < 0)
                lineEnd = text.Length;

            return (line, offset - lineStart + 1, text[lineStart..lineEnd].Trim());
        }

        /// <summary>
        /// Problem with reading or writing a file or folder.
        /// </summary>
        public static string ForFile(string path, Exception e)
        {
            string reason = e switch
            {
                FileNotFoundException => "file doesn't exist, it may have been moved or deleted",
                DirectoryNotFoundException => "folder doesn't exist, it may have been moved or deleted",
                UnauthorizedAccessException => "no permission, the file may be read-only or need administrator rights",
                PathTooLongException => "path is too long",
                IOException io when IsLocked(io) => "file is open in another program, close it and try again",
                _ => e.Message,
            };

            return $"{path}\n{reason}";
        }

        private static bool IsLocked(IOException e)
        {
            // ERROR_SHARING_VIOLATION and ERROR_LOCK_VIOLATION on Windows
            int code = e.HResult & 0xFFFF;
            return code == 32 || code == 33;
        }
    }
}
