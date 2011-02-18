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
using System.Linq;
using System.Text;

namespace VDS.RDF.Query.Datasets
{
    /// <summary>
    /// Interfaces for Datasets that SPARQL Queries and Updates can be applied to
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Note:</strong> For all operations that take a Graph URI a <em>null</em> Uri should be considered to refer to the Default Graph of the dataset
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
        /// Sets the Active Graph to be the given Graph
        /// </summary>
        /// <param name="g">Graph</param>
        void SetActiveGraph(IGraph g);

        /// <summary>
        /// Sets the Default Graph to be the given Graph
        /// </summary>
        /// <param name="g">Graph</param>
        void SetDefaultGraph(IGraph g);

        /// <summary>
        /// Resets the Active Graph to the previous Active Graph
        /// </summary>
        void ResetActiveGraph();

        /// <summary>
        /// Resets the Default Graph to the previous Default Graph
        /// </summary>
        void ResetDefaultGraph();

        /// <summary>
        /// Gets the current Default Graph (null if none)
        /// </summary>
        IGraph DefaultGraph
        {
            get;
        }

        /// <summary>
        /// Gets the current Active Graph (null if none)
        /// </summary>
        IGraph ActiveGraph
        {
            get;
        }

        #endregion

        #region Graph Existence and Retrieval

        /// <summary>
        /// Adds a Graph to the Dataset
        /// </summary>
        /// <param name="g">Graph</param>
        void AddGraph(IGraph g);

        /// <summary>
        /// Removes a Graph from the Dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        void RemoveGraph(Uri graphUri);

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
        /// Ensures that any changes to the Dataset are flushed to the underlying Storage (if any)
        /// </summary>
        void Flush();
    }
}
