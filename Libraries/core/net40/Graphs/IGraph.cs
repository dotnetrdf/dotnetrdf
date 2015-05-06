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
using System.Collections.Generic;
using System.Collections.Specialized;
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
        #region Properties

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

        #endregion

        #region Assertion & Retraction

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
        /// Retracts all Triples from the Graph
        /// </summary>
        /// <remarks>
        /// <para>
        /// The Graph should raise the <see cref="ClearRequested">ClearRequested</see> event at the start of the Clear operation and abort the operation if the operation is cancelled by an event handler.  On completing the Clear the <see cref="Cleared">Cleared</see> event should be raised.
        /// </para>
        /// </remarks>
        void Clear();

        #endregion

        #region Node Creation

        /// <summary>
        /// Creates a URI Node for the given prefixed name using the graphs associated namespace map to resolve the prefixed name
        /// </summary>
        /// <param name="prefixedName">Prefixed name</param>
        /// <returns>URI Node</returns>
        INode CreateUriNode(string prefixedName);

        #endregion

        #region Selection

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

        #endregion

        #region Advanced Graph Operations

        /// <summary>
        /// Checks whether a Graph is equal to another Graph and if so returns the mapping of Blank Nodes
        /// </summary>
        /// <param name="g">Graph to compare with</param>
        /// <param name="mapping">Mapping of Blank Nodes</param>
        /// <returns></returns>
        bool Equals(IGraph g, out Dictionary<INode, INode> mapping);

        /// <summary>
        /// Checks whether this Graph is a sub-graph of the given Graph
        /// </summary>
        /// <param name="g">Graph</param>
        /// <returns></returns>
        bool IsSubGraphOf(IGraph g);

        /// <summary>
        /// Checks whether this Graph is a sub-graph of the given Graph
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="mapping">Mapping of Blank Nodes</param>
        /// <returns></returns>
        bool IsSubGraphOf(IGraph g, out Dictionary<INode, INode> mapping);

        /// <summary>
        /// Checks whether this Graph has the given Graph as a sub-graph
        /// </summary>
        /// <param name="g">Graph</param>
        /// <returns></returns>
        bool HasSubGraph(IGraph g);

        /// <summary>
        /// Checks whether this Graph has the given Graph as a sub-graph
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="mapping">Mapping of Blank Nodes</param>
        /// <returns></returns>
        bool HasSubGraph(IGraph g, out Dictionary<INode, INode> mapping);

        /// <summary>
        /// Calculates the difference between this Graph and the given Graph
        /// </summary>
        /// <param name="g">Graph</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// Produces a report which shows the changes that must be made to this Graph to produce the given Graph
        /// </para>
        /// </remarks>
        GraphDiffReport Difference(IGraph g);

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

        #endregion
    }

    /// <summary>
    /// Interface for graphs that support events, events are provided via implementation of the standard <see cref="INotifyCollectionChanged" /> interface
    /// </summary>
    public interface IEventedGraph
        : IGraph, INotifyCollectionChanged
    {
        /// <summary>
        /// Indicates whether a graph actually has events
        /// </summary>
        /// <remarks>
        /// While generally speaking use of this interface will be sufficient to indicate that a graph supports events in some cases where complex graph types such as decorators, unions, etc are used the availability of events may be dictated by the underlying graphs even if the wrapper is capable of providing them.  Thus users intending to consume events should check that this method returns true.
        /// </remarks>
        bool HasEvents { get; }
    }

    /// <summary>
    /// Interface for RDF Graphs which provide Transactions i.e. changes to them can be performed in a transaction and committed or rolled back as desired
    /// </summary>
    public interface ITransactionalGraph
        : IGraph
    {
        /// <summary>
        /// Begins a transaction
        /// </summary>
        void Begin();

        /// <summary>
        /// Commits a transaction
        /// </summary>
        void Commit();

        /// <summary>
        /// Aborts and rollbacks a transaction
        /// </summary>
        void Rollback();
    }
}
