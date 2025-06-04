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
/// Represents a SPARQL View over an arbitrary native Triple Store.
/// </summary>
public class NativeSparqlView
    : BaseSparqlView
{
    /// <summary>
    /// Creates a new SPARQL View.
    /// </summary>
    /// <param name="sparqlQuery">SPARQL Query.</param>
    /// <param name="store">Triple Store to query.</param>
    public NativeSparqlView(string sparqlQuery, INativelyQueryableStore store)
        : base(sparqlQuery, store) { }

    /// <summary>
    /// Creates a new SPARQL View.
    /// </summary>
    /// <param name="sparqlQuery">SPARQL Query.</param>
    /// <param name="store">Triple Store to query.</param>
    public NativeSparqlView(SparqlParameterizedString sparqlQuery, INativelyQueryableStore store)
        : this(sparqlQuery.ToString(), store) { }

    /// <summary>
    /// Creates a new SPARQL View.
    /// </summary>
    /// <param name="sparqlQuery">SPARQL Query.</param>
    /// <param name="store">Triple Store to query.</param>
    public NativeSparqlView(SparqlQuery sparqlQuery, INativelyQueryableStore store)
        : base(sparqlQuery, store) { }

    /// <summary>
    /// Updates the view by making the query over the Native Store (i.e. the query is handled by the stores SPARQL implementation).
    /// </summary>
    protected override void UpdateViewInternal()
    {
        try
        {
            var results = ((INativelyQueryableStore)_store).ExecuteQuery(_q.ToString());
            if (results is IGraph g)
            {
                DetachEventHandlers(_triples);
                _triples = g.Triples.ToTripleCollection();
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