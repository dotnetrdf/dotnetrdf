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
    /// Abstract Base Class for Datasets which provides implementation of Active and Default Graph management
    /// </summary>
    public abstract class BaseDataset : ISparqlDataset
    {
        /// <summary>
        /// Reference to the Active Graph being used for executing a SPARQL Query
        /// </summary>
        protected IGraph _activeGraph = null;
        /// <summary>
        /// Default Graph for executing SPARQL Queries against
        /// </summary>
        protected IGraph _defaultGraph = null;
        /// <summary>
        /// Stack of Default Graph References used for executing a SPARQL Query when a Query may choose to change the Default Graph from the Dataset defined one
        /// </summary>
        protected Stack<IGraph> _defaultGraphs = new Stack<IGraph>();
        /// <summary>
        /// Stack of Active Graph References used for executing a SPARQL Query when there are nested GRAPH Clauses
        /// </summary>
        protected Stack<IGraph> _activeGraphs = new Stack<IGraph>();

        #region Active and Default Graph Management

        /// <summary>
        /// Sets the Default Graph for the SPARQL Query
        /// </summary>
        /// <param name="g"></param>
        public void SetDefaultGraph(IGraph g)
        {
            this._defaultGraphs.Push(this._defaultGraph);
            this._defaultGraph = g;
        }

        /// <summary>
        /// Sets the Active Graph for the SPARQL Query
        /// </summary>
        /// <param name="g">Active Graph</param>
        public void SetActiveGraph(IGraph g)
        {
            this._activeGraphs.Push(this._activeGraph);
            this._activeGraph = g;
        }

        /// <summary>
        /// Sets the Active Graph for the SPARQL query
        /// </summary>
        /// <param name="graphUri">Uri of the Active Graph</param>
        /// <remarks>
        /// Helper function used primarily in the execution of GRAPH Clauses
        /// </remarks>
        public void SetActiveGraph(Uri graphUri)
        {
            if (graphUri == null)
            {
                //Change the Active Graph so that the query operates over the default graph
                //If the default graph is null then it operates over the entire dataset
                this._activeGraphs.Push(this._activeGraph);
                this._activeGraph = this._defaultGraph;
            }
            else if (this.HasGraph(graphUri))
            {
                //Push current Active Graph on the Stack
                this._activeGraphs.Push(this._activeGraph);

                //Set the new Active Graph
                this._activeGraph = this[graphUri];
            }
            else
            {
                //Active Graph is an empty Graph in the case where the Graph is not present in the Dataset
                this._activeGraphs.Push(this._activeGraph);
                this._activeGraph = new Graph();
            }
        }

        /// <summary>
        /// Sets the Active Graph for the SPARQL query
        /// </summary>
        /// <param name="graphUris">URIs of the Graphs which form the Active Graph</param>
        /// <remarks>Helper function used primarily in the execution of GRAPH Clauses</remarks>
        public void SetActiveGraph(IEnumerable<Uri> graphUris)
        {
            if (graphUris.Count() == 1)
            {
                //If only 1 Graph Uri call the simpler SetActiveGraph method which will be quicker
                this.SetActiveGraph(graphUris.First());
            }
            else
            {
                //Multiple Graph URIs
                //Build a merged Graph of all the Graph URIs
                Graph g = new Graph();
                foreach (Uri u in graphUris)
                {
                    if (this.HasGraph(u))
                    {
                        g.Merge(this[u], true);
                    }
                    //else
                    //{
                    //    throw new RdfQueryException("A Graph with URI '" + u.ToString() + "' does not exist in this Triple Store, a GRAPH Clause cannot be used to change the Active Graph to a Graph that doesn't exist");
                    //}
                }

                //Push current Active Graph on the Stack
                this._activeGraphs.Push(this._activeGraph);

                //Set the new Active Graph
                this._activeGraph = g;
            }
        }

        /// <summary>
        /// Sets the Active Graph for the SPARQL query to be the previous Active Graph
        /// </summary>
        public void ResetActiveGraph()
        {
            if (this._activeGraphs.Count > 0)
            {
                this._activeGraph = this._activeGraphs.Pop();
            }
            else
            {
                throw new RdfQueryException("Unable to reset the Active Graph since no previous Active Graphs exist");
            }
        }

        /// <summary>
        /// Sets the Default Graph for the SPARQL Query to be the previous Default Graph
        /// </summary>
        public void ResetDefaultGraph()
        {
            if (this._defaultGraphs.Count > 0)
            {
                this._defaultGraph = this._defaultGraphs.Pop();
            }
            else
            {
                throw new RdfQueryException("Unable to reset the Default Graph since no previous Default Graphs exist");
            }
        }

        /// <summary>
        /// Gets the current Default Graph (null if none)
        /// </summary>
        public IGraph DefaultGraph
        {
            get
            {
                return this._defaultGraph;
            }
        }

        /// <summary>
        /// Gets the current Active Graph (null if none)
        /// </summary>
        public IGraph ActiveGraph
        {
            get
            {
                return this._activeGraph;
            }
        }

        #endregion

        /// <summary>
        /// Adds a Graph to the Dataset
        /// </summary>
        /// <param name="g">Graph</param>
        public abstract void AddGraph(IGraph g);

        /// <summary>
        /// Removes a Graph from the Dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        public abstract void RemoveGraph(Uri graphUri);

        /// <summary>
        /// Gets whether a Graph with the given URI is the Dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        public abstract bool HasGraph(Uri graphUri);

        /// <summary>
        /// Gets all the Graphs in the Dataset
        /// </summary>
        public abstract IEnumerable<IGraph> Graphs
        {
            get;
        }

        /// <summary>
        /// Gets all the URIs of Graphs in the Dataset
        /// </summary>
        public abstract IEnumerable<Uri> GraphUris
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
        public abstract IGraph this[Uri graphUri]
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
        public virtual IGraph GetModifiableGraph(Uri graphUri)
        {
            return this[graphUri];
        }

        /// <summary>
        /// Gets whether the Dataset has any Triples
        /// </summary>
        public virtual bool HasTriples
        {
            get 
            { 
                return this.Triples.Any(); 
            }
        }

        /// <summary>
        /// Gets whether the Dataset contains a specific Triple
        /// </summary>
        /// <param name="t">Triple</param>
        /// <returns></returns>
        public abstract bool ContainsTriple(Triple t);

        /// <summary>
        /// Gets all the Triples in the Dataset
        /// </summary>
        public virtual IEnumerable<Triple> Triples
        {
            get
            {
                if (this._activeGraph == null)
                {
                    if (this._defaultGraph == null)
                    {
                        //No specific Active Graph which implies that the Default Graph is the entire Triple Store
                        return this.GetAllTriples();
                    }
                    else
                    {
                        //Specific Default Graph so return that
                        return this._defaultGraph.Triples;
                    }
                }
                else
                {
                    //Active Graph is used (which may happen to be the Default Graph)
                    return this._activeGraph.Triples;
                }
            }
        }

        /// <summary>
        /// Abstract method that concrete implementations must implement to return an enumerable of all the Triples in the Dataset
        /// </summary>
        /// <returns></returns>
        protected abstract IEnumerable<Triple> GetAllTriples();

        /// <summary>
        /// Gets all the Triples in the Dataset with the given Subject
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <returns></returns>
        public abstract IEnumerable<Triple> GetTriplesWithSubject(INode subj);

        /// <summary>
        /// Gets all the Triples in the Dataset with the given Predicate
        /// </summary>
        /// <param name="pred">Predicate</param>
        /// <returns></returns>
        public abstract IEnumerable<Triple> GetTriplesWithPredicate(INode pred);

        /// <summary>
        /// Gets all the Triples in the Dataset with the given Object
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        public abstract IEnumerable<Triple> GetTriplesWithObject(INode obj);

        /// <summary>
        /// Gets all the Triples in the Dataset with the given Subject and Predicate
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <param name="pred">Predicate</param>
        /// <returns></returns>
        public abstract IEnumerable<Triple> GetTriplesWithSubjectPredicate(INode subj, INode pred);

        /// <summary>
        /// Gets all the Triples in the Dataset with the given Subject and Object
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        public abstract IEnumerable<Triple> GetTriplesWithSubjectObject(INode subj, INode obj);

        /// <summary>
        /// Gets all the Triples in the Dataset with the given Predicate and Object
        /// </summary>
        /// <param name="pred">Predicate</param>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        public abstract IEnumerable<Triple> GetTriplesWithPredicateObject(INode pred, INode obj);

        /// <summary>
        /// Ensures that any changes to the Dataset are flushed to the underlying Storage (if any)
        /// </summary>
        public abstract void Flush();
    }
}
