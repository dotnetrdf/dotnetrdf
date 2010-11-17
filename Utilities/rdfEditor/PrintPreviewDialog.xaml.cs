using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ICSharpCode.AvalonEdit;

namespace rdfEditor
{
    /// <summary>
    /// Interaction logic for PrintPreviewDialog.xaml
    /// </summary>
    public partial class PrintPreviewDialog : Window
    {
        private TextEditor _editor;
        private String _filename;

        public PrintPreviewDialog(TextEditor editor, String filename, bool withHighlighting)
        {
            InitializeComponent();

            this._filename = filename;
            if (filename != null) this.Title = "Print Preview - " + filename;

            this._editor = editor;

            this.docPreview.Document = DocumentPrinter.CreateFlowDocumentForEditor(editor, withHighlighting);
            this.chkWithHighlighting.IsChecked = withHighlighting;

            this.chkWithHighlighting.Checked += new RoutedEventHandler(chkWithHighlighting_Checked);
        }

        void chkWithHighlighting_Checked(object sender, RoutedEventArgs e)
        {
            this.docPreview.Document = DocumentPrinter.CreateFlowDocumentForEditor(this._editor, (bool)this.chkWithHighlighting.IsChecked);
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            if (DocumentPrinter.Print(this._editor, this._filename, (bool)this.chkWithHighlighting.IsChecked))
            {
                this.Close();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
