using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public static FlowDocument CreateFlowDocumentForEditor(TextEditor editor, bool withHighlighting)
        {
            IHighlighter highlighter = (withHighlighting) ? editor.TextArea.GetService(typeof(IHighlighter)) as IHighlighter : null;
            FlowDocument doc = new FlowDocument(ConvertTextDocumentToBlock(editor.Document, highlighter));
            doc.FontFamily = editor.FontFamily;
            doc.FontSize = editor.FontSize;
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
