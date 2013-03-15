using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Query.Spin
{
    internal class SpinVariableTable
    {
        private Dictionary<String, INode> _vars = new Dictionary<string, INode>();
        private IGraph _g;

        public SpinVariableTable(IGraph g)
        {
            this._g = g;
        }

        public INode this[String var]
        {
            get
            {
                if (!this._vars.ContainsKey(var))
                {
                    this._vars.Add(var, this._g.CreateBlankNode());
                }
                return this._vars[var];
            }
        }
    }
}
