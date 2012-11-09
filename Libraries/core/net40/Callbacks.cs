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
using VDS.RDF.Query;
using VDS.RDF.Query.Inference.Pellet.Services;

namespace VDS.RDF
{
    //Callback Delegates for Async operations

    /// <summary>
    /// Callback for methods that return a <see cref="SparqlResultSet">SparqlResultSet</see> asynchronously
    /// </summary>
    /// <param name="results">SPARQL Results</param>
    /// <param name="state">State</param>
    public delegate void SparqlResultsCallback(SparqlResultSet results, Object state);

    /// <summary>
    /// Callback for methods that return a <see cref="IGraph">IGraph</see> asynchronously
    /// </summary>
    /// <param name="g">Graph</param>
    /// <param name="state">State</param>
    public delegate void GraphCallback(IGraph g, Object state);

    /// <summary>
    /// Callback for methods that return a <see cref="ITripleStore">ITripleStore</see> asynchronously
    /// </summary>
    /// <param name="store">Triple Store</param>
    /// <param name="state">State</param>
    public delegate void TripleStoreCallback(ITripleStore store, Object state);

    /// <summary>
    /// Callbacks for methods that process the resulting triples with an RDF Handler asynchronously
    /// </summary>
    /// <param name="handler">RDF Handler</param>
    /// <param name="state">State</param>
    public delegate void RdfHandlerCallback(IRdfHandler handler, Object state);

    /// <summary>
    /// Callbacks for methods that process the results with an SPARQL Results Handler asynchronously
    /// </summary>
    /// <param name="handler">SPARQL Results Handler</param>
    /// <param name="state">State</param>
    public delegate void SparqlResultsHandlerCallback(ISparqlResultsHandler handler, Object state);

    /// <summary>
    /// Callbacks for methods that may process the results with either an RDF or a SPARQL Results Handler
    /// </summary>
    /// <param name="rdfHandler">RDF Handler</param>
    /// <param name="resultsHandler">SPARQL Results Handler</param>
    /// <param name="state">State</param>
    public delegate void QueryCallback(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, Object state);

    /// <summary>
    /// Callbacks for methods that perform SPARQL Updates
    /// </summary>
    /// <param name="state">State</param>
    public delegate void UpdateCallback(Object state);

    /// <summary>
    /// Callback for methods that return a Namespace Map
    /// </summary>
    /// <param name="nsmap">Namespace Map</param>
    /// <param name="state">State</param>
    public delegate void NamespaceCallback(INamespaceMapper nsmap, Object state);

    /// <summary>
    /// Callbacks for methods that return a list of nodes
    /// </summary>
    /// <param name="nodes">Node List</param>
    /// <param name="state">State</param>
    public delegate void NodeListCallback(List<INode> nodes, Object state);

}

namespace VDS.RDF.Query.Inference.Pellet
{
    /// <summary>
    /// Callback that occurs when the connection to a Pellet Server instance is ready for use
    /// </summary>
    /// <param name="server">Pellet Server</param>
    /// <param name="state">State</param>
    public delegate void PelletServerReadyCallback(PelletServer server, Object state);

    /// <summary>
    /// Callback for Pellet Constistency Service
    /// </summary>
    /// <param name="isConsistent">Whether the Knowledge Base is consistent</param>
    /// <param name="state">State</param>
    public delegate void PelletConsistencyCallback(bool isConsistent, Object state);

    /// <summary>
    /// Callback for Pellet Search Service
    /// </summary>
    /// <param name="results">Pellet Search Results</param>
    /// <param name="state">State</param>
    public delegate void PelletSearchServiceCallback(List<SearchServiceResult> results, Object state);

    /// <summary>
    /// Callback for Pellet Cluster Service
    /// </summary>
    /// <param name="clusters">Clusters</param>
    /// <param name="state">State</param>
    public delegate void PelletClusterServiceCallback(List<List<INode>> clusters, Object state);

    /// <summary>
    /// Callback for Pellet Similarity Service
    /// </summary>
    /// <param name="results">Similarity Results</param>
    /// <param name="state">State</param>
    public delegate void PelletSimilarityServiceCallback(List<KeyValuePair<INode, double>> results, Object state);

}
