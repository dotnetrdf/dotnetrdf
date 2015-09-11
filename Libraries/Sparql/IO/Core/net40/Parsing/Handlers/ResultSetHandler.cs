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

using System;
using VDS.RDF.Query.Results;

namespace VDS.RDF.Parsing.Handlers
{
    /// <summary>
    /// A SPARQL Results Handler which loads Results into a <see cref="IMutableTabularResults" />
    /// </summary>
    public class ResultSetHandler
        : BaseResultsHandler
    {
        private readonly IMutableTabularResults _results;

        /// <summary>
        /// Creates a new Result Set Handler
        /// </summary>
        /// <param name="results">Result Set</param>
        public ResultSetHandler(IMutableTabularResults results)
        {
            if (results == null) throw new ArgumentNullException("results");
            this._results = results;
        }

        /// <summary>
        /// Starts Results Handling
        /// </summary>
        protected override void StartResultsInternal()
        {
            
        }

        /// <summary>
        /// Handles a Boolean Result
        /// </summary>
        /// <param name="result">Result</param>
        protected override void HandleBooleanResultInternal(bool result)
        {
            throw new RdfParseException("Cannot handle boolean results with this handler");
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
        protected override bool HandleResultInternal(IResultRow result)
        {
            this._results.Add(result as IMutableResultRow ?? new MutableResultRow(result));
            return true;
        }
    }
}
