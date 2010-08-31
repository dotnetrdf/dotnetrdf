using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace rdfEditor.AutoComplete
{
    public class NewBaseDeclaration : ICompletionData
    {
        #region ICompletionData Members

        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, "base <Enter Base URI here>");
        }

        public object Content
        {
            get 
            {
                return this.Text;
            }
        }

        public object Description
        {
            get 
            {
                return "Insert a new Base Declaration";
            }
        }

        public System.Windows.Media.ImageSource Image
        {
            get 
            {
                return null; 
            }
        }

        public double Priority
        {
            get 
            {
                return 100.0d; 
            }
        }

        public string Text
        {
            get 
            {
                return "<New Base Declaration>";
            }
        }

        #endregion
    }
}
