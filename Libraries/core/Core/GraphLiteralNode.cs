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
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
#if !SILVERLIGHT
using VDS.RDF.Writing.Serialization;
#endif

namespace VDS.RDF
{
    /// <summary>
    /// Abstract Base Class for Graph Literal Nodes
    /// </summary>
#if !SILVERLIGHT
    [Serializable,XmlRoot(ElementName="graphliteral")]
#endif
    public abstract class BaseGraphLiteralNode
        : BaseNode, IGraphLiteralNode, IEquatable<BaseGraphLiteralNode>, IComparable<BaseGraphLiteralNode>
    {
        private IGraph _subgraph;

        /// <summary>
        /// Creates a new Graph Literal Node in the given Graph which represents the given Subgraph
        /// </summary>
        /// <param name="g">Graph this node is in</param>
        /// <param name="subgraph">Sub Graph this node represents</param>
        protected internal BaseGraphLiteralNode(IGraph g, IGraph subgraph)
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
        protected internal BaseGraphLiteralNode(IGraph g)
            : base(g, NodeType.GraphLiteral)
        {
            this._subgraph = new Graph();

            //Compute Hash Code
            this._hashcode = (this._nodetype + this.ToString()).GetHashCode();
        }

#if !SILVERLIGHT
        /// <summary>
        /// Deserializer Constructor
        /// </summary>
        /// <param name="info">Serialization Information</param>
        /// <param name="context">Streaming Context</param>
        protected BaseGraphLiteralNode(SerializationInfo info, StreamingContext context)
            : base(null, NodeType.GraphLiteral)
        {
            this._subgraph = (IGraph)info.GetValue("subgraph", typeof(Graph));
            //Compute Hash Code
            this._hashcode = (this._nodetype + this.ToString()).GetHashCode();
        }
#endif

        /// <summary>
        /// Deserialization Only constructor
        /// </summary>
        protected BaseGraphLiteralNode()
            : base(null, NodeType.GraphLiteral) { }

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
                return EqualityHelper.AreGraphLiteralsEqual(this, (IGraphLiteralNode)other);
            }
            else
            {
                //Can only be equal to a Graph Literal Node
                return false;
            }
        }

        /// <summary>
        /// Determines whether this Node is equal to a Blank Node (should always be false)
        /// </summary>
        /// <param name="other">Blank Node</param>
        /// <returns></returns>
        public override bool Equals(IBlankNode other)
        {
            if (ReferenceEquals(this, other)) return true;
            return false;
        }

        /// <summary>
        /// Determines whether this Node is equal to a Graph Literal Node
        /// </summary>
        /// <param name="other">Graph Literal Node</param>
        /// <returns></returns>
        public override bool Equals(IGraphLiteralNode other)
        {
            if (ReferenceEquals(this, other)) return true;
            return EqualityHelper.AreGraphLiteralsEqual(this, other);
        }

        /// <summary>
        /// Determines whether this Node is equal to a Literal Node (should always be false)
        /// </summary>
        /// <param name="other">Literal Node</param>
        /// <returns></returns>
        public override bool Equals(ILiteralNode other)
        {
            if (ReferenceEquals(this, other)) return true;
            return false;
        }

        /// <summary>
        /// Determines whether this Node is equal to a URI Node (should always be false)
        /// </summary>
        /// <param name="other">URI Node</param>
        /// <returns></returns>
        public override bool Equals(IUriNode other)
        {
            if (ReferenceEquals(this, other)) return true;
            return false;
        }

        /// <summary>
        /// Determines whether this Node is equal to a Variable Node (should always be false)
        /// </summary>
        /// <param name="other">Variable Node</param>
        /// <returns></returns>
        public override bool Equals(IVariableNode other)
        {
            if (ReferenceEquals(this, other)) return true;
            return false;
        }

        /// <summary>
        /// Determines whether this Node is equal to a Graph Literal Node
        /// </summary>
        /// <param name="other">Graph Literal Node</param>
        /// <returns></returns>
        public bool Equals(BaseGraphLiteralNode other)
        {
            return this.Equals((IGraphLiteralNode)other);
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
            if (ReferenceEquals(this, other)) return 0;

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
                return ComparisonHelper.CompareGraphLiterals(this, (IGraphLiteralNode)other);
            }
            else
            {
                //Anything else is Greater Than us
                return -1;
            }
        }

        /// <summary>
        /// Returns an Integer indicating the Ordering of this Node compared to another Node
        /// </summary>
        /// <param name="other">Node to test against</param>
        /// <returns></returns>
        public override int CompareTo(IBlankNode other)
        {
            if (ReferenceEquals(this, other)) return 0;

            //We are always greater than everything
            return 1;
        }

        /// <summary>
        /// Returns an Integer indicating the Ordering of this Node compared to another Node
        /// </summary>
        /// <param name="other">Node to test against</param>
        /// <returns></returns>
        public override int CompareTo(IGraphLiteralNode other)
        {
            return ComparisonHelper.CompareGraphLiterals(this, (IGraphLiteralNode)other);
        }

        /// <summary>
        /// Returns an Integer indicating the Ordering of this Node compared to another Node
        /// </summary>
        /// <param name="other">Node to test against</param>
        /// <returns></returns>
        public override int CompareTo(ILiteralNode other)
        {
            if (ReferenceEquals(this, other)) return 0;

            //We are always greater than everything
            return 1;
        }

        /// <summary>
        /// Returns an Integer indicating the Ordering of this Node compared to another Node
        /// </summary>
        /// <param name="other">Node to test against</param>
        /// <returns></returns>
        public override int CompareTo(IUriNode other)
        {
            if (ReferenceEquals(this, other)) return 0;

            //We are always greater than everything
            return 1;
        }

        /// <summary>
        /// Returns an Integer indicating the Ordering of this Node compared to another Node
        /// </summary>
        /// <param name="other">Node to test against</param>
        /// <returns></returns>
        public override int CompareTo(IVariableNode other)
        {
            if (ReferenceEquals(this, other)) return 0;

            //We are always greater than everything
            return 1;
        }

        /// <summary>
        /// Returns an Integer indicating the Ordering of this Node compared to another Node
        /// </summary>
        /// <param name="other">Node to test against</param>
        /// <returns></returns>
        public int CompareTo(BaseGraphLiteralNode other)
        {
            return this.CompareTo((IGraphLiteralNode)other);
        }

#if !SILVERLIGHT
        #region Serialization

        /// <summary>
        /// Gets the Serialization Information
        /// </summary>
        /// <param name="info">Serialization Information</param>
        /// <param name="context">Streaming Context</param>
        public sealed override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("subgraph", this._subgraph);
        }

        /// <summary>
        /// Reads the data for XML deserialization
        /// </summary>
        /// <param name="reader">XML Reader</param>
        public sealed override void ReadXml(XmlReader reader)
        {
            reader.Read();
            this._subgraph = reader.DeserializeGraph();
            //Compute Hash Code
            this._hashcode = (this._nodetype + this.ToString()).GetHashCode();
        }

        /// <summary>
        /// Writes the data for XML serialization
        /// </summary>
        /// <param name="writer">XML Writer</param>
        public sealed override void WriteXml(XmlWriter writer)
        {
            this._subgraph.SerializeGraph(writer);
        }

        #endregion
#endif
    }

    /// <summary>
    /// Class for representing Graph Literal Nodes which are supported in highly expressive RDF syntaxes like Notation 3
    /// </summary>
#if !SILVERLIGHT
    [Serializable,XmlRoot(ElementName="graphliteral")]
#endif
    public class GraphLiteralNode 
        : BaseGraphLiteralNode, IEquatable<GraphLiteralNode>, IComparable<GraphLiteralNode>
    {
        /// <summary>
        /// Creates a new Graph Literal Node in the given Graph which represents the given Subgraph
        /// </summary>
        /// <param name="g">Graph this node is in</param>
        protected internal GraphLiteralNode(IGraph g)
            : base(g) { }

        /// <summary>
        /// Creates a new Graph Literal Node whose value is an empty Subgraph
        /// </summary>
        /// <param name="g">Graph this node is in</param>
        /// <param name="subgraph">Sub-graph this node represents</param>
        protected internal GraphLiteralNode(IGraph g, IGraph subgraph)
            : base(g, subgraph) { }

#if !SILVERLIGHT
        /// <summary>
        /// Deserialization Constructor
        /// </summary>
        /// <param name="info">Serialization Information</param>
        /// <param name="context">Streaming Context</param>
        protected GraphLiteralNode(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
#endif
        /// <summary>
        /// Deserialization Only Constructor
        /// </summary>
        protected GraphLiteralNode()
            : base() { }

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
            return this.CompareTo((IGraphLiteralNode)other);
        }

        /// <summary>
        /// Determines whether this Node is equal to a Graph Literal Node
        /// </summary>
        /// <param name="other">Graph Literal Node</param>
        /// <returns></returns>
        public bool Equals(GraphLiteralNode other)
        {
            return base.Equals((IGraphLiteralNode)other);
        }

    }
}
