using System;
using VDS.RDF.Query.Results;

namespace VDS.RDF.Query
{
    /// <summary>
    /// Callback for asynchronous query execution
    /// </summary>
    /// <param name="result">Query Result</param>
    /// <param name="state">State</param>
    public delegate void QueryCallback(IQueryResult result, Object state);
}
