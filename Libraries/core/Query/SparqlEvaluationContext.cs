/*

Copyright Robert Vesse 2009-10
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
        private BaseMultiset _inputSet, _outputSet;
        private ISparqlDataset _data;
        private SparqlQuery _query;
        private SparqlResultBinder _binder;
#if !NO_STOPWATCH
        private Stopwatch _timer = new Stopwatch();
#else
        private DateTime _start, _end;
#endif

        /// <summary>
        /// Creates a new Evaluation Context for the given Query over the given Triple Store
        /// </summary>
        /// <param name="q">Query</param>
        /// <param name="data">Dataset</param>
        public SparqlEvaluationContext(SparqlQuery q, ISparqlDataset data)
        {
            this._query = q;
            this._data = data;
            this._inputSet = new IdentityMultiset();
            this._binder = new LeviathanResultBinder(this);
        }

        /// <summary>
        /// Creates a new Evaluation Context which is a Container for the given Result Binder
        /// </summary>
        /// <param name="binder"></param>
        public SparqlEvaluationContext(SparqlResultBinder binder)
        {
            this._binder = binder;
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
        /// Starts the Execution Timer
        /// </summary>
        public void StartExecution()
        {
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
            if (this._query != null)
            {
                if (this._query.Timeout > 0)
                {
#if !NO_STOPWATCH
                    if (this._timer.ElapsedMilliseconds > this._query.Timeout)
                    {
                        this._timer.Stop();
                        throw new RdfQueryTimeoutException("Query Execution Time exceeded the Timeout of " + this._query.Timeout + "ms, query aborted after " + this._timer.ElapsedMilliseconds + "ms");
                    }
#else
                    TimeSpan elapsed = DateTime.Now - this._start;
                    if (elapsed.Milliseconds > this._query.Timeout)
                    {
                        this._end = DateTime.Now;
                        throw new RdfQueryTimeoutException("Query Execution Time exceeded the Timeout of " + this._query.Timeout + "ms, query aborted after " + elapsed.Milliseconds + "ms");
                    }
#endif
                }
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
                return (this._end - this._start).Milliseconds;
#endif
            }
        }

    }
}
