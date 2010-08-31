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
    public class PrefixCompletionData : ICompletionData
    {
        private String _prefix, _uri, _description = String.Empty;
        private double _priority = 0.0d;

        public PrefixCompletionData(String prefix, String uri)
        {
            this._prefix = prefix;
            this._uri = uri;
        }

        public PrefixCompletionData(String prefix, String uri, String description)
            : this(prefix, uri)
        {
            this._description = description;
        }

        public PrefixCompletionData(String prefix, String uri, double priority)
            : this(prefix, uri)
        {
            this._priority = priority;
        }

        public PrefixCompletionData(String prefix, String uri, String description, double priority)
            : this(prefix, uri, description)
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
            set
            {
                this._priority = value;
            }
        }

        public string Text
        {
            get 
            {
                return "prefix " + this._prefix + ": <" + this._uri.Replace(">", "\\>") + ">";
            }
        }

        #endregion

        public String NamespaceUri
        {
            get
            {
                return this._uri;
            }
        }
    }
}
