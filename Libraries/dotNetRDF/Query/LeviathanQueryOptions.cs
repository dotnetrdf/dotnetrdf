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
        private long _queryExecutionTimeout = 180000;
        private ISparqlNodeComparer _nodeComparer = new SparqlNodeComparer(CultureInfo.InvariantCulture, CompareOptions.Ordinal);

        /// <summary>
        /// Get or set the node comparer to use in evaluation.
        /// </summary>
        /// <remarks>You may want to provide a <see cref="SparqlNodeComparer"/> instance to specify a different culture or compare options for string comparison.</remarks>
        public ISparqlNodeComparer NodeComparer { 
            get => _nodeComparer; 
            set => _nodeComparer = value ?? throw new ArgumentNullException(nameof(value));
        }

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

        /// <summary>
        /// Gets/Sets whether Algebra Optimization should be used.
        /// </summary>
        public bool AlgebraOptimisation { get; set; } = true;

        /// <summary>
        /// Gets/Sets whether to use strict operators.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Strict Operators refers to the interpretation of certain operators like + and - in SPARQL expression evaluation.
        /// If enabled then the operators will function only as specified in the SPARQL specification.
        /// If disabled (which is the default) then certain extensions (which the SPARQL specification allows an implementation to provide) will be allowed e.g. date time arithmetic.
        /// </para>
        /// <para>
        /// The only time you may want to disable this is if you are developing queries locally which you want to ensure are portable to other systems or when running the SPARQL compliance tests.
        /// </para>
        /// </remarks>
        public bool StrictOperators { get; set; }

        /// <summary>
        /// Gets/Sets whether the query engine will try to use PLinq where applicable to evaluate suitable SPARQL constructs in parallel.
        /// </summary>
        /// <remarks>
        /// Defaults to true.
        /// </remarks>
        public bool UsePLinqEvaluation { get; set; } = true;

        /// <summary>
        /// Gets/Sets whether to use rigorous query evaluation.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Rigorous Query evaluation applies more checks to the triples produced by datasets to ensure they actually match the patterns being scanned.  If the underlying index structures are able to guarantee this then rigorous evaluation may be turned off for faster evaluation which it is by default since our default <see cref="TreeIndexedTripleCollection"/> and <see cref="TripleCollection"/> implementations will guarantee this.
        /// </para>
        /// </remarks>
        public bool RigorousEvaluation { get; set; } = false;
    }
}