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
    public class NewBlankNode : BaseCompletionData
    {
        private String _id;

        public NewBlankNode(String id)
        {
            this._id = id;
        }

        public override object Content
        {
            get
            {
                return "<New Blank Node>";
            }
        }

        public override object Description
        {
            get
            {
                return "Insert a new Blank Node";
            }
        }

        public override double Priority
        {
            get
            {
                return 50.0d;
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
                return "_:" + this._id;
            }
        }
    }
}
