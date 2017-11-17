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
using System.Collections.Generic;
using System.Diagnostics;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Datasets;

namespace VDS.RDF.Query
{
    /// <summary>
    /// Stores information about the Evaluation of a Query during it's evaluation
    /// </summary>
    public class SparqlEvaluationContext
    {
        private ISparqlQueryAlgebraProcessor<BaseMultiset, SparqlEvaluationContext> _processor;
        private BaseMultiset _inputSet, _outputSet;
        private bool _trimTemporaries = true;//, _rigorous = Options.RigorousQueryEvaluation;
        private ISparqlDataset _data;
        private SparqlQuery _query;
        private SparqlResultBinder _binder;
        private Stopwatch _timer = new Stopwatch();
        private Dictionary<String, Object> _functionContexts = new Dictionary<string, object>();
        private long _timeout;

        /// <summary>
        /// Creates a new Evaluation Context for the given Query over the given Dataset
        /// </summary>
        /// <param name="q">Query</param>
        /// <param name="data">Dataset</param>
        public SparqlEvaluationContext(SparqlQuery q, ISparqlDataset data)
        {
            _query = q;
            _data = data;
            _inputSet = new IdentityMultiset();
            _binder = new LeviathanResultBinder(this);

            CalculateTimeout();
        }

        /// <summary>
        /// Creates a new Evaluation Context for the given Query over the given Dataset using a specific processor
        /// </summary>
        /// <param name="q">Query</param>
        /// <param name="data">Dataset</param>
        /// <param name="processor">Query Processor</param>
        public SparqlEvaluationContext(SparqlQuery q, ISparqlDataset data, ISparqlQueryAlgebraProcessor<BaseMultiset, SparqlEvaluationContext> processor)
            : this(q, data)
        {
            _processor = processor;
        }

        /// <summary>
        /// Creates a new Evaluation Context which is a Container for the given Result Binder
        /// </summary>
        /// <param name="binder"></param>
        public SparqlEvaluationContext(SparqlResultBinder binder)
        {
            _binder = binder;
        }

        private void CalculateTimeout()
        {
            if (_query != null)
            {
                if (_query.Timeout > 0)
                {
                    if (Options.QueryExecutionTimeout == 0 || (_query.Timeout <= Options.QueryExecutionTimeout && Options.QueryExecutionTimeout > 0))
                    {
                        // Query Timeout is used provided it is less than global timeout unless global timeout is zero
                        _timeout = _query.Timeout;
                    }
                    else
                    {
                        // Query Timeout cannot be set higher than global timeout
                        _timeout = Options.QueryExecutionTimeout;
                    }
                }
                else
                {
                    // If Query Timeout set to zero (i.e. no timeout) then global timeout is used
                    _timeout = Options.QueryExecutionTimeout;
                }
            }
            else
            {
                // If no query then global timeout is used
                _timeout = Options.QueryExecutionTimeout;
            }
        }

        /// <summary>
        /// Gets the Query that is being evaluated
        /// </summary>
        public SparqlQuery Query
        {
            get
            {
                return _query;
            }
        }

        /// <summary>
        /// Gets the Dataset the query is over
        /// </summary>
        public ISparqlDataset Data
        {
            get
            {
                return _data;
            }
        }

        /// <summary>
        /// Gets the custom query processor that is in use (if any)
        /// </summary>
        public ISparqlQueryAlgebraProcessor<BaseMultiset, SparqlEvaluationContext> Processor
        {
            get
            {
                return _processor;
            }
        }

        /// <summary>
        /// Gets/Sets the Input Multiset
        /// </summary>
        public BaseMultiset InputMultiset
        {
            get
            {
                return _inputSet;
            }
            set
            {
                _inputSet = value;
            }
        }

        /// <summary>
        /// Gets/Sets the Output Multiset
        /// </summary>
        public BaseMultiset OutputMultiset
        {
            get
            {
                return _outputSet;
            }
            set
            {
                _outputSet = value;
            }
        }

        /// <summary>
        /// Gets/Sets the Results Binder
        /// </summary>
        public SparqlResultBinder Binder
        {
            get
            {
                return _binder;
            }
            set
            {
                _binder = value;
            }
        }

        /// <summary>
        /// Gets/Sets whether BGPs should trim temporary variables
        /// </summary>
        public bool TrimTemporaryVariables
        {
            get
            {
                return _trimTemporaries;
            }
            set
            {
                _trimTemporaries = value;
            }
        }

        ///// <summary>
        ///// Gets/Sets whether additional checks should be made during evaluation
        ///// </summary>
        // public bool RigorousEvaluation
        // {
        //    get
        //    {
        //        return this._rigorous;
        //    }
        //    set
        //    {
        //        this._rigorous = value;
        //    }
        // }

        /// <summary>
        /// Starts the Execution Timer
        /// </summary>
        public void StartExecution()
        {
            CalculateTimeout();
            _timer.Start();
        }

        /// <summary>
        /// Ends the Execution Timer
        /// </summary>
        public void EndExecution()
        {
            _timer.Stop();
        }

        /// <summary>
        /// Checks whether Execution should Time out
        /// </summary>
        /// <exception cref="RdfQueryTimeoutException">Thrown if the Query has exceeded the Execution Timeout</exception>
        public void CheckTimeout()
        {
            if (_timeout > 0)
            {
                if (_timer.ElapsedMilliseconds > _timeout)
                {
                    _timer.Stop();
                    throw new RdfQueryTimeoutException("Query Execution Time exceeded the Timeout of " + _timeout + "ms, query aborted after " + _timer.ElapsedMilliseconds + "ms");
                }
            }
        }

        /// <summary>
        /// Gets the Remaining Timeout i.e. the Timeout taking into account time already elapsed
        /// </summary>
        /// <remarks>
        /// If there is no timeout then this is always zero, if there is a timeout this is always >= 1 since any operation that wants to respect the timeout must have a non-zero timeout to actually timeout properly.
        /// </remarks>
        public long RemainingTimeout
        {
            get
            {
                if (_timeout <= 0)
                {
                    return 0;
                }
                else
                {
                    long timeout = _timeout - QueryTime;
                    if (timeout <= 0)
                    {
                        return 1;
                    }
                    else
                    {
                        return timeout;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the Query Timeout used for the Query
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is taken either from the <see cref="SparqlQuery.Timeout">Timeout</see> property of the <see cref="SparqlQuery">SparqlQuery</see> to which this evaluation context pertains (if any) or from the global option <see cref="Options.QueryExecutionTimeout">Options.QueryExecutionTimeout</see>.  To set the Timeout to be used set whichever of those is appropriate prior to evaluating the query.  If there is a Query present then it's timeout takes precedence unless it is set to zero (no timeout) in which case the global timeout setting is applied.  You cannot set the Query Timeout to be higher than the global timeout unless the global timeout is set to zero (i.e. no global timeout)
        /// </para>
        /// </remarks>
        public long QueryTimeout
        {
            get
            {
                return _timeout;
            }
        }

        /// <summary>
        /// Retrieves the Time in milliseconds the query took to evaluate
        /// </summary>
        public long QueryTime
        {
            get
            {
                return _timer.ElapsedMilliseconds;
            }
        }

        /// <summary>
        /// Retrieves the Time in ticks the query took to evaluate
        /// </summary>
        public long QueryTimeTicks
        {
            get
            {
                return _timer.ElapsedTicks;
            }
        }

        /// <summary>
        /// Gets/Sets a Object that should be persisted over the entire Evaluation Context
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns></returns>
        /// <remarks>
        /// May be used by parts of the Evaluation Process that need to ensure a persistent state across the entire Evaluation Query (e.g. the implementation of the BNODE() function)
        /// </remarks>
        public Object this[String key]
        {
            get
            {
                if (_functionContexts.ContainsKey(key))
                {
                    return _functionContexts[key];
                }
                else
                {
                    return null;
                }
            }
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
        /// Evalutes an Algebra Operator in this Context using the current Query Processor (if any) or the default <see cref="ISparqlAlgebra.Evaluate">Evaluate()</see> method
        /// </summary>
        /// <param name="algebra">Algebra</param>
        /// <returns></returns>
        public BaseMultiset Evaluate(ISparqlAlgebra algebra)
        {
            if (_processor == null)
            {
                return algebra.Evaluate(this);
            }
            else
            {
                return _processor.ProcessAlgebra(algebra, this);
            }
        }
    }
}
