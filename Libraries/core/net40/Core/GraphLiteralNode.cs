/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
#if !SILVERLIGHT
using VDS.RDF.Writing.Serialization;
#endif
using VDS.RDF.Query;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Nodes;

namespace VDS.RDF
{
    /// <summary>
    /// Abstract Base Class for Graph Literal Nodes
    /// </summary>
#if !SILVERLIGHT
    [Serializable,XmlRoot(ElementName="graphliteral")]
#endif
    public abstract class BaseGraphLiteralNode
        : BaseNode, IEquatable<BaseGraphLiteralNode>, IComparable<BaseGraphLiteralNode>, IValuedNode
    {
        /// <summary>
        /// Creates a new Graph Literal Node in the given Graph which represents the given Subgraph
        /// </summary>
        /// <param name="g">Graph this node is in</param>
        /// <param name="subgraph">Sub Graph this node represents</param>
        protected internal BaseGraphLiteralNode(IGraph subgraph)
            : base(NodeType.GraphLiteral)
        {
            this.SubGraph = subgraph;

            //Compute Hash Code
            this._hashcode = Tools.CombineHashCodes(NodeType.GraphLiteral, this.SubGraph.GetHashCode());
        }

        /// <summary>
        /// Creates a new Graph Literal Node whose value is an empty Subgraph
        /// </summary>
        /// <param name="g">Graph this node is in</param>
        protected internal BaseGraphLiteralNode()
            : this(new Graph()) { }

#if !SILVERLIGHT

        /// <summary>
        /// Deserializer Constructor
        /// </summary>
        /// <param name="info">Serialization Information</param>
        /// <param name="context">Streaming Context</param>
        protected BaseGraphLiteralNode(SerializationInfo info, StreamingContext context)
            : base(NodeType.GraphLiteral)
        {
            this.SubGraph = (IGraph)info.GetValue("subgraph", typeof(Graph));
            //Compute Hash Code
            this._hashcode = Tools.CombineHashCodes(NodeType.GraphLiteral, this.SubGraph.GetHashCode());
        }
#endif

        /// <summary>
        /// Gets the Subgraph that this Node represents
        /// </summary>
        public override IGraph SubGraph { get; protected set; }

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
                return EqualityHelper.AreGraphLiteralsEqual(this, other);
            }
            else
            {
                //Can only be equal to a Graph Literal Node
                return false;
            }
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

        #region IValuedNode Members

        /// <summary>
        /// Throws an error as Graph Literals cannot be cast to a string
        /// </summary>
        /// <returns></returns>
        public string AsString()
        {
            throw new NodeValueException("Unable to cast a Graph Literal Node to a type");
        }

        /// <summary>
        /// Throws an error as Graph Literals cannot be cast to an integer
        /// </summary>
        /// <returns></returns>
        public long AsInteger()
        {
            throw new NodeValueException("Unable to cast a Graph Literal Node to a type");
        }

        /// <summary>
        /// Throws an error as Graph Literals cannot be cast to a decimal
        /// </summary>
        /// <returns></returns>
        public decimal AsDecimal()
        {
            throw new NodeValueException("Unable to cast a Graph Literal Node to a type");
        }

        /// <summary>
        /// Throws an error as Graph Literals cannot be cast to a float
        /// </summary>
        /// <returns></returns>
        public float AsFloat()
        {
            throw new NodeValueException("Unable to cast a Graph Literal Node to a type");
        }

        /// <summary>
        /// Throws an error as Graph Literals cannot be cast to a double
        /// </summary>
        /// <returns></returns>
        public double AsDouble()
        {
            throw new NodeValueException("Unable to cast a Graph Literal Node to a type");
        }

        /// <summary>
        /// Throws an error as Graph Literals cannot be cast to a boolean
        /// </summary>
        /// <returns></returns>
        public bool AsBoolean()
        {
            throw new NodeValueException("Unable to cast a Graph Literal Node to a type");
        }

        /// <summary>
        /// Throws an error as Graph Literals cannot be cast to a date time
        /// </summary>
        /// <returns></returns>
        public DateTime AsDateTime()
        {
            throw new NodeValueException("Unable to cast a Graph Literal Node to a type");
        }

        /// <summary>
        /// Throws an error as Graph Literals cannot be cast to a date time
        /// </summary>
        /// <returns></returns>
        public DateTimeOffset AsDateTimeOffset()
        {
            throw new NodeValueException("Unable to cast a Graph Literal Node to a type");
        }

        /// <summary>
        /// Throws an error as Graph Literals cannot be cast to a time span
        /// </summary>
        /// <returns></returns>
        public TimeSpan AsTimeSpan()
        {
            throw new NodeValueException("Unable to cast a Graph Literal Node to a type");
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
#if !SILVERLIGHT
    [Serializable,XmlRoot(ElementName="graphliteral")]
#endif
    public class GraphLiteralNode 
        : BaseGraphLiteralNode, IEquatable<GraphLiteralNode>, IComparable<GraphLiteralNode>
    {
        /// <summary>
        /// Creates a new Graph Literal Node whose value is an empty Subgraph
        /// </summary>
        /// <param name="g">Graph this node is in</param>
        /// <param name="subgraph">Sub-graph this node represents</param>
        protected internal GraphLiteralNode(IGraph subgraph)
            : base(subgraph) { }

#if !SILVERLIGHT

        /// <summary>
        /// Deserialization only constructor
        /// </summary>
        protected GraphLiteralNode()
            : base() { }

        /// <summary>
        /// Deserialization Constructor
        /// </summary>
        /// <param name="info">Serialization Information</param>
        /// <param name="context">Streaming Context</param>
        protected GraphLiteralNode(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
#endif

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
