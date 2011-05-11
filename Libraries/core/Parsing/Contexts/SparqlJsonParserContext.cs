/*

Copyright Robert Vesse 2009-11
rvesse@vdesign-studios.com

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

using System;
using Newtonsoft.Json;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;

namespace VDS.RDF.Parsing.Contexts
{
    /// <summary>
    /// SPARQL JSON Parser Context
    /// </summary>
    public class SparqlJsonParserContext : BaseResultsParserContext
    {
        private JsonTextReader _reader;

        /// <summary>
        /// Creates a new Parser Context
        /// </summary>
        /// <param name="reader">JSON Text Reader</param>
        /// <param name="handler">Results Handler</param>
        public SparqlJsonParserContext(JsonTextReader reader, ISparqlResultsHandler handler)
            : base(handler)
        {
            if (reader == null) throw new ArgumentNullException("reader");
            this._reader = reader;
        }

        /// <summary>
        /// Creates a new Parser Context
        /// </summary>
        /// <param name="reader">JSON Text Reader</param>
        /// <param name="results">SPARQL Result Set</param>
        public SparqlJsonParserContext(JsonTextReader reader, SparqlResultSet results)
            : this(reader, new ResultSetHandler(results)) { }

        /// <summary>
        /// Gets the JSON Text Reader
        /// </summary>
        public JsonTextReader Input
        {
            get
            {
                return this._reader;
            }
        }
    }
}
