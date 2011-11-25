using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Utilities.Editor.AutoComplete.Data
{
    public class VariableData : BaseCompletionData
    {
        private String _var;
        private double _priority = 0.0d;

        public VariableData(String var)
            : base(var, var, "Variable " + var) { }
    }
}
