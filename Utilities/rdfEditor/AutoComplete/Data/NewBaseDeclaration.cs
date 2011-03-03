using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace VDS.RDF.Utilities.Editor.AutoComplete.Data
{
    public class NewBaseDeclaration : BaseCompletionData
    {
        public override void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, "base <Enter Base URI here>");
        }

        public override object Description
        {
            get 
            {
                return "Insert a new Base Declaration";
            }
        }

        public override double Priority
        {
            get 
            {
                return 100.0d; 
            }
            set 
            {
                //Does nothing
            }
        }

        public override string Text
        {
            get 
            {
                return "<New Base Declaration>";
            }
        }
    }
}
