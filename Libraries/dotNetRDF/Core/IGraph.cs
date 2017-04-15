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
using System.Collections.Generic;
using System.Xml.Serialization;

namespace VDS.RDF
{
    /// <summary>
    /// Interface for RDF Graphs
    /// </summary>
    /// <remarks>
    /// <para>
    /// Most implementations will probably want to inherit from the abstract class <see cref="BaseGraph">BaseGraph</see> since it contains reference implementations of various algorithms (Graph Equality/Graph Difference/Sub-Graph testing etc) which will save considerable work in implementation and ensure consistent behaviour of some methods across implementations.
    /// </para>
    /// </remarks>
    public interface IGraph : INodeFactory, IDisposable
#if !NETCORE
        , IXmlSerializable
#endif
    {
        #region Properties

        /// <summary>
        /// Gets/Sets the Base Uri for the Graph
        /// </summary>
        Uri BaseUri 
        { 
            get; 
            set; 
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
        INamespaceMapper NamespaceMap 
        {
            get;
        }

        /// <summary>
        /// Gets the Nodes of the Graph
        /// </summary>
        IEnumerable<INode> Nodes 
        {
            get;
        }

        /// <summary>
        /// Gets the Triple Collection for the Graph
        /// </summary>
        BaseTripleCollection Triples 
        { 
            get; 
        }

        #endregion

        #region Assertion & Retraction

        /// <summary>
        /// Asserts a Triple in the Graph
        /// </summary>
        /// <param name="t">A Triple</param>
        bool Assert(Triple t);

        /// <summary>
        /// Asserts an Enumerable of Triples in the Graph
        /// </summary>
        /// <param name="ts">An Enumerable of Triples</param>
        bool Assert(IEnumerable<Triple> ts);

        /// <summary>
        /// Retracts a Triple from the Graph
        /// </summary>
        /// <param name="t">A Triple</param>
        bool Retract(Triple t);

        /// <summary>
        /// Retracts an Enumerable of Triples from the Graph
        /// </summary>
        /// <param name="ts">Enumerable of Triples</param>
        bool Retract(IEnumerable<Triple> ts);

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
        /// Creates a URI Node that corresponds to the Base URI of the Graph
        /// </summary>
        /// <returns></returns>
        IUriNode CreateUriNode();

        /// <summary>
        /// Creates a URI Node for the given QName using the Graphs NamespaceMap to resolve the QName
        /// </summary>
        /// <param name="qname">QName</param>
        /// <returns></returns>
        IUriNode CreateUriNode(string qname);

        #endregion

        #region Selection

        /// <summary>
        /// Selects the Blank Node with the given ID if it exists in the Graph, returns null otherwise
        /// </summary>
        /// <param name="nodeId">Node ID</param>
        /// <returns>The Node if it exists in the Graph or null</returns>
        IBlankNode GetBlankNode(string nodeId);

        /// <summary>
        /// Selects the Literal Node with the given Value and Language if it exists in the Graph, returns null otherwise
        /// </summary>
        /// <param name="literal">Value of the Literal</param>
        /// <param name="langspec">Language Specifier of the Literal</param>
        /// <returns>The Node if it exists in the Graph or null</returns>
        ILiteralNode GetLiteralNode(string literal, string langspec);

        /// <summary>
        /// Selects the Literal Node with the given Value if it exists in the Graph, returns null otherwise
        /// </summary>
        /// <param name="literal">Value of the Literal</param>
        /// <returns>The Node if it exists in the Graph or null</returns>
        ILiteralNode GetLiteralNode(string literal);

        /// <summary>
        /// Selects the Literal Node with the given Value and DataType if it exists in the Graph, returns otherwise
        /// </summary>
        /// <param name="literal">Value of the Literal</param>
        /// <param name="datatype">Data Type of the Literal</param>
        /// <returns>The Node if it exists in the Graph or null</returns>
        ILiteralNode GetLiteralNode(string literal, Uri datatype);

        /// <summary>
        /// Selects all Triples which have a Uri Node with the given Uri
        /// </summary>
        /// <param name="uri">Uri</param>
        /// <returns></returns>
        IEnumerable<Triple> GetTriples(Uri uri);

        /// <summary>
        /// Selects all Triples which contain the given Node
        /// </summary>
        /// <param name="n">Node</param>
        /// <returns></returns>
        IEnumerable<Triple> GetTriples(INode n);

        /// <summary>
        /// Selects all Triples where the Object is a Uri Node with the given Uri
        /// </summary>
        /// <param name="u">Uri</param>
        /// <returns></returns>
        IEnumerable<Triple> GetTriplesWithObject(Uri u);

        /// <summary>
        /// Selects all Triples where the Object is a given Node
        /// </summary>
        /// <param name="n">Node</param>
        /// <returns></returns>
        IEnumerable<Triple> GetTriplesWithObject(INode n);

        /// <summary>
        /// Selects all Triples where the Predicate is a given Node
        /// </summary>
        /// <param name="n">Node</param>
        /// <returns></returns>
        IEnumerable<Triple> GetTriplesWithPredicate(INode n);

        /// <summary>
        /// Selects all Triples where the Predicate is a Uri Node with the given Uri
        /// </summary>
        /// <param name="u">Uri</param>
        /// <returns></returns>
        IEnumerable<Triple> GetTriplesWithPredicate(Uri u);

        /// <summary>
        /// Selects all Triples where the Subject is a given Node
        /// </summary>
        /// <param name="n">Node</param>
        /// <returns></returns>
        IEnumerable<Triple> GetTriplesWithSubject(INode n);

        /// <summary>
        /// Selects all Triples where the Subject is a Uri Node with the given Uri
        /// </summary>
        /// <param name="u">Uri</param>
        /// <returns></returns>
        IEnumerable<Triple> GetTriplesWithSubject(Uri u);

        /// <summary>
        /// Selects all Triples with the given Subject and Predicate
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <param name="pred">Predicate</param>
        /// <returns></returns>
        IEnumerable<Triple> GetTriplesWithSubjectPredicate(INode subj, INode pred);

        /// <summary>
        /// Selects all Triples with the given Subject and Object
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        IEnumerable<Triple> GetTriplesWithSubjectObject(INode subj, INode obj);

        /// <summary>
        /// Selects all Triples with the given Predicate and Object
        /// </summary>
        /// <param name="pred">Predicate</param>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        IEnumerable<Triple> GetTriplesWithPredicateObject(INode pred, INode obj);

        /// <summary>
        /// Selects the Uri Node with the given QName if it exists in the Graph, returns null otherwise
        /// </summary>
        /// <param name="qname">QName</param>
        /// <returns>The Node if it exists in the Graph or null</returns>
        IUriNode GetUriNode(string qname);

        /// <summary>
        /// Selects the Uri Node with the given Uri if it exists in the Graph, returns null otherwise
        /// </summary>
        /// <param name="uri">Uri</param>
        /// <returns>The Node if it exists in the Graph or null</returns>
        IUriNode GetUriNode(Uri uri);

        /// <summary>
        /// Gets whether a given Triple is in this Graph
        /// </summary>
        /// <param name="t">Triple to test</param>
        /// <returns></returns>
        bool ContainsTriple(Triple t);

        #endregion

        #region Advanced Graph Operations

        /// <summary>
        /// Merges the given Graph into this Graph
        /// </summary>
        /// <param name="g">Graph to merge</param>
        /// <remarks>
        /// <para>
        /// The Graph should raise the <see cref="MergeRequested">MergeRequested</see> event at the start of the Merge operation and abort the operation if the operation is cancelled by an event handler.  On completing the Merge the <see cref="Merged">Merged</see> event should be raised.
        /// </para>
        /// </remarks>
        void Merge(IGraph g);

        /// <summary>
        /// Merges the given Graph into this Graph
        /// </summary>
        /// <param name="g">Graph to merge</param>
        /// <param name="keepOriginalGraphUri">Indicates that the Merge should preserve the Graph URIs of Nodes</param>
        /// <remarks>
        /// <para>
        /// The Graph should raise the <see cref="MergeRequested">MergeRequested</see> event at the start of the Merge operation and abort the operation if the operation is cancelled by an event handler.  On completing the Merge the <see cref="Merged">Merged</see> event should be raised.
        /// </para>
        /// </remarks>
        void Merge(IGraph g, bool keepOriginalGraphUri);

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

        #endregion

        #region Helper Functions

        /// <summary>
        /// Resolves a QName into a URI using the Namespace Map and Base URI of this Graph
        /// </summary>
        /// <param name="qname">QName</param>
        /// <returns></returns>
        Uri ResolveQName(String qname);

        #endregion

        #region Events

        /// <summary>
        /// Event which is raised when a Triple is asserted in the Graph
        /// </summary>
        /// <remarks>
        /// Whenever this event is raised the <see cref="Changed">Changed</see> event should also be raised
        /// </remarks>
        event TripleEventHandler TripleAsserted;

        /// <summary>
        /// Event which is raised when a Triple is retracted from the Graph
        /// </summary>
        /// <remarks>
        /// Whenever this event is raised the <see cref="Changed">Changed</see> event should also be raised
        /// </remarks>
        event TripleEventHandler TripleRetracted;

        /// <summary>
        /// Event which is raised when the Graph contents change
        /// </summary>
        event GraphEventHandler Changed;

        /// <summary>
        /// Event which is raised just before the Graph is cleared of its contents
        /// </summary>
        event CancellableGraphEventHandler ClearRequested;

        /// <summary>
        /// Event which is raised after the Graph is cleared of its contents
        /// </summary>
        event GraphEventHandler Cleared;

        /// <summary>
        /// Event which is raised just before a Merge operation begins on the Graph
        /// </summary>
        event CancellableGraphEventHandler MergeRequested;

        /// <summary>
        /// Event which is raised when a Merge operation is completed on the Graph
        /// </summary>
        event GraphEventHandler Merged;

        #endregion
    }

    /// <summary>
    /// Interface for RDF Graphs which provide Transactions i.e. changes to them can be Flushed (committed) or Discard (rolled back) as desired
    /// </summary>
    public interface ITransactionalGraph
        : IGraph
    {
        /// <summary>
        /// Flushes any changes to the Graph
        /// </summary>
        void Flush();

        /// <summary>
        /// Discards any changes to the Graph
        /// </summary>
        void Discard();
    }
}
