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

using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Query;

namespace VDS.RDF.Storage;

/// <summary>
/// Provides a Read-Only wrapper that can be placed around another <see cref="IQueryableStorage">IQueryableStorage</see> instance.
/// </summary>
/// <remarks>
/// <para>
/// This is useful if you want to allow some code read-only access to a mutable store and ensure that it cannot modify the store via the manager instance.
/// </para>
/// </remarks>
public class QueryableReadOnlyConnector
    : ReadOnlyConnector, IQueryableStorage
{
    private readonly IQueryableStorage _queryManager;

    /// <summary>
    /// Creates a new Queryable Read-Only connection which is a read-only wrapper around another store.
    /// </summary>
    /// <param name="manager">Manager for the Store you want to wrap as read-only.</param>
    public QueryableReadOnlyConnector(IQueryableStorage manager)
        : base(manager)
    {
        _queryManager = manager;
    }

    /// <summary>
    /// Executes a SPARQL Query on the underlying Store.
    /// </summary>
    /// <param name="sparqlQuery">SPARQL Query.</param>
    /// <returns></returns>
    public object Query(string sparqlQuery)
    {
        return _queryManager.Query(sparqlQuery);
    }

    /// <summary>
    /// Executes a SPARQL Query on the underlying Store processing the results with an appropriate handler from those provided.
    /// </summary>
    /// <param name="rdfHandler">RDF Handler.</param>
    /// <param name="resultsHandler">Results Handler.</param>
    /// <param name="sparqlQuery">SPARQL Query.</param>
    /// <returns></returns>
    public void Query(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, string sparqlQuery)
    {
        _queryManager.Query(rdfHandler, resultsHandler, sparqlQuery);
    }

    /// <summary>
    /// Lists the Graphs in the Store.
    /// </summary>
    /// <returns></returns>
    [Obsolete("Replaced by ListGraphNames")]
    public override IEnumerable<Uri> ListGraphs()
    {
        if (base.ListGraphsSupported)
        {
            // Use the base classes ListGraphs method if it provides one
            return base.ListGraphs();
        }

        try
        {
            if (Query("SELECT DISTINCT ?g WHERE { GRAPH ?g { ?s ?p ?o } }") is SparqlResultSet results)
            {
                var graphs = new List<Uri>();
                foreach (SparqlResult r in results)
                {
                    if (r.HasValue("g"))
                    {
                        INode temp = r["g"];
                        if (temp.NodeType == NodeType.Uri)
                        {
                            graphs.Add(((IUriNode)temp).Uri);
                        }
                    }
                }

                return graphs;
            }

            return Enumerable.Empty<Uri>();
        }
        catch (Exception ex)
        {
            throw new RdfStorageException("Underlying Store returned an error while trying to List Graphs", ex);
        }
    }

    /// <summary>
    /// Gets an enumeration of the names of the graphs in the store.
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// <para>
    /// Implementations should implement this method only if they need to provide a custom way of listing Graphs.  If the Store for which you are providing a manager can efficiently return the Graphs using a SELECT DISTINCT ?g WHERE { GRAPH ?g { ?s ?p ?o } } query then there should be no need to implement this function.
    /// </para>
    /// </remarks>
    public override IEnumerable<string> ListGraphNames()
    {
        if (base.ListGraphsSupported)
        {
            // Use the base classes ListGraphs method if it provides one
            return base.ListGraphNames();
        }
        else
        {
            try
            {
                if (Query("SELECT DISTINCT ?g WHERE { GRAPH ?g { ?s ?p ?o } }") is SparqlResultSet results)
                {
                    var graphs = new List<string>();
                    foreach (SparqlResult r in results)
                    {
                        if (r.HasValue("g"))
                        {
                            INode temp = r["g"];
                            if (temp.NodeType == NodeType.Uri)
                            {
                                graphs.Add(((IUriNode)temp).Uri.AbsoluteUri);
                            } else if (temp.NodeType == NodeType.Blank)
                            {
                                graphs.Add("_:" + ((IBlankNode)temp).InternalID);
                            }
                        }
                    }
                    return graphs;
                }

                return Enumerable.Empty<string>();
            }
            catch (Exception ex)
            {
                throw new RdfStorageException("Underlying Store returned an error while trying to List Graphs", ex);
            }
        }
    }

    /// <summary>
    /// Returns that listing Graphs is supported.
    /// </summary>
    public override bool ListGraphsSupported
    {
        get
        {
            return true;
        }
    }
}