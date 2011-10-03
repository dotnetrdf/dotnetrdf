using System;
using System.Drawing.Printing;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Document;
using PrintEngine;


namespace ICSharpCode.AvalonEdit
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// This code by Vdue from an AvalonEdit related <a href="http://community.sharpdevelop.net/forums/p/12012/32756.aspx#32756">forum post</a>.  Minor adaptions and code style changes made by Rob Vesse
    /// </remarks>
    public static class Printing
    {
        private static PageSettings m_PageSettings;
        private static PrintQueue m_PrintQueue = LocalPrintServer.GetDefaultPrintQueue();
        private static PrintTicket m_PrintTicket = m_PrintQueue.DefaultPrintTicket;
        private static string m_DocumentTitle;

        /// <summary>
        /// Invokes a Windows.Forms.PrintPreviewDialog.
        /// </summary>
        public static void PageSetupDialog(this TextEditor textEditor)
        {
            PageSetupDialog();
        }

        /// <summary>
        /// Invokes a Windows.Forms.PrintPreviewDialog.
        /// </summary>
        public static void PageSetupDialog()
        {
            InitPageSettings();

            if (m_PrintTicket.PageOrientation == PageOrientation.Landscape)
            {
                m_PageSettings.Landscape = true;
            }
            else
            {
                m_PageSettings.Landscape = false;
            }

            PageSetupDialog setup = new PageSetupDialog();
            setup.EnableMetric = true;
            setup.PageSettings = m_PageSettings;
            if (setup.ShowDialog() == DialogResult.OK)
            {
                m_PageSettings = setup.PageSettings;
                m_PrintTicket.PageOrientation = (m_PageSettings.Landscape ? PageOrientation.Landscape : PageOrientation.Portrait);
                m_PrintTicket.PageMediaSize = ConvertPaperSizeToMediaSize(m_PageSettings.PaperSize);
            }
        }

        /// <summary>
        /// Invokes a PrintEngine.PrintPreviewDialog to print preview the TextEditor.Document.
        /// </summary>
        public static void PrintPreviewDialog(this TextEditor textEditor)
        {
            PrintPreviewDialog(textEditor, String.Empty);
        }

        /// <summary>
        /// Invokes a PrintEngine.PrintPreviewDialog to print preview the TextEditor.Document.
        /// </summary>
        public static void PrintPreviewDialog(this TextEditor textEditor, string title)
        {
            PrintPreviewDialog(textEditor, title, true);
        }

        /// <summary>
        /// Invokes a PrintEngine.PrintPreviewDialog to print preview the TextEditor.Document with specified title.
        /// </summary>
        public static void PrintPreviewDialog(this TextEditor textEditor, string title, bool withHighlighting)
        {
            m_DocumentTitle = (title != null) ? title : String.Empty;
            InitPageSettings();
            PrintEngine.PrintPreviewDialog printPreview = new PrintEngine.PrintPreviewDialog();
            printPreview.DocumentViewer.FitToMaxPagesAcross(1);
            printPreview.DocumentViewer.PrintQueue = m_PrintQueue;

            if (m_PageSettings.Landscape)
            {
                m_PrintTicket.PageOrientation = PageOrientation.Landscape;
            }

            printPreview.DocumentViewer.PrintTicket = m_PrintTicket;
            printPreview.DocumentViewer.PrintQueue.DefaultPrintTicket.PageOrientation = m_PrintTicket.PageOrientation;
            printPreview.LoadDocument(CreateDocumentPaginatorToPrint(textEditor, withHighlighting));
            
            // this is stupid, but must be done to view a whole page:
            DocumentViewer.FitToMaxPagesAcrossCommand.Execute("1", printPreview.DocumentViewer);

            // we never get a return code 'true', since we keep the DocumentViewer open, until user closes the window
            printPreview.ShowDialog();

            m_PrintQueue = printPreview.DocumentViewer.PrintQueue;
            m_PrintTicket = printPreview.DocumentViewer.PrintTicket;
        }

        /// <summary>
        /// Invokes a System.Windows.Controls.PrintDialog to print the TextEditor.Document.
        /// </summary>
        public static void PrintDialog(this TextEditor textEditor)
        {
            PrintDialog(textEditor, String.Empty);
        }

        /// <summary>
        /// Invokes a System.Windows.Controls.PrintDialog to print the TextEditor.Document.
        /// </summary>
        public static void PrintDialog(this TextEditor textEditor, String title)
        {
            PrintDialog(textEditor, title, true);
        }
        
        /// <summary>
        /// Invokes a System.Windows.Controls.PrintDialog to print the TextEditor.Document with specified title.
        /// </summary>
        public static void PrintDialog(this TextEditor textEditor, string title, bool withHighlighting)
        {
            m_DocumentTitle = (title != null) ? title : String.Empty;
            InitPageSettings();
            System.Windows.Controls.PrintDialog printDialog = new System.Windows.Controls.PrintDialog();
            printDialog.PrintQueue = m_PrintQueue;

            if (m_PageSettings.Landscape)
            {
                m_PrintTicket.PageOrientation = PageOrientation.Landscape;
            }

            printDialog.PrintTicket = m_PrintTicket;
            printDialog.PrintQueue.DefaultPrintTicket.PageOrientation = m_PrintTicket.PageOrientation;
            if (printDialog.ShowDialog() == true)
            {
                m_PrintQueue = printDialog.PrintQueue;
                m_PrintTicket = printDialog.PrintTicket;
                printDialog.PrintDocument(CreateDocumentPaginatorToPrint(textEditor, withHighlighting), "PrintJob");
            }
        }

        /// <summary>
        /// Prints the the TextEditor.Document to the current printer (no dialogs).
        /// </summary>
        public static void PrintDirect(this TextEditor textEditor)
        {
            PrintDirect(textEditor, String.Empty);
        }

        /// <summary>
        /// Prints the the TextEditor.Document to the current printer (no dialogs).
        /// </summary>
        public static void PrintDirect(this TextEditor textEditor, String title)
        {
            PrintDirect(textEditor, title, true);
        }
        
        /// <summary>
        /// Prints the the TextEditor.Document to the current printer (no dialogs) with specified title.
        /// </summary>
        public static void PrintDirect(this TextEditor textEditor, string title, bool withHighlighting)
        {
            m_DocumentTitle = (title != null) ? title : String.Empty;
            InitPageSettings();
            System.Windows.Controls.PrintDialog printDialog = new System.Windows.Controls.PrintDialog();
            printDialog.PrintQueue = m_PrintQueue;

            if (m_PageSettings.Landscape)
            {
                m_PrintTicket.PageOrientation = PageOrientation.Landscape;
            }

            printDialog.PrintTicket = m_PrintTicket;
            printDialog.PrintQueue.DefaultPrintTicket.PageOrientation = m_PrintTicket.PageOrientation;
            printDialog.PrintDocument(CreateDocumentPaginatorToPrint(textEditor, withHighlighting), "PrintDirectJob");
        }

        /// <summary>
        /// If not initialized, initialize a new instance of the PageSettings and sets the default margins.
        /// </summary>
        static void InitPageSettings()
        {
            if (m_PageSettings == null)
            {
                m_PageSettings = new PageSettings();
                m_PageSettings.Margins = new Margins(40, 40, 40, 40);
            }
        }

        /// <summary>
        /// Creates a DocumentPaginatorWrapper from TextEditor text to print.
        /// </summary>
        static DocumentPaginatorWrapper CreateDocumentPaginatorToPrint(TextEditor textEditor, bool withHighlighting)
        {
            // this baby adds headers and footers
            IDocumentPaginatorSource dps = CreateFlowDocumentToPrint(textEditor, withHighlighting);
            DocumentPaginatorWrapper dpw = new DocumentPaginatorWrapper(dps.DocumentPaginator, m_PageSettings, m_PrintTicket, textEditor.FontFamily);
            dpw.Title = m_DocumentTitle;
            return dpw;
        }

        /// <summary>
        /// Creates a FlowDocument from TextEditor text to print.
        /// </summary>
        static FlowDocument CreateFlowDocumentToPrint(TextEditor textEditor, bool withHighlighting)
        {
            // this baby has all settings to be printed or previewed in the PrintEngine.PrintPreviewDialog
            FlowDocument doc = CreateFlowDocumentForEditor(textEditor, withHighlighting);
            
            doc.ColumnWidth = m_PageSettings.PrintableArea.Width;
            doc.PageHeight = (m_PageSettings.Landscape ? (int)m_PrintTicket.PageMediaSize.Width : (int)m_PrintTicket.PageMediaSize.Height);
            doc.PageWidth = (m_PageSettings.Landscape ? (int)m_PrintTicket.PageMediaSize.Height : (int)m_PrintTicket.PageMediaSize.Width);
            doc.PagePadding = ConvertPageMarginsToThickness(m_PageSettings.Margins);
            doc.FontFamily = textEditor.FontFamily;
            doc.FontSize = textEditor.FontSize;
            
            return doc;
        }

        /// <summary>
        /// Creates a FlowDocument from TextEditor text.
        /// </summary>
        /// <remarks>
        /// This portion of code by Daniel Grunwald from an AvalonEdit related <a href="http://community.sharpdevelop.net/forums/t/12012.aspx">forum post</a>
        /// </remarks>
        static FlowDocument CreateFlowDocumentForEditor(TextEditor editor, bool withHighlighting)
        {
            IHighlighter highlighter = (withHighlighting) ? editor.TextArea.GetService(typeof(IHighlighter)) as IHighlighter : null;
            FlowDocument doc = new FlowDocument(ConvertTextDocumentToBlock(editor.Document, highlighter));
            return doc;
        }

        /// <summary>
        /// Converts a TextDocument to Block.
        /// </summary>
        /// <remarks>
        /// This portion of code by Daniel Grunwald from an AvalonEdit related <a href="http://community.sharpdevelop.net/forums/t/12012.aspx">forum post</a>
        /// </remarks>
        static Block ConvertTextDocumentToBlock(TextDocument document, IHighlighter highlighter)
        {
            if (document == null) throw new ArgumentNullException("document");
            Paragraph p = new Paragraph();

            foreach (DocumentLine line in document.Lines)
            {
                int lineNumber = line.LineNumber;
                HighlightedInlineBuilder inlineBuilder = new HighlightedInlineBuilder(document.GetText(line));
                if (highlighter != null)
                {
                    HighlightedLine highlightedLine = highlighter.HighlightLine(lineNumber);
                    int lineStartOffset = line.Offset;

                    foreach (HighlightedSection section in highlightedLine.Sections)
                    {
                        inlineBuilder.SetHighlighting(section.Offset - lineStartOffset, section.Length, section.Color);
                    }
                }
                p.Inlines.AddRange(inlineBuilder.CreateRuns());
                p.Inlines.Add(new LineBreak());
            }

            return p;
        }

        /// <summary>
        /// Converts PaperSize (hundredths of an inch) to PageMediaSize (px).
        /// </summary>
        static PageMediaSize ConvertPaperSizeToMediaSize(PaperSize paperSize)
        {
            return new PageMediaSize(ConvertToPx(paperSize.Width), ConvertToPx(paperSize.Height));
        }

        /// <summary>
        /// Converts specified Margins (hundredths of an inch) to Thickness (px).
        /// </summary>
        static Thickness ConvertPageMarginsToThickness(Margins margins)
        {
            Thickness thickness = new Thickness();
            thickness.Left = ConvertToPx(margins.Left);
            thickness.Top = ConvertToPx(margins.Top);
            thickness.Right = ConvertToPx(margins.Right);
            thickness.Bottom = ConvertToPx(margins.Bottom);
            return thickness;
        }
        
        /// <summary>
        /// Converts specified inch (hundredths of an inch) to pixels (px).
        /// </summary>
        static double ConvertToPx(double inch)
        {
            return inch * 0.96;
        }
    }
}