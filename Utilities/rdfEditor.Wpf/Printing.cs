/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

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
    /// Provides Printing Support for AvalonEdit Text Editors
    /// </summary>
    /// <remarks>
    /// Based upon code by Vdue from an AvalonEdit related <a href="http://community.sharpdevelop.net/forums/p/12012/32756.aspx#32756">forum post</a>.  Heavily refactored to support printing in multi-document editors by Rob Vesse
    /// </remarks>
    public static class Printing
    {
        /// <summary>
        /// Invokes a Windows.Forms.PrintPreviewDialog.
        /// </summary>
        public static void PageSetupDialog(this TextEditor textEditor)
        {
            PrintSettings settings = textEditor.Tag as PrintSettings;
            if (settings == null)
            {
                settings = new PrintSettings();
                textEditor.Tag = settings;
            }

            settings.PageSettings.Landscape = (settings.PrintTicket.PageOrientation == PageOrientation.Landscape);

            PageSetupDialog setup = new PageSetupDialog();
            setup.EnableMetric = true;
            setup.PageSettings = settings.PageSettings;
            if (setup.ShowDialog() == DialogResult.OK)
            {
                settings.PageSettings = setup.PageSettings;
                settings.PrintTicket.PageOrientation = (settings.PageSettings.Landscape ? PageOrientation.Landscape : PageOrientation.Portrait);
                settings.PrintTicket.PageMediaSize = ConvertPaperSizeToMediaSize(settings.PageSettings.PaperSize);
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
            PrintSettings settings = textEditor.Tag as PrintSettings;
            if (settings == null)
            {
                settings = new PrintSettings();
                textEditor.Tag = settings;
            }

            settings.DocumentTitle = (title != null) ? title : String.Empty;
            PrintEngine.PrintPreviewDialog printPreview = new PrintEngine.PrintPreviewDialog();
            printPreview.DocumentViewer.FitToMaxPagesAcross(1);
            printPreview.DocumentViewer.PrintQueue = settings.PrintQueue;

            if (settings.PageSettings.Landscape)
            {
                settings.PrintTicket.PageOrientation = PageOrientation.Landscape;
            }

            printPreview.DocumentViewer.PrintTicket = settings.PrintTicket;
            printPreview.DocumentViewer.PrintQueue.DefaultPrintTicket.PageOrientation = settings.PrintTicket.PageOrientation;
            printPreview.LoadDocument(CreateDocumentPaginatorToPrint(textEditor, withHighlighting));
            
            // this is stupid, but must be done to view a whole page:
            DocumentViewer.FitToMaxPagesAcrossCommand.Execute("1", printPreview.DocumentViewer);

            // we never get a return code 'true', since we keep the DocumentViewer open, until user closes the window
            printPreview.ShowDialog();

            settings.PrintQueue = printPreview.DocumentViewer.PrintQueue;
            settings.PrintTicket = printPreview.DocumentViewer.PrintTicket;
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
            PrintSettings settings = textEditor.Tag as PrintSettings;
            if (settings == null)
            {
                settings = new PrintSettings();
                textEditor.Tag = settings;
            }

            settings.DocumentTitle = (title != null) ? title : String.Empty;
            System.Windows.Controls.PrintDialog printDialog = new System.Windows.Controls.PrintDialog();
            printDialog.PrintQueue = settings.PrintQueue;

            if (settings.PageSettings.Landscape)
            {
                settings.PrintTicket.PageOrientation = PageOrientation.Landscape;
            }

            printDialog.PrintTicket = settings.PrintTicket;
            printDialog.PrintQueue.DefaultPrintTicket.PageOrientation = settings.PrintTicket.PageOrientation;
            if (printDialog.ShowDialog() == true)
            {
                settings.PrintQueue = printDialog.PrintQueue;
                settings.PrintTicket = printDialog.PrintTicket;
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
            PrintSettings settings = textEditor.Tag as PrintSettings;
            if (settings == null)
            {
                settings = new PrintSettings();
                textEditor.Tag = settings;
            }

            settings.DocumentTitle = (title != null) ? title : String.Empty;
            System.Windows.Controls.PrintDialog printDialog = new System.Windows.Controls.PrintDialog();
            printDialog.PrintQueue = settings.PrintQueue;

            if (settings.PageSettings.Landscape)
            {
                settings.PrintTicket.PageOrientation = PageOrientation.Landscape;
            }

            printDialog.PrintTicket = settings.PrintTicket;
            printDialog.PrintQueue.DefaultPrintTicket.PageOrientation = settings.PrintTicket.PageOrientation;
            printDialog.PrintDocument(CreateDocumentPaginatorToPrint(textEditor, withHighlighting), "PrintDirectJob");
        }

        /// <summary>
        /// Creates a DocumentPaginatorWrapper from TextEditor text to print.
        /// </summary>
        static DocumentPaginatorWrapper CreateDocumentPaginatorToPrint(TextEditor textEditor, bool withHighlighting)
        {
            PrintSettings settings = textEditor.Tag as PrintSettings;
            if (settings == null)
            {
                settings = new PrintSettings();
                textEditor.Tag = settings;
            }

            // this baby adds headers and footers
            IDocumentPaginatorSource dps = CreateFlowDocumentToPrint(textEditor, withHighlighting);
            DocumentPaginatorWrapper dpw = new DocumentPaginatorWrapper(dps.DocumentPaginator, settings.PageSettings, settings.PrintTicket, textEditor.FontFamily);
            dpw.Title = settings.DocumentTitle;
            return dpw;
        }

        /// <summary>
        /// Creates a FlowDocument from TextEditor text to print.
        /// </summary>
        static FlowDocument CreateFlowDocumentToPrint(TextEditor textEditor, bool withHighlighting)
        {
            PrintSettings settings = textEditor.Tag as PrintSettings;
            if (settings == null)
            {
                settings = new PrintSettings();
                textEditor.Tag = settings;
            }

            // this baby has all settings to be printed or previewed in the PrintEngine.PrintPreviewDialog
            FlowDocument doc = CreateFlowDocumentForEditor(textEditor, withHighlighting);
            
            doc.ColumnWidth = settings.PageSettings.PrintableArea.Width;
            doc.PageHeight = (settings.PageSettings.Landscape ? (int)settings.PrintTicket.PageMediaSize.Width : (int)settings.PrintTicket.PageMediaSize.Height);
            doc.PageWidth = (settings.PageSettings.Landscape ? (int)settings.PrintTicket.PageMediaSize.Height : (int)settings.PrintTicket.PageMediaSize.Width);
            doc.PagePadding = ConvertPageMarginsToThickness(settings.PageSettings.Margins);
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

    /// <summary>
    /// Represents Print Settings
    /// </summary>
    class PrintSettings
    {
        public PrintSettings()
        {
            this.PrintQueue = LocalPrintServer.GetDefaultPrintQueue();
            this.PrintTicket = this.PrintQueue.DefaultPrintTicket;
            this.PageSettings = new PageSettings();
            this.PageSettings.Margins = new Margins(40, 40, 40, 40);
        }

        public PageSettings PageSettings
        {
            get;
            set;
        }

        public PrintQueue PrintQueue
        {
            get;
            set;
        }

        public PrintTicket PrintTicket
        {
            get;
            set;
        }

        public String DocumentTitle
        {
            get;
            set;
        }
    }
}