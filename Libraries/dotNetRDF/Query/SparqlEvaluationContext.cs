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
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Datasets;

namespace VDS.RDF.Query
{
    /// <summary>
    /// Stores information about the Evaluation of a Query during it's evaluation.
    /// </summary>
    public class SparqlEvaluationContext
    {
        private readonly Stopwatch _timer = new Stopwatch();
        private readonly Dictionary<string, object> _functionContexts = new Dictionary<string, object>();
        private HttpClient _defaultHttpClient;

        /// <summary>
        /// Creates a new Evaluation Context for the given Query over the given Dataset.
        /// </summary>
        /// <param name="q">Query.</param>
        /// <param name="data">Dataset.</param>
        /// <param name="options">The query processor options to use.</param>
        public SparqlEvaluationContext(SparqlQuery q, ISparqlDataset data, LeviathanQueryOptions options)
        {
            Query = q;
            Data = data;
            InputMultiset = new IdentityMultiset();
            Binder = new LeviathanResultBinder(this);
            Options = options;
            NodeComparer = Options.NodeComparer;
            OrderingComparer = new SparqlOrderingComparer(NodeComparer);
        }

        /// <summary>
        /// Creates a new Evaluation Context for the given Query over the given Dataset using a specific processor.
        /// </summary>
        /// <param name="q">Query.</param>
        /// <param name="data">Dataset.</param>
        /// <param name="processor">Query Processor.</param>
        /// <param name="options">The query processor options to use.</param>
        public SparqlEvaluationContext(SparqlQuery q, ISparqlDataset data, ISparqlQueryAlgebraProcessor<BaseMultiset, SparqlEvaluationContext> processor, LeviathanQueryOptions options)
            : this(q, data, options)
        {
            Processor = processor;
        }

        /// <summary>
        /// Creates a new Evaluation Context which is a Container for the given Result Binder.
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="options">The query processor options to use.</param>
        public SparqlEvaluationContext(SparqlResultBinder binder, LeviathanQueryOptions options)
        {
            Binder = binder;
            Options = options;
            OrderingComparer = new SparqlOrderingComparer(Options.NodeComparer);
        }

        /// <summary>
        /// Return the execution timeout to be applied to this evaluation context given the specified processor-defined maximum execution timeout.
        /// </summary>
        /// <param name="maxTimeout"></param>
        /// <returns>The execution timeout for the query in this context (if any).</returns>
        public long CalculateTimeout(long maxTimeout)
        {
            if (Query != null && Query.Timeout > 0 &&
                (maxTimeout <= 0 || Query.Timeout <= maxTimeout))
            {
                return Query.Timeout;
            }

            return maxTimeout;
        }

        /// <summary>
        /// Gets the Query that is being evaluated.
        /// </summary>
        public SparqlQuery Query { get; }

        /// <summary>
        /// Gets the Dataset the query is over.
        /// </summary>
        public ISparqlDataset Data { get; }

        /// <summary>
        /// Get the configured query options for this evaluation context.
        /// </summary>
        public LeviathanQueryOptions Options { get; }

        /// <summary>
        /// Gets the custom query processor that is in use (if any).
        /// </summary>
        public ISparqlQueryAlgebraProcessor<BaseMultiset, SparqlEvaluationContext> Processor { get; }

        /// <summary>
        /// Gets/Sets the Input Multiset.
        /// </summary>
        public BaseMultiset InputMultiset { get; set; }

        /// <summary>
        /// Gets/Sets the Output Multiset.
        /// </summary>
        public BaseMultiset OutputMultiset { get; set; }

        /// <summary>
        /// Gets/Sets the Results Binder.
        /// </summary>
        public SparqlResultBinder Binder { get; set; }

        /// <summary>
        /// Gets/Sets whether BGPs should trim temporary variables.
        /// </summary>
        public bool TrimTemporaryVariables { get; set; } = true;

        /// <summary>
        /// Get the comparer to use when ordering nodes during query processing.
        /// </summary>
        public ISparqlNodeComparer NodeComparer { get; }

        /// <summary>
        /// Get the comparer to use when sorting query results.
        /// </summary>
        public SparqlOrderingComparer OrderingComparer { get; }


        /// <summary>
        /// Starts the Execution Timer.
        /// </summary>
        /// <param name="maxTimeout">The maximum time (in milliseconds) to allow the query to run for. This may be overridden by the timeout specified in the query itself.</param>
        /// <remarks>
        /// A value of zero or less for <paramref name="maxTimeout"/> indicates no timeout. If a finite timeout is specified both by <paramref name="maxTimeout"/> and in the query, then the shorter of these two timeout values will be used.
        /// </remarks>
        public void StartExecution(long maxTimeout)
        {
            QueryTimeout = CalculateTimeout(maxTimeout);
            _timer.Start();
        }

        /// <summary>
        /// Ends the Execution Timer.
        /// </summary>
        public void EndExecution()
        {
            _timer.Stop();
        }

        /// <summary>
        /// Checks whether Execution should Time out.
        /// </summary>
        /// <exception cref="RdfQueryTimeoutException">Thrown if the Query has exceeded the Execution Timeout.</exception>
        public void CheckTimeout()
        {
            if (QueryTimeout > 0 && _timer.ElapsedMilliseconds > QueryTimeout)
            {
                _timer.Stop();
                throw new RdfQueryTimeoutException(
                    $"Query Execution Time exceeded the Timeout of {QueryTimeout}ms, query aborted after {_timer.ElapsedMilliseconds}ms");
            }
        }

        /// <summary>
        /// Gets the Remaining Timeout i.e. the Timeout taking into account time already elapsed.
        /// </summary>
        /// <remarks>
        /// If there is no timeout then this is always zero, if there is a timeout this is always >= 1 since any operation that wants to respect the timeout must have a non-zero timeout to actually timeout properly.
        /// </remarks>
        public long RemainingTimeout
        {
            get
            {
                if (QueryTimeout <= 0)
                {
                    return 0;
                }

                var timeout = QueryTimeout - QueryTime;
                return timeout <= 0 ? 1 : timeout;
            }
        }

        /// <summary>
        /// Gets the Query Timeout used for the Query.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is taken either from the <see cref="SparqlQuery.Timeout">Timeout</see> property of the <see cref="SparqlQuery">SparqlQuery</see> to which this
        /// evaluation context pertains (if any) or from the the processor-defined timeout value passed as a parameter to <see cref="StartExecution"/> method.
        /// You cannot set the Query Timeout to be higher than the processor-defined timeout unless the processor-defined timeout is set to zero (i.e. no processor-defined timeout).
        /// </para>
        /// </remarks>
        public long QueryTimeout { get; private set; }

        /// <summary>
        /// Retrieves the Time in milliseconds the query took to evaluate.
        /// </summary>
        public long QueryTime => _timer.ElapsedMilliseconds;

        /// <summary>
        /// Retrieves the Time in ticks the query took to evaluate.
        /// </summary>
        public long QueryTimeTicks => _timer.ElapsedTicks;

        /// <summary>
        /// Gets/Sets a Object that should be persisted over the entire Evaluation Context.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <returns></returns>
        /// <remarks>
        /// May be used by parts of the Evaluation Process that need to ensure a persistent state across the entire Evaluation Query (e.g. the implementation of the BNODE() function).
        /// </remarks>
        public object this[string key]
        {
            get => _functionContexts.ContainsKey(key) ? _functionContexts[key] : null;
            set
            {
                if (_functionContexts.ContainsKey(key))
                {
                    _functionContexts[key] = value;
                }
                else
                {
                    _functionContexts.Add(key, value);
                }
            }
        }

        /// <summary>
        /// Evaluates an Algebra Operator in this Context using the current Query Processor (if any) or the default <see cref="ISparqlAlgebra.Evaluate">Evaluate()</see> method.
        /// </summary>
        /// <param name="algebra">Algebra.</param>
        /// <returns></returns>
        public BaseMultiset Evaluate(ISparqlAlgebra algebra)
        {
            return Processor == null ? algebra.Evaluate(this) : Processor.ProcessAlgebra(algebra, this);
        }

        /// <summary>
        /// Return an HttpClient instance to use when connecting to a specific URI endpoint.
        /// </summary>
        /// <param name="endpointUri">The endpoint to connect to.</param>
        /// <returns>An HttpClient instance to use for connections to the specified endpoint.</returns>
        public HttpClient GetHttpClient(Uri endpointUri)
        {
            return _defaultHttpClient ??= new HttpClient();
        }
    }
}
