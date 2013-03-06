/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2013 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

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
using System.Text;

namespace VDS.RDF
{
    /// <summary>
    /// Interface for Graph Stores
    /// </summary>
    /// <remarks>
    /// <para>
    /// The IGraphStore interface is a hybrid of the old ITripleStore and ISparqlDataset interfaces from the 1.0 APIs, it allows for data to be manipulated at either the Graph or Quad level as desired.
    /// </para>
    /// </remarks>
    public interface IGraphStore
    {
        /// <summary>
        /// Gets the URIs of the Graphs in the Store
        /// </summary>
        IEnumerable<Uri> GraphUris
        { 
            get;
        }

        /// <summary>
        /// Gets the Graphs in the Store
        /// </summary>
        IEnumerable<IGraph> Graphs
        { 
            get;
        }

        /// <summary>
        /// Gets a specific Graph from the Store
        /// </summary>
        /// <param name="u">Graph URI</param>
        /// <returns>A Graph if it exists in the Store, an error otherwise</returns>
        IGraph this[Uri u]
        { 
            get; 
        }

        /// <summary>
        /// Determines whether the store has a specific graph
        /// </summary>
        /// <param name="u">Graph URI</param>
        /// <returns>True if the Graph exists in the store, false otherwise</returns>
        bool HasGraph(Uri u);

        /// <summary>
        /// Adds the contents of a Graph to the store using the Graphs stated <see cref="IGraph.BaseUri"/> as the Graph URI for the added Quads
        /// </summary>
        /// <param name="g">Graph</param>
        /// <returns>True if any quads are added, false otherwise</returns>
        bool Add(IGraph g);

        /// <summary>
        /// Adds the contents of a Graph to the store using the given URI for the added Quads
        /// </summary>
        /// <param name="graphUri">URI of the Graph to add to</param>
        /// <param name="g">Graph</param>
        /// <returns>True if any quads are added, false otherwise</returns>
        bool Add(Uri graphUri, IGraph g);

        /// <summary>
        /// Adds a Triple to the Store as a Quad using the given URI
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <param name="t">Triple</param>
        /// <returns>True if a quad is added, false otherwise</returns>
        bool Add(Uri graphUri, Triple t);

        /// <summary>
        /// Adds a Quad to the Store
        /// </summary>
        /// <param name="q">Quad</param>
        /// <returns>True if a quad is added, false otherwise</returns>
        bool Add(Quad q);

        /// <summary>
        /// Copies the contents of one graph to another
        /// </summary>
        /// <param name="srcUri">Source Graph URI</param>
        /// <param name="destUri">Target Graph URI</param>
        /// <param name="overwrite">If true the contents of the target graph are overwritten, if false the copied data is added to the existing data in the target graph</param>
        /// <returns>True if any quads are copied, false otherwise</returns>
        bool Copy(Uri srcUri, Uri destUri, bool overwrite);

        /// <summary>
        /// Moves the contents of one graph to another
        /// </summary>
        /// <param name="srcUri">Source Graph URI</param>
        /// <param name="destUri">Destination Graph URI</param>
        /// <param name="overwrite">If true the contents of the target graph are overwritten, if false the moved data is added to the existing data in the target graph</param>
        /// <returns>True if any quads are moved, false otherwise</returns>
        bool Move(Uri srcUri, Uri destUri, bool overwrite);

        /// <summary>
        /// Removes the contents of the given graph from the store using the graphs stated <see cref="IGraph.BaseUri"/> as the Graph URI for the removed Quads
        /// </summary>
        /// <param name="g">Graph</param>
        /// <returns>True if any quads are removed, false otherwise</returns>
        bool Remove(IGraph g);

        /// <summary>
        /// Removes the contents of the given graph from a specific graph in the store
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <param name="g">Graph</param>
        /// <returns>True if any quads are removed, false otherwise</returns>
        bool Remove(Uri graphUri, IGraph g);

        /// <summary>
        /// Removes the contents of the graph with the given URI
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns>True if any quads are removed, false otherwise</returns>
        bool Remove(Uri graphUri);

        /// <summary>
        /// Removes a Triple from the store using the given Graph URI
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <param name="t">Triple</param>
        /// <returns>True if a quad is removed, false otherwise</returns>
        bool Remove(Uri graphUri, Triple t);

        /// <summary>
        /// Removes a Quad from the store
        /// </summary>
        /// <param name="q">Quad</param>
        /// <returns>True if a quad is removed, false otherwise</returns>
        bool Remove(Quad q);

        /// <summary>
        /// Get all Triples in the Store
        /// </summary>
        IEnumerable<Triple> Triples 
        { 
            get;
        }

        /// <summary>
        /// Get all Triples with given Subject in given Graph(s)
        /// </summary>
        /// <param name="graphUris">Graph URIS to restrict to</param>
        /// <param name="subj">Subject</param>
        /// <returns></returns>
        IEnumerable<Triple> GetTriplesWithSubject(IEnumerable<Uri> graphUris, INode subj);

        /// <summary>
        /// Get all Triples with given Subject and Predicate in given Graph(s)
        /// </summary>
        /// <param name="graphUris">Graph URIS to restrict to</param>
        /// <param name="subj">Subject</param>
        /// <param name="pred">Predicate</param>
        /// <returns></returns>
        IEnumerable<Triple> GetTriplesWithSubjectPredicate(IEnumerable<Uri> graphUris, INode subj, INode pred);

        /// <summary>
        /// Get all Triples with given Subject and Object in given Graph(s)
        /// </summary>
        /// <param name="graphUris">Graph URIS to restrict to</param>
        /// <param name="subj">Subject</param>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        IEnumerable<Triple> GetTriplesWithSubjectObject(IEnumerable<Uri> graphUris, INode subj, INode obj);

        /// <summary>
        /// Get all Triples with given Predicate in given Graph(s)
        /// </summary>
        /// <param name="graphUris">Graph URIS to restrict to</param>
        /// <param name="pred">Predicate</param>
        /// <returns></returns>
        IEnumerable<Triple> GetTriplesWithPredicate(IEnumerable<Uri> graphUris, INode pred);

        /// <summary>
        /// Get all Triples with given Predicate and Object in given Graph(s)
        /// </summary>
        /// <param name="graphUris">Graph URIS to restrict to</param>
        /// <param name="pred">Predicate</param>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        IEnumerable<Triple> GetTriplesWithPredicateObject(IEnumerable<Uri> graphUris, INode pred, INode obj);

        /// <summary>
        /// Get all Triples with given Object in given Graph(s)
        /// </summary>
        /// <param name="graphUris">Graph URIS to restrict to</param>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        IEnumerable<Triple> GetTriplesWithObject(IEnumerable<Uri> graphUris, INode obj);

        /// <summary>
        /// Get all Quads in the store
        /// </summary>
        IEnumerable<Quad> Quads 
        {
            get;
        }

        /// <summary>
        /// Get all quads that match the given subject across all graphs
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <returns></returns>
        IEnumerable<Quad> GetQuadsWithSubject(INode subj);

        /// <summary>
        /// Get all quads that match the given subject and predicate across all graphs
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <param name="pred">Predicate</param>
        /// <returns></returns>
        IEnumerable<Quad> GetQuadsWithSubjectPredicate(INode subj, INode pred);

        /// <summary>
        /// Get all quads that match the given subject across all graphs
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <returns></returns>
        IEnumerable<Quad> GetQuadsWithSubjectObject(INode subj);

        /// <summary>
        /// Get all quads that match the given subject and object across all graphs
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        IEnumerable<Quad> GetQuadsWithPredicate(INode subj, INode obj);

        /// <summary>
        /// Get all quads that match the given subject across all graphs
        /// </summary>
        /// <param name="pred">Predicate</param>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        IEnumerable<Quad> GetQuadsWithPredicateObject(INode pred, INode obj);

        /// <summary>
        /// Get all quads that match the given subject across all graphs
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        IEnumerable<Quad> GetQuadsWithObject(INode obj);

        /// <summary>
        /// Is the given Triple contained in any graph in the store?
        /// </summary>
        /// <param name="t">Triple</param>
        /// <returns></returns>
        bool Contains(Triple t);

        /// <summary>
        /// Is the given Triple contained in the given graphs?
        /// </summary>
        /// <param name="graphUris">Graph URIs</param>
        /// <param name="t">Triple</param>
        /// <returns></returns>
        bool Contains(IEnumerable<Uri> graphUris, Triple t);

        /// <summary>
        /// Is the given Quad contained in the store?
        /// </summary>
        /// <param name="q">Quad</param>
        /// <returns></returns>
        bool Contains(Quad q);
    }
}
