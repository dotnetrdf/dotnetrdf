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
            _counter = 0;
        }

        /// <summary>
        /// Handles a Boolean Result
        /// </summary>
        /// <param name="result">Result</param>
        protected override void HandleBooleanResultInternal(bool result)
        {
            _counter = result ? 1 : 0;
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
            _counter++;
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
                return _counter;
            }
        }
    }
}
