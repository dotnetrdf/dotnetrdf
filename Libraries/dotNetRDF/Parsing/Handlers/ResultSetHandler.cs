/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
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
            _results = results;
        }

        /// <summary>
        /// Starts Results Handling
        /// </summary>
        protected override void StartResultsInternal()
        {
            // Ensure Empty Result Set
            if (!_results.IsEmpty || _results.ResultsType != SparqlResultsType.Unknown)
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
            _results.SetResult(result);
        }

        /// <summary>
        /// Handles a Variable Declaration by adding the Variable to the Result Set
        /// </summary>
        /// <param name="var">Variable Name</param>
        /// <returns></returns>
        protected override bool HandleVariableInternal(string var)
        {
            _results.AddVariable(var);
            return true;
        }

        /// <summary>
        /// Handles a Result by adding it to the Result Set
        /// </summary>
        /// <param name="result">Result</param>
        /// <returns></returns>
        protected override bool HandleResultInternal(SparqlResult result)
        {
            _results.AddResult(result);
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
            // Don't do an empty check
        }
    }
}
