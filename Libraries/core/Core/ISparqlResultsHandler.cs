using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query;

namespace VDS.RDF
{
    public interface ISparqlResultsHandler : INodeFactory
    {
        void StartResults();

        void EndResults(bool ok);

        void HandleBooleanResult(bool result);

        bool HandleVariable(String var);

        bool HandleResult(SparqlResult result);
    }
}
