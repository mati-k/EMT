using DialogHostAvalonia;
using EMT.Models;
using Serilog;
using System;
using System.Threading.Tasks;

namespace EMT.Helpers
{
    /// <summary>
    /// Messages and questions shown over the main window.
    /// </summary>
    public static class Dialogs
    {
        public const string HostId = "MainDialogHost";

        public static async Task ShowInfo(string title, string text)
        {
            await Show(new InfoDialogData(title, text));
        }

        public static async Task ShowError(string title, string details)
        {
            await Show(new InfoDialogData(title, details + $"\n\nMore details in the log: {AppPaths.LogFolder}") { IsError = true });
        }

        /// <returns>True if the user confirmed</returns>
        public static async Task<bool> Confirm(string title, string text, string confirmText)
        {
            return await Show(new ConfirmDialogData(title, text, confirmText)) is true;
        }

        private static async Task<object?> Show(object content)
        {
            try
            {
                // Only one dialog can be open, the new one is more important
                if (DialogHost.IsDialogOpen(HostId))
                    DialogHost.Close(HostId);

                return await DialogHost.Show(content, HostId);
            }
            catch (Exception e)
            {
                Log.Error(e, "Showing dialog");
                return null;
            }
        }
    }
}
