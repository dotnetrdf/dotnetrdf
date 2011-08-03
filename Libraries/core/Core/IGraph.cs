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
#if !NO_DATA
using System.Data;
#endif
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
#if !SILVERLIGHT
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
        /// Gets the Node Collection for the Graph
        /// </summary>
        BaseNodeCollection Nodes 
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
        /// Asserts a List of Triples in the Graph
        /// </summary>
        /// <param name="ts">List of Triples</param>
        void Assert(List<Triple> ts);

        /// <summary>
        /// Asserts an array of Triples in the Graph
        /// </summary>
        /// <param name="ts">Array of Triples</param>
        void Assert(Triple[] ts);

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
        /// Retracts an Array of Triples from the Graph
        /// </summary>
        /// <param name="ts">Array of Triples</param>
        void Retract(Triple[] ts);

        /// <summary>
        /// Retracts a Triple from the Graph
        /// </summary>
        /// <param name="t">A Triple</param>
        void Retract(Triple t);

        /// <summary>
        /// Retracts a List of Triples from the Graph
        /// </summary>
        /// <param name="ts">List of Triples</param>
        void Retract(List<Triple> ts);

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
        /// Selects the Blank Node with the given ID
        /// </summary>
        /// <param name="nodeId">Node ID</param>
        /// <returns></returns>
        IBlankNode GetBlankNode(string nodeId);

        /// <summary>
        /// Selects the Literal Node with the given Value and Language
        /// </summary>
        /// <param name="literal">Value of the Literal</param>
        /// <param name="langspec">Language Specifier of the Literal</param>
        /// <returns></returns>
        ILiteralNode GetLiteralNode(string literal, string langspec);

        /// <summary>
        /// Selects the Literal Node with the given Value
        /// </summary>
        /// <param name="literal">Value of the Literal</param>
        /// <returns></returns>
        ILiteralNode GetLiteralNode(string literal);

        /// <summary>
        /// Selects the Literal Node with the given Value and DataType
        /// </summary>
        /// <param name="literal">Value of the Literal</param>
        /// <param name="datatype">Data Type of the Literal</param>
        /// <returns></returns>
        ILiteralNode GetLiteralNode(string literal, Uri datatype);

        /// <summary>
        /// Selects all Nodes that meet the criteria of a given ISelector
        /// </summary>
        /// <param name="selector">A Selector on Nodes</param>
        /// <returns></returns>
        IEnumerable<INode> GetNodes(ISelector<INode> selector);

        /// <summary>
        /// Selects all Triples which are selected by a chain of Selectors where the results of each Selector influence the next selector and selection at each stage is over the entire Graph
        /// </summary>
        /// <param name="firstSelector">First Selector in the Chain</param>
        /// <param name="selectorChain">Dependent Selectors which form the rest of the Chain</param>
        /// <returns></returns>
        IEnumerable<Triple> GetTriples(ISelector<Triple> firstSelector, List<IDependentSelector<Triple>> selectorChain);

        /// <summary>
        /// Selects all Triples which are selected by a chain of Selectors where each Selector is independent and selection at each stage is over the results of the previous selection stages
        /// </summary>
        /// <param name="selectorChain">Chain of Independent Selectors</param>
        /// <returns></returns>
        IEnumerable<Triple> GetTriples(List<ISelector<Triple>> selectorChain);

        /// <summary>
        /// Selects all Triples which have a Uri Node with the given Uri
        /// </summary>
        /// <param name="uri">Uri</param>
        /// <returns></returns>
        IEnumerable<Triple> GetTriples(Uri uri);

        /// <summary>
        /// Selects all Triples which meet the criteria of an ISelector
        /// </summary>
        /// <param name="selector">A Selector on Triples</param>
        /// <returns></returns>
        IEnumerable<Triple> GetTriples(ISelector<Triple> selector);

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
        /// Selects all Triples where the Object Node meets the criteria of an ISelector
        /// </summary>
        /// <param name="selector">A Selector on Nodes</param>
        /// <returns></returns>
        IEnumerable<Triple> GetTriplesWithObject(ISelector<INode> selector);

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
        /// Selects all Triples where the Predicate meets the criteria of an ISelector
        /// </summary>
        /// <param name="selector">A Selector on Nodes</param>
        /// <returns></returns>
        IEnumerable<Triple> GetTriplesWithPredicate(ISelector<INode> selector);

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
        /// Selects all Triples where the Subject meets the criteria of an ISelector
        /// </summary>
        /// <param name="selector">A Selector on Nodes</param>
        /// <returns></returns>
        IEnumerable<Triple> GetTriplesWithSubject(ISelector<INode> selector);

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
        /// Selects the Uri Node with the given QName
        /// </summary>
        /// <param name="qname">QName</param>
        /// <returns></returns>
        IUriNode GetUriNode(string qname);

        /// <summary>
        /// Selects the Uri Node with the given Uri
        /// </summary>
        /// <param name="uri">Uri</param>
        /// <returns></returns>
        IUriNode GetUriNode(Uri uri);

        /// <summary>
        /// Checks whether any Triples meeting the criteria of an ISelector can be found
        /// </summary>
        /// <param name="selector">A Selector on Triples</param>
        /// <returns></returns>
        bool TriplesExist(ISelector<Triple> selector);

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
    public interface ITransactionalGraph : IGraph
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
