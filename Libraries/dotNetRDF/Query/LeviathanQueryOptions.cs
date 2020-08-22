/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2020 dotNetRDF Project (http://dotnetrdf.org/)
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
using System.Globalization;

namespace VDS.RDF.Query
{
    /// <summary>
    /// A class encapsulating run-time options that can be configured for a <see cref="LeviathanQueryProcessor"/>.
    /// </summary>
    public class LeviathanQueryOptions
    {
        private CultureInfo _culture = CultureInfo.InvariantCulture;
        private long _queryExecutionTimeout = 180000;


        /// <summary>
        /// Get or set the culture to use for string comparisons in the query processor.
        /// </summary>
        /// <remarks>
        /// By default the value of this option is <see cref="CultureInfo.InvariantCulture"/> which when combined with the default value of <see cref="CompareOptions"/> will result in
        /// sorting that conforms to the SPARQL 1.1 specification. Using a different culture or comparison option for sorting may produce non-standard results.
        /// </remarks>
        public CultureInfo Culture
        {
            get => _culture;
            set => _culture = value ?? throw new ArgumentNullException(nameof(value), "A non-null CultureInfo value must be provided.");
        }

        /// <summary>
        /// Get or set the method to use for string comparisons in the query processor.
        /// </summary>
        /// <remarks>The SPARQL specification requires that string comparisons should be ordinal (<see cref="System.Globalization.CompareOptions.Ordinal"/>). This is the default value.</remarks>
        public CompareOptions CompareOptions { get; set; } = CompareOptions.Ordinal;

        /// <summary>
        /// Gets/Sets the Hard Timeout limit for SPARQL Query Execution (in milliseconds).
        /// </summary>
        /// <remarks>
        /// This is used to stop SPARQL queries running away and never completing execution, it defaults to 3 mins (180,000 milliseconds).
        /// </remarks>
        public long QueryExecutionTimeout
        {
            get => _queryExecutionTimeout;
            set => _queryExecutionTimeout = Math.Max(value, 0);
        }

    }
}