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
        bool Add(IGraph g);

        /// <summary>
        /// Adds a Graph into the Triple Store
        /// </summary>
        /// <param name="g">Graph to add</param>
        /// <param name="mergeIfExists">Controls whether the Graph should be merged with an existing Graph of the same Uri if it already exists in the Triple Store</param>
        bool Add(IGraph g, bool mergeIfExists);

        /// <summary>
        /// Adds a Graph into the Triple Store by dereferencing the Graph Uri to get the RDF and then load the resulting Graph into the Triple Store
        /// </summary>
        /// <param name="graphUri">Uri of the Graph to be added</param>
        bool AddFromUri(Uri graphUri);

        /// <summary>
        /// Adds a Graph into the Triple Store by dereferencing the Graph Uri to get the RDF and then load the resulting Graph into the Triple Store
        /// </summary>
        /// <param name="graphUri">Uri of the Graph to be added</param>
        /// <param name="mergeIfExists">Controls whether the Graph should be merged with an existing Graph of the same Uri if it already exists in the Triple Store</param>
        bool AddFromUri(Uri graphUri, bool mergeIfExists);

        /// <summary>
        /// Removes a Graph from the Triple Store
        /// </summary>
        /// <param name="graphUri">Graph Uri of the Graph to remove</param>
        bool Remove(Uri graphUri);

        #endregion

        #region Graph Retrieval

        /// <summary>
        /// Checks whether the Graph with the given Uri is in this Triple Store
        /// </summary>
        /// <param name="graphUri">Graph Uri</param>
        /// <returns></returns>
        bool HasGraph(Uri graphUri);

        /// <summary>
        /// Gets a Graph from the Triple Store;
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        IGraph this[Uri graphUri]
        {
            get;
        }

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
    /// Interface for Triple Stores which can be queried in memory using method calls or the SPARQL implementation contained in this library
    /// </summary>
    /// <remarks>
    /// <para>
    /// An in memory Triple Store will typically load most of the Graphs and consequently Triples contained within it into Memory as the in memory SPARQL implementation only operates over the part of the Triple Store loaded in memory.  This being said there is no reason why an in memory store can't provide a Snapshot view of an underlying store to allow only the relevant parts of Store to be loaded and queried.
    /// </para>
    /// <para>
    /// All the Selection Methods which do not specify a subset of Graphs on such a Triple Store <strong>should</strong> operate over the entire store
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
        /// Selects all Triples which have a Uri Node with the given Uri from all the Query Triples
        /// </summary>
        /// <param name="uri">Uri</param>
        /// <returns></returns>
        IEnumerable<Triple> GetTriples(Uri uri);

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
        /// Selects all Triples which have a Uri Node with the given Uri from a Subset of Graphs in the Triple Store
        /// </summary>
        /// <param name="graphUris">List of the Graph URIs of Graphs you want to select over</param>
        /// <param name="uri">Uri</param>
        /// <returns></returns>
        IEnumerable<Triple> GetTriples(List<Uri> graphUris, Uri uri);

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

        #endregion

        #region In Memory SPARQL

        /// <summary>
        /// Executes a SPARQL Query on the Triple Store
        /// </summary>
        /// <param name="query">SPARQL Query as an unparsed string</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// This method of making queries often leads to no results because of misconceptions about what data is being queries.  dotNetRDF's SPARQL engine only queries the default unnamed graph of the triple store (the graph added with a null URI) by default unless your query uses FROM clauses to change the default graph or you use GRAPH clauses to access named graphs in the store.  Therefore a common mistake is to add a single graph to the store and then query the store which typically results in no results because usually the added graph is named and so is not queried.
        /// </para>
        /// <para>
        /// We recommend using a <see cref="ISparqlQueryProcessor"/> instead for making queries over in-memory data since using our standard implementation (<see cref="LeviathanQueryProcessor"/>) affords you much more explicit control over which graphs are queried.
        /// </para>
        /// </remarks>
        [Obsolete("This method of making queries is often error prone due to misconceptions about what data is being queries and we recommend using an ISparqlQueryProcessor instead, see remarks for more discussion")]
        Object ExecuteQuery(String query);

        /// <summary>
        /// Executes a SPARQL Query on the Triple Store
        /// </summary>
        /// <param name="query">SPARQL Query as a <see cref="SparqlQuery">SparqlQuery</see> instance</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// This method of making queries often leads to no results because of misconceptions about what data is being queries.  dotNetRDF's SPARQL engine only queries the default unnamed graph of the triple store (the graph added with a null URI) by default unless your query uses FROM clauses to change the default graph or you use GRAPH clauses to access named graphs in the store.  Therefore a common mistake is to add a single graph to the store and then query the store which typically results in no results because usually the added graph is named and so is not queried.
        /// </para>
        /// <para>
        /// We recommend using a <see cref="ISparqlQueryProcessor"/> instead for making queries over in-memory data since using our standard implementation (<see cref="LeviathanQueryProcessor"/>) affords you much more explicit control over which graphs are queried.
        /// </para>
        /// </remarks>
        [Obsolete("This method of making queries is often error prone due to misconceptions about what data is being queries and we recommend using an ISparqlQueryProcessor instead, see remarks for more discussion")]
        Object ExecuteQuery(SparqlQuery query);

        /// <summary>
        /// Executes a SPARQL Query on the Triple Store processing the results with an appropriate handler from those provided
        /// </summary>
        /// <param name="rdfHandler">RDF Handler</param>
        /// <param name="resultsHandler">Results Handler</param>
        /// <param name="query">SPARQL Query as an unparsed string</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// This method of making queries often leads to no results because of misconceptions about what data is being queries.  dotNetRDF's SPARQL engine only queries the default unnamed graph of the triple store (the graph added with a null URI) by default unless your query uses FROM clauses to change the default graph or you use GRAPH clauses to access named graphs in the store.  Therefore a common mistake is to add a single graph to the store and then query the store which typically results in no results because usually the added graph is named and so is not queried.
        /// </para>
        /// <para>
        /// We recommend using a <see cref="ISparqlQueryProcessor"/> instead for making queries over in-memory data since using our standard implementation (<see cref="LeviathanQueryProcessor"/>) affords you much more explicit control over which graphs are queried.
        /// </para>
        /// </remarks>
        [Obsolete("This method of making queries is often error prone due to misconceptions about what data is being queries and we recommend using an ISparqlQueryProcessor instead, see remarks for more discussion")]
        void ExecuteQuery(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, String query);

        /// <summary>
        /// Executes a SPARQL Query on the Triple Store processing the results with an appropriate handler from those provided
        /// </summary>
        /// <param name="rdfHandler">RDF Handler</param>
        /// <param name="resultsHandler">Results Handler</param>
        /// <param name="query">Parsed SPARQL Query</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// This method of making queries often leads to no results because of misconceptions about what data is being queries.  dotNetRDF's SPARQL engine only queries the default unnamed graph of the triple store (the graph added with a null URI) by default unless your query uses FROM clauses to change the default graph or you use GRAPH clauses to access named graphs in the store.  Therefore a common mistake is to add a single graph to the store and then query the store which typically results in no results because usually the added graph is named and so is not queried.
        /// </para>
        /// <para>
        /// We recommend using a <see cref="ISparqlQueryProcessor"/> instead for making queries over in-memory data since using our standard implementation (<see cref="LeviathanQueryProcessor"/>) affords you much more explicit control over which graphs are queried.
        /// </para>
        /// </remarks>
        [Obsolete("This method of making queries is often error prone due to misconceptions about what data is being queries and we recommend using an ISparqlQueryProcessor instead, see remarks for more discussion")]
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
    /// Interface for Triple Stores which can have a <see cref="IInferenceEngine">IInferenceEngine</see> attached to them
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
