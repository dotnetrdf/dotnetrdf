#if !SILVERLIGHT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace VDS.RDF.Writing.Serialization
{
    class ResultSetDeserializationInfo
    {
        private SparqlResultsType _type = SparqlResultsType.Unknown;
        private bool _r;
        private List<String> _vars;
        private List<SparqlResult> _results;

        public ResultSetDeserializationInfo(SerializationInfo info, StreamingContext context)
        {
            this._type = (SparqlResultsType)info.GetValue("type", typeof(SparqlResultsType));
            switch (this._type)
            {
                case SparqlResultsType.Boolean:
                    this._r = info.GetBoolean("result");
                    break;
                case SparqlResultsType.VariableBindings:
                    this._vars = (List<String>)info.GetValue("variables", typeof(List<String>));
                    this._results = (List<SparqlResult>)info.GetValue("results", typeof(List<SparqlResult>));
                    break;
                default:
                    throw new RdfParseException("The type property of a serialized SparqlResultSet did not contain a valid value");
            }
        }

        public void Apply(SparqlResultSet results)
        {
            switch (this._type)
            {
                case SparqlResultsType.Boolean:
                    results.SetResult(this._r);
                    break;
                case SparqlResultsType.VariableBindings:
                    foreach (String var in this._vars)
                    {
                        results.AddVariable(var);
                    }
                    foreach (SparqlResult res in this._results)
                    {
                        results.AddResult(res);
                    }
                    break;
                default:
                    throw new RdfParseException("The type property of a serialized SparqlResultSet did not contain a valid value");
            }
        }
    }
}


#endif