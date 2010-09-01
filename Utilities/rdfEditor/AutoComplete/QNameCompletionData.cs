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
    public class QNameCompletionData : BaseCompletionData
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
                return this._qname; 
            }
        }
    }
}
