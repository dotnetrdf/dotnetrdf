using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query;
using VDS.RDF.Query.Inference.Pellet.Services;

namespace VDS.RDF
{
    //Callback Delegates for Async operations

    public delegate void SparqlResultsCallback(SparqlResultSet results, Object state);

    public delegate void GraphCallback(IGraph g, Object state);

    public delegate void TripleStoreCallback(ITripleStore store, Object state);

    public delegate void RdfHandlerCallback(IRdfHandler handler, Object state);

    public delegate void SparqlResultsHandlerCallback(ISparqlResultsHandler handler, Object state);

    public delegate void QueryCallback(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, Object state);

    public delegate void UpdateCallback(Object state);

    public delegate void NamespaceCallback(INamespaceMapper nsmap, Object state);

    public delegate void NodeListCallback(List<INode> nodes, Object state);

}

namespace VDS.RDF.Query.Inference.Pellet
{
    public delegate void PelletServerReadyCallback(PelletServer server, Object state);

    public delegate void PelletConsistencyCallback(bool isConsistent, Object state);

    public delegate void PelletSearchServiceCallback(List<SearchServiceResult> results, Object state);

    public delegate void PelletClusterServiceCallback(List<List<INode>> clusters, Object state);

    public delegate void PelletSimilarityServiceCallback(List<KeyValuePair<INode, double>> results, Object state);

}
