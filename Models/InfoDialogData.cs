namespace EMT.Models
{
    public class InfoDialogData
    {
        public string Title { get; }
        public string Text { get; }
        public bool IsError { get; init; }

        public InfoDialogData(string title, string text)
        {
            Title = title;
            Text = text;
        }
    }

    public class ConfirmDialogData
    {
        public string Title { get; }
        public string Text { get; }
        public string ConfirmText { get; }

        public ConfirmDialogData(string title, string text, string confirmText)
        {
            Title = title;
            Text = text;
            ConfirmText = confirmText;
        }
    }
}
