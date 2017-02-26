/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

#if !(SILVERLIGHT||NETCORE)

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