using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query;

namespace VDS.RDF.Parsing.Handlers
{
    public class ResultSetHandler : BaseResultsHandler
    {
        private SparqlResultSet _results;

        public ResultSetHandler(SparqlResultSet results)
        {
            if (results == null) throw new ArgumentNullException("results");
            this._results = results;
        }

        protected override void StartResultsInternal()
        {
            //Ensure Empty Result Set
            if (!this._results.IsEmpty || this._results.ResultsType != SparqlResultsType.Unknown)
            {
                throw new RdfParseException("Cannot start Results Handling into a non-empty Result Set");
            }
        }

        protected override void HandleBooleanResultInternal(bool result)
        {
            this._results.SetResult(result);
        }

        protected override bool HandleVariableInternal(string var)
        {
            this._results.AddVariable(var);
            return true;
        }

        protected override bool HandleResultInternal(SparqlResult result)
        {
            this._results.AddResult(result);
            return true;
        }
    }
}
