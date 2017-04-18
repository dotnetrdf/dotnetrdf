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
using System.Threading;

namespace VDS.RDF.Query.Datasets
{
    /// <summary>
    /// Interfaces for Datasets that SPARQL Queries and Updates can be applied to
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Note:</strong> For all operations that take a Graph URI a <em>null</em> Uri should be considered to refer to the Default Graph of the dataset
    /// </para>
    /// <h3>Default and Active Graph</h3>
    /// <para>
    /// Leviathan expects that a Query operates over the Dataset in the following order:
    /// <ol>
    ///     <li>If an Active Graph is set then Queries operate over that</li>
    ///     <li>Otherwise if a Default Graph is set then Queries operate over that</li>
    ///     <li>Finally the Queries operate over the all triples, the notion of all triples may be dataset implementation specific i.e. may be union of all graphs, the default unnamed graph only or something else entirely</li>
    /// </ol>
    /// Please note that the Query may change the Active and Default Graph over the course of the query depending on the Query e.g. FROM, FROM NAMED and GRAPH all can potentially change these.
    /// </para>
    /// <para>
    /// You can limit your queries to use specific portions of your dataset by using the SetActiveGraph() and SetDefaultGraph() methods on your dataset instance before then passing it to the <see cref="LeviathanQueryProcessor">LeviathanQueryProcessor</see>
    /// </para>
    /// <para>
    /// <strong>Note: </strong> By default the <see cref="InMemoryDataset">InMemoryDataset</see> uses the Union of all Graphs in the Dataset if no Active/Default Graph is otherwise specified.  Use the <see cref="ISparqlDataset.UsesUnionDefaultGraph">UsesUnionDefaultGraph</see> property to see whether a Dataset implementation behaves in this way.
    /// </para>
    /// </remarks>
    public interface ISparqlDataset
    {
        #region Active and Default Graph Management

        /// <summary>
        /// Sets the Active Graph to be the merge of the Graphs with the given URIs
        /// </summary>
        /// <param name="graphUris">Graph URIs</param>
        void SetActiveGraph(IEnumerable<Uri> graphUris);

        /// <summary>
        /// Sets the Active Graph to be the Graph with the given URI
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        void SetActiveGraph(Uri graphUri);

        /// <summary>
        /// Sets the Default Graph to be the Graph with the given URI
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        void SetDefaultGraph(Uri graphUri);

        /// <summary>
        /// Sets the Default Graph to be the merge of the Graphs with the given URIs
        /// </summary>
        /// <param name="graphUris">Graph URIs</param>
        void SetDefaultGraph(IEnumerable<Uri> graphUris);

        /// <summary>
        /// Resets the Active Graph to the previous Active Graph
        /// </summary>
        void ResetActiveGraph();

        /// <summary>
        /// Resets the Default Graph to the previous Default Graph
        /// </summary>
        void ResetDefaultGraph();

        /// <summary>
        /// Gets the enumeration of the Graph URIs that currently make up the default graph
        /// </summary>
        IEnumerable<Uri> DefaultGraphUris
        {
            get;
        }

        /// <summary>
        /// Gets the enumeration of the Graph URIs that currently make up the active graph
        /// </summary>
        IEnumerable<Uri> ActiveGraphUris
        {
            get;
        }

        /// <summary>
        /// Gets whether the Default Graph is treated as being the union of all Graphs in the dataset when no Default Graph is otherwise set
        /// </summary>
        bool UsesUnionDefaultGraph
        {
            get;
        }

        #endregion

        #region Graph Existence and Retrieval

        /// <summary>
        /// Adds a Graph to the Dataset
        /// </summary>
        /// <param name="g">Graph</param>
        /// <exception cref="NotSupportedException">May be thrown if the Dataset is immutable i.e. Updates not supported</exception>        /// <exception cref="NotSupportedException">May be thrown if the Dataset is immutable</exception>
        bool AddGraph(IGraph g);

        /// <summary>
        /// Removes a Graph from the Dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <exception cref="NotSupportedException">May be thrown if the Dataset is immutable i.e. Updates not supported</exception>        /// <exception cref="NotSupportedException">May be thrown if the Dataset is immutable</exception>
        bool RemoveGraph(Uri graphUri);

        /// <summary>
        /// Gets whether a Graph with the given URI is the Dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        bool HasGraph(Uri graphUri);

        /// <summary>
        /// Gets all the Graphs in the Dataset
        /// </summary>
        IEnumerable<IGraph> Graphs
        {
            get;
        }

        /// <summary>
        /// Gets all the URIs of Graphs in the Dataset
        /// </summary>
        IEnumerable<Uri> GraphUris
        {
            get;
        }

        /// <summary>
        /// Gets the Graph with the given URI from the Dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// This property need only return a read-only view of the Graph, code which wishes to modify Graphs should use the <see cref="ISparqlDataset.GetModifiableGraph">GetModifiableGraph()</see> method to guarantee a Graph they can modify and will be persisted to the underlying storage
        /// </para>
        /// </remarks>
        IGraph this[Uri graphUri]
        {
            get;
        }

        /// <summary>
        /// Gets the Graph with the given URI from the Dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">May be thrown if the Dataset is immutable i.e. Updates not supported</exception>        /// <exception cref="NotSupportedException">May be thrown if the Dataset is immutable</exception>
        /// <remarks>
        /// <para>
        /// Graphs returned from this method must be modifiable and the Dataset must guarantee that when it is Flushed or Disposed of that any changes to the Graph are persisted
        /// </para>
        /// </remarks>
        IGraph GetModifiableGraph(Uri graphUri);

        #endregion

        #region Triple Existence and Retrieval

        /// <summary>
        /// Gets whether the Dataset has any Triples
        /// </summary>
        bool HasTriples
        {
            get;
        }

        /// <summary>
        /// Gets whether the Dataset contains a specific Triple
        /// </summary>
        /// <param name="t">Triple</param>
        /// <returns></returns>
        bool ContainsTriple(Triple t);

        /// <summary>
        /// Gets all the Triples in the Dataset
        /// </summary>
        /// <remarks>
        /// <para>
        /// <strong>Note:</strong> The Triples returned from the method should be limited to those in the current Active Graph if present, otherwise those in the current Default Graph if present and finally the entire Dataset
        /// </para>
        /// </remarks>
        IEnumerable<Triple> Triples
        {
            get;
        }

        /// <summary>
        /// Gets all the Triples in the Dataset with the given Subject
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// <strong>Note:</strong> The Triples returned from the method should be limited to those in the current Active Graph if present, otherwise those in the current Default Graph if present and finally the entire Dataset
        /// </para>
        /// </remarks>
        IEnumerable<Triple> GetTriplesWithSubject(INode subj);

        /// <summary>
        /// Gets all the Triples in the Dataset with the given Predicate
        /// </summary>
        /// <param name="pred">Predicate</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// <strong>Note:</strong> The Triples returned from the method should be limited to those in the current Active Graph if present, otherwise those in the current Default Graph if present and finally the entire Dataset
        /// </para>
        /// </remarks>
        IEnumerable<Triple> GetTriplesWithPredicate(INode pred);

        /// <summary>
        /// Gets all the Triples in the Dataset with the given Object
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// <strong>Note:</strong> The Triples returned from the method should be limited to those in the current Active Graph if present, otherwise those in the current Default Graph if present and finally the entire Dataset
        /// </para>
        /// </remarks>
        IEnumerable<Triple> GetTriplesWithObject(INode obj);

        /// <summary>
        /// Gets all the Triples in the Dataset with the given Subject and Predicate
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <param name="pred">Predicate</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// <strong>Note:</strong> The Triples returned from the method should be limited to those in the current Active Graph if present, otherwise those in the current Default Graph if present and finally the entire Dataset
        /// </para>
        /// </remarks>
        IEnumerable<Triple> GetTriplesWithSubjectPredicate(INode subj, INode pred);

        /// <summary>
        /// Gets all the Triples in the Dataset with the given Subject and Object
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// <strong>Note:</strong> The Triples returned from the method should be limited to those in the current Active Graph if present, otherwise those in the current Default Graph if present and finally the entire Dataset
        /// </para>
        /// </remarks>
        IEnumerable<Triple> GetTriplesWithSubjectObject(INode subj, INode obj);

        /// <summary>
        /// Gets all the Triples in the Dataset with the given Predicate and Object
        /// </summary>
        /// <param name="pred">Predicate</param>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// <strong>Note:</strong> The Triples returned from the method should be limited to those in the current Active Graph if present, otherwise those in the current Default Graph if present and finally the entire Dataset
        /// </para>
        /// </remarks>
        IEnumerable<Triple> GetTriplesWithPredicateObject(INode pred, INode obj);

        #endregion

        /// <summary>
        /// Ensures that any changes to the Dataset (if any) are flushed to the underlying Storage
        /// </summary>
        /// <remarks>
        /// <para>
        /// While partly intended for use in implementations which support transactions though other implementations may wish to use this to ensure that changes to the dataset are persisted properly
        /// </para>
        /// </remarks>
        void Flush();

        /// <summary>
        /// Ensures that any changes to the Dataset (if any) are discarded
        /// </summary>
        /// <remarks>
        /// <para>
        /// Primarily intended for use in implementations which support transactions though other implementations may wish to use this to ensure that changes to the dataset are persisted properly
        /// </para>
        /// </remarks>
        void Discard();
    }

    /// <summary>
    /// Interface for SPARQL Datasets which also provide a Lock by which threading can be controlled
    /// </summary>
    /// <remarks>
    /// Note that there is no guarantees that consuming code will respect the fact that a Dataset is Thread Safe and use the <see cref="IThreadSafeDataset.Lock">Lock</see> property appropriately.  Additionally some datasets may choose to implement thread safety in other ways which don't rely on this interface
    /// </remarks>
    public interface IThreadSafeDataset : ISparqlDataset
    {
        /// <summary>
        /// Gets the Lock used to ensure MRSW concurrency of the Dataset when used with the Leviathan SPARQL processors
        /// </summary>
        ReaderWriterLockSlim Lock
        {
            get;
        }
    }
}
