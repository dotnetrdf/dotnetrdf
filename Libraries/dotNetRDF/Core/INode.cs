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
using System.Xml;
using System.Xml.Serialization;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF
{
    /// <summary>
    /// Node Type Values
    /// </summary>
    public enum NodeType
    {
        /// <summary>
        /// A Blank Node
        /// </summary>
        Blank = 0, 
        /// <summary>
        /// A Uri Node
        /// </summary>
        Uri = 1, 
        /// <summary>
        /// A Literal Node
        /// </summary>
        Literal = 2,
        /// <summary>
        /// A Graph Literal Node
        /// </summary>
        GraphLiteral = 3,
        /// <summary>
        /// A Variable Node (currently only used in N3)
        /// </summary>
        Variable = 4
    }

    /// <summary>
    /// Interface for Nodes
    /// </summary>
    public interface INode 
        : IComparable<INode>, IComparable<IBlankNode>, IComparable<IGraphLiteralNode>, IComparable<ILiteralNode>,
          IComparable<IUriNode>, IComparable<IVariableNode>,
          IEquatable<INode>, IEquatable<IBlankNode>, IEquatable<IGraphLiteralNode>, IEquatable<ILiteralNode>,
          IEquatable<IUriNode>, IEquatable<IVariableNode>
#if !NETCORE
          ,ISerializable, IXmlSerializable
#endif
    {
        /// <summary>
        /// Nodes have a Type
        /// </summary>
        /// <remarks>Primarily provided so can do quick integer comparison to see what type of Node you have without having to do actual full blown Type comparison</remarks>
        NodeType NodeType
        {
            get;
        }

        /// <summary>
        /// Gets the Graph the Node belongs to
        /// </summary>
        IGraph Graph
        {
            get;
        }

        /// <summary>
        /// Gets/Sets the Graph URI associated with a Node
        /// </summary>
        Uri GraphUri
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the String representation of the Node
        /// </summary>
        /// <returns></returns>
        String ToString();

        /// <summary>
        /// Gets the String representation of the Node formatted with the given Node formatter
        /// </summary>
        /// <param name="formatter">Formatter</param>
        /// <returns></returns>
        String ToString(INodeFormatter formatter);

        /// <summary>
        /// Gets the String representation of the Node formatted with the given Node formatter
        /// </summary>
        /// <param name="formatter">Formatter</param>
        /// <param name="segment">Triple Segment</param>
        /// <returns></returns>
        String ToString(INodeFormatter formatter, TripleSegment segment);
    }

    /// <summary>
    /// Interface for URI Nodes
    /// </summary>
    public interface IUriNode
        : INode
    {
        /// <summary>
        /// Gets the URI the Node represents
        /// </summary>
        Uri Uri
        {
            get;
        }
    }

    /// <summary>
    /// Interface for Blank Nodes
    /// </summary>
    public interface IBlankNode 
        : INode
    {
        /// <summary>
        /// Gets the Internal ID of the Blank Node
        /// </summary>
        String InternalID
        {
            get;
        }
    }

    /// <summary>
    /// Interface for Literal Nodes
    /// </summary>
    public interface ILiteralNode
        : INode
    {
        /// <summary>
        /// Gets the Lexical Value of the Literal
        /// </summary>
        String Value
        {
            get;
        }

        /// <summary>
        /// Gets the Language specifier (if any) of the Literal or the Empty String
        /// </summary>
        String Language
        {
            get;
        }

        /// <summary>
        /// Gets the DataType URI (if any) of the Literal or null
        /// </summary>
        Uri DataType
        {
            get;
        }
    }

    /// <summary>
    /// Interface for Graph Literal Nodes
    /// </summary>
    public interface IGraphLiteralNode
        : INode
    {
        /// <summary>
        /// Gets the Sub-graph the Graph Literal represents
        /// </summary>
        IGraph SubGraph
        {
            get;
        }
    }

    /// <summary>
    /// Interface for Variable Nodes
    /// </summary>
    public interface IVariableNode
        : INode
    {
        /// <summary>
        /// Gets the Variable Name
        /// </summary>
        String VariableName
        {
            get;
        }
    }
}
