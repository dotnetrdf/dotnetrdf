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

namespace VDS.RDF.Query;

/// <summary>
/// Represents a SPARQL View over an in-memory store.
/// </summary>
public class SparqlView
    : BaseSparqlView
{
    /// <summary>
    /// Specifies whether this view should fully index any graph result sets.
    /// </summary>
    public bool FullTripleIndexing { get; set; } = true;

    /// <summary>
    /// Creates a new SPARQL View.
    /// </summary>
    /// <param name="sparqlQuery">SPARQL Query.</param>
    /// <param name="store">Triple Store to query.</param>
    /// <param name="name">The graph name to assign to the view.</param>
    public SparqlView(string sparqlQuery, IInMemoryQueryableStore store, IRefNode name = null)
        : base(sparqlQuery, store, name) { }

    /// <summary>
    /// Creates a new SPARQL View.
    /// </summary>
    /// <param name="sparqlQuery">SPARQL Query.</param>
    /// <param name="store">Triple Store to query.</param>
    /// <param name="name">The graph name to assign to the view.</param>
    public SparqlView(SparqlParameterizedString sparqlQuery, IInMemoryQueryableStore store, IRefNode name = null)
        : this(sparqlQuery.ToString(), store, name) { }

    /// <summary>
    /// Creates a new SPARQL View.
    /// </summary>
    /// <param name="sparqlQuery">SPARQL Query.</param>
    /// <param name="store">Triple Store to query.</param>
    /// <param name="name">The graph name to assign to the view.</param>
    public SparqlView(SparqlQuery sparqlQuery, IInMemoryQueryableStore store, IRefNode name = null)
        : base(sparqlQuery, store, name) { }


    /// <summary>
    /// Updates the view by making the SPARQL Query in-memory over the relevant Triple Store.
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
                BaseTripleCollection triples = g.Triples.ToTripleCollection(indexed: FullTripleIndexing);
                _triples = triples;
                AttachEventHandlers(_triples);
            }
            else
            {
                // Note that we replace the existing triple collection with an entirely new one as otherwise nasty race conditions can happen
                // This does mean that while the update is occurring the user may be viewing a stale graph
                DetachEventHandlers(_triples);
                _triples = ((SparqlResultSet)results).ToTripleCollection(this, "s", "p", "o", FullTripleIndexing);
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
