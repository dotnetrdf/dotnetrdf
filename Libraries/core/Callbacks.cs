/*

Copyright Robert Vesse 2009-11
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
