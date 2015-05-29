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
using System.Linq;
using VDS.RDF.Graphs;
using VDS.RDF.Graphs.Utilities;
using VDS.RDF.Nodes;

namespace VDS.RDF
{
    /// <summary>
    /// Extension functions for working with graphs
    /// </summary>
    public static class GraphExtensions
    {
        /// <summary>
        /// Selects all Triples which contain the given Node
        /// </summary>
        /// <param name="n">Node</param>
        /// <param name="g">Graph</param>
        /// <returns></returns>
        public static IEnumerable<Triple> GetTriples(this IGraph g, INode n)
        {
            return g.Triples.Where(t => t.Involves(n));
        }

        /// <summary>
        /// Selects all Triples which have a URI Node with the given URI
        /// </summary>
        /// <param name="uri">URI</param>
        /// <param name="g">Graph</param>
        /// <returns></returns>
        public static IEnumerable<Triple> GetTriples(this IGraph g, Uri uri)
        {
            return ReferenceEquals(uri, null) ? Enumerable.Empty<Triple>() : GetTriples(g, g.CreateUriNode(uri));
        }

        /// <summary>
        /// Selects all Triples where the Object is a URI Node with the given Uri
        /// </summary>
        /// <param name="u">Uri</param>
        /// <param name="g">Graph</param>
        /// <returns></returns>
        public static IEnumerable<Triple> GetTriplesWithObject(this IGraph g, Uri u)
        {
            return ReferenceEquals(u, null) ? Enumerable.Empty<Triple>() : g.GetTriplesWithObject(g.CreateUriNode(u));
        }

        /// <summary>
        /// Selects all Triples where the Object is a given Node
        /// </summary>
        /// <param name="n">Node</param>
        /// <param name="g">Graph</param>
        /// <returns></returns>
        public static IEnumerable<Triple> GetTriplesWithObject(this IGraph g, INode n)
        {
            return ReferenceEquals(n, null) ? g.Triples : g.Find(null, null, n);
        }

        /// <summary>
        /// Selects all Triples where the Predicate is a given Node
        /// </summary>
        /// <param name="n">Node</param>
        /// <param name="g">Graph</param>
        /// <returns></returns>
        public static IEnumerable<Triple> GetTriplesWithPredicate(this IGraph g, INode n)
        {
            return ReferenceEquals(n, null) ? g.Triples : g.Find(null, n, null);
        }

        /// <summary>
        /// Selects all Triples where the Predicate is a Uri Node with the given Uri
        /// </summary>
        /// <param name="u">Uri</param>
        /// <param name="g">Graph</param>
        /// <returns></returns>
        public static IEnumerable<Triple> GetTriplesWithPredicate(this IGraph g, Uri u)
        {
            return ReferenceEquals(u, null) ? Enumerable.Empty<Triple>() : g.GetTriplesWithPredicate(g.CreateUriNode(u));
        }

        /// <summary>
        /// Selects all Triples where the Subject is a given Node
        /// </summary>
        /// <param name="n">Node</param>
        /// <param name="g">Graph</param>
        /// <returns></returns>
        public static IEnumerable<Triple> GetTriplesWithSubject(this IGraph g, INode n)
        {
            return ReferenceEquals(n, null) ? g.Triples : g.Find(n, null, null);
        }

        /// <summary>
        /// Selects all Triples where the Subject is a Uri Node with the given Uri
        /// </summary>
        /// <param name="u">Uri</param>
        /// <param name="g">Graph</param>
        /// <returns></returns>
        public static IEnumerable<Triple> GetTriplesWithSubject(this IGraph g, Uri u)
        {
            return ReferenceEquals(u, null) ? Enumerable.Empty<Triple>() : g.GetTriplesWithSubject(g.CreateUriNode(u));
        }

        /// <summary>
        /// Selects all Triples with the given Subject and Predicate
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <param name="pred">Predicate</param>
        /// <param name="g">Graph</param>
        /// <returns></returns>
        public static IEnumerable<Triple> GetTriplesWithSubjectPredicate(this IGraph g, INode subj, INode pred)
        {
            return g.Find(subj, pred, null);
        }

        /// <summary>
        /// Selects all Triples with the given Subject and Object
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <param name="obj">Object</param>
        /// <param name="g">Graph</param>
        /// <returns></returns>
        public static IEnumerable<Triple> GetTriplesWithSubjectObject(this IGraph g, INode subj, INode obj)
        {
            return g.Find(subj, null, obj);
        }

        /// <summary>
        /// Selects all Triples with the given Predicate and Object
        /// </summary>
        /// <param name="pred">Predicate</param>
        /// <param name="obj">Object</param>
        /// <param name="g">Graph</param>
        /// <returns></returns>
        public static IEnumerable<Triple> GetTriplesWithPredicateObject(this IGraph g, INode pred, INode obj)
        {
            return g.Find(null, pred, obj);
        }

        /// <summary>
        /// Asserts a new Triple in the Graph
        /// </summary>
        /// <param name="g">Graph to assert in</param>
        /// <param name="subj">Subject</param>
        /// <param name="pred">Predicate</param>
        /// <param name="obj">Object</param>
        /// <remarks>Handy method which means you can assert a Triple by specifying the Subject, Predicate and Object without having to explicity declare a new Triple</remarks>
        public static void Assert(this IGraph g, INode subj, INode pred, INode obj)
        {
            g.Assert(new Triple(subj, pred, obj));
        }

        /// <summary>
        /// Retracts a Triple from the Graph
        /// </summary>
        /// <param name="g">Graph to retract from</param>
        /// <param name="subj">Subject</param>
        /// <param name="pred">Predicate</param>
        /// <param name="obj">Object</param>
        /// <remarks>Handy method which means you can retract a Triple by specifying the Subject, Predicate and Object without having to explicity declare a new Triple</remarks>
        public static void Retract(this IGraph g, INode subj, INode pred, INode obj)
        {
            g.Retract(new Triple(subj, pred, obj));
        }

        /// <summary>
        /// Checks whether a Graph is equivalent (isomorphic) to another Graph
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="other">Graph to compare with for equivalence</param>
        /// <returns>True if graphs are isomorphic, false otherwise</returns>
        /// <remarks>
        /// <para>
        /// Graphs are considered equivalent or more formally isomorphic if there is a mapping such that every sub-graph that contains a set of blank nodes may be mapped to an equivalent sub-graph in the other graph.
        /// </para>
        /// <para>
        /// Please see the <see cref="GraphMatcher"/> class for notes on the isomorphism algorithm used
        /// </para>
        /// </remarks>
        public static bool IsIsomorphicWith(this IGraph g, IGraph other)
        {
            Dictionary<INode, INode> mapping = null;
            return IsIsomorphicWith(g, other, out mapping);
        }

        /// <summary>
        /// Checks whether a Graph is equivalent (isomorphic) to another Graph and if so returns the mapping of Blank Nodes
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="other">Graph to compare with for equivalence</param>
        /// <param name="mapping">Mapping of Blank Nodes</param>
        /// <returns>True if graphs are isomorphic, false otherwise</returns>
        /// <remarks>
        /// <para>
        /// Graphs are considered equivalent or more formally isomorphic if there is a mapping such that every sub-graph that contains a set of blank nodes may be mapped to an equivalent sub-graph in the other graph.
        /// </para>
        /// <para>
        /// Please see the <see cref="GraphMatcher"/> class for notes on the isomorphism algorithm used
        /// </para>
        /// </remarks>
        public static bool IsIsomorphicWith(this IGraph g, IGraph other, out Dictionary<INode, INode> mapping)
        {
            //Set the mapping to be null
            mapping = null;

            GraphMatcher matcher = new GraphMatcher();
            if (!matcher.Equals(g, other)) return false;

            // Return the mapping
            mapping = matcher.Mapping;
            return true;
        }

        /// <summary>
        /// Checks whether a Graph is a sub-graph of another Graph
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="superGraph">Graph that should contain <paramref name="g"/> as a sub-graph</param>
        /// <returns></returns>
        public static bool IsSubGraphOf(this IGraph g, IGraph superGraph)
        {
            Dictionary<INode, INode> temp;
            return IsSubGraphOf(g, superGraph, out temp);
        }

        /// <summary>
        /// Checks whether a Graph is a sub-graph of another Graph
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="superGraph">Graph that should contain <paramref name="g"/> as a sub-graph</param>
        /// <param name="mapping">Mapping of Blank Nodes</param>
        /// <returns></returns>
        public static bool IsSubGraphOf(this IGraph g, IGraph superGraph, out Dictionary<INode, INode> mapping)
        {
            //Set the mapping to be null
            mapping = null;

            SubGraphMatcher matcher = new SubGraphMatcher();
            if (matcher.IsSubGraph(g, superGraph))
            {
                mapping = matcher.Mapping;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks whether a Graph has another Graph as a sub-graph
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="subGraph">Graph that should be contained in <paramref name="g"/> as a sub-graph</param>
        /// <returns></returns>
        public static bool HasSubGraph(this IGraph g, IGraph subGraph)
        {
            return IsSubGraphOf(subGraph, g);
        }

        /// <summary>
        /// Checks whether a Graph has another Graph as a sub-graph
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="subGraph">Graph that should be contained in <paramref name="g"/> as a sub-graph</param>
        /// <param name="mapping">Mapping of Blank Nodes</param>
        /// <returns></returns>
        public static bool HasSubGraph(this IGraph g, IGraph subGraph, out Dictionary<INode, INode> mapping)
        {
            return IsSubGraphOf(subGraph, g, out mapping);
        }

        /// <summary>
        /// Calculates the difference between two graphs
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="other">Graph to compare against</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// Produces a report which shows the changes that must be made to <paramref name="g"/> in order to produce <paramref name="other"/> or if they represent the same graph reports that
        /// </para>
        /// </remarks>
        public static GraphDiffReport Difference(this IGraph g, IGraph other)
        {
            GraphDiff differ = new GraphDiff();
            return differ.Difference(g, other);
        }
    }
}