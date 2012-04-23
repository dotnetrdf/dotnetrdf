using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Storage
{
    public delegate void LoadGraphCallback(Object sender, IGraph g, Exception e, Object state);

    public delegate void LoadHandlerCallback(Object sender, IRdfHandler handler, Exception e, Object state);

    public delegate void SaveGraphCallback(Object sender, IGraph g, Exception e, Object state);

    public delegate void UpdateGraphCallback(Object sender, Uri graphUri, Exception e, Object state);

    public delegate void DeleteGraphCallback(Object sender, Uri graphUri, Exception e, Object state);

    public delegate void ListGraphsCallback(Object sender, IEnumerable<Uri> uris, Exception e, Object state);

    public delegate void SparqlQueryCallback(Object sender, String query, Object results, Exception e, Object state);

    public delegate void SparqlQueryHandlerCallback(Object sender, String query, IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, Exception e, Object state);

    public delegate void SparqlUpdateCallback(Object sender, String updates, Exception e, Object state);

    public delegate void TransactionCallback(Object sender, Exception e, Object state);
}
