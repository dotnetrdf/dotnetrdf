/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2025 dotNetRDF Project (http://dotnetrdf.org/)
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

using System.Diagnostics;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Datasets;

namespace VDS.RDF.Update;

/// <summary>
/// Evaluation Context for SPARQL Updates evaluated by the libraries Leviathan SPARQL Engine.
/// </summary>
public class SparqlUpdateEvaluationContext
{
    private Stopwatch _timer = new Stopwatch();

    /// <summary>
    /// Creates a new SPARQL Update Evaluation Context.
    /// </summary>
    /// <param name="commands">Command Set.</param>
    /// <param name="data">SPARQL Dataset.</param>
    /// <param name="processor">Query Processor for WHERE clauses.</param>
    /// <param name="options">Update (and query) processor options.</param>
    public SparqlUpdateEvaluationContext(SparqlUpdateCommandSet commands, ISparqlDataset data, ISparqlQueryAlgebraProcessor<BaseMultiset, SparqlEvaluationContext> processor, LeviathanUpdateOptions options)
        : this(commands, data, options)
    {
        QueryProcessor = processor;
    }

    /// <summary>
    /// Creates a new SPARQL Update Evaluation Context.
    /// </summary>
    /// <param name="commands">Command Set.</param>
    /// <param name="data">SPARQL Dataset.</param>
    /// <param name="options">Update (and query) processor options.</param>
    public SparqlUpdateEvaluationContext(SparqlUpdateCommandSet commands, ISparqlDataset data, LeviathanUpdateOptions options)
        : this(data, options)
    {
        Commands = commands;
    }

    /// <summary>
    /// Creates a new SPARQL Update Evaluation Context.
    /// </summary>
    /// <param name="data">SPARQL Dataset.</param>
    /// <param name="processor">Query Processor for WHERE clauses.</param>
    /// <param name="options">Update (and query) processor options.</param>
    public SparqlUpdateEvaluationContext(ISparqlDataset data, ISparqlQueryAlgebraProcessor<BaseMultiset, SparqlEvaluationContext> processor, LeviathanUpdateOptions options)
        : this(data, options)
    {
        QueryProcessor = processor;
    }

    /// <summary>
    /// Creates a new SPARQL Update Evaluation Context.
    /// </summary>
    /// <param name="data">SPARQL Dataset.</param>
    /// <param name="options">Update (and query) processor options.</param>
    public SparqlUpdateEvaluationContext(ISparqlDataset data, LeviathanUpdateOptions options)
    {
        Data = data;
        Options = options;
    }

    /// <summary>
    /// Gets the Command Set (if any) that this context pertains to.
    /// </summary>
    public SparqlUpdateCommandSet Commands { get; }

    /// <summary>
    /// Dataset upon which the Updates are applied.
    /// </summary>
    public ISparqlDataset Data { get; }

    /// <summary>
    /// Get the processor options for this context.
    /// </summary>
    public LeviathanUpdateOptions Options { get; }

    /// <summary>
    /// Gets the Query Processor used to process the WHERE clauses of DELETE or INSERT commands.
    /// </summary>
    public ISparqlQueryAlgebraProcessor<BaseMultiset, SparqlEvaluationContext> QueryProcessor
    {
        get;
    }

    /// <summary>
    /// Retrieves the Time in milliseconds the update took to evaluate.
    /// </summary>
    public long UpdateTime => _timer.ElapsedMilliseconds;

    /// <summary>
    /// Retrieves the Time in ticks the updates took to evaluate.
    /// </summary>
    public long UpdateTimeTicks => _timer.ElapsedTicks;

    private void CalculateTimeout()
    {
        if (Commands != null)
        {
            if (Commands.Timeout > 0)
            {
                if (Options.UpdateExecutionTimeout == 0 || (Commands.Timeout <= Options.UpdateExecutionTimeout && Options.UpdateExecutionTimeout > 0))
                {
                    // Update Timeout is used provided it is less than global timeout unless global timeout is zero
                    UpdateTimeout = Commands.Timeout;
                }
                else
                {
                    // Update Timeout cannot be set higher than global timeout
                    UpdateTimeout = Options.UpdateExecutionTimeout;
                }
            }
            else
            {
                // If Update Timeout set to zero (i.e. no timeout) then global timeout is used
                UpdateTimeout = Options.UpdateExecutionTimeout;
            }
        }
        else
        {
            // If no updates then global timeout is used
            UpdateTimeout = Options.UpdateExecutionTimeout;
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
            if (UpdateTimeout <= 0)
            {
                return 0;
            }
            else
            {
                var timeout = UpdateTimeout - UpdateTime;
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
    /// Gets the Update Timeout used for the Command Set.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is taken either from the <see cref="SparqlUpdateCommandSet.Timeout">Timeout</see> property of the
    /// <see cref="SparqlUpdateCommandSet">SparqlUpdateCommandSet</see> to which this evaluation context pertains,
    /// or from the processor-defined option <see cref="LeviathanUpdateOptions.UpdateExecutionTimeout"/>.
    /// To set the Timeout to be used set whichever of those is appropriate prior to evaluating the updates.
    /// If there is a Command Set present then it's timeout takes precedence unless it is set to zero (no timeout)
    /// in which case the processor-defined timeout setting is applied.
    /// You cannot set the Update Timeout to be higher than the processor-defined timeout unless the
    /// processor-defined timeout is set to zero (i.e. no global timeout).
    /// </para>
    /// </remarks>
    public long UpdateTimeout { get; private set; }

    /// <summary>
    /// Checks whether Execution should Time out.
    /// </summary>
    /// <exception cref="SparqlUpdateTimeoutException">Thrown if the Update has exceeded the Execution Timeout.</exception>
    public void CheckTimeout()
    {
        if (UpdateTimeout > 0)
        {
            if (_timer.ElapsedMilliseconds > UpdateTimeout)
            {
                _timer.Stop();
                throw new SparqlUpdateTimeoutException("Update Execution Time exceeded the Timeout of " + UpdateTimeout + "ms, updates aborted after " + _timer.ElapsedMilliseconds + "ms");
            }
        }
    }

    /// <summary>
    /// Starts the Execution Timer.
    /// </summary>
    public void StartExecution()
    {
        CalculateTimeout();
        _timer.Start();
    }

    /// <summary>
    /// Ends the Execution Timer.
    /// </summary>
    public void EndExecution()
    {
        _timer.Stop();
    }
}
