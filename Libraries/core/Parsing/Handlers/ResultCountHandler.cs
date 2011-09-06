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

using VDS.RDF.Query;

namespace VDS.RDF.Parsing.Handlers
{
    /// <summary>
    /// A SPARQL Results Handler which just counts Results
    /// </summary>
    /// <remarks>
    /// <strong>Note: </strong> For a Boolean Result Set the counter will either be 1 for true or 0 for false
    /// </remarks>
    public class ResultCountHandler
        : BaseResultsHandler
    {
        private int _counter = 0;

        /// <summary>
        /// Creates a new Result Count Handler
        /// </summary>
        public ResultCountHandler()
            : base(new MockNodeFactory()) { }

        /// <summary>
        /// Starts Results Handling and resets the counter to zero
        /// </summary>
        protected override void StartResultsInternal()
        {
            this._counter = 0;
        }

        /// <summary>
        /// Handles a Boolean Result
        /// </summary>
        /// <param name="result">Result</param>
        protected override void HandleBooleanResultInternal(bool result)
        {
            this._counter = result ? 1 : 0;
        }

        /// <summary>
        /// Handles a Variable Declaration
        /// </summary>
        /// <param name="var">Variable Name</param>
        /// <returns></returns>
        protected override bool HandleVariableInternal(string var)
        {
            return true;
        }

        /// <summary>
        /// Handles a SPARQL Result by incrementing the counter
        /// </summary>
        /// <param name="result">Result</param>
        /// <returns></returns>
        protected override bool HandleResultInternal(SparqlResult result)
        {
            this._counter++;
            return true;
        }

        /// <summary>
        /// Gets the Count of Results 
        /// </summary>
        /// <remarks>
        /// For Boolean Results counter will be either 1 or 0 depending on whether the result was True/False
        /// </remarks>
        public int Count
        {
            get
            {
                return this._counter;
            }
        }
    }
}
