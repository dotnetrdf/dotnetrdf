using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Rendering;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace rdfEditor.Syntax
{
    public class ValidationErrorElementGenerator : VisualLineElementGenerator
    {
        private EditorManager _manager;

        public ValidationErrorElementGenerator(EditorManager manager)
        {
            this._manager = manager;
        }

        public override VisualLineElement ConstructElement(int offset)
        {
            RdfParseException parseEx = this.GetException();
            if (parseEx == null) return null;
            if (parseEx.StartLine > CurrentContext.Document.LineCount) return null;

            int startOffset = this.CurrentContext.Document.GetOffset(parseEx.StartLine, parseEx.StartPosition);
            if (startOffset > 0 && startOffset > offset && parseEx.StartLine == parseEx.EndLine && parseEx.StartPosition == parseEx.EndPosition) startOffset--;
            int endOffset = Math.Min(this.CurrentContext.Document.GetOffset(parseEx.EndLine, parseEx.EndPosition), this.CurrentContext.VisualLine.LastDocumentLine.EndOffset);
            if (startOffset == endOffset) return null;
            if (startOffset > endOffset) return null;

            return new ValidationErrorLineText(this.CurrentContext.VisualLine, endOffset - startOffset);
        }

        public override int GetFirstInterestedOffset(int startOffset)
        {
            RdfParseException parseEx = this.GetException();
            if (parseEx == null) return -1;
            if (parseEx.StartLine > CurrentContext.Document.LineCount) return -1;

            int endOffset = CurrentContext.VisualLine.LastDocumentLine.EndOffset;
            int offset = CurrentContext.Document.GetOffset(parseEx.StartLine, parseEx.StartPosition);
            if (offset < startOffset || offset > endOffset)
            {
                return -1;
            }
            else
            {
                if (offset > 0 && offset > (startOffset + 1) && parseEx.StartLine == parseEx.EndLine && parseEx.StartPosition == parseEx.EndPosition)
                {
                    return offset - 1;
                }
                else
                {
                    return offset;
                }
            }
        }

        private RdfParseException GetException()
        {
            if (this._manager.LastValidationError == null) return null;
            if (this._manager.LastValidationError is RdfParseException)
            {
                RdfParseException parseEx = (RdfParseException)this._manager.LastValidationError;
                if (parseEx.HasPositionInformation)
                {
                    return parseEx;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
    }
}
