using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query;

namespace VDS.RDF
{
    /// <summary>
    /// Callback for methods that return a <see cref="SparqlResultSet">SparqlResultSet</see> asynchronously
    /// </summary>
    /// <param name="results">SPARQL Results</param>
    /// <param name="state">State</param>
    public delegate void SparqlResultsCallback(SparqlResultSet results, Object state);

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

