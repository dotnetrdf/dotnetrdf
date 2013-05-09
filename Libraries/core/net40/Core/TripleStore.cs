/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

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
    public class TripleStore
        : BaseTripleStore, IInMemoryQueryableStore, IInferencingTripleStore, IUpdateableTripleStore
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
        protected Uri _inferenceGraphUri = UriFactory.Create("dotNetRDF:inference-graph");

        private LeviathanQueryProcessor _processor;

        /// <summary>
        /// Creates a new Triple Store using a new empty Graph collection
        /// </summary>
        public TripleStore()
            : this(new GraphCollection()) { }

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

        #endregion

        #region SPARQL Selection

        /// <summary>
        /// Executes a SPARQL Query on the Triple Store
        /// </summary>
        /// <param name="query">SPARQL Query as unparsed String</param>
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
        /// <remarks>
        /// <para>
        /// This method of making queries often leads to no results because of misconceptions about what data is being queries.  dotNetRDF's SPARQL engine only queries the default unnamed graph of the triple store (the graph added with a null URI) by default unless your query uses FROM clauses to change the default graph or you use GRAPH clauses to access named graphs in the store.  Therefore a common mistake is to add a single graph to the store and then query the store which typically results in no results because usually the added graph is named and so is not queried.
        /// </para>
        /// <para>
        /// We recommend using a <see cref="ISparqlQueryProcessor"/> instead for making queries over in-memory data since using our standard implementation (<see cref="LeviathanQueryProcessor"/>) affords you much more explicit control over which graphs are queried.
        /// </para>
        /// </remarks>
        [Obsolete("This method of making queries is often error prone due to misconceptions about what data is being queries and we recommend using an ISparqlQueryProcessor instead, see remarks for more discussion")]
        public virtual Object ExecuteQuery(SparqlQuery query)
        {
            if (this._processor == null) this._processor = new LeviathanQueryProcessor(new InMemoryQuadDataset(this));
            return this._processor.ProcessQuery(query);
        }

        /// <summary>
        /// Executes a SPARQL Query on the Triple Store processing the results with an appropriate handler from those provided
        /// </summary>
        /// <param name="rdfHandler">RDF Handler</param>
        /// <param name="resultsHandler">Results Handler</param>
        /// <param name="query">SPARQL Query as unparsed String</param>
        /// <remarks>
        /// <para>
        /// This method of making queries often leads to no results because of misconceptions about what data is being queries.  dotNetRDF's SPARQL engine only queries the default unnamed graph of the triple store (the graph added with a null URI) by default unless your query uses FROM clauses to change the default graph or you use GRAPH clauses to access named graphs in the store.  Therefore a common mistake is to add a single graph to the store and then query the store which typically results in no results because usually the added graph is named and so is not queried.
        /// </para>
        /// <para>
        /// We recommend using a <see cref="ISparqlQueryProcessor"/> instead for making queries over in-memory data since using our standard implementation (<see cref="LeviathanQueryProcessor"/>) affords you much more explicit control over which graphs are queried.
        /// </para>
        /// </remarks>
        [Obsolete("This method of making queries is often error prone due to misconceptions about what data is being queries and we recommend using an ISparqlQueryProcessor instead, see remarks for more discussion")]
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
        /// <remarks>
        /// <para>
        /// This method of making queries often leads to no results because of misconceptions about what data is being queries.  dotNetRDF's SPARQL engine only queries the default unnamed graph of the triple store (the graph added with a null URI) by default unless your query uses FROM clauses to change the default graph or you use GRAPH clauses to access named graphs in the store.  Therefore a common mistake is to add a single graph to the store and then query the store which typically results in no results because usually the added graph is named and so is not queried.
        /// </para>
        /// <para>
        /// We recommend using a <see cref="ISparqlQueryProcessor"/> instead for making queries over in-memory data since using our standard implementation (<see cref="LeviathanQueryProcessor"/>) affords you much more explicit control over which graphs are queried.
        /// </para>
        /// </remarks>
        [Obsolete("This method of making queries is often error prone due to misconceptions about what data is being queries and we recommend using an ISparqlQueryProcessor instead, see remarks for more discussion")]
        public virtual void ExecuteQuery(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, SparqlQuery query)
        {
            if (this._processor == null) this._processor = new LeviathanQueryProcessor(new InMemoryQuadDataset(this));
            this._processor.ProcessQuery(rdfHandler, resultsHandler, query);
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

    /// <summary>
    /// A thread safe variant of <see cref="TripleStore"/>, simply a <see cref="TripleStore"/> instance with a <see cref="ThreadSafeGraphCollection"/> decorator around it's underlying <see cref="BaseGraphCollection"/>
    /// </summary>
    public class ThreadSafeTripleStore
        : TripleStore
    {
        /// <summary>
        /// Creates a new Thread Safe triple store
        /// </summary>
        public ThreadSafeTripleStore()
            : base(new ThreadSafeGraphCollection()) { }

        /// <summary>
        /// Creates a new Thread safe triple store using the given Thread safe graph collection
        /// </summary>
        /// <param name="collection">Collection</param>
        public ThreadSafeTripleStore(ThreadSafeGraphCollection collection)
            : base(collection) { }

        /// <summary>
        /// Creates a new Thread safe triple store using a thread safe decorator around the given graph collection
        /// </summary>
        /// <param name="collection">Collection</param>
        public ThreadSafeTripleStore(BaseGraphCollection collection)
            : this(new ThreadSafeGraphCollection(collection)) { }
    }
}
