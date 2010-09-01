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
    public class NewPrefixDeclaration : BaseCompletionData
    {
        private String _prefix;

        public NewPrefixDeclaration(String prefix)
        {
            this._prefix = prefix;
        }

        public override object Description
        {
            get
            {
                return "Insert a new Prefix Declaration";
            }
        }

        public override object  Content
        {
	        get 
	        { 
		         return "<New Prefix Declaration>";
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
                return "prefix " + this._prefix + ": <Enter Namespace URI here>";
            }
        }
    }
}
