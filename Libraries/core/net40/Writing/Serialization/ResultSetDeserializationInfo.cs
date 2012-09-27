/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

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