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
    /// Class for representing Graph Literal Nodes which are supported in highly expressive RDF syntaxes like Notation 3
    /// </summary>
    public class GraphLiteralNode : BaseNode, IComparable<GraphLiteralNode>
    {
        private IGraph _subgraph;

        /// <summary>
        /// Creates a new Graph Literal Node in the given Graph which represents the given Subgraph
        /// </summary>
        /// <param name="g">Graph this node is in</param>
        /// <param name="subgraph">Sub Graph this node represents</param>
        protected internal GraphLiteralNode(IGraph g, IGraph subgraph)
            : base(g, NodeType.GraphLiteral)
        {
            this._subgraph = subgraph;

            //Compute Hash Code
            this._hashcode = (this._nodetype + this.ToString()).GetHashCode();
        }

        /// <summary>
        /// Creates a new Graph Literal Node whose value is an empty Subgraph
        /// </summary>
        /// <param name="g">Graph this node is in</param>
        protected internal GraphLiteralNode(IGraph g)
            : base(g, NodeType.GraphLiteral)
        {
            this._subgraph = new Graph();

            //Compute Hash Code
            this._hashcode = (this._nodetype + this.ToString()).GetHashCode();
        }

        /// <summary>
        /// Gets the Subgraph that this Node represents
        /// </summary>
        public IGraph SubGraph
        {
            get
            {
                return this._subgraph;
            }
        }

        /// <summary>
        /// Implementation of the Equals method for Graph Literal Nodes.  Graph Literals are considered Equal if their respective Subgraphs are equal
        /// </summary>
        /// <param name="obj">Object to compare the Node with</param>
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
        /// Implementation of the Equals method for Graph Literal Nodes.  Graph Literals are considered Equal if their respective Subgraphs are equal
        /// </summary>
        /// <param name="other">Object to compare the Node with</param>
        /// <returns></returns>
        public override bool Equals(INode other)
        {
            if ((Object)other == null) return false;

            if (ReferenceEquals(this, other)) return true;

            if (other.NodeType == NodeType.GraphLiteral)
            {
                //Use Graph Equality to determine equality
                GraphLiteralNode temp = (GraphLiteralNode)other;

                return this._subgraph.Equals(temp.SubGraph);
            }
            else
            {
                //Can only be equal to a Graph Literal Node
                return false;
            }
        }

        /// <summary>
        /// Implementation of ToString for Graph Literals which produces a String representation of the Subgraph in N3 style syntax
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();

            //Use N3 Style notation for Graph Literal string representation
            output.Append("{");

            //Add all the Triples in the Subgraph
            foreach (Triple t in this._subgraph.Triples)
            {
                output.Append(t.ToString());
            }

            output.Append("}");
            return output.ToString();
        }

        /// <summary>
        /// Implementation of CompareTo for Graph Literals
        /// </summary>
        /// <param name="other">Node to compare to</param>
        /// <returns></returns>
        /// <remarks>
        /// Graph Literal Nodes are greater than Blank Nodes, Uri Nodes, Literal Nodes and Nulls
        /// </remarks>
        public override int CompareTo(INode other)
        {
            if (other == null)
            {
                //Everything is greater than a null
                //Return a 1 to indicate this
                return 1;
            }
            else if (other.NodeType != NodeType.GraphLiteral)
            {
                //Graph Literal Nodes are greater than Blank, Variable, Uri and Literal Nodes
                //Return a 1 to indicate this
                return 1;
            }
            else if (other.NodeType == NodeType.GraphLiteral)
            {
                //Consider all Graph Literals to be Equal
                return 0;
            }
            else
            {
                //Anything else is Greater Than us
                return -1;
            }
        }

        /// <summary>
        /// Implementation of Compare To for Graph Literal Nodes
        /// </summary>
        /// <param name="other">Graph Literal Node to Compare To</param>
        /// <returns></returns>
        /// <remarks>
        /// Simply invokes the more general implementation of this method
        /// </remarks>
        public int CompareTo(GraphLiteralNode other)
        {
            return this.CompareTo((INode)other);
        }

    }
}
