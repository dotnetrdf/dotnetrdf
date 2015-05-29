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
using System.Collections.Generic;
#if !NO_DATA
using System.Data;
#endif
using System.Xml.Serialization;
using VDS.RDF.Namespaces;
using VDS.RDF.Nodes;

namespace VDS.RDF.Graphs
{
    /// <summary>
    /// Interface for RDF Graphs
    /// </summary>
    /// <remarks>
    /// <para>
    /// Most implementations will probably want to inherit from the abstract class <see cref="BaseGraph">BaseGraph</see> since it contains reference implementations of various algorithms (Graph Equality/Graph Difference/Sub-Graph testing etc) which will save considerable work in implementation and ensure consistent behaviour of some methods across implementations.
    /// </para>
    /// </remarks>
    public interface IGraph 
        : INodeFactory, IDisposable
#if !SILVERLIGHT
        , IXmlSerializable
#endif
    {
        /// <summary>
        /// Gets the number of triples in the graph
        /// </summary>
        long Count
        {
            get;
        }

        /// <summary>
        /// Gets whether a Graph is Empty
        /// </summary>
        bool IsEmpty 
        { 
            get; 
        }

        /// <summary>
        /// Gets the Namespace Map for the Graph
        /// </summary>
        INamespaceMapper Namespaces 
        {
            get;
        }

        /// <summary>
        /// Gets the nodes that are used as vertices in the graph i.e. those which occur in the subject or object position of a triple
        /// </summary>
        IEnumerable<INode> Vertices 
        {
            get;
        }

        /// <summary>
        /// Gets the nodes that are used as edges in the graph i.e. those which occur in the predicate position of a triple
        /// </summary>
        IEnumerable<INode> Edges { get; }

        /// <summary>
        /// Gets the triples present in the graph
        /// </summary>
        IEnumerable<Triple> Triples 
        { 
            get; 
        }

        /// <summary>
        /// Gets the quads present in the graph
        /// </summary>
        /// <remarks>Since a graph does not itself have a time quads retrieved in this way have the <see cref="Quad.Graph"/> field set to <see cref="Quad.DefaultGraphNode" /></remarks>
        IEnumerable<Quad> Quads
        {
            get;
        }

        /// <summary>
        /// Gets the capabilities of the graph
        /// </summary>
        IGraphCapabilities Capabilities { get; }

        /// <summary>
        /// Asserts a Triple in the Graph
        /// </summary>
        /// <param name="t">A Triple</param>
        void Assert(Triple t);

        /// <summary>
        /// Asserts an Enumerable of Triples in the Graph
        /// </summary>
        /// <param name="ts">An Enumerable of Triples</param>
        void Assert(IEnumerable<Triple> ts);

        /// <summary>
        /// Retracts a Triple from the Graph
        /// </summary>
        /// <param name="t">A Triple</param>
        void Retract(Triple t);

        /// <summary>
        /// Retracts an Enumerable of Triples from the Graph
        /// </summary>
        /// <param name="ts">Enumerable of Triples</param>
        void Retract(IEnumerable<Triple> ts);

        /// <summary>
        /// Clears the graph of all data
        /// </summary>
        void Clear();

        /// <summary>
        /// Creates a URI Node for the given prefixed name using the graphs associated namespace map to resolve the prefixed name
        /// </summary>
        /// <param name="prefixedName">Prefixed name</param>
        /// <returns>URI Node</returns>
        INode CreateUriNode(string prefixedName);

        /// <summary>
        /// Finds triples matching the given search criteria i.e. those where the given nodes occur in the appropriate position(s).  Null values are treated as wildcards for a position.
        /// </summary>
        /// <param name="s">Subject</param>
        /// <param name="p">Predicate</param>
        /// <param name="o">Object</param>
        /// <returns>Triples</returns>
        IEnumerable<Triple> Find(INode s, INode p, INode o); 

        /// <summary>
        /// Gets whether a given Triple is in this Graph
        /// </summary>
        /// <param name="t">Triple to test</param>
        /// <returns></returns>
        bool ContainsTriple(Triple t);

#if !NO_DATA

        /// <summary>
        /// Converts the Graph into a DataTable
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// <strong>Warning:</strong> Not available under builds which remove the Data Storage layer from dotNetRDF e.g. Silverlight
        /// </remarks>
        DataTable ToDataTable();

#endif
    }
}
