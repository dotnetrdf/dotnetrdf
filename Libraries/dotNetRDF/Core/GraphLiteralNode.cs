/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using System;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using VDS.RDF.Nodes;
using VDS.RDF.Query;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Writing.Serialization;

namespace VDS.RDF
{
    /// <summary>
    /// Abstract Base Class for Graph Literal Nodes
    /// </summary>
#if !NETCORE
    [Serializable,XmlRoot(ElementName="graphliteral")]
#endif
    public abstract class BaseGraphLiteralNode
        : BaseNode, IGraphLiteralNode, IEquatable<BaseGraphLiteralNode>, IComparable<BaseGraphLiteralNode>, IValuedNode
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
            _subgraph = subgraph;

            // Compute Hash Code
            _hashcode = (_nodetype + ToString()).GetHashCode();
        }

        /// <summary>
        /// Creates a new Graph Literal Node whose value is an empty Subgraph
        /// </summary>
        /// <param name="g">Graph this node is in</param>
        protected internal BaseGraphLiteralNode(IGraph g)
            : base(g, NodeType.GraphLiteral)
        {
            _subgraph = new Graph();

            // Compute Hash Code
            _hashcode = (_nodetype + ToString()).GetHashCode();
        }

#if !NETCORE
        /// <summary>
        /// Deserializer Constructor
        /// </summary>
        /// <param name="info">Serialization Information</param>
        /// <param name="context">Streaming Context</param>
        protected BaseGraphLiteralNode(SerializationInfo info, StreamingContext context)
            : base(null, NodeType.GraphLiteral)
        {
            _subgraph = (IGraph)info.GetValue("subgraph", typeof(Graph));
            // Compute Hash Code
            _hashcode = (_nodetype + ToString()).GetHashCode();
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
                return _subgraph;
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
                return Equals((INode)obj);
            }
            else
            {
                // Can only be equal to other Nodes
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
                // Can only be equal to a Graph Literal Node
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
            return Equals((IGraphLiteralNode)other);
        }

        /// <summary>
        /// Implementation of ToString for Graph Literals which produces a String representation of the Subgraph in N3 style syntax
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();

            // Use N3 Style notation for Graph Literal string representation
            output.Append("{");

            // Add all the Triples in the Subgraph
            foreach (Triple t in _subgraph.Triples)
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
                // Everything is greater than a null
                // Return a 1 to indicate this
                return 1;
            }
            else if (other.NodeType != NodeType.GraphLiteral)
            {
                // Graph Literal Nodes are greater than Blank, Variable, Uri and Literal Nodes
                // Return a 1 to indicate this
                return 1;
            }
            else if (other.NodeType == NodeType.GraphLiteral)
            {
                return ComparisonHelper.CompareGraphLiterals(this, (IGraphLiteralNode)other);
            }
            else
            {
                // Anything else is Greater Than us
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

            // We are always greater than everything
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

            // We are always greater than everything
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

            // We are always greater than everything
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

            // We are always greater than everything
            return 1;
        }

        /// <summary>
        /// Returns an Integer indicating the Ordering of this Node compared to another Node
        /// </summary>
        /// <param name="other">Node to test against</param>
        /// <returns></returns>
        public int CompareTo(BaseGraphLiteralNode other)
        {
            return CompareTo((IGraphLiteralNode)other);
        }

#if !NETCORE
        #region Serialization

        /// <summary>
        /// Gets the Serialization Information
        /// </summary>
        /// <param name="info">Serialization Information</param>
        /// <param name="context">Streaming Context</param>
        public sealed override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("subgraph", _subgraph);
        }

        /// <summary>
        /// Reads the data for XML deserialization
        /// </summary>
        /// <param name="reader">XML Reader</param>
        public sealed override void ReadXml(XmlReader reader)
        {
            reader.Read();
            _subgraph = reader.DeserializeGraph();
            // Compute Hash Code
            _hashcode = (_nodetype + ToString()).GetHashCode();
        }

        /// <summary>
        /// Writes the data for XML serialization
        /// </summary>
        /// <param name="writer">XML Writer</param>
        public sealed override void WriteXml(XmlWriter writer)
        {
            _subgraph.SerializeGraph(writer);
        }

        #endregion
#endif

        #region IValuedNode Members

        /// <summary>
        /// Throws an error as Graph Literals cannot be cast to a string
        /// </summary>
        /// <returns></returns>
        public string AsString()
        {
            throw new RdfQueryException("Unable to cast a Graph Literal Node to a type");
        }

        /// <summary>
        /// Throws an error as Graph Literals cannot be cast to an integer
        /// </summary>
        /// <returns></returns>
        public long AsInteger()
        {
            throw new RdfQueryException("Unable to cast a Graph Literal Node to a type");
        }

        /// <summary>
        /// Throws an error as Graph Literals cannot be cast to a decimal
        /// </summary>
        /// <returns></returns>
        public decimal AsDecimal()
        {
            throw new RdfQueryException("Unable to cast a Graph Literal Node to a type");
        }

        /// <summary>
        /// Throws an error as Graph Literals cannot be cast to a float
        /// </summary>
        /// <returns></returns>
        public float AsFloat()
        {
            throw new RdfQueryException("Unable to cast a Graph Literal Node to a type");
        }

        /// <summary>
        /// Throws an error as Graph Literals cannot be cast to a double
        /// </summary>
        /// <returns></returns>
        public double AsDouble()
        {
            throw new RdfQueryException("Unable to cast a Graph Literal Node to a type");
        }

        /// <summary>
        /// Throws an error as Graph Literals cannot be cast to a boolean
        /// </summary>
        /// <returns></returns>
        public bool AsBoolean()
        {
            throw new RdfQueryException("Unable to cast a Graph Literal Node to a type");
        }

        /// <summary>
        /// Throws an error as Graph Literals cannot be cast to a date time
        /// </summary>
        /// <returns></returns>
        public DateTime AsDateTime()
        {
            throw new RdfQueryException("Unable to cast a Graph Literal Node to a type");
        }

        /// <summary>
        /// Throws an error as Graph Literals cannot be cast to a date time
        /// </summary>
        /// <returns></returns>
        public DateTimeOffset AsDateTimeOffset()
        {
            throw new RdfQueryException("Unable to cast a Graph Literal Node to a type");
        }

        /// <summary>
        /// Throws an error as Graph Literals cannot be cast to a time span
        /// </summary>
        /// <returns></returns>
        public TimeSpan AsTimeSpan()
        {
            throw new RdfQueryException("Unable to cast a Graph Literal Node to a type");
        }

        /// <summary>
        /// Gets the URI of the datatype this valued node represents as a String
        /// </summary>
        public String EffectiveType
        {
            get
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// Gets the numeric type of the node
        /// </summary>
        public SparqlNumericType NumericType
        {
            get 
            {
                return SparqlNumericType.NaN; 
            }
        }

        #endregion
    }

    /// <summary>
    /// Class for representing Graph Literal Nodes which are supported in highly expressive RDF syntaxes like Notation 3
    /// </summary>
#if !NETCORE
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

#if !NETCORE
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
            return CompareTo((IGraphLiteralNode)other);
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
