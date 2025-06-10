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
/// Interface for classes that support retrieval of triples by one or more of their nodes.
/// </summary>
public interface ITripleIndex
{
    /// <summary>
    /// Selects all Triples which have a Uri Node with the given Uri.
    /// </summary>
    /// <param name="uri">Uri.</param>
    /// <returns></returns>
    IEnumerable<Triple> GetTriples(Uri uri);

    /// <summary>
    /// Selects all Triples which contain the given Node.
    /// </summary>
    /// <param name="n">Node.</param>
    /// <returns></returns>
    IEnumerable<Triple> GetTriples(INode n);

    /// <summary>
    /// Selects all Triples where the Object is a Uri Node with the given Uri.
    /// </summary>
    /// <param name="u">Uri.</param>
    /// <returns></returns>
    IEnumerable<Triple> GetTriplesWithObject(Uri u);

    /// <summary>
    /// Selects all Triples where the Object is a given Node.
    /// </summary>
    /// <param name="n">Node.</param>
    /// <returns></returns>
    IEnumerable<Triple> GetTriplesWithObject(INode n);

    /// <summary>
    /// Selects all Triples where the Predicate is a given Node.
    /// </summary>
    /// <param name="n">Node.</param>
    /// <returns></returns>
    IEnumerable<Triple> GetTriplesWithPredicate(INode n);

    /// <summary>
    /// Selects all Triples where the Predicate is a Uri Node with the given Uri.
    /// </summary>
    /// <param name="u">Uri.</param>
    /// <returns></returns>
    IEnumerable<Triple> GetTriplesWithPredicate(Uri u);

    /// <summary>
    /// Selects all Triples where the Subject is a given Node.
    /// </summary>
    /// <param name="n">Node.</param>
    /// <returns></returns>
    IEnumerable<Triple> GetTriplesWithSubject(INode n);

    /// <summary>
    /// Selects all Triples where the Subject is a Uri Node with the given Uri.
    /// </summary>
    /// <param name="u">Uri.</param>
    /// <returns></returns>
    IEnumerable<Triple> GetTriplesWithSubject(Uri u);

    /// <summary>
    /// Selects all Triples with the given Subject and Predicate.
    /// </summary>
    /// <param name="subj">Subject.</param>
    /// <param name="pred">Predicate.</param>
    /// <returns></returns>
    IEnumerable<Triple> GetTriplesWithSubjectPredicate(INode subj, INode pred);

    /// <summary>
    /// Selects all Triples with the given Subject and Object.
    /// </summary>
    /// <param name="subj">Subject.</param>
    /// <param name="obj">Object.</param>
    /// <returns></returns>
    IEnumerable<Triple> GetTriplesWithSubjectObject(INode subj, INode obj);

    /// <summary>
    /// Selects all Triples with the given Predicate and Object.
    /// </summary>
    /// <param name="pred">Predicate.</param>
    /// <param name="obj">Object.</param>
    /// <returns></returns>
    IEnumerable<Triple> GetTriplesWithPredicateObject(INode pred, INode obj);

    /// <summary>
    /// Selects all quoted triples which have a Uri Node with the given Uri.
    /// </summary>
    /// <param name="uri">Uri.</param>
    /// <returns></returns>
    IEnumerable<Triple> GetQuoted(Uri uri);

    /// <summary>
    /// Selects all quoted triples which contain the given Node.
    /// </summary>
    /// <param name="n">Node.</param>
    /// <returns></returns>
    IEnumerable<Triple> GetQuoted(INode n);

    /// <summary>
    /// Selects all quoted triples where the Object is a Uri Node with the given Uri.
    /// </summary>
    /// <param name="u">Uri.</param>
    /// <returns></returns>
    IEnumerable<Triple> GetQuotedWithObject(Uri u);

    /// <summary>
    /// Selects all quoted triples where the Object is a given Node.
    /// </summary>
    /// <param name="n">Node.</param>
    /// <returns></returns>
    IEnumerable<Triple> GetQuotedWithObject(INode n);

    /// <summary>
    /// Selects all quoted triples where the Predicate is a given Node.
    /// </summary>
    /// <param name="n">Node.</param>
    /// <returns></returns>
    IEnumerable<Triple> GetQuotedWithPredicate(INode n);

    /// <summary>
    /// Selects all quoted triples where the Predicate is a Uri Node with the given Uri.
    /// </summary>
    /// <param name="u">Uri.</param>
    /// <returns></returns>
    IEnumerable<Triple> GetQuotedWithPredicate(Uri u);

    /// <summary>
    /// Selects all quoted triples where the Subject is a given Node.
    /// </summary>
    /// <param name="n">Node.</param>
    /// <returns></returns>
    IEnumerable<Triple> GetQuotedWithSubject(INode n);

    /// <summary>
    /// Selects all quoted triples where the Subject is a Uri Node with the given Uri.
    /// </summary>
    /// <param name="u">Uri.</param>
    /// <returns></returns>
    IEnumerable<Triple> GetQuotedWithSubject(Uri u);

    /// <summary>
    /// Selects all quoted triples with the given Subject and Predicate.
    /// </summary>
    /// <param name="subj">Subject.</param>
    /// <param name="pred">Predicate.</param>
    /// <returns></returns>
    IEnumerable<Triple> GetQuotedWithSubjectPredicate(INode subj, INode pred);

    /// <summary>
    /// Selects all quoted triples with the given Subject and Object.
    /// </summary>
    /// <param name="subj">Subject.</param>
    /// <param name="obj">Object.</param>
    /// <returns></returns>
    IEnumerable<Triple> GetQuotedWithSubjectObject(INode subj, INode obj);

    /// <summary>
    /// Selects all quoted triples with the given Predicate and Object.
    /// </summary>
    /// <param name="pred">Predicate.</param>
    /// <param name="obj">Object.</param>
    /// <returns></returns>
    IEnumerable<Triple> GetQuotedWithPredicateObject(INode pred, INode obj);
}
