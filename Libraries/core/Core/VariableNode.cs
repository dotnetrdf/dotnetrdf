/*

Copyright Robert Vesse 2009-10
rvesse@vdesign-studios.com

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF
{
    /// <summary>
    /// Class representing Variable Nodes (only used for N3)
    /// </summary>
    public class VariableNode : BaseNode, IComparable<VariableNode>
    {
        private String _var;

        /// <summary>
        /// Creates a new Variable Node
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="varname">Variable Name</param>
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

        /// <summary>
        /// Gets the Variable Name
        /// </summary>
        public String VariableName
        {
            get
            {
                return this._var;
            }
        }

        /// <summary>
        /// Gets whether this Node is equal to some other Node
        /// </summary>
        /// <param name="other">Node to test</param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets whether this Node is equal to some Object
        /// </summary>
        /// <param name="obj">Object to test</param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets the String representation of this Node
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "?" + this._var;
        }

        /// <summary>
        /// Compares this Node to another Node
        /// </summary>
        /// <param name="other">Node to compare with</param>
        /// <returns></returns>
        public override int CompareTo(INode other)
        {
            if (other == null)
            {
                //Variables are considered greater than null
                return 1;
            }
            else if (other.NodeType == NodeType.Variable)
            {
                return this.CompareTo((VariableNode)other);
            }
            else
            {
                //Variable Nodes are less than everything else
                return -1;
            }
        }

        /// <summary>
        /// Compares this Node to another Variable Node
        /// </summary>
        /// <param name="other">Variable Node</param>
        /// <returns></returns>
        public int CompareTo(VariableNode other)
        {
            return this._var.CompareTo(other.VariableName);
        }
    }
}
