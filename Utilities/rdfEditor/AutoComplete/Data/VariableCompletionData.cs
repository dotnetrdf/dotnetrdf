using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rdfEditor.AutoComplete.Data
{
    public class VariableCompletionData : BaseCompletionData
    {
        private String _var;
        private double _priority = 0.0d;

        public VariableCompletionData(String var)
        {
            this._var = var;
        }

        public override object Description
        {
            get 
            {
                return "Variable " + this._var; 
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
                return this._var; 
            }
        }
    }
}
