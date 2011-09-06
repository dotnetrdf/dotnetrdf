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
using VDS.RDF.Query;

namespace VDS.RDF.Parsing.Handlers
{
    /// <summary>
    /// A SPARQL Results Handler which loads Results into a <see cref="SparqlResultSet">SparqlResultSet</see>
    /// </summary>
    public class ResultSetHandler
        : BaseResultsHandler
    {
        private SparqlResultSet _results;

        /// <summary>
        /// Creates a new Result Set Handler
        /// </summary>
        /// <param name="results">Result Set</param>
        public ResultSetHandler(SparqlResultSet results)
        {
            if (results == null) throw new ArgumentNullException("results");
            this._results = results;
        }

        /// <summary>
        /// Starts Results Handling
        /// </summary>
        protected override void StartResultsInternal()
        {
            //Ensure Empty Result Set
            if (!this._results.IsEmpty || this._results.ResultsType != SparqlResultsType.Unknown)
            {
                throw new RdfParseException("Cannot start Results Handling into a non-empty Result Set");
            }
        }

        /// <summary>
        /// Handles a Boolean Result by setting the <see cref="SparqlResultSet.Result">Result</see> property of the Result Set
        /// </summary>
        /// <param name="result">Result</param>
        protected override void HandleBooleanResultInternal(bool result)
        {
            this._results.SetResult(result);
        }

        /// <summary>
        /// Handles a Variable Declaration by adding the Variable to the Result Set
        /// </summary>
        /// <param name="var">Variable Name</param>
        /// <returns></returns>
        protected override bool HandleVariableInternal(string var)
        {
            this._results.AddVariable(var);
            return true;
        }

        /// <summary>
        /// Handles a Result by adding it to the Result Set
        /// </summary>
        /// <param name="result">Result</param>
        /// <returns></returns>
        protected override bool HandleResultInternal(SparqlResult result)
        {
            this._results.AddResult(result);
            return true;
        }
    }

    /// <summary>
    /// A SPARQL Results Handler which allows you to load multiple Result Sets into a single <see cref="SparqlResultSet">SparqlResultSet</see> which the standard <see cref="ResultSetHandler">ResultSetHandler</see> does not permit
    /// </summary>
    public class MergingResultSetHandler
        : ResultSetHandler
    {
        /// <summary>
        /// Creates a new Merging Result Set Handler
        /// </summary>
        /// <param name="results">Result Set</param>
        public MergingResultSetHandler(SparqlResultSet results)
            : base(results) { }

        /// <summary>
        /// Overrides the base classes logic to avoid the empty check on the Result Set thus allowing multiple result sets to be merged
        /// </summary>
        protected override void StartResultsInternal()
        {
            //Don't do an empty check
        }
    }
}
