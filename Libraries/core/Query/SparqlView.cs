using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;

namespace VDS.RDF.Query
{
    /// <summary>
    /// Abstract Base class for SPARQL Views which are Graphs which are generated from SPARQL Queries and get automatically updated when the Store they are attached to changes
    /// </summary>
    /// <remarks>
    /// <para>
    /// CONSTRUCT, DESCRIBE or SELECT queries can be used to generate a Graph.  If you use a SELECT query the returned variables must contain ?s, ?p and ?o in order to generate a view correctly
    /// </para>
    /// </remarks>
    public abstract class BaseSparqlView : Graph
    {
        protected SparqlQuery _q;
        protected HashSet<String> _graphs;
        protected ITripleStore _store;

        private UpdateViewDelegate _async;
        private IAsyncResult _asyncResult;
        private bool _requiresInvalidate = false;

        public BaseSparqlView(String sparqlQuery, ITripleStore store)
        {
            SparqlQueryParser parser = new SparqlQueryParser();
            this._q = parser.ParseFromString(sparqlQuery);
            this._store = store;

            this._async = new UpdateViewDelegate(this.UpdateViewInternal);
            this.Initialise();
        }

        public BaseSparqlView(SparqlParameterizedString sparqlQuery, ITripleStore store)
            : this(sparqlQuery.ToString(), store) { }

        public BaseSparqlView(SparqlQuery q, ITripleStore store)
        {
            this._q = q;
            this._store = store;

            this._async = new UpdateViewDelegate(this.UpdateViewInternal);
            this.Initialise();
        }

        private void Initialise()
        {
            if (this._q.QueryType == SparqlQueryType.Ask)
            {
                throw new RdfQueryException("Cannot create a SPARQL View based on an ASK Query");
            }

            //Does this Query operate over specific Graphs?
            if (this._q.DefaultGraphs.Any() || this._q.NamedGraphs.Any())
            {
                this._graphs = new HashSet<string>();
                foreach (Uri u in this._q.DefaultGraphs)
                {
                    this._graphs.Add(u.ToSafeString());
                }
                foreach (Uri u in this._q.NamedGraphs)
                {
                    this._graphs.Add(u.ToSafeString());
                }
            }

            //Attach a Handler to the Store's Graph Added, Removed and Changed events
            this._store.GraphChanged += this.OnGraphChanged;
            this._store.GraphAdded += this.OnGraphAdded;
            this._store.GraphAdded += this.OnGraphRemoved;

            //Fill the Graph with the results of the Query
            this.UpdateView();
        }

        private void InvalidateView()
        {
            //Can't invalidate if an async UpdateView() call is in progress
            if (this._asyncResult != null)
            {
                this._requiresInvalidate = true;
                return;
            }

            this._asyncResult = this._async.BeginInvoke(new AsyncCallback(this.InvalidateViewCompleted), null);
        }

        private void InvalidateViewCompleted(IAsyncResult result)
        {
            try
            {
                this._async.EndInvoke(result);
                this._asyncResult = null;

                //If we've been further invalidated then need to re-query
                if (this._requiresInvalidate)
                {
                    this.InvalidateView();
                    this._requiresInvalidate = false;
                }
            }
            catch
            {
                //Ignore errors
            }
        }

        private delegate void UpdateViewDelegate();

        public void UpdateView()
        {
            if (this._asyncResult != null)
            {
                this._asyncResult.AsyncWaitHandle.WaitOne(new TimeSpan(1000));
            }
            else
            {
                this.UpdateViewInternal();
            }
        }

        protected abstract void UpdateViewInternal();

        private void OnGraphChanged(Object sender, TripleStoreEventArgs args)
        {
            if (args.GraphEvent != null)
            {
                IGraph g = args.GraphEvent.Graph;
                if (g != null)
                {
                    //Ignore Changes to self
                    if (ReferenceEquals(g, this)) return;

                    if (this._graphs == null)
                    {
                        //No specific Graphs so any change causes an invalidation
                        this.InvalidateView();
                    }
                    else
                    {
                        //If specific Graphs only invalidate when those Graphs change
                        if (this._graphs.Contains(g.BaseUri.ToSafeString()))
                        {
                            this.InvalidateView();
                        }
                    }
                }
            }
        }

        private void OnGraphAdded(Object sender, TripleStoreEventArgs args)
        {
            if (args.GraphEvent != null)
            {
                IGraph g = args.GraphEvent.Graph;
                if (g != null)
                {
                    //Ignore Changes to self
                    if (ReferenceEquals(g, this)) return;

                    if (this._graphs == null)
                    {
                        //No specific Graphs so any change causes an invalidation
                        this.InvalidateView();
                    }
                    else
                    {
                        //If specific Graphs only invalidate when those Graphs change
                        if (this._graphs.Contains(g.BaseUri.ToSafeString()))
                        {
                            this.InvalidateView();
                        }
                    }
                }
            }
        }

        private void OnGraphRemoved(Object sender, TripleStoreEventArgs args)
        {
            if (args.GraphEvent != null)
            {
                IGraph g = args.GraphEvent.Graph;
                if (g != null)
                {
                    //Ignore Changes to self
                    if (ReferenceEquals(g, this)) return;

                    if (this._graphs == null)
                    {
                        //No specific Graphs so any change causes an invalidation
                        this.InvalidateView();
                    }
                    else
                    {
                        //If specific Graphs only invalidate when those Graphs change
                        if (this._graphs.Contains(g.BaseUri.ToSafeString()))
                        {
                            this.InvalidateView();
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Represents a SPARQL View over an in-memory store
    /// </summary>
    public class SparqlView : BaseSparqlView
    {

        public SparqlView(String sparqlQuery, IInMemoryQueryableStore store)
            : base(sparqlQuery, store) { }

        public SparqlView(SparqlParameterizedString sparqlQuery, IInMemoryQueryableStore store)
            : this(sparqlQuery.ToString(), store) { }

        public SparqlView(SparqlQuery q, IInMemoryQueryableStore store)
            : base(q, store) { }

        protected override void UpdateViewInternal()
        {
            try
            {
                Object results = ((IInMemoryQueryableStore)this._store).ExecuteQuery(this._q);
                if (results is Graph)
                {
                    this.DetachEventHandlers(this._triples);
                    Graph g = (Graph)results;
                    foreach (Triple t in g.Triples)
                    {
                        this._triples.Add(t.CopyTriple(this));
                    }
                    this.AttachEventHandlers(this._triples);
                }
                else
                {
                    this.DetachEventHandlers(this._triples);
                    this._triples = ((SparqlResultSet)results).ToTripleCollection(this);
                    this.AttachEventHandlers(this._triples);
                }
                this.RaiseGraphChanged();
            }
            catch (RdfQueryException queryEx)
            {
                throw new RdfQueryException("Unable to Update a SPARQL View as an error occurred in processing the Query - see Inner Exception for details", queryEx);
            }
        }
    }

    /// <summary>
    /// Represents a SPARQL View over an arbitrary native Triple Store
    /// </summary>
    public class NativeSparqlView : BaseSparqlView
    {

        public NativeSparqlView(String sparqlQuery, INativelyQueryableStore store)
            : base(sparqlQuery, store) { }

        public NativeSparqlView(SparqlParameterizedString sparqlQuery, INativelyQueryableStore store)
            : this(sparqlQuery.ToString(), store) { }

        public NativeSparqlView(SparqlQuery q, INativelyQueryableStore store)
            : base(q, store) { }

        protected override void UpdateViewInternal()
        {
            try
            {
                Object results = ((INativelyQueryableStore)this._store).ExecuteQuery(this._q.ToString());
                if (results is Graph)
                {
                    this.DetachEventHandlers(this._triples);
                    Graph g = (Graph)results;
                    foreach (Triple t in g.Triples)
                    {
                        this._triples.Add(t.CopyTriple(this));
                    }
                    this.AttachEventHandlers(this._triples);
                }
                else
                {
                    this.DetachEventHandlers(this._triples);
                    this._triples = ((SparqlResultSet)results).ToTripleCollection(this);
                    this.AttachEventHandlers(this._triples);
                }
                this.RaiseGraphChanged();
            }
            catch (RdfQueryException queryEx)
            {
                throw new RdfQueryException("Unable to Update a SPARQL View as an error occurred in processing the Query - see Inner Exception for details", queryEx);
            }
        }
    }
}
