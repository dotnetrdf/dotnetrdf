/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-develop@lists.sf.net)

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
using System.Xml.Serialization;
using VDS.RDF.Graphs;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Nodes
{
    /// <summary>
    /// Interface for Nodes
    /// </summary>
    public interface INode 
        : IComparable<INode>, IEquatable<INode>
#if !SILVERLIGHT
          ,ISerializable, IXmlSerializable
#endif
    {
        /// <summary>
        /// Nodes have a Type
        /// </summary>
        /// <remarks>Primarily provided so can do quick comparison to see what type of Node you have without having to do actual full blown Type comparison</remarks>
        NodeType NodeType
        {
            get;
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
        String ToString(INodeFormatter formatter, QuadSegment segment);

        /// <summary>
        /// Gets the URI the Node represents if it is a URI node, otherwise produces an error
        /// </summary>
        /// <exception cref="NodeValueException">Thrown if this is not a URI node</exception>
        Uri Uri
        {
            get;
        }

        /// <summary>
        /// Gets the ID if it is a blank node, otherwise produces an error
        /// </summary>
        /// <exception cref="NodeValueException">Thrown if this is not a blank node</exception>
        Guid AnonID
        {
            get;
        }

        /// <summary>
        /// Gets the Lexical Value if this is a literal node, otherwise produces an error
        /// </summary>
        /// <exception cref="NodeValueException">Thrown if this is not a literal node</exception>
        String Value
        {
            get;
        }

        /// <summary>
        /// Gets whether the node has a language specifier if this is a literal node, otherwise produces an error
        /// </summary>
        /// <exception cref="NodeValueException">Thrown if this is not a literal node</exception>
        bool HasLanguage { get; }

        /// <summary>
        /// Gets whether the ode has a data type URI if this is a literal node, otherwise produces an error
        /// </summary>
        /// <exception cref="NodeValueException">Thrown if this is not a literal node</exception>
        bool HasDataType { get; }

        /// <summary>
        /// Gets the Language specifier (if any) or null (if none) if this is a literal node, otherwise produces an error 
        /// </summary>
        /// <exception cref="NodeValueException">Thrown if this is not a literal node</exception>
        String Language
        {
            get;
        }

        /// <summary>
        /// Gets the Data Type URI (if any) or null (if none) if this is a literal node, otherwise produces an error
        /// </summary>
        /// <exception cref="NodeValueException">Thrown if this is not a literal node</exception>
        Uri DataType
        {
            get;
        }

        /// <summary>
        /// Gets the Sub-graph the node represents if this is a graph literal, otherwise produces an error
        /// </summary>
        /// <exception cref="NodeValueException">Thrown if this is not a graph literal node</exception>
        IGraph SubGraph
        {
            get;
        }

        /// <summary>
        /// Gets the Variable Name the node represents if this is a variable node, otherwise produces an error
        /// </summary>
        /// <exception cref="NodeValueException">Thrown if this is not a variable node</exception>
        String VariableName
        {
            get;
        }
    }
}
