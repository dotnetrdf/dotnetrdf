using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Parsing.Handlers
{
    public class ResultCountHandler : BaseResultsHandler
    {
        private int _counter = 0;

        protected override void StartResultsInternal()
        {
            this._counter = 0;
        }

        protected override void HandleBooleanResultInternal(bool result)
        {
            //Nothing to do
        }

        protected override bool HandleVariableInternal(string var)
        {
            return true;
        }

        protected override bool HandleResultInternal(VDS.RDF.Query.SparqlResult result)
        {
            this._counter++;
            return true;
        }

        public int Count
        {
            get
            {
                return this._counter;
            }
        }
    }
}
