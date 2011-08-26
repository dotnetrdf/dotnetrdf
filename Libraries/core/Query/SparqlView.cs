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
        private IAsyncResult _asyncResult;
        private bool _requiresInvalidate = false;
        private RdfQueryException _lastError;

        /// <summary>
        /// Creates a new SPARQL View
        /// </summary>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <param name="store">Triple Store to query</param>
        public BaseSparqlView(String sparqlQuery, ITripleStore store)
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
        public BaseSparqlView(SparqlParameterizedString sparqlQuery, ITripleStore store)
            : this(sparqlQuery.ToString(), store) { }

        /// <summary>
        /// Creates a new SPARQL View
        /// </summary>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <param name="store">Triple Store to query</param>
        public BaseSparqlView(SparqlQuery sparqlQuery, ITripleStore store)
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
            this._store.GraphAdded += this.OnGraphRemoved;

            //Fill the Graph with the results of the Query
            this.UpdateViewInternal();
        }

        /// <summary>
        /// Invalidates the View causing it to be updated
        /// </summary>
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

        /// <summary>
        /// Callback for when asychronous invalidation completes
        /// </summary>
        /// <param name="result">Async call results</param>
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

        /// <summary>
        /// Forces the view to be updated
        /// </summary>
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
