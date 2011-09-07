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
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Query.Inference;
using VDS.RDF.Parsing;
using VDS.RDF.Update;

namespace VDS.RDF
{
    /// <summary>
    /// Class for representing Triple Stores which are collections of RDF Graphs
    /// </summary>
    public class TripleStore : BaseTripleStore, IInMemoryQueryableStore, IInferencingTripleStore, IUpdateableTripleStore
    {
        /// <summary>
        /// List of Reasoners that are applied to Graphs as they are added to the Triple Store
        /// </summary>
        protected List<IInferenceEngine> _reasoners = new List<IInferenceEngine>();
        /// <summary>
        /// Controls whether inferred information is stored in a special Graph or in the original Graph
        /// </summary>
        protected bool _storeInferencesExternally = false;
        /// <summary>
        /// Graph Uri for the special Graph used to store inferred information
        /// </summary>
        protected Uri _inferenceGraphUri = new Uri("dotNetRDF:inference-graph");

        /// <summary>
        /// Creates a new Triple Store using a new empty Graph collection
        /// </summary>
        public TripleStore()
            : base(new GraphCollection()) { }

        /// <summary>
        /// Creates a new Triple Store using the given Graph collection which may be non-empty
        /// </summary>
        /// <param name="graphCollection">Graph Collection</param>
        public TripleStore(BaseGraphCollection graphCollection)
            : base(graphCollection) { }

        #region Selection

        /// <summary>
        /// Returns whether the Store contains the given Triple within the Query Triples
        /// </summary>
        /// <param name="t">Triple to search for</param>
        /// <returns></returns>
        public bool Contains(Triple t)
        {
            return this._graphs.Any(g => g.Triples.Contains(t));
        }

        #region Selection over Entire Triple Store

        /// <summary>
        /// Selects all Nodes that meet the criteria of a given ISelector from all the Query Triples
        /// </summary>
        /// <param name="selector">A Selector on Nodes</param>
        /// <returns></returns>
        public IEnumerable<INode> GetNodes(ISelector<INode> selector)
        {
            return (from g in this._graphs
                    from n in g.Nodes
                    where selector.Accepts(n)
                    select n);           
        }

        /// <summary>
        /// Selects all Triples which are selected by a chain of Selectors from all the Query Triples
        /// </summary>
        /// <param name="firstSelector">First Selector in the Chain</param>
        /// <param name="selectorChain">Dependent Selectors which form the rest of the Chain</param>
        /// <returns></returns>
        public IEnumerable<Triple> GetTriples(ISelector<Triple> firstSelector, List<IDependentSelector<Triple>> selectorChain)
        {
            if (selectorChain.Count == 0)
            {
                //Just return the Result of using the first Selector
                return this.GetTriples(firstSelector);
            }
            else
            {
                //Chain the Dependant Selectors together
                IEnumerable<Triple> ts = this.GetTriples(firstSelector);

                for (int i = 0; i < selectorChain.Count(); i++)
                {
                    //Initialise the Next Selector
                    selectorChain[i].Initialise(ts);
                    //Run the Next Selector
                    ts = this.GetTriples(selectorChain[i]);
                }

                //Return the end results
                return ts;
            }
        }

        /// <summary>
        /// Selects all Triples which are selected by a chain of Selectors from all the Query Triples
        /// </summary>
        /// <param name="selectorChain">Chain of Independent Selectors</param>
        /// <returns></returns>
        public IEnumerable<Triple> GetTriples(List<ISelector<Triple>> selectorChain)
        {
            if (selectorChain.Count == 0)
            {
                //Use the None Selector to give an empty enumeration
                return Enumerable.Empty<Triple>();
            }
            else if (selectorChain.Count == 1)
            {
                //Only 1 Selector
                return this.GetTriples(selectorChain[0]);
            }
            else
            {
                //Multiple Selectors

                //Use 1st to get an initial enumeration
                IEnumerable<Triple> ts = this.GetTriples(selectorChain[0]);

                //Chain the subsequent Selectors
                for (int i = 1; i < selectorChain.Count(); i++)
                {
                    ts = this.GetTriples(ts, selectorChain[i]);
                }

                //Return the end results
                return ts;
            }
        }

        /// <summary>
        /// Internal Helper method for applying a Selector to a subset of the Triples in the Triple Store
        /// </summary>
        /// <param name="triples">Subset of Triples</param>
        /// <param name="selector">Selector Class to perform the Selection</param>
        /// <returns></returns>
        private IEnumerable<Triple> GetTriples(IEnumerable<Triple> triples, ISelector<Triple> selector)
        {
            IEnumerable<Triple> ts = from t in triples
                                     where selector.Accepts(t)
                                     select t;

            return ts;
        }

        /// <summary>
        /// Selects all Triples which have a Uri Node with the given Uri from all the Query Triples
        /// </summary>
        /// <param name="uri">Uri</param>
        /// <returns></returns>
        public IEnumerable<Triple> GetTriples(Uri uri)
        {
            IUriNode u = new UriNode(null, uri);
            return from g in this._graphs
                   from t in g.Triples
                   where t.Involves(u)
                   select t;

        }

        /// <summary>
        /// Selects all Triples which meet the criteria of an ISelector from all the Query Triples
        /// </summary>
        /// <param name="selector">A Selector on Triples</param>
        /// <returns></returns>
        public IEnumerable<Triple> GetTriples(ISelector<Triple> selector)
        {
            return (from g in this._graphs
                    from t in g.Triples
                    where selector.Accepts(t)
                    select t);
        }

        /// <summary>
        /// Selects all Triples which contain the given Node from all Graphs in the Triple Store
        /// </summary>
        /// <param name="n">Node</param>
        /// <returns></returns>
        public IEnumerable<Triple> GetTriples(INode n)
        {
            return (from g in this._graphs
                    from t in g.Triples
                    where t.Involves(n)
                    select t);
        }

        /// <summary>
        /// Selects all Triples where the Object is a Uri Node with the given Uri from all Graphs in the Triple Store
        /// </summary>
        /// <param name="u">Uri</param>
        /// <returns></returns>
        public IEnumerable<Triple> GetTriplesWithObject(Uri u)
        {
            return this.GetTriplesWithObject(new UriNode(null, u));
        }

        /// <summary>
        /// Selects all Triples where the Object Node meets the criteria of an ISelector from all Graphs in the Triple Store
        /// </summary>
        /// <param name="selector">A Selector on Nodes</param>
        /// <returns></returns>
        public IEnumerable<Triple> GetTriplesWithObject(ISelector<INode> selector)
        {
            return (from g in this._graphs
                    from t in g.Triples
                    where selector.Accepts(t.Object)
                    select t);
        }

        /// <summary>
        /// Selects all Triples where the Object is a given Node from all Graphs in the Triple Store
        /// </summary>
        /// <param name="n">Node</param>
        /// <returns></returns>
        public IEnumerable<Triple> GetTriplesWithObject(INode n)
        {
            return (from g in this._graphs
                    from t in g.Triples.WithObject(n)
                    select t);
        }

        /// <summary>
        /// Selects all Triples where the Predicate is a given Node from all Graphs in the Triple Store
        /// </summary>
        /// <param name="n">Node</param>
        /// <returns></returns>
        public IEnumerable<Triple> GetTriplesWithPredicate(INode n)
        {
            return (from g in this._graphs
                    from t in g.Triples.WithPredicate(n)
                    select t);
        }

        /// <summary>
        /// Selects all Triples where the Predicate is a Uri Node with the given Uri from all Graphs in the Triple Store
        /// </summary>
        /// <param name="u">Uri</param>
        /// <returns></returns>
        public IEnumerable<Triple> GetTriplesWithPredicate(Uri u)
        {
            return this.GetTriplesWithPredicate(new UriNode(null, u));
        }

        /// <summary>
        /// Selects all Triples where the Predicate meets the criteria of an ISelector from all Graphs in the Triple Store
        /// </summary>
        /// <param name="selector">A Selector on Nodes</param>
        /// <returns></returns>
        public IEnumerable<Triple> GetTriplesWithPredicate(ISelector<INode> selector)
        {
            return (from g in this._graphs
                    from t in g.Triples
                    where selector.Accepts(t.Predicate)
                    select t);
        }

        /// <summary>
        /// Selects all Triples where the Subject is a given Node from all Graphs in the Triple Store
        /// </summary>
        /// <param name="n">Node</param>
        /// <returns></returns>
        public IEnumerable<Triple> GetTriplesWithSubject(INode n)
        {
            return (from g in this._graphs
                    from t in g.Triples.WithSubject(n)
                    select t);
        }

        /// <summary>
        /// Selects all Triples where the Subject is a Uri Node with the given Uri from all Graphs in the Triple Store
        /// </summary>
        /// <param name="u">Uri</param>
        /// <returns></returns>
        public IEnumerable<Triple> GetTriplesWithSubject(Uri u)
        {
            return this.GetTriplesWithSubject(new UriNode(null, u));
        }

        /// <summary>
        /// Selects all Triples where the Subject meets the criteria of an ISelector from all Graphs in the Triple Store
        /// </summary>
        /// <param name="selector">A Selector on Nodes</param>
        /// <returns></returns>
        public IEnumerable<Triple> GetTriplesWithSubject(ISelector<INode> selector)
        {
            return (from g in this._graphs
                    from t in g.Triples
                    where selector.Accepts(t.Subject)
                    select t);
        }

        /// <summary>
        /// Checks whether any Triples meeting the criteria of an ISelector can be found from all Graphs in the Triple Store
        /// </summary>
        /// <param name="selector">A Selector on Triples</param>
        /// <returns></returns>
        public bool TriplesExist(ISelector<Triple> selector)
        {
            return (this.GetTriples(selector).Any());
        }

        /// <summary>
        /// Selects all the Triples with the given Subject-Predicate pair from all the Query Triples
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <param name="pred">Predicate</param>
        /// <returns></returns>
        public IEnumerable<Triple> GetTriplesWithSubjectPredicate(INode subj, INode pred)
        {
            return (from g in this._graphs
                    from t in g.Triples.WithSubjectPredicate(subj, pred)
                    select t);
        }

        /// <summary>
        /// Selects all the Triples with the given Predicate-Object pair from all the Query Triples
        /// </summary>
        /// <param name="pred">Predicate</param>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        public IEnumerable<Triple> GetTriplesWithPredicateObject(INode pred, INode obj)
        {
            return (from g in this._graphs
                    from t in g.Triples.WithPredicateObject(pred, obj)
                    select t);
        }

        /// <summary>
        /// Selects all the Triples with the given Subject-Object pair from all the Query Triples
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        public IEnumerable<Triple> GetTriplesWithSubjectObject(INode subj, INode obj)
        {
            return (from g in this._graphs
                    from t in g.Triples.WithSubjectObject(subj, obj)
                    select t);
        }

        #endregion

        #region Selection over Subset of Triple Store

        /// <summary>
        /// Selects all Nodes that meet the criteria of a given ISelector from a Subset of Graphs in the Triple Store
        /// </summary>
        /// <param name="selector">A Selector on Nodes</param>
        /// <param name="graphUris">List of the Graph URIs of Graphs you want to select over</param>
        /// <returns></returns>
        public IEnumerable<INode> GetNodes(List<Uri> graphUris, ISelector<INode> selector)
        {
            IEnumerable<INode> ns = from g in this._graphs
                                    where graphUris.Contains(g.BaseUri)
                                    from n in g.Nodes
                                    where selector.Accepts(n)
                                    select n;

            return ns;
        }

        /// <summary>
        /// Selects all Triples which are selected by a chain of Selectors from a Subset of Graphs in the Triple Store where the results of each Selector influence the next selector and selection at each stage is over the selected subset of Graphs
        /// </summary>
        /// <param name="graphUris">List of the Graph URIs of Graphs you want to select over</param>
        /// <param name="firstSelector">First Selector in the Chain</param>
        /// <param name="selectorChain">Dependent Selectors which form the rest of the Chain</param>
        /// <returns></returns>
        public IEnumerable<Triple> GetTriples(List<Uri> graphUris, ISelector<Triple> firstSelector, List<IDependentSelector<Triple>> selectorChain)
        {
            if (selectorChain.Count == 0)
            {
                //Just return the Result of using the first Selector
                return this.GetTriples(graphUris, firstSelector);
            }
            else
            {
                //Chain the Dependant Selectors together
                IEnumerable<Triple> ts = this.GetTriples(graphUris, firstSelector);

                for (int i = 0; i < selectorChain.Count(); i++)
                {
                    //Initialise the Next Selector
                    selectorChain[i].Initialise(ts);
                    //Run the Next Selector
                    ts = this.GetTriples(graphUris, selectorChain[i]);
                }

                //Return the end results
                return ts;
            }
        }

        /// <summary>
        /// Selects all Triples which are selected by a chain of Selectors from a Subset of Graphs in the Triple Store where each Selector is independent and selection at each stage is over the results of the previous selection stages
        /// </summary>
        /// <param name="graphUris">List of the Graph URIs of Graphs you want to select over</param>
        /// <param name="selectorChain">Chain of Independent Selectors</param>
        /// <returns></returns>
        public IEnumerable<Triple> GetTriples(List<Uri> graphUris, List<ISelector<Triple>> selectorChain)
        {
            if (selectorChain.Count == 0)
            {
                //Use the None Selector to give an empty enumeration
                return this.GetTriples(new NoneSelector<Triple>());
            }
            else if (selectorChain.Count == 1)
            {
                //Only 1 Selector
                return this.GetTriples(graphUris, selectorChain[0]);
            }
            else
            {
                //Multiple Selectors

                //Use 1st to get an initial enumeration
                IEnumerable<Triple> ts = this.GetTriples(graphUris, selectorChain[0]);

                //Chain the subsequent Selectors
                for (int i = 1; i < selectorChain.Count(); i++)
                {
                    ts = this.GetTriples(ts, selectorChain[i]);
                }

                //Return the end results
                return ts;
            }
        }

        /// <summary>
        /// Selects all Triples which have a Uri Node with the given Uri from a Subset of Graphs in the Triple Store
        /// </summary>
        /// <param name="graphUris">List of the Graph URIs of Graphs you want to select over</param>
        /// <param name="uri">Uri</param>
        /// <returns></returns>
        public IEnumerable<Triple> GetTriples(List<Uri> graphUris, Uri uri)
        {
            IUriNode u = new UriNode(null, uri);

            IEnumerable<Triple> ts = from g in this._graphs
                                     where graphUris.Contains(g.BaseUri)
                                     from t in g.Triples
                                     where t.Involves(u)
                                     select t;

            return ts;
        }

        /// <summary>
        /// Selects all Triples which meet the criteria of an ISelector from a Subset of Graphs in the Triple Store
        /// </summary>
        /// <param name="graphUris">List of the Graph URIs of Graphs you want to select over</param>
        /// <param name="selector">A Selector on Triples</param>
        /// <returns></returns>
        public IEnumerable<Triple> GetTriples(List<Uri> graphUris, ISelector<Triple> selector)
        {
            IEnumerable<Triple> ts = from g in this._graphs
                                     where graphUris.Contains(g.BaseUri)
                                     from t in g.Triples
                                     where selector.Accepts(t)
                                     select t;

            return ts;
        }

        /// <summary>
        /// Selects all Triples which contain the given Node from a Subset of Graphs in the Triple Store
        /// </summary>
        /// <param name="graphUris">List of the Graph URIs of Graphs you want to select over</param>
        /// <param name="n">Node</param>
        /// <returns></returns>
        public IEnumerable<Triple> GetTriples(List<Uri> graphUris, INode n)
        {
            IEnumerable<Triple> ts = from g in this._graphs
                                     where graphUris.Contains(g.BaseUri)
                                     from t in g.Triples
                                     where t.Involves(n)
                                     select t;

            return ts;
        }

        /// <summary>
        /// Selects all Triples where the Object is a Uri Node with the given Uri from a Subset of Graphs in the Triple Store
        /// </summary>
        /// <param name="graphUris">List of the Graph URIs of Graphs you want to select over</param>
        /// <param name="u">Uri</param>
        /// <returns></returns>
        public IEnumerable<Triple> GetTriplesWithObject(List<Uri> graphUris, Uri u)
        {
            IUriNode uri = new UriNode(null, u);

            IEnumerable<Triple> ts = from g in this._graphs
                                     where graphUris.Contains(g.BaseUri)
                                     from t in g.Triples
                                     where t.HasObject(uri)
                                     select t;

            return ts;
        }

        /// <summary>
        /// Selects all Triples where the Object Node meets the criteria of an ISelector from a Subset of Graphs in the Triple Store
        /// </summary>
        /// <param name="graphUris">List of the Graph URIs of Graphs you want to select over</param>
        /// <param name="selector">A Selector on Nodes</param>
        /// <returns></returns>
        public IEnumerable<Triple> GetTriplesWithObject(List<Uri> graphUris, ISelector<INode> selector)
        {
            IEnumerable<Triple> ts = from g in this._graphs
                                     where graphUris.Contains(g.BaseUri)
                                     from t in g.Triples
                                     where selector.Accepts(t.Object)
                                     select t;

            return ts;
        }

        /// <summary>
        /// Selects all Triples where the Object is a given Node from a Subset of Graphs in the Triple Store
        /// </summary>
        /// <param name="graphUris">List of the Graph URIs of Graphs you want to select over</param>
        /// <param name="n">Node</param>
        /// <returns></returns>
        public IEnumerable<Triple> GetTriplesWithObject(List<Uri> graphUris, INode n)
        {
            IEnumerable<Triple> ts = from g in this._graphs
                                     where graphUris.Contains(g.BaseUri)
                                     from t in g.Triples
                                     where t.HasObject(n)
                                     select t;

            return ts;
        }

        /// <summary>
        /// Selects all Triples where the Predicate is a given Node from a Subset of Graphs in the Triple Store
        /// </summary>
        /// <param name="graphUris">List of the Graph URIs of Graphs you want to select over</param>
        /// <param name="n">Node</param>
        /// <returns></returns>
        public IEnumerable<Triple> GetTriplesWithPredicate(List<Uri> graphUris, INode n)
        {
            IEnumerable<Triple> ts = from g in this._graphs
                                     where graphUris.Contains(g.BaseUri)
                                     from t in g.Triples
                                     where t.HasPredicate(n)
                                     select t;

            return ts;
        }

        /// <summary>
        /// Selects all Triples where the Predicate is a Uri Node with the given Uri from a Subset of Graphs in the Triple Store
        /// </summary>
        /// <param name="graphUris">List of the Graph URIs of Graphs you want to select over</param>
        /// <param name="u">Uri</param>
        /// <returns></returns>
        public IEnumerable<Triple> GetTriplesWithPredicate(List<Uri> graphUris, Uri u)
        {
            IUriNode uri = new UriNode(null, u);

            IEnumerable<Triple> ts = from g in this._graphs
                                     where graphUris.Contains(g.BaseUri)
                                     from t in g.Triples
                                     where t.HasPredicate(uri)
                                     select t;

            return ts;
        }

        /// <summary>
        /// Selects all Triples where the Predicate meets the criteria of an ISelector from a Subset of Graphs in the Triple Store
        /// </summary>
        /// <param name="graphUris">List of the Graph URIs of Graphs you want to select over</param>
        /// <param name="selector">A Selector on Nodes</param>
        /// <returns></returns>
        public IEnumerable<Triple> GetTriplesWithPredicate(List<Uri> graphUris, ISelector<INode> selector)
        {
            IEnumerable<Triple> ts = from g in this._graphs
                                     where graphUris.Contains(g.BaseUri)
                                     from t in g.Triples
                                     where selector.Accepts(t.Predicate)
                                     select t;

            return ts;
        }

        /// <summary>
        /// Selects all Triples where the Subject is a given Node from a Subset of Graphs in the Triple Store
        /// </summary>
        /// <param name="graphUris">List of the Graph URIs of Graphs you want to select over</param>
        /// <param name="n">Node</param>
        /// <returns></returns>
        public IEnumerable<Triple> GetTriplesWithSubject(List<Uri> graphUris, INode n)
        {
            IEnumerable<Triple> ts = from g in this._graphs
                                     where graphUris.Contains(g.BaseUri)
                                     from t in g.Triples
                                     where t.HasSubject(n)
                                     select t;

            return ts;
        }

        /// <summary>
        /// Selects all Triples where the Subject is a Uri Node with the given Uri from a Subset of Graphs in the Triple Store
        /// </summary>
        /// <param name="graphUris">List of the Graph URIs of Graphs you want to select over</param>
        /// <param name="u">Uri</param>
        /// <returns></returns>
        public IEnumerable<Triple> GetTriplesWithSubject(List<Uri> graphUris, Uri u)
        {
            IUriNode uri = new UriNode(null, u);

            IEnumerable<Triple> ts = from g in this._graphs
                                     where graphUris.Contains(g.BaseUri)
                                     from t in g.Triples
                                     where t.HasSubject(uri)
                                     select t;

            return ts;
        }

        /// <summary>
        /// Selects all Triples where the Subject meets the criteria of an ISelector from a Subset of Graphs in the Triple Store
        /// </summary>
        /// <param name="graphUris">List of the Graph URIs of Graphs you want to select over</param>
        /// <param name="selector">A Selector on Nodes</param>
        /// <returns></returns>
        public IEnumerable<Triple> GetTriplesWithSubject(List<Uri> graphUris, ISelector<INode> selector)
        {
            IEnumerable<Triple> ts = from g in this._graphs
                                     where graphUris.Contains(g.BaseUri)
                                     from t in g.Triples
                                     where selector.Accepts(t.Subject)
                                     select t;

            return ts;
        }

        /// <summary>
        /// Checks whether any Triples meeting the criteria of an ISelector can be found from a Subset of Graphs in the Triple Store
        /// </summary>
        /// <param name="graphUris">List of the Graph URIs of Graphs you want to select over</param>
        /// <param name="selector">A Selector on Triples</param>
        /// <returns></returns>
        public bool TriplesExist(List<Uri> graphUris, ISelector<Triple> selector)
        {
            return (this.GetTriples(graphUris, selector).Count() > 0);
        }

        #endregion

        #region SPARQL Selection

        /// <summary>
        /// Executes a SPARQL Query on the Triple Store
        /// </summary>
        /// <param name="query">SPARQL Query as unparsed String</param>
        /// <returns></returns>
        public virtual Object ExecuteQuery(String query)
        {
            //Parse the Query
            SparqlQueryParser sparqlparser = new SparqlQueryParser();
            SparqlQuery q = sparqlparser.ParseFromString(query);

            //Invoke other execute method
            return this.ExecuteQuery(q);
        }

        /// <summary>
        /// Executes a SPARQL Query on the Triple Store
        /// </summary>
        /// <param name="query">SPARQL Query as a <see cref="SparqlQuery">SparqlQuery</see> instance</param>
        /// <returns></returns>
        public virtual Object ExecuteQuery(SparqlQuery query)
        {
            //Invoke Query's Evaluate method
            return query.Evaluate(this);
        }

        /// <summary>
        /// Executes a SPARQL Query on the Triple Store processing the results with an appropriate handler from those provided
        /// </summary>
        /// <param name="rdfHandler">RDF Handler</param>
        /// <param name="resultsHandler">Results Handler</param>
        /// <param name="query">SPARQL Query as unparsed String</param>
        public virtual void ExecuteQuery(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, String query)
        {
            //Parse the Query
            SparqlQueryParser sparqlparser = new SparqlQueryParser();
            SparqlQuery q = sparqlparser.ParseFromString(query);

            //Invoke other execute method
            this.ExecuteQuery(rdfHandler, resultsHandler, q);
        }

        /// <summary>
        /// Executes a SPARQL Query on the Triple Store processing the results with an appropriate handler from those provided
        /// </summary>
        /// <param name="rdfHandler">RDF Handler</param>
        /// <param name="resultsHandler">Results Handler</param>
        /// <param name="query">SPARQL Query as unparsed String</param>
        public virtual void ExecuteQuery(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, SparqlQuery query)
        {
            query.Evaluate(rdfHandler, resultsHandler, this);
        }

        #endregion

        #endregion

        #region Loading with Inference

        /// <summary>
        /// Applies Inference to the given Graph
        /// </summary>
        /// <param name="g">Graph to apply inference to</param>
        public void ApplyInference(IGraph g)
        {
            //Apply Inference if we have any Inference Engines defined
            if (this._reasoners.Count > 0)
            {
                //Set up Inference Graph if needed
                if (this._storeInferencesExternally)
                {
                    if (!this._graphs.Contains(this._inferenceGraphUri))
                    {
#if !NO_RWLOCK
                        IGraph i = new ThreadSafeGraph();
#else
                        IGraph i = new Graph();
#endif
                        i.BaseUri = this._inferenceGraphUri;
                        this._graphs.Add(i, true);
                    }
                }

                //Apply inference
                foreach (IInferenceEngine reasoner in this._reasoners)
                {
                    if (this._storeInferencesExternally)
                    {
                        reasoner.Apply(g, this._graphs[this._inferenceGraphUri]);
                    }
                    else
                    {
                        reasoner.Apply(g);
                    }
                }
            }
        }

        /// <summary>
        /// Adds an Inference Engine to the Triple Store
        /// </summary>
        /// <param name="reasoner">Reasoner to add</param>
        public void AddInferenceEngine(IInferenceEngine reasoner)
        {
            this._reasoners.Add(reasoner);

            //Apply Inference to all existing Graphs
            if (this._graphs.Count > 0)
            {
                lock (this._graphs)
                {
                    //Have to do a ToList() in case someone else inserts a Graph
                    //Which ApplyInference may do if the Inference information is stored in a special Graph
                    foreach (IGraph g in this._graphs.ToList())
                    {
                        this.ApplyInference(g);
                    }
                }
            }
        }

        /// <summary>
        /// Removes an Inference Engine from the Triple Store
        /// </summary>
        /// <param name="reasoner">Reasoner to remove</param>
        public void RemoveInferenceEngine(IInferenceEngine reasoner)
        {
            this._reasoners.Remove(reasoner);
        }

        /// <summary>
        /// Clears all Inference Engines from the Triple Store
        /// </summary>
        public void ClearInferenceEngines()
        {
            this._reasoners.Clear();
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Disposes of a Triple Store
        /// </summary>
        public override void Dispose()
        {
            this._graphs.Dispose();
        }

        #endregion

        #region IUpdateableTripleStore Members

        /// <summary>
        /// Executes an Update against the Triple Store
        /// </summary>
        /// <param name="update">SPARQL Update Command(s)</param>
        /// <remarks>
        /// As per the SPARQL 1.1 Update specification the command string may be a sequence of commands
        /// </remarks>
        public void ExecuteUpdate(string update)
        {
            SparqlUpdateParser parser = new SparqlUpdateParser();
            SparqlUpdateCommandSet commandSet = parser.ParseFromString(update);
            this.ExecuteUpdate(commandSet);
        }

        /// <summary>
        /// Executes a single Update Command against the Triple Store
        /// </summary>
        /// <param name="update">SPARQL Update Command</param>
        public void ExecuteUpdate(SparqlUpdateCommand update)
        {
            SparqlUpdateEvaluationContext context = new SparqlUpdateEvaluationContext(new InMemoryDataset(this));
            update.Evaluate(context);
        }

        /// <summary>
        /// Executes a set of Update Commands against the Triple Store
        /// </summary>
        /// <param name="updates">SPARQL Update Command Set</param>
        public void ExecuteUpdate(SparqlUpdateCommandSet updates)
        {
            SparqlUpdateEvaluationContext context = new SparqlUpdateEvaluationContext(new InMemoryDataset(this));

            for (int i = 0; i < updates.CommandCount; i++)
            {
                updates[i].Evaluate(context);
            }
        }

        #endregion

        /// <summary>
        /// Event Handler for the <see cref="BaseGraphCollection.GraphAdded">Graph Added</see> event of the underlying Graph Collection which calls the normal event processing of the parent class <see cref="BaseTripleStore">BaseTripleStore</see> and then applies Inference to the newly added Graph
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="args">Graph Event Arguments</param>
        protected override void OnGraphAdded(object sender, GraphEventArgs args)
        {
            base.OnGraphAdded(sender, args);
            this.ApplyInference(args.Graph);
        }
    }
}
