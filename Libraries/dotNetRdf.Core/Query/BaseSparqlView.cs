/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2025 dotNetRDF Project (http://dotnetrdf.org/)
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

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VDS.RDF.Parsing;

namespace VDS.RDF.Query;

/// <summary>
/// Abstract Base class for SPARQL Views which are Graphs which are generated from SPARQL Queries and get automatically updated when the Store they are attached to changes.
/// </summary>
/// <remarks>
/// <para>
/// CONSTRUCT, DESCRIBE or SELECT queries can be used to generate a Graph.  If you use a SELECT query the returned variables must contain ?s, ?p and ?o in order to generate a view correctly.
/// </para>
/// </remarks>
public abstract class BaseSparqlView 
    : Graph
{
    /// <summary>
    /// SPARQL Query.
    /// </summary>
    protected SparqlQuery _q;
    /// <summary>
    /// Graphs that are mentioned in the Query.
    /// </summary>
    protected HashSet<string> _graphs;
    /// <summary>
    /// Triple Store the query operates over.
    /// </summary>
    protected ITripleStore _store;

    private bool _requiresInvalidate ;
    private readonly object _lock = new object();

    /// <summary>
    /// Creates a new SPARQL View.
    /// </summary>
    /// <param name="sparqlQuery">SPARQL Query.</param>
    /// <param name="store">Triple Store to query.</param>
    /// <param name="name">The graph name to assign to the view.</param>
    protected BaseSparqlView(string sparqlQuery, ITripleStore store, IRefNode name = null)
        : base(name)
    {
        var parser = new SparqlQueryParser();
        _q = parser.ParseFromString(sparqlQuery);
        _store = store;
        Initialise();
    }

    /// <summary>
    /// Creates a new SPARQL View.
    /// </summary>
    /// <param name="sparqlQuery">SPARQL Query.</param>
    /// <param name="store">Triple Store to query.</param>
    /// <param name="name">The graph name to assign to the view.</param>
    protected BaseSparqlView(SparqlParameterizedString sparqlQuery, ITripleStore store, IRefNode name = null)
        : this(sparqlQuery.ToString(), store, name) { }

    /// <summary>
    /// Creates a new SPARQL View.
    /// </summary>
    /// <param name="sparqlQuery">SPARQL Query.</param>
    /// <param name="store">Triple Store to query.</param>
    /// <param name="name">The graph name to assign to the view.</param>
    protected BaseSparqlView(SparqlQuery sparqlQuery, ITripleStore store, IRefNode name = null)
        : base(name)
    {
        _q = sparqlQuery;
        _store = store;
        Initialise();
    }

    /// <summary>
    /// Initializes the SPARQL View.
    /// </summary>
    private void Initialise()
    {
        if (_q.QueryType == SparqlQueryType.Ask)
        {
            throw new RdfQueryException("Cannot create a SPARQL View based on an ASK Query");
        }

        // Does this Query operate over specific Graphs?
        if (_q.DefaultGraphNames.Any() || _q.NamedGraphNames.Any())
        {
            _graphs = new HashSet<string>();
            foreach (IRefNode graphName in _q.DefaultGraphNames)
            {
                _graphs.Add(graphName.ToSafeString());
            }
            foreach (IRefNode graphName in _q.NamedGraphNames)
            {
                _graphs.Add(graphName.ToSafeString());
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
    /// Invalidates the View causing it to be updated.
    /// </summary>
    private void InvalidateView()
    {
        Task.Factory.StartNew(UpdateViewInternal).ContinueWith(antecedent =>
        {
            if (antecedent.IsFaulted)
            {
                LastError = new RdfQueryException(
                    "Unable to complete update of SPARQL View, see inner exception for details",
                    antecedent.Exception);
            }
            else
            {
                if (_requiresInvalidate)
                {
                    InvalidateView();
                    _requiresInvalidate = false;
                }
            }
        });
    }

    /// <summary>
    /// Forces the view to be updated.
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
    /// Abstract method that derived classes should implement to update the view.
    /// </summary>
    protected abstract void UpdateViewInternal();

    /// <summary>
    /// Gets the error that occurred during the last update (if any).
    /// </summary>
    public RdfQueryException LastError { get; protected set; }

    private void OnGraphChanged(object sender, TripleStoreEventArgs args)
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

    private void OnGraphMerged(object sender, TripleStoreEventArgs args)
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

    private void OnGraphAdded(object sender, TripleStoreEventArgs args)
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

    private void OnGraphRemoved(object sender, TripleStoreEventArgs args)
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