using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace rdfEditor.AutoComplete.Data
{
    public class PrefixCompletionData : BaseCompletionData
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

        public override object Description
        {
            get 
            {
                return this._description;
            }
        }

        public override double Priority
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

        public override string Text
        {
            get 
            {
                return "prefix " + this._prefix + ": <" + this._uri.Replace(">", "\\>") + ">";
            }
        }

        public String NamespaceUri
        {
            get
            {
                return this._uri;
            }
        }

        public String Prefix
        {
            get
            {
                return this._prefix;
            }
        }
    }
}
