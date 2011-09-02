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
using VDS.RDF.Query;
using VDS.RDF.Query.Inference;
using VDS.RDF.Update;

namespace VDS.RDF
{
    /// <summary>
    /// Interface for Triple Stores
    /// </summary>
    /// <remarks>A Triple Store may be a representation of some storage backed actual store or just a temporary collection of Graphs created for working with.  Note that an implementation is not required to provide a definitive view of a Triple Store and may only provide a limited/partial snapshot of the underlying store.  Check the documentation for the various implementations to see what type of view of a Triple Store they actually provide.</remarks>
    public interface ITripleStore 
        : IDisposable
    {
        #region Properties

        /// <summary>
        /// Gets whether a TripleStore is Empty
        /// </summary>
        bool IsEmpty
        {
            get;
        }

        /// <summary>
        /// Gets the Graph Collection of Graphs in this Triple Store
        /// </summary>
        BaseGraphCollection Graphs
        {
            get;
        }

        /// <summary>
        /// Gets all the Triples in the Triple Store which are currently loaded in memory (see remarks)
        /// </summary>
        /// <remarks>Since a Triple Store object may represent only a snapshot of the underlying Store evaluating this enumerator may only return some of the Triples in the Store and may depending on specific Triple Store return nothing.</remarks>
        IEnumerable<Triple> Triples
        {
            get;
        }

        #endregion

        #region Loading & Unloading Graphs

        /// <summary>
        /// Adds a Graph into the Triple Store
        /// </summary>
        /// <param name="g">Graph to add</param>
        void Add(IGraph g);

        /// <summary>
        /// Adds a Graph into the Triple Store
        /// </summary>
        /// <param name="g">Graph to add</param>
        /// <param name="mergeIfExists">Controls whether the Graph should be merged with an existing Graph of the same Uri if it already exists in the Triple Store</param>
        void Add(IGraph g, bool mergeIfExists);

        /// <summary>
        /// Adds a Graph into the Triple Store by dereferencing the Graph Uri to get the RDF and then load the resulting Graph into the Triple Store
        /// </summary>
        /// <param name="graphUri">Uri of the Graph to be added</param>
        void AddFromUri(Uri graphUri);

        /// <summary>
        /// Adds a Graph into the Triple Store by dereferencing the Graph Uri to get the RDF and then load the resulting Graph into the Triple Store
        /// </summary>
        /// <param name="graphUri">Uri of the Graph to be added</param>
        /// <param name="mergeIfExists">Controls whether the Graph should be merged with an existing Graph of the same Uri if it already exists in the Triple Store</param>
        void AddFromUri(Uri graphUri, bool mergeIfExists);

        /// <summary>
        /// Removes a Graph from the Triple Store
        /// </summary>
        /// <param name="graphUri">Graph Uri of the Graph to remove</param>
        void Remove(Uri graphUri);

        #endregion

        #region Graph Retrieval

        /// <summary>
        /// Checks whether the Graph with the given Uri is in this Triple Store
        /// </summary>
        /// <param name="graphUri">Graph Uri</param>
        /// <returns></returns>
        bool HasGraph(Uri graphUri);

        /// <summary>
        /// Gets a Graph from the Triple Store
        /// </summary>
        /// <param name="graphUri">Uri of the Graph to retrieve</param>
        /// <returns></returns>
        IGraph Graph(Uri graphUri);

        #endregion

        #region Events

        /// <summary>
        /// Event which is raised when a Graph is added
        /// </summary>
        event TripleStoreEventHandler GraphAdded;

        /// <summary>
        /// Event which is raised when a Graph is removed
        /// </summary>
        event TripleStoreEventHandler GraphRemoved;

        /// <summary>
        /// Event which is raised when a Graphs contents changes
        /// </summary>
        event TripleStoreEventHandler GraphChanged;

        /// <summary>
        /// Event which is raised when a Graph is cleared
        /// </summary>
        event TripleStoreEventHandler GraphCleared;

        /// <summary>
        /// Event which is raised when a Graph has a merge operation performed on it
        /// </summary>
        event TripleStoreEventHandler GraphMerged;

        #endregion

    }

    /// <summary>
    /// Interface for Triple Stores which can be queried in memory using either <see cref="ISelector&lt;T&gt;">ISelector&lt;T&gt;"</see> or the SPARQL implementation contained in this library
    /// </summary>
    /// <remarks>
    /// <para>
    /// An in memory Triple Store will typically load most of the Graphs and consequently Triples contained within it into Memory as the in memory SPARQL implementation only operates over the part of the Triple Store loaded in memory.  This being said there is no reason why an in memory store can't provide a Snapshot view of an underlying store to allow only the relevant parts of Store to be loaded and queried (the <see cref="OnDemandTripleStore">OnDemandTripleStore</see> does just this)
    /// </para>
    /// <para>
    /// All the Selection Methods which do not specify a subset of Graphs on such a Triple Store <strong>should</strong> operate over the <see cref="IInMemoryQueryableStore.QueryTriples">QueryTriples</see> enumerable
    /// </para>
    /// </remarks>
    public interface IInMemoryQueryableStore
        : ITripleStore
    {
        /// <summary>
        /// Returns whether a given Triple is contained anywhere in the Query Triples
        /// </summary>
        /// <param name="t">Triple to check for existence of</param>
        /// <returns></returns>
        bool Contains(Triple t);

        #region Selection over Entire Triple Store

        /// <summary>
        /// Selects all Nodes that meet the criteria of a given ISelector from all the Query Triples
        /// </summary>
        /// <param name="selector">A Selector on Nodes</param>
        /// <returns></returns>
        IEnumerable<INode> GetNodes(ISelector<INode> selector);

        /// <summary>
        /// Selects all Triples which are selected by a chain of Selectors from all the Query Triples
        /// </summary>
        /// <param name="firstSelector">First Selector in the Chain</param>
        /// <param name="selectorChain">Dependent Selectors which form the rest of the Chain</param>
        /// <returns></returns>
        IEnumerable<Triple> GetTriples(ISelector<Triple> firstSelector, List<IDependentSelector<Triple>> selectorChain);

        /// <summary>
        /// Selects all Triples which are selected by a chain of Selectors from all the Query Triples
        /// </summary>
        /// <param name="selectorChain">Chain of Independent Selectors</param>
        /// <returns></returns>
        IEnumerable<Triple> GetTriples(List<ISelector<Triple>> selectorChain);

        /// <summary>
        /// Selects all Triples which have a Uri Node with the given Uri from all the Query Triples
        /// </summary>
        /// <param name="uri">Uri</param>
        /// <returns></returns>
        IEnumerable<Triple> GetTriples(Uri uri);

        /// <summary>
        /// Selects all Triples which meet the criteria of an ISelector from all the Query Triples
        /// </summary>
        /// <param name="selector">A Selector on Triples</param>
        /// <returns></returns>
        IEnumerable<Triple> GetTriples(ISelector<Triple> selector);

        /// <summary>
        /// Selects all Triples which contain the given Node from all the Query Triples
        /// </summary>
        /// <param name="n">Node</param>
        /// <returns></returns>
        IEnumerable<Triple> GetTriples(INode n);

        /// <summary>
        /// Selects all Triples where the Object is a Uri Node with the given Uri from all the Query Triples
        /// </summary>
        /// <param name="u">Uri</param>
        /// <returns></returns>
        IEnumerable<Triple> GetTriplesWithObject(Uri u);

        /// <summary>
        /// Selects all Triples where the Object Node meets the criteria of an ISelector from all the Query Triples
        /// </summary>
        /// <param name="selector">A Selector on Nodes</param>
        /// <returns></returns>
        IEnumerable<Triple> GetTriplesWithObject(ISelector<INode> selector);

        /// <summary>
        /// Selects all Triples where the Object is a given Node from all the Query Triples
        /// </summary>
        /// <param name="n">Node</param>
        /// <returns></returns>
        IEnumerable<Triple> GetTriplesWithObject(INode n);

        /// <summary>
        /// Selects all Triples where the Predicate is a given Node from all the Query Triples
        /// </summary>
        /// <param name="n">Node</param>
        /// <returns></returns>
        IEnumerable<Triple> GetTriplesWithPredicate(INode n);

        /// <summary>
        /// Selects all Triples where the Predicate is a Uri Node with the given Uri from all the Query Triples
        /// </summary>
        /// <param name="u">Uri</param>
        /// <returns></returns>
        IEnumerable<Triple> GetTriplesWithPredicate(Uri u);

        /// <summary>
        /// Selects all Triples where the Predicate meets the criteria of an ISelector from all the Query Triples
        /// </summary>
        /// <param name="selector">A Selector on Nodes</param>
        /// <returns></returns>
        IEnumerable<Triple> GetTriplesWithPredicate(ISelector<INode> selector);

        /// <summary>
        /// Selects all Triples where the Subject is a given Node from all the Query Triples
        /// </summary>
        /// <param name="n">Node</param>
        /// <returns></returns>
        IEnumerable<Triple> GetTriplesWithSubject(INode n);

        /// <summary>
        /// Selects all Triples where the Subject is a Uri Node with the given Uri from all the Query Triples
        /// </summary>
        /// <param name="u">Uri</param>
        /// <returns></returns>
        IEnumerable<Triple> GetTriplesWithSubject(Uri u);

        /// <summary>
        /// Selects all Triples where the Subject meets the criteria of an ISelector from all the Query Triples
        /// </summary>
        /// <param name="selector">A Selector on Nodes</param>
        /// <returns></returns>
        IEnumerable<Triple> GetTriplesWithSubject(ISelector<INode> selector);

        /// <summary>
        /// Checks whether any Triples meeting the criteria of an ISelector can be found from all the Query Triples
        /// </summary>
        /// <param name="selector">A Selector on Triples</param>
        /// <returns></returns>
        bool TriplesExist(ISelector<Triple> selector);

        /// <summary>
        /// Selects all the Triples with the given Subject-Predicate pair from all the Query Triples
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <param name="pred">Predicate</param>
        /// <returns></returns>
        IEnumerable<Triple> GetTriplesWithSubjectPredicate(INode subj, INode pred);

        /// <summary>
        /// Selects all the Triples with the given Predicate-Object pair from all the Query Triples
        /// </summary>
        /// <param name="pred">Predicate</param>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        IEnumerable<Triple> GetTriplesWithPredicateObject(INode pred, INode obj);

        /// <summary>
        /// Selects all the Triples with the given Subject-Object pair from all the Query Triples
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        IEnumerable<Triple> GetTriplesWithSubjectObject(INode subj, INode obj);

        #endregion

        #region Selection over a Subset of the Store

        /// <summary>
        /// Selects all Nodes that meet the criteria of a given ISelector from a Subset of Graphs in the Triple Store
        /// </summary>
        /// <param name="selector">A Selector on Nodes</param>
        /// <param name="graphUris">List of the Graph URIs of Graphs you want to select over</param>
        /// <returns></returns>
        IEnumerable<INode> GetNodes(List<Uri> graphUris, ISelector<INode> selector);

        /// <summary>
        /// Selects all Triples which are selected by a chain of Selectors from a Subset of Graphs in the Triple Store where the results of each Selector influence the next selector and selection at each stage is over the selected subset of Graphs
        /// </summary>
        /// <param name="graphUris">List of the Graph URIs of Graphs you want to select over</param>
        /// <param name="firstSelector">First Selector in the Chain</param>
        /// <param name="selectorChain">Dependent Selectors which form the rest of the Chain</param>
        /// <returns></returns>
        IEnumerable<Triple> GetTriples(List<Uri> graphUris, ISelector<Triple> firstSelector, List<IDependentSelector<Triple>> selectorChain);

        /// <summary>
        /// Selects all Triples which are selected by a chain of Selectors from a Subset of Graphs in the Triple Store where each Selector is independent and selection at each stage is over the results of the previous selection stages
        /// </summary>
        /// <param name="graphUris">List of the Graph URIs of Graphs you want to select over</param>
        /// <param name="selectorChain">Chain of Independent Selectors</param>
        /// <returns></returns>
        IEnumerable<Triple> GetTriples(List<Uri> graphUris, List<ISelector<Triple>> selectorChain);

        /// <summary>
        /// Selects all Triples which have a Uri Node with the given Uri from a Subset of Graphs in the Triple Store
        /// </summary>
        /// <param name="graphUris">List of the Graph URIs of Graphs you want to select over</param>
        /// <param name="uri">Uri</param>
        /// <returns></returns>
        IEnumerable<Triple> GetTriples(List<Uri> graphUris, Uri uri);

        /// <summary>
        /// Selects all Triples which meet the criteria of an ISelector from a Subset of Graphs in the Triple Store
        /// </summary>
        /// <param name="graphUris">List of the Graph URIs of Graphs you want to select over</param>
        /// <param name="selector">A Selector on Triples</param>
        /// <returns></returns>
        IEnumerable<Triple> GetTriples(List<Uri> graphUris, ISelector<Triple> selector);

        /// <summary>
        /// Selects all Triples which contain the given Node from a Subset of Graphs in the Triple Store
        /// </summary>
        /// <param name="graphUris">List of the Graph URIs of Graphs you want to select over</param>
        /// <param name="n">Node</param>
        /// <returns></returns>
        IEnumerable<Triple> GetTriples(List<Uri> graphUris, INode n);

        /// <summary>
        /// Selects all Triples where the Object is a Uri Node with the given Uri from a Subset of Graphs in the Triple Store
        /// </summary>
        /// <param name="graphUris">List of the Graph URIs of Graphs you want to select over</param>
        /// <param name="u">Uri</param>
        /// <returns></returns>
        IEnumerable<Triple> GetTriplesWithObject(List<Uri> graphUris, Uri u);

        /// <summary>
        /// Selects all Triples where the Object Node meets the criteria of an ISelector from a Subset of Graphs in the Triple Store
        /// </summary>
        /// <param name="graphUris">List of the Graph URIs of Graphs you want to select over</param>
        /// <param name="selector">A Selector on Nodes</param>
        /// <returns></returns>
        IEnumerable<Triple> GetTriplesWithObject(List<Uri> graphUris, ISelector<INode> selector);

        /// <summary>
        /// Selects all Triples where the Object is a given Node from a Subset of Graphs in the Triple Store
        /// </summary>
        /// <param name="graphUris">List of the Graph URIs of Graphs you want to select over</param>
        /// <param name="n">Node</param>
        /// <returns></returns>
        IEnumerable<Triple> GetTriplesWithObject(List<Uri> graphUris, INode n);

        /// <summary>
        /// Selects all Triples where the Predicate is a given Node from a Subset of Graphs in the Triple Store
        /// </summary>
        /// <param name="graphUris">List of the Graph URIs of Graphs you want to select over</param>
        /// <param name="n">Node</param>
        /// <returns></returns>
        IEnumerable<Triple> GetTriplesWithPredicate(List<Uri> graphUris, INode n);

        /// <summary>
        /// Selects all Triples where the Predicate is a Uri Node with the given Uri from a Subset of Graphs in the Triple Store
        /// </summary>
        /// <param name="graphUris">List of the Graph URIs of Graphs you want to select over</param>
        /// <param name="u">Uri</param>
        /// <returns></returns>
        IEnumerable<Triple> GetTriplesWithPredicate(List<Uri> graphUris, Uri u);

        /// <summary>
        /// Selects all Triples where the Predicate meets the criteria of an ISelector from a Subset of Graphs in the Triple Store
        /// </summary>
        /// <param name="graphUris">List of the Graph URIs of Graphs you want to select over</param>
        /// <param name="selector">A Selector on Nodes</param>
        /// <returns></returns>
        IEnumerable<Triple> GetTriplesWithPredicate(List<Uri> graphUris, ISelector<INode> selector);

        /// <summary>
        /// Selects all Triples where the Subject is a given Node from a Subset of Graphs in the Triple Store
        /// </summary>
        /// <param name="graphUris">List of the Graph URIs of Graphs you want to select over</param>
        /// <param name="n">Node</param>
        /// <returns></returns>
        IEnumerable<Triple> GetTriplesWithSubject(List<Uri> graphUris, INode n);

        /// <summary>
        /// Selects all Triples where the Subject is a Uri Node with the given Uri from a Subset of Graphs in the Triple Store
        /// </summary>
        /// <param name="graphUris">List of the Graph URIs of Graphs you want to select over</param>
        /// <param name="u">Uri</param>
        /// <returns></returns>
        IEnumerable<Triple> GetTriplesWithSubject(List<Uri> graphUris, Uri u);

        /// <summary>
        /// Selects all Triples where the Subject meets the criteria of an ISelector from a Subset of Graphs in the Triple Store
        /// </summary>
        /// <param name="graphUris">List of the Graph URIs of Graphs you want to select over</param>
        /// <param name="selector">A Selector on Nodes</param>
        /// <returns></returns>
        IEnumerable<Triple> GetTriplesWithSubject(List<Uri> graphUris, ISelector<INode> selector);

        /// <summary>
        /// Checks whether any Triples meeting the criteria of an ISelector can be found from a Subset of Graphs in the Triple Store
        /// </summary>
        /// <param name="graphUris">List of the Graph URIs of Graphs you want to select over</param>
        /// <param name="selector">A Selector on Triples</param>
        /// <returns></returns>
        bool TriplesExist(List<Uri> graphUris, ISelector<Triple> selector);

        #endregion

        #region In Memory SPARQL

        /// <summary>
        /// Executes a SPARQL Query on the Triple Store
        /// </summary>
        /// <param name="query">SPARQL Query as an unparsed string</param>
        /// <returns></returns>
        Object ExecuteQuery(String query);

        /// <summary>
        /// Executes a SPARQL Query on the Triple Store
        /// </summary>
        /// <param name="query">SPARQL Query as a <see cref="SparqlQuery">SparqlQuery</see> instance</param>
        /// <returns></returns>
        Object ExecuteQuery(SparqlQuery query);

        /// <summary>
        /// Executes a SPARQL Query on the Triple Store processing the results with an appropriate handler from those provided
        /// </summary>
        /// <param name="rdfHandler">RDF Handler</param>
        /// <param name="resultsHandler">Results Handler</param>
        /// <param name="query">SPARQL Query as an unparsed string</param>
        /// <returns></returns>
        void ExecuteQuery(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, String query);

        /// <summary>
        /// Executes a SPARQL Query on the Triple Store processing the results with an appropriate handler from those provided
        /// </summary>
        /// <param name="rdfHandler">RDF Handler</param>
        /// <param name="resultsHandler">Results Handler</param>
        /// <param name="query">Parsed SPARQL Query</param>
        /// <returns></returns>
        void ExecuteQuery(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, SparqlQuery query);

        #endregion
    }

    /// <summary>
    /// Interface for Triple Stores which can be queried natively i.e. the Stores provide their own SPARQL implementations
    /// </summary>
    /// <remarks>
    /// A Natively Queryable store will typically not load its Graphs and Triples into memory as this is generally unecessary.
    /// </remarks>
    public interface INativelyQueryableStore 
        : ITripleStore
    {
        /// <summary>
        /// Executes a SPARQL Query on the Triple Store
        /// </summary>
        /// <param name="query">Sparql Query as unparsed String</param>
        /// <returns></returns>
        /// <remarks>
        /// This assumes that the Store has access to some native SPARQL query processor on/at the Store which will be used to return the results.  Implementations should parse the returned result into a <see cref="SparqlResultSet">SparqlResultSet</see> or <see cref="Graph">Graph</see>.
        /// </remarks>
        Object ExecuteQuery(String query);

        /// <summary>
        /// Executes a SPARQL Query on the Triple Store processing the results using an appropriate handler from those provided
        /// </summary>
        /// <param name="rdfHandler">RDF Handler</param>
        /// <param name="resultsHandler">Results Handler</param>
        /// <param name="query">SPARQL Query as unparsed String</param>
        void ExecuteQuery(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, String query);
    }

    /// <summary>
    /// Interface for Triple Stores which support SPARQL Update as per the SPARQL 1.1 specifications
    /// </summary>
    /// <remarks>
    /// <para>
    /// A Store which supports this may implement various access control mechanisms which limit what operations are actually permitted
    /// </para>
    /// <para>
    /// It is the responsibility of the Store class to ensure that commands are permissible before invoking them
    /// </para>
    /// </remarks>
    public interface IUpdateableTripleStore
        : ITripleStore
    {
        /// <summary>
        /// Executes an Update against the Triple Store
        /// </summary>
        /// <param name="update">SPARQL Update Command(s)</param>
        /// <remarks>
        /// As per the SPARQL 1.1 Update specification the command string may be a sequence of commands
        /// </remarks>
        void ExecuteUpdate(String update);

        /// <summary>
        /// Executes a single Update Command against the Triple Store
        /// </summary>
        /// <param name="update">SPARQL Update Command</param>
        void ExecuteUpdate(SparqlUpdateCommand update);

        /// <summary>
        /// Executes a set of Update Commands against the Triple Store
        /// </summary>
        /// <param name="updates">SPARQL Update Command Set</param>
        void ExecuteUpdate(SparqlUpdateCommandSet updates);
    }

    /// <summary>
    /// Interface for Triple Stores which can have a <see cref="InferenceEngine">InferenceEngine</see> attached to them
    /// </summary>
    public interface IInferencingTripleStore
        : ITripleStore
    {
        /// <summary>
        /// Adds an Inference Engine to the Triple Store
        /// </summary>
        /// <param name="reasoner">Reasoner to add</param>
        void AddInferenceEngine(IInferenceEngine reasoner);

        /// <summary>
        /// Removes an Inference Engine from the Triple Store
        /// </summary>
        /// <param name="reasoner">Reasoner to remove</param>
        void RemoveInferenceEngine(IInferenceEngine reasoner);

        /// <summary>
        /// Clears all Inference Engines from the Triple Store
        /// </summary>
        void ClearInferenceEngines();

        /// <summary>
        /// Applies Inference to the given Graph
        /// </summary>
        /// <param name="g">Graph to apply inference to</param>
        /// <remarks>
        /// Allows you to apply Inference to a Graph even if you're not putting that Graph into the Store
        /// </remarks>
        void ApplyInference(IGraph g);
    }

    /// <summary>
    /// Interface for Triple Stores which are backed by some storage layer that may delay persistence and thus require flushing to ensure changes are persisted to the backing store, as a by product such stores will typically have some notion of transactionality
    /// </summary>
    public interface ITransactionalStore
        : ITripleStore
    {
        /// <summary>
        /// Flushes any outstanding changes to the underlying store
        /// </summary>
        void Flush();

        /// <summary>
        /// Discards any outstanding changes to the underlying store
        /// </summary>
        void Discard();
    }
}
