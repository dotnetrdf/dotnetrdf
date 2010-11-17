using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;

namespace rdfEditor
{
    /// <summary>
    /// Document Printing for AvalonEdit
    /// </summary>
    /// <remarks>
    /// <para>
    /// Code by Daniel Grunwald taken from an AvalonEdit <a href="http://community.sharpdevelop.net/forums/p/12012/32757.aspx#32757">forum post</a>.  Minor modifications for rdfEditor
    /// </para>
    /// </remarks>
    public static class DocumentPrinter
    {
        public static bool Print(TextEditor editor, String filename, bool withHighlighting)
        {
            PrintDialog dialog = new PrintDialog();

            //Create a FlowDocument and use it's paginator to
            FlowDocument doc = DocumentPrinter.CreateFlowDocumentForEditor(editor, withHighlighting);
            IDocumentPaginatorSource paginator = (IDocumentPaginatorSource)doc;
            dialog.MinPage = 1;
            paginator.DocumentPaginator.ComputePageCount();
            dialog.MaxPage = (uint)paginator.DocumentPaginator.PageCount;
            dialog.UserPageRangeEnabled = true;
            dialog.PageRange = new PageRange(1, paginator.DocumentPaginator.PageCount);

            if (dialog.ShowDialog() == true)
            {
                String descrip = "rdfEditor";
                if (filename != null) descrip += " - " + filename;
                dialog.PrintDocument(paginator.DocumentPaginator, descrip);
                return true;
            }
            else
            {
                return false;
            }
        }

        public static void QuickPrint(TextEditor editor, String filename, bool withHighlighting)
        {
            PrintDialog dialog = new PrintDialog();

            //Create a FlowDocument and use it's paginator to
            FlowDocument doc = DocumentPrinter.CreateFlowDocumentForEditor(editor, withHighlighting);
            IDocumentPaginatorSource paginator = (IDocumentPaginatorSource)doc;
            dialog.MinPage = 1;
            paginator.DocumentPaginator.ComputePageCount();
            dialog.MaxPage = (uint)paginator.DocumentPaginator.PageCount;
            dialog.UserPageRangeEnabled = true;
            dialog.PageRange = new PageRange(1, paginator.DocumentPaginator.PageCount);

            String descrip = "rdfEditor";
            if (filename != null) descrip += " - " + filename;
            dialog.PrintDocument(paginator.DocumentPaginator, descrip);
        }

        public static FlowDocument CreateFlowDocumentForEditor(TextEditor editor, bool withHighlighting)
        {
            IHighlighter highlighter = (withHighlighting) ? editor.TextArea.GetService(typeof(IHighlighter)) as IHighlighter : null;
            FlowDocument doc = new FlowDocument(ConvertTextDocumentToBlock(editor.Document, highlighter));
            doc.FontFamily = editor.FontFamily;
            doc.FontSize = editor.FontSize;
            doc.ColumnWidth = 40 * doc.FontSize;
            //doc.PageWidth = 21.0 * (96.0 / 2.54);
            //doc.PageHeight = 29.7 * (96.0 / 2.54);
            return doc;
        }


        public static Block ConvertTextDocumentToBlock(TextDocument document, IHighlighter highlighter)
        {
            if (document == null)
                throw new ArgumentNullException("document");
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
                        inlineBuilder.SetHighlighting(section.Offset - lineStartOffset, section.Length, section.Color);
                }
                p.Inlines.AddRange(inlineBuilder.CreateRuns());
                p.Inlines.Add(new LineBreak());
            }
            return p;
        }
    }
}
