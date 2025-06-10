/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2025 dotNetRDF Project (http://dotnetrdf.org/)
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

namespace VDS.RDF;

/// <summary>
/// Interface for Triple Stores which can be queried in memory using method calls or the SPARQL implementation contained in this library.
/// </summary>
/// <remarks>
/// <para>
/// An in memory Triple Store will typically load most of the Graphs and consequently Triples contained within it into Memory as the in memory SPARQL implementation only operates over the part of the Triple Store loaded in memory.  This being said there is no reason why an in memory store can't provide a Snapshot view of an underlying store to allow only the relevant parts of Store to be loaded and queried.
/// </para>
/// <para>
/// All the Selection Methods which do not specify a subset of Graphs on such a Triple Store <strong>should</strong> operate over the entire store.
/// </para>
/// </remarks>
public interface IInMemoryQueryableStore
    : ITripleStore
{
    /// <summary>
    /// Returns whether a given Triple is contained anywhere in the Query Triples.
    /// </summary>
    /// <param name="t">Triple to check for existence of.</param>
    /// <returns></returns>
    bool Contains(Triple t);

    #region Selection over Entire Triple Store

    /// <summary>
    /// Selects all Triples which have a Uri Node with the given Uri from all the Query Triples.
    /// </summary>
    /// <param name="uri">Uri.</param>
    /// <returns></returns>
    IEnumerable<Triple> GetTriples(Uri uri);

    /// <summary>
    /// Selects all Triples which contain the given Node from all the Query Triples.
    /// </summary>
    /// <param name="n">Node.</param>
    /// <returns></returns>
    IEnumerable<Triple> GetTriples(INode n);

    /// <summary>
    /// Selects all Triples where the Object is a Uri Node with the given Uri from all the Query Triples.
    /// </summary>
    /// <param name="u">Uri.</param>
    /// <returns></returns>
    IEnumerable<Triple> GetTriplesWithObject(Uri u);

    /// <summary>
    /// Selects all Triples where the Object is a given Node from all the Query Triples.
    /// </summary>
    /// <param name="n">Node.</param>
    /// <returns></returns>
    IEnumerable<Triple> GetTriplesWithObject(INode n);

    /// <summary>
    /// Selects all Triples where the Predicate is a given Node from all the Query Triples.
    /// </summary>
    /// <param name="n">Node.</param>
    /// <returns></returns>
    IEnumerable<Triple> GetTriplesWithPredicate(INode n);

    /// <summary>
    /// Selects all Triples where the Predicate is a Uri Node with the given Uri from all the Query Triples.
    /// </summary>
    /// <param name="u">Uri.</param>
    /// <returns></returns>
    IEnumerable<Triple> GetTriplesWithPredicate(Uri u);

    /// <summary>
    /// Selects all Triples where the Subject is a given Node from all the Query Triples.
    /// </summary>
    /// <param name="n">Node.</param>
    /// <returns></returns>
    IEnumerable<Triple> GetTriplesWithSubject(INode n);

    /// <summary>
    /// Selects all Triples where the Subject is a Uri Node with the given Uri from all the Query Triples.
    /// </summary>
    /// <param name="u">Uri.</param>
    /// <returns></returns>
    IEnumerable<Triple> GetTriplesWithSubject(Uri u);

    /// <summary>
    /// Selects all the Triples with the given Subject-Predicate pair from all the Query Triples.
    /// </summary>
    /// <param name="subj">Subject.</param>
    /// <param name="predicate">Predicate.</param>
    /// <returns></returns>
    IEnumerable<Triple> GetTriplesWithSubjectPredicate(INode subj, INode predicate);

    /// <summary>
    /// Selects all the Triples with the given Predicate-Object pair from all the Query Triples.
    /// </summary>
    /// <param name="predicate">Predicate.</param>
    /// <param name="obj">Object.</param>
    /// <returns></returns>
    IEnumerable<Triple> GetTriplesWithPredicateObject(INode predicate, INode obj);

    /// <summary>
    /// Selects all the Triples with the given Subject-Object pair from all the Query Triples.
    /// </summary>
    /// <param name="subj">Subject.</param>
    /// <param name="obj">Object.</param>
    /// <returns></returns>
    IEnumerable<Triple> GetTriplesWithSubjectObject(INode subj, INode obj);

    #endregion

    #region Selection over a Subset of the Store

    /// <summary>
    /// Selects all Triples which have a Uri Node with the given Uri from a Subset of Graphs in the Triple Store.
    /// </summary>
    /// <param name="graphUris">List of the Graph URIs of Graphs you want to select over.</param>
    /// <param name="uri">Uri.</param>
    /// <returns></returns>
    [Obsolete("Replaced by GetTriples(List<IRefNode>, Uri)")]
    IEnumerable<Triple> GetTriples(List<Uri> graphUris, Uri uri);

    /// <summary>
    /// Selects all Triples which contain the given Node from a Subset of Graphs in the Triple Store.
    /// </summary>
    /// <param name="graphUris">List of the Graph URIs of Graphs you want to select over.</param>
    /// <param name="n">Node.</param>
    /// <returns></returns>
    [Obsolete("Replaced by GetTriples(List<IRefNode>, INode)")]
    IEnumerable<Triple> GetTriples(List<Uri> graphUris, INode n);

    /// <summary>
    /// Selects all Triples where the Object is a Uri Node with the given Uri from a Subset of Graphs in the Triple Store.
    /// </summary>
    /// <param name="graphUris">List of the Graph URIs of Graphs you want to select over.</param>
    /// <param name="u">Uri.</param>
    /// <returns></returns>
    [Obsolete("Replaced by GetTriplesWithObject(List<IRefNode>, Uri)")]
    IEnumerable<Triple> GetTriplesWithObject(List<Uri> graphUris, Uri u);

    /// <summary>
    /// Selects all Triples where the Object is a given Node from a Subset of Graphs in the Triple Store.
    /// </summary>
    /// <param name="graphUris">List of the Graph URIs of Graphs you want to select over.</param>
    /// <param name="n">Node.</param>
    /// <returns></returns>
    [Obsolete("Replaced by GetTriplesWithObject(List<IRefNode>, INode)")]
    IEnumerable<Triple> GetTriplesWithObject(List<Uri> graphUris, INode n);

    /// <summary>
    /// Selects all Triples where the Predicate is a given Node from a Subset of Graphs in the Triple Store.
    /// </summary>
    /// <param name="graphUris">List of the Graph URIs of Graphs you want to select over.</param>
    /// <param name="n">Node.</param>
    /// <returns></returns>
    [Obsolete("Replaced by GetTriplesWithPredicate(List<IRefNode>, INode)")]
    IEnumerable<Triple> GetTriplesWithPredicate(List<Uri> graphUris, INode n);

    /// <summary>
    /// Selects all Triples where the Predicate is a Uri Node with the given Uri from a Subset of Graphs in the Triple Store.
    /// </summary>
    /// <param name="graphUris">List of the Graph URIs of Graphs you want to select over.</param>
    /// <param name="u">Uri.</param>
    /// <returns></returns>
    [Obsolete("Replaced by GetTriplesWithPredicate(List<IRefNode>, Uri)")]
    IEnumerable<Triple> GetTriplesWithPredicate(List<Uri> graphUris, Uri u);

    /// <summary>
    /// Selects all Triples where the Subject is a given Node from a Subset of Graphs in the Triple Store.
    /// </summary>
    /// <param name="graphUris">List of the Graph URIs of Graphs you want to select over.</param>
    /// <param name="n">Node.</param>
    /// <returns></returns>
    [Obsolete("Replaced by GetTriplesWithSubject(List<IRefNode>, INode)")]
    IEnumerable<Triple> GetTriplesWithSubject(List<Uri> graphUris, INode n);

    /// <summary>
    /// Selects all Triples where the Subject is a Uri Node with the given Uri from a Subset of Graphs in the Triple Store.
    /// </summary>
    /// <param name="graphUris">List of the Graph URIs of Graphs you want to select over.</param>
    /// <param name="u">Uri.</param>
    /// <returns></returns>
    [Obsolete("Replaced by GetTriplesWithSubject(List<IRefNode>, Uri)")]
    IEnumerable<Triple> GetTriplesWithSubject(List<Uri> graphUris, Uri u);


    /// <summary>
    /// Selects all Triples which have a Uri Node with the given Uri from a Subset of Graphs in the Triple Store.
    /// </summary>
    /// <param name="graphNames">List of the names of Graphs you want to select over.</param>
    /// <param name="uri">Uri.</param>
    /// <returns></returns>
    IEnumerable<Triple> GetTriples(List<IRefNode> graphNames, Uri uri);

    /// <summary>
    /// Selects all Triples which contain the given Node from a Subset of Graphs in the Triple Store.
    /// </summary>
    /// <param name="graphNames">List of the names of Graphs you want to select over.</param>
    /// <param name="n">Node.</param>
    /// <returns></returns>
    IEnumerable<Triple> GetTriples(List<IRefNode> graphNames, INode n);

    /// <summary>
    /// Selects all Triples where the Object is a Uri Node with the given Uri from a Subset of Graphs in the Triple Store.
    /// </summary>
    /// <param name="graphNames">List of the names of Graphs you want to select over.</param>
    /// <param name="u">Uri.</param>
    /// <returns></returns>
    IEnumerable<Triple> GetTriplesWithObject(List<IRefNode> graphNames, Uri u);

    /// <summary>
    /// Selects all Triples where the Object is a given Node from a Subset of Graphs in the Triple Store.
    /// </summary>
    /// <param name="graphNames">List of the names of Graphs you want to select over.</param>
    /// <param name="n">Node.</param>
    /// <returns></returns>
    IEnumerable<Triple> GetTriplesWithObject(List<IRefNode> graphNames, INode n);

    /// <summary>
    /// Selects all Triples where the Predicate is a given Node from a Subset of Graphs in the Triple Store.
    /// </summary>
    /// <param name="graphNames">List of the names of Graphs you want to select over.</param>
    /// <param name="n">Node.</param>
    /// <returns></returns>
    IEnumerable<Triple> GetTriplesWithPredicate(List<IRefNode> graphNames, INode n);

    /// <summary>
    /// Selects all Triples where the Predicate is a Uri Node with the given Uri from a Subset of Graphs in the Triple Store.
    /// </summary>
    /// <param name="graphNames">List of the names of Graphs you want to select over.</param>
    /// <param name="u">Uri.</param>
    /// <returns></returns>
    IEnumerable<Triple> GetTriplesWithPredicate(List<IRefNode> graphNames, Uri u);

    /// <summary>
    /// Selects all Triples where the Subject is a given Node from a Subset of Graphs in the Triple Store.
    /// </summary>
    /// <param name="graphNames">List of the names of Graphs you want to select over.</param>
    /// <param name="n">Node.</param>
    /// <returns></returns>
    IEnumerable<Triple> GetTriplesWithSubject(List<IRefNode> graphNames, INode n);

    /// <summary>
    /// Selects all Triples where the Subject is a Uri Node with the given Uri from a Subset of Graphs in the Triple Store.
    /// </summary>
    /// <param name="graphNames">List of the names of Graphs you want to select over.</param>
    /// <param name="u">Uri.</param>
    /// <returns></returns>
    IEnumerable<Triple> GetTriplesWithSubject(List<IRefNode> graphNames, Uri u);
    #endregion

}