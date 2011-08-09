using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query;

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
}
