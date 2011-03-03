using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Utilities.Editor.AutoComplete.Data
{
    public class BlankNodeCompletionData : BaseCompletionData
    {
        private String _id;
        private double _priority = 0.0d;

        public BlankNodeCompletionData(String id)
        {
            this._id = id;
        }

        public override object Description
        {
            get 
            {
                return "Blank Node with ID " + this._id.Substring(2);
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
                return this._id; 
            }
        }
    }
}
