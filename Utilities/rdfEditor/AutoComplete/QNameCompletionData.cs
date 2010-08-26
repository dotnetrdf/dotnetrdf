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
    public class QNameCompletionData
    {
        private String _qname, _description = String.Empty;
        private double _priority = 0.0d;

        public QNameCompletionData(String qname)
        {
            this._qname = qname;
        }

        public QNameCompletionData(String qname, String description)
            : this(qname)
        {
            this._description = description;
        }

        public QNameCompletionData(String qname, double priority)
            : this(qname)
        {
            this._priority = priority;
        }

        public QNameCompletionData(String qname, String description, double priority)
            : this(qname, description)
        {
            this._priority = priority;
        }

        #region ICompletionData Members

        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, this.Text);
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
                return this._description; 
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
                return this._priority;
            }
        }

        public string Text
        {
            get 
            {
                return this._qname; 
            }
        }

        #endregion
    }
}
