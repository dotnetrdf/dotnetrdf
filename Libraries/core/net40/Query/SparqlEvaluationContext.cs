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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
#if !NO_STOPWATCH
        private Stopwatch _timer = new Stopwatch();
#else
        private DateTime _start, _end;
#endif
        private Dictionary<String, Object> _functionContexts = new Dictionary<string, object>();
        private long _timeout;

        /// <summary>
        /// Creates a new Evaluation Context for the given Query over the given Dataset
        /// </summary>
        /// <param name="q">Query</param>
        /// <param name="data">Dataset</param>
        public SparqlEvaluationContext(SparqlQuery q, ISparqlDataset data)
        {
            this._query = q;
            this._data = data;
            this._inputSet = new IdentityMultiset();
            this._binder = new LeviathanResultBinder(this);

            this.CalculateTimeout();
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
            this._processor = processor;
        }

        /// <summary>
        /// Creates a new Evaluation Context which is a Container for the given Result Binder
        /// </summary>
        /// <param name="binder"></param>
        public SparqlEvaluationContext(SparqlResultBinder binder)
        {
            this._binder = binder;
        }

        private void CalculateTimeout()
        {
            if (this._query != null)
            {
                if (this._query.Timeout > 0)
                {
                    if (Options.QueryExecutionTimeout == 0 || (this._query.Timeout <= Options.QueryExecutionTimeout && Options.QueryExecutionTimeout > 0))
                    {
                        //Query Timeout is used provided it is less than global timeout unless global timeout is zero
                        this._timeout = this._query.Timeout;
                    }
                    else
                    {
                        //Query Timeout cannot be set higher than global timeout
                        this._timeout = Options.QueryExecutionTimeout;
                    }
                }
                else
                {
                    //If Query Timeout set to zero (i.e. no timeout) then global timeout is used
                    this._timeout = Options.QueryExecutionTimeout;
                }
            }
            else
            {
                //If no query then global timeout is used
                this._timeout = Options.QueryExecutionTimeout;
            }
        }

        /// <summary>
        /// Gets the Query that is being evaluated
        /// </summary>
        public SparqlQuery Query
        {
            get
            {
                return this._query;
            }
        }

        /// <summary>
        /// Gets the Dataset the query is over
        /// </summary>
        public ISparqlDataset Data
        {
            get
            {
                return this._data;
            }
        }

        /// <summary>
        /// Gets the custom query processor that is in use (if any)
        /// </summary>
        public ISparqlQueryAlgebraProcessor<BaseMultiset, SparqlEvaluationContext> Processor
        {
            get
            {
                return this._processor;
            }
        }

        /// <summary>
        /// Gets/Sets the Input Multiset
        /// </summary>
        public BaseMultiset InputMultiset
        {
            get
            {
                return this._inputSet;
            }
            set
            {
                this._inputSet = value;
            }
        }

        /// <summary>
        /// Gets/Sets the Output Multiset
        /// </summary>
        public BaseMultiset OutputMultiset
        {
            get
            {
                return this._outputSet;
            }
            set
            {
                this._outputSet = value;
            }
        }

        /// <summary>
        /// Gets/Sets the Results Binder
        /// </summary>
        public SparqlResultBinder Binder
        {
            get
            {
                return this._binder;
            }
            set
            {
                this._binder = value;
            }
        }

        /// <summary>
        /// Gets/Sets whether BGPs should trim temporary variables
        /// </summary>
        public bool TrimTemporaryVariables
        {
            get
            {
                return this._trimTemporaries;
            }
            set
            {
                this._trimTemporaries = value;
            }
        }

        ///// <summary>
        ///// Gets/Sets whether additional checks should be made during evaluation
        ///// </summary>
        //public bool RigorousEvaluation
        //{
        //    get
        //    {
        //        return this._rigorous;
        //    }
        //    set
        //    {
        //        this._rigorous = value;
        //    }
        //}

        /// <summary>
        /// Starts the Execution Timer
        /// </summary>
        public void StartExecution()
        {
            this.CalculateTimeout();
#if !NO_STOPWATCH
            this._timer.Start();
#else
            this._start = DateTime.Now;
#endif
        }

        /// <summary>
        /// Ends the Execution Timer
        /// </summary>
        public void EndExecution()
        {
#if !NO_STOPWATCH
            this._timer.Stop();
#else
            this._end = DateTime.Now;
#endif
        }

        /// <summary>
        /// Checks whether Execution should Time out
        /// </summary>
        /// <exception cref="RdfQueryTimeoutException">Thrown if the Query has exceeded the Execution Timeout</exception>
        public void CheckTimeout()
        {
            if (this._timeout > 0)
            {
#if !NO_STOPWATCH
                if (this._timer.ElapsedMilliseconds > this._timeout)
                {
                    this._timer.Stop();
                    throw new RdfQueryTimeoutException("Query Execution Time exceeded the Timeout of " + this._timeout + "ms, query aborted after " + this._timer.ElapsedMilliseconds + "ms");
                }
#else
                TimeSpan elapsed = DateTime.Now - this._start;
                if (elapsed.Milliseconds > this._timeout)
                {
                    this._end = DateTime.Now;
                    throw new RdfQueryTimeoutException("Query Execution Time exceeded the Timeout of " + this._timeout + "ms, query aborted after " + elapsed.Milliseconds + "ms");
                }
#endif
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
                if (this._timeout <= 0)
                {
                    return 0;
                }
                else
                {
                    long timeout = this._timeout - this.QueryTime;
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
                return this._timeout;
            }
        }

        /// <summary>
        /// Retrieves the Time in milliseconds the query took to evaluate
        /// </summary>
        public long QueryTime
        {
            get
            {
#if !NO_STOPWATCH
                return this._timer.ElapsedMilliseconds;
#else
                if (this._end == null || this._end < this._start) this._end = DateTime.Now;
                return (this._end - this._start).Milliseconds;
#endif
            }
        }

        /// <summary>
        /// Retrieves the Time in ticks the query took to evaluate
        /// </summary>
        public long QueryTimeTicks
        {
            get
            {
#if !NO_STOPWATCH
                return this._timer.ElapsedTicks;
#else
                if (this._end == null || this._end < this._start) this._end = DateTime.Now;
                return (this._end - this._start).Ticks;
#endif
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
                if (this._functionContexts.ContainsKey(key))
                {
                    return this._functionContexts[key];
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (this._functionContexts.ContainsKey(key))
                {
                    this._functionContexts[key] = value;
                }
                else
                {
                    this._functionContexts.Add(key, value);
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
            if (this._processor == null)
            {
                return algebra.Evaluate(this);
            }
            else
            {
                return this._processor.ProcessAlgebra(algebra, this);
            }
        }
    }
}
