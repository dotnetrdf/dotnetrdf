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
            if (this._manager.LastValidationError == null) return null;

            return null;
        }

        public override int GetFirstInterestedOffset(int startOffset)
        {
            if (this._manager.LastValidationError == null) return -1;

            int endOffset = CurrentContext.VisualLine.LastDocumentLine.EndOffset;
            int startLine = CurrentContext.Document.GetLineByOffset(startOffset).LineNumber;
            int endLine = CurrentContext.Document.GetLineByOffset(endOffset).LineNumber;

            return -1;
        }
    }
}
