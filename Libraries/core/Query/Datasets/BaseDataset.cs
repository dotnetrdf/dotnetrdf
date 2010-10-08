using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Query.Datasets
{
    public abstract class BaseDataset : ISparqlDataset
    {
        /// <summary>
        /// Reference to the Active Graph being used for executing a Sparql Query
        /// </summary>
        protected IGraph _activeGraph = null;
        /// <summary>
        /// Default Graph for executing Sparql Queries against
        /// </summary>
        protected IGraph _defaultGraph = null;
        /// <summary>
        /// Stack of Default Graph References used for executing a SPARQL Query
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
                throw new RdfQueryException("A Graph with URI '" + graphUri.ToString() + "' does not exist in this Triple Store, a GRAPH Clause cannot be used to change the Active Graph to a Graph that doesn't exist");
            }
        }

        /// <summary>
        /// Sets the Active Graph for the Sparql query
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
                    else
                    {
                        throw new RdfQueryException("A Graph with URI '" + u.ToString() + "' does not exist in this Triple Store, a GRAPH Clause cannot be used to change the Active Graph to a Graph that doesn't exist");
                    }
                }

                //Push current Active Graph on the Stack
                this._activeGraphs.Push(this._activeGraph);

                //Set the new Active Graph
                this._activeGraph = g;
            }
        }

        /// <summary>
        /// Sets the Active Graph for the Sparql query to be the previous Active Graph
        /// </summary>
        /// <remarks>Helper function used primarily in the execution of GRAPH Clauses</remarks>
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

        #endregion

        public abstract void AddGraph(IGraph g);

        public abstract void RemoveGraph(Uri graphUri);

        public abstract bool HasGraph(Uri graphUri);

        public abstract IEnumerable<IGraph> Graphs
        {
            get;
        }

        public abstract IEnumerable<Uri> GraphUris
        {
            get;
        }

        public abstract IGraph this[Uri graphUri]
        {
            get;
        }

        public virtual bool HasTriples
        {
            get 
            { 
                return this.Triples.Any(); 
            }
        }

        public abstract bool ContainsTriple(Triple t);

        public abstract IEnumerable<Triple> Triples
        {
            get;
        }

        public abstract IEnumerable<Triple> GetTriplesWithSubject(INode subj);

        public abstract IEnumerable<Triple> GetTriplesWithPredicate(INode pred);

        public abstract IEnumerable<Triple> GetTriplesWithObject(INode obj);

        public abstract IEnumerable<Triple> GetTriplesWithSubjectPredicate(INode subj, INode pred);

        public abstract IEnumerable<Triple> GetTriplesWithSubjectObject(INode subj, INode obj);

        public abstract IEnumerable<Triple> GetTriplesWithPredicateObject(INode pred, INode obj);

        public abstract void Flush();
    }
}
