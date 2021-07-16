using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace EMT.Views
{
    public class BindableRichTextBox : RichTextBox
    {
        public static readonly DependencyProperty BindableDocumentProperty = DependencyProperty.Register("BindableDocument", typeof(FlowDocument), typeof(BindableRichTextBox), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnBindableDocumentChanged)));
        public static readonly DependencyProperty TextAlignProperty = DependencyProperty.Register("TextAlign", typeof(TextAlignment), typeof(BindableRichTextBox), new FrameworkPropertyMetadata(TextAlignment.Justify, new PropertyChangedCallback(OnTextAlignmentChanged)));

        public FlowDocument BindableDocument
        {
            get
            {
                return (FlowDocument)this.GetValue(BindableDocumentProperty);
            }
            set
            {
                this.SetValue(BindableDocumentProperty, value);
            }
        }

        public TextAlignment TextAlign
        {
            get { return (TextAlignment)this.GetValue(TextAlignProperty); }
            set { this.SetValue(TextAlignProperty, value); }
        }

        public static void OnBindableDocumentChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            BindableRichTextBox rtb = (BindableRichTextBox)obj;
            rtb.Document = (FlowDocument)args.NewValue;
            rtb.Document.TextAlignment = rtb.TextAlign;
        }

        public static void OnTextAlignmentChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            BindableRichTextBox rtb = (BindableRichTextBox)obj;
            if (rtb.Document != null)
                rtb.Document.TextAlignment = (TextAlignment)args.NewValue;
        }
    }
}