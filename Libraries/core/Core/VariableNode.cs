using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Core
{
    /// <summary>
    /// Class representing Variable Nodes (only used for N3)
    /// </summary>
    public class VariableNode : BaseNode, IComparable<VariableNode>
    {
        private String _var;

        protected internal VariableNode(IGraph g, String varname)
            : base(g, NodeType.Variable)
        {
            if (varname.StartsWith("?") || varname.StartsWith("$"))
            {
                this._var = varname.Substring(1);
            }
            else
            {
                this._var = varname;
            }
            this._hashcode = (this._nodetype + this.ToString()).GetHashCode();
        }

        public String VariableName
        {
            get
            {
                return this._var;
            }
        }

        public override bool Equals(INode other)
        {
            if ((Object)other == null) return false;

            if (ReferenceEquals(this, other)) return true;

            if (other.NodeType == NodeType.Variable)
            {
                return this._var.Equals(((VariableNode)other).VariableName, StringComparison.Ordinal);
            }
            else
            {
                //Can only be equal to other Variables
                return false;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            if (ReferenceEquals(this, obj)) return true;

            if (obj is INode)
            {
                return this.Equals((INode)obj);
            }
            else
            {
                //Can only be equal to other Nodes
                return false;
            }
        }

        public override string ToString()
        {
            return "?" + this._var;
        }

        //TODO: Decide Sort Order for Variable Nodes and update other Node classes CompareTo method to be aware of this


        public override int CompareTo(INode other)
        {
            throw new NotImplementedException();
        }

        public int CompareTo(VariableNode other)
        {
            throw new NotImplementedException();
        }
    }
}
