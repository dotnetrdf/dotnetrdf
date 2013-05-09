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
using System.Threading;
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
    public abstract class BaseSparqlView 
        : Graph
    {
        /// <summary>
        /// SPARQL Query
        /// </summary>
        protected SparqlQuery _q;
        /// <summary>
        /// Graphs that are mentioned in the Query
        /// </summary>
        protected HashSet<String> _graphs;
        /// <summary>
        /// Triple Store the query operates over
        /// </summary>
        protected ITripleStore _store;

        private UpdateViewDelegate _async;
        private bool _requiresInvalidate = false;
        private RdfQueryException _lastError;
        private ReaderWriterLock _lock = new ReaderWriterLock();

        /// <summary>
        /// Creates a new SPARQL View
        /// </summary>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <param name="store">Triple Store to query</param>
        protected BaseSparqlView(String sparqlQuery, ITripleStore store)
        {
            SparqlQueryParser parser = new SparqlQueryParser();
            this._q = parser.ParseFromString(sparqlQuery);
            this._store = store;

            this._async = new UpdateViewDelegate(this.UpdateViewInternal);
            this.Initialise();
        }

        /// <summary>
        /// Creates a new SPARQL View
        /// </summary>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <param name="store">Triple Store to query</param>
        protected BaseSparqlView(SparqlParameterizedString sparqlQuery, ITripleStore store)
            : this(sparqlQuery.ToString(), store) { }

        /// <summary>
        /// Creates a new SPARQL View
        /// </summary>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <param name="store">Triple Store to query</param>
        protected BaseSparqlView(SparqlQuery sparqlQuery, ITripleStore store)
        {
            this._q = sparqlQuery;
            this._store = store;

            this._async = new UpdateViewDelegate(this.UpdateViewInternal);
            this.Initialise();
        }

        /// <summary>
        /// Initialises the SPARQL View
        /// </summary>
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
            this._store.GraphRemoved += this.OnGraphRemoved;
            this._store.GraphMerged += this.OnGraphMerged;

            //Fill the Graph with the results of the Query
            this.UpdateViewInternal();
        }

        /// <summary>
        /// Invalidates the View causing it to be updated
        /// </summary>
        private void InvalidateView()
        {
            //Can't invalidate if an async UpdateView() call is in progress
            if (this._lock.IsWriterLockHeld)
            {
                this._requiresInvalidate = true;
                return;
            }

            this._lock.AcquireWriterLock(1000);
            if (this._lock.IsWriterLockHeld)
            {
                this._async.BeginInvoke(new AsyncCallback(this.InvalidateViewCompleted), null);
            }
            else
            {
                throw new RdfQueryException("Unable to acquire lock to update SPARQL view");
            }
        }

        /// <summary>
        /// Callback for when asychronous invalidation completes
        /// </summary>
        /// <param name="result">Async call results</param>
        private void InvalidateViewCompleted(IAsyncResult result)
        {
            try
            {
                this._async.EndInvoke(result);
                this._lock.ReleaseWriterLock();

                //If we've been further invalidated then need to re-query
                if (this._requiresInvalidate)
                {
                    this.InvalidateView();
                    this._requiresInvalidate = false;
                }
            }
            catch (Exception ex)
            {
                //Ignore errors
                this.LastError = new RdfQueryException("Unable to complete update of SPARQL View, see inner exception for details", ex);
            }
        }

        private delegate void UpdateViewDelegate();

        /// <summary>
        /// Forces the view to be updated
        /// </summary>
        public void UpdateView()
        {
            this.UpdateViewInternal();
            if (this.LastError != null) throw this.LastError;
        }

        /// <summary>
        /// Abstract method that derived classes should implement to update the view
        /// </summary>
        protected abstract void UpdateViewInternal();

        /// <summary>
        /// Gets the error that occurred during the last update (if any)
        /// </summary>
        public RdfQueryException LastError
        {
            get
            {
                return this._lastError;
            }
            protected set
            {
                this._lastError = value;
            }
        }

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

        private void OnGraphMerged(Object sender, TripleStoreEventArgs args)
        {
            if (args.GraphEvent != null)
            {
                IGraph g = args.GraphEvent.Graph;
                if (g != null)
                {
                    //Ignore merges to self
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
    public class SparqlView
        : BaseSparqlView
    {

        /// <summary>
        /// Creates a new SPARQL View
        /// </summary>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <param name="store">Triple Store to query</param>
        public SparqlView(String sparqlQuery, IInMemoryQueryableStore store)
            : base(sparqlQuery, store) { }

        /// <summary>
        /// Creates a new SPARQL View
        /// </summary>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <param name="store">Triple Store to query</param>
        public SparqlView(SparqlParameterizedString sparqlQuery, IInMemoryQueryableStore store)
            : this(sparqlQuery.ToString(), store) { }

        /// <summary>
        /// Creates a new SPARQL View
        /// </summary>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <param name="store">Triple Store to query</param>
        public SparqlView(SparqlQuery sparqlQuery, IInMemoryQueryableStore store)
            : base(sparqlQuery, store) { }

        /// <summary>
        /// Updates the view by making the SPARQL Query in-memory over the relevant Triple Store
        /// </summary>
        protected override void UpdateViewInternal()
        {
            try
            {
                Object results = ((IInMemoryQueryableStore)this._store).ExecuteQuery(this._q);
                if (results is IGraph)
                {
                    this.DetachEventHandlers(this._triples);
                    IGraph g = (IGraph)results;
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
                this.LastError = null;
                this.RaiseGraphChanged();
            }
            catch (RdfQueryException queryEx)
            {
                this.LastError = new RdfQueryException("Unable to Update a SPARQL View as an error occurred in processing the Query - see Inner Exception for details", queryEx);
            }
        }
    }

    /// <summary>
    /// Represents a SPARQL View over an arbitrary native Triple Store
    /// </summary>
    public class NativeSparqlView
        : BaseSparqlView
    {
        /// <summary>
        /// Creates a new SPARQL View
        /// </summary>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <param name="store">Triple Store to query</param>
        public NativeSparqlView(String sparqlQuery, INativelyQueryableStore store)
            : base(sparqlQuery, store) { }

        /// <summary>
        /// Creates a new SPARQL View
        /// </summary>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <param name="store">Triple Store to query</param>
        public NativeSparqlView(SparqlParameterizedString sparqlQuery, INativelyQueryableStore store)
            : this(sparqlQuery.ToString(), store) { }

        /// <summary>
        /// Creates a new SPARQL View
        /// </summary>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <param name="store">Triple Store to query</param>
        public NativeSparqlView(SparqlQuery sparqlQuery, INativelyQueryableStore store)
            : base(sparqlQuery, store) { }

        /// <summary>
        /// Updates the view by making the query over the Native Store (i.e. the query is handled by the stores SPARQL implementation)
        /// </summary>
        protected override void UpdateViewInternal()
        {
            try
            {
                Object results = ((INativelyQueryableStore)this._store).ExecuteQuery(this._q.ToString());
                if (results is IGraph)
                {
                    this.DetachEventHandlers(this._triples);
                    IGraph g = (IGraph)results;
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
                this.LastError = null;
                this.RaiseGraphChanged();
            }
            catch (RdfQueryException queryEx)
            {
                this.LastError = new RdfQueryException("Unable to Update a SPARQL View as an error occurred in processing the Query - see Inner Exception for details", queryEx);
            }
        }
    }
}
