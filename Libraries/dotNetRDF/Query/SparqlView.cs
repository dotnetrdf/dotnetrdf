/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
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
        private bool _requiresInvalidate = false;
        private RdfQueryException _lastError;
        private Object _lock = new Object();

        /// <summary>
        /// Creates a new SPARQL View
        /// </summary>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <param name="store">Triple Store to query</param>
        protected BaseSparqlView(String sparqlQuery, ITripleStore store)
        {
            SparqlQueryParser parser = new SparqlQueryParser();
            _q = parser.ParseFromString(sparqlQuery);
            _store = store;

            _async = new UpdateViewDelegate(UpdateViewInternal);
            Initialise();
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
            _q = sparqlQuery;
            _store = store;

            _async = new UpdateViewDelegate(UpdateViewInternal);
            Initialise();
        }

        /// <summary>
        /// Initialises the SPARQL View
        /// </summary>
        private void Initialise()
        {
            if (_q.QueryType == SparqlQueryType.Ask)
            {
                throw new RdfQueryException("Cannot create a SPARQL View based on an ASK Query");
            }

            // Does this Query operate over specific Graphs?
            if (_q.DefaultGraphs.Any() || _q.NamedGraphs.Any())
            {
                _graphs = new HashSet<string>();
                foreach (Uri u in _q.DefaultGraphs)
                {
                    _graphs.Add(u.ToSafeString());
                }
                foreach (Uri u in _q.NamedGraphs)
                {
                    _graphs.Add(u.ToSafeString());
                }
            }

            // Attach a Handler to the Store's Graph Added, Removed and Changed events
            _store.GraphChanged += OnGraphChanged;
            _store.GraphAdded += OnGraphAdded;
            _store.GraphRemoved += OnGraphRemoved;
            _store.GraphMerged += OnGraphMerged;

            // Fill the Graph with the results of the Query
            UpdateViewInternal();
        }

        /// <summary>
        /// Invalidates the View causing it to be updated
        /// </summary>
        private void InvalidateView()
        {
            _async.BeginInvoke(new AsyncCallback(InvalidateViewCompleted), null);
        }

        /// <summary>
        /// Callback for when asychronous invalidation completes
        /// </summary>
        /// <param name="result">Async call results</param>
        private void InvalidateViewCompleted(IAsyncResult result)
        {
            try
            {
                _async.EndInvoke(result);

                // If we've been further invalidated then need to re-query
                if (_requiresInvalidate)
                {
                    InvalidateView();
                    _requiresInvalidate = false;
                }
            }
            catch (Exception ex)
            {
                // Ignore errors
                LastError = new RdfQueryException("Unable to complete update of SPARQL View, see inner exception for details", ex);
            }
        }

        private delegate void UpdateViewDelegate();

        /// <summary>
        /// Forces the view to be updated
        /// </summary>
        public void UpdateView()
        {
            lock (_lock)
            {
                UpdateViewInternal();
                if (LastError != null) throw LastError;
            }
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
                return _lastError;
            }
            protected set
            {
                _lastError = value;
            }
        }

        private void OnGraphChanged(Object sender, TripleStoreEventArgs args)
        {
            if (args.GraphEvent != null)
            {
                IGraph g = args.GraphEvent.Graph;
                if (g != null)
                {
                    // Ignore Changes to self
                    if (ReferenceEquals(g, this)) return;

                    if (_graphs == null)
                    {
                        // No specific Graphs so any change causes an invalidation
                        InvalidateView();
                    }
                    else
                    {
                        // If specific Graphs only invalidate when those Graphs change
                        if (_graphs.Contains(g.BaseUri.ToSafeString()))
                        {
                            InvalidateView();
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
                    // Ignore merges to self
                    if (ReferenceEquals(g, this)) return;

                    if (_graphs == null)
                    {
                        // No specific Graphs so any change causes an invalidation
                        InvalidateView();
                    }
                    else
                    {
                        // If specific Graphs only invalidate when those Graphs change
                        if (_graphs.Contains(g.BaseUri.ToSafeString()))
                        {
                            InvalidateView();
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
                    // Ignore Changes to self
                    if (ReferenceEquals(g, this)) return;

                    if (_graphs == null)
                    {
                        // No specific Graphs so any change causes an invalidation
                        InvalidateView();
                    }
                    else
                    {
                        // If specific Graphs only invalidate when those Graphs change
                        if (_graphs.Contains(g.BaseUri.ToSafeString()))
                        {
                            InvalidateView();
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
                    // Ignore Changes to self
                    if (ReferenceEquals(g, this)) return;

                    if (_graphs == null)
                    {
                        // No specific Graphs so any change causes an invalidation
                        InvalidateView();
                    }
                    else
                    {
                        // If specific Graphs only invalidate when those Graphs change
                        if (_graphs.Contains(g.BaseUri.ToSafeString()))
                        {
                            InvalidateView();
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
                var processor = new LeviathanQueryProcessor((IInMemoryQueryableStore)_store);
                var results = processor.ProcessQuery(_q);
                if (results is IGraph g)
                {
                    // Note that we replace the existing triple collection with an entirely new one as otherwise nasty race conditions can happen
                    // This does mean that while the update is occurring the user may be viewing a stale graph
                    DetachEventHandlers(_triples);
                    TreeIndexedTripleCollection triples = new TreeIndexedTripleCollection();
                    foreach (Triple t in g.Triples)
                    {
                        triples.Add(t.CopyTriple(this));
                    }
                    _triples = triples;
                    AttachEventHandlers(_triples);
                }
                else
                {
                    // Note that we replace the existing triple collection with an entirely new one as otherwise nasty race conditions can happen
                    // This does mean that while the update is occurring the user may be viewing a stale graph
                    DetachEventHandlers(_triples);
                    _triples = ((SparqlResultSet)results).ToTripleCollection(this);
                    AttachEventHandlers(_triples);
                }
                LastError = null;
                RaiseGraphChanged();
            }
            catch (RdfQueryException queryEx)
            {
                LastError = new RdfQueryException("Unable to Update a SPARQL View as an error occurred in processing the Query - see Inner Exception for details", queryEx);
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
                Object results = ((INativelyQueryableStore)_store).ExecuteQuery(_q.ToString());
                if (results is IGraph)
                {
                    DetachEventHandlers(_triples);
                    IGraph g = (IGraph)results;
                    foreach (Triple t in g.Triples)
                    {
                        _triples.Add(t.CopyTriple(this));
                    }
                    AttachEventHandlers(_triples);
                }
                else
                {
                    DetachEventHandlers(_triples);
                    _triples = ((SparqlResultSet)results).ToTripleCollection(this);
                    AttachEventHandlers(_triples);
                }
                LastError = null;
                RaiseGraphChanged();
            }
            catch (RdfQueryException queryEx)
            {
                LastError = new RdfQueryException("Unable to Update a SPARQL View as an error occurred in processing the Query - see Inner Exception for details", queryEx);
            }
        }
    }
}
