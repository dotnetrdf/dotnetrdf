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
using System.Linq;
using System.Text;
using System.Threading;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query
{
    /// <summary>
    /// Represents the level of Query Explanation that is desired
    /// </summary>
    [Flags]
    public enum ExplanationLevel
    {
        /// <summary>
        /// Specifies No Explanations
        /// </summary>
        None = 0,

        /// <summary>
        /// Specifies Explanations are output to Debug
        /// </summary>
        OutputToDebug = 1,
        /// <summary>
        /// Specifies Explanations are output to Trace
        /// </summary>
        OutputToTrace = 2,
        /// <summary>
        /// Specifies Explanations are output to Console Standard Output
        /// </summary>
        OutputToConsoleStdOut = 4,
        /// <summary>
        /// Specifies Explanations are output to Console Standard Error
        /// </summary>
        OutputToConsoleStdErr = 8,

        /// <summary>
        /// Specifies Explanations are output to Debug and Console Standard Output
        /// </summary>
        OutputDefault = OutputToDebug | OutputToConsoleStdOut,
        /// <summary>
        /// Specifies Explanations are output to all
        /// </summary>
        OutputAll = OutputToDebug | OutputToTrace | OutputToConsoleStdOut | OutputToConsoleStdErr,

        /// <summary>
        /// Show the Thread ID of the Thread evaluating the query (useful in multi-threaded environments)
        /// </summary>
        ShowThreadID = 16,
        /// <summary>
        /// Show the Depth of the Algebra Operator
        /// </summary>
        ShowDepth = 32,
        /// <summary>
        /// Show the Type of the Algebra Operator
        /// </summary>
        ShowOperator = 64,
        /// <summary>
        /// Show the Action being performed (makes it clear whether the explanation marks the start/end of an operation)
        /// </summary>
        ShowAction = 128,
        /// <summary>
        /// Shows Timings for the Query
        /// </summary>
        ShowTimings = 256,
        /// <summary>
        /// Show Intermediate Result Counts at each stage of evaluation
        /// </summary>
        ShowIntermediateResultCount = 512,

        /// <summary>
        /// Shows Basic Information (Depth, Operator and Action)
        /// </summary>
        ShowBasic = ShowDepth | ShowOperator | ShowAction,
        /// <summary>
        /// Shows Default Information (Thread ID, Depth, Operator and Action)
        /// </summary>
        ShowDefault = ShowThreadID | ShowDepth | ShowOperator | ShowAction,
        /// <summary>
        /// Shows All Information
        /// </summary>
        ShowAll = ShowThreadID | ShowDepth | ShowOperator | ShowAction | ShowTimings | ShowIntermediateResultCount,

        /// <summary>
        /// Shows an analysis of BGPs prior to evaluating them
        /// </summary>
        /// <remarks>
        /// This lets you see how many joins, cross products, filters, assignments etc must be applied in each BGP
        /// </remarks>
        AnalyseBgps = 1024,
        /// <summary>
        /// Shows an analysis of Joins prior to evaluating them
        /// </summary>
        /// <remarks>
        /// This lets you see whether the join is a join/cross product and in the case of a Minus whether the RHS can be ignored completely
        /// </remarks>
        AnalyseJoins = 2048,

        /// <summary>
        /// Sets whether Evaluation should be simulated (means timings will not be accurate but allows you to explain queries without needing actual data to evaluate them against)
        /// </summary>
        Simulate = 4096,

        /// <summary>
        /// Shows all analysis information
        /// </summary>
        AnalyseAll = AnalyseBgps | AnalyseJoins,

        /// <summary>
        /// Basic Explanation Level (Console Standard Output and Basic Information)
        /// </summary>
        Basic = OutputToConsoleStdOut | ShowBasic,
        /// <summary>
        /// Default Explanation Level (Default Outputs and Default Information)
        /// </summary>
        Default = OutputDefault | ShowDefault,
        /// <summary>
        /// Detailed Explanation Level (Default Outputs and All Information)
        /// </summary>
        Detailed = OutputDefault | ShowAll,
        /// <summary>
        /// Full Explanation Level (All Outputs, All Information and All Analysis)
        /// </summary>
        Full = OutputAll | ShowAll | AnalyseAll,

        /// <summary>
        /// Basic Explanation Level with Query Evaluation simulated
        /// </summary>
        BasicSimulation = Basic | Simulate,
        /// <summary>
        /// Default Explanation Level with Query Evaluation simulated
        /// </summary>
        DefaultSimulation = Default | Simulate,
        /// <summary>
        /// Detailed Explanation Level with Query Evaluation simulated
        /// </summary>
        DetailedSimulation = Detailed | Simulate,
        /// <summary>
        /// Full Explanation Level with Query Evaluation simulated
        /// </summary>
        FullSimulation = Full | Simulate
    }

    /// <summary>
    /// A Query Processor which evaluates queries while printing explanations to any/all of Debug, Trace, Console Standard Output and Console Standard Error
    /// </summary>
    public class ExplainQueryProcessor
        : LeviathanQueryProcessor
    {
        private ThreadIsolatedValue<int> _depthCounter;
        private ThreadIsolatedReference<Stack<DateTime>> _startTimes;
        private ExplanationLevel _level = ExplanationLevel.Default;
        private SparqlFormatter _formatter = new SparqlFormatter();

        /// <summary>
        /// Creates a new Explain Query Processor that will use the Default Explanation Level
        /// </summary>
        /// <param name="dataset">Dataset</param>
        public ExplainQueryProcessor(ISparqlDataset dataset)
            : base(dataset)
        {
            this._depthCounter = new ThreadIsolatedValue<int>(() => 0);
            this._startTimes = new ThreadIsolatedReference<Stack<DateTime>>(() => new Stack<DateTime>());
        }

        /// <summary>
        /// Creates a new Explain Query Processor with the desired Explanation Level
        /// </summary>
        /// <param name="dataset">Dataset</param>
        /// <param name="level">Explanation Level</param>
        public ExplainQueryProcessor(ISparqlDataset dataset, ExplanationLevel level)
            : this(dataset)
        {
            this._level = level;
        }

        /// <summary>
        /// Creates a new Explain Query Processor that will use the Default Explanation Level
        /// </summary>
        /// <param name="store">Triple Store</param>
        public ExplainQueryProcessor(IInMemoryQueryableStore store)
            : this(new InMemoryDataset(store)) { }

        /// <summary>
        /// Creates a new Explain Query Processor with the desired Explanation Level
        /// </summary>
        /// <param name="store">Triple Store</param>
        /// <param name="level">Explanation Level</param>
        public ExplainQueryProcessor(IInMemoryQueryableStore store, ExplanationLevel level)
            : this(new InMemoryDataset(store), level) { }

        /// <summary>
        /// Gets/Sets the Explanation Level
        /// </summary>
        public ExplanationLevel ExplanationLevel
        {
            get 
            {
                return this._level;
            }
            set 
            {
                this._level = value;
            }
        }

        /// <summary>
        /// Determines whether a given Flag is present
        /// </summary>
        /// <param name="flag">Flag</param>
        /// <returns></returns>
        private bool HasFlag(ExplanationLevel flag)
        {
            return ((this._level & flag) == flag);
        }

        /// <summary>
        /// Prints Analysis
        /// </summary>
        /// <param name="algebra">Algebra</param>
        private void PrintAnalysis(ISparqlAlgebra algebra)
        {
            if (algebra is IBgp)
            {
                this.PrintBgpAnalysis((IBgp)algebra);
            }
            else if (algebra is IAbstractJoin)
            {
                this.PrintJoinAnalysis((IAbstractJoin)algebra);
            }
        }

        /// <summary>
        /// Prints BGP Analysis
        /// </summary>
        /// <param name="bgp">Analysis</param>
        private void PrintBgpAnalysis(IBgp bgp)
        {
            if (!this.HasFlag(ExplanationLevel.AnalyseBgps)) return;

            List<ITriplePattern> ps = bgp.TriplePatterns.ToList();
            if (ps.Count == 0)
            {
                this.PrintExplanations("Empty BGP");
            }
            else
            {
                HashSet<String> vars = new HashSet<string>();
                for (int i = 0; i < ps.Count; i++)
                {
                    StringBuilder output = new StringBuilder();

                    //Print what will happen
                    if (ps[i] is FilterPattern)
                    {
                        output.Append("Apply ");
                    }
                    else if (ps[i] is IAssignmentPattern)
                    {
                        output.Append("Extend by Assignment with ");
                    }
                    else if (ps[i] is SubQueryPattern)
                    {
                        output.Append("Sub-query ");
                    }
                    else if (ps[i] is PropertyPathPattern)
                    {
                        output.Append("Property Path ");
                    }

                    //Print the type of Join to be performed
                    if (i > 0 && (ps[i] is TriplePattern || ps[i] is SubQueryPattern || ps[i] is PropertyPathPattern))
                    {
                        if (vars.IsDisjoint<String>(ps[i].Variables))
                        {
                            output.Append("Cross Product with ");
                        }
                        else
                        {
                            List<String> joinVars = vars.Intersect<String>(ps[i].Variables).ToList();
                            output.Append("Join on {");
                            joinVars.ForEach(v => output.Append(" ?" + v + ","));
                            output.Remove(output.Length - 1, 1);
                            output.Append(" } with ");
                        }
                    }

                    //Print the actual Triple Pattern
                    output.Append(this._formatter.Format(ps[i]));

                    //Update variables seen so far unless a FILTER which cannot introduce new variables
                    if (!(ps[i] is FilterPattern))
                    {
                        foreach (String var in ps[i].Variables)
                        {
                            vars.Add(var);
                        }
                    }

                    this.PrintExplanations(output);
                }
            }
        }

        /// <summary>
        /// Prints Join Analysis
        /// </summary>
        /// <param name="join">Join</param>
        private void PrintJoinAnalysis(IAbstractJoin join)
        {
            if (!this.HasFlag(ExplanationLevel.AnalyseJoins)) return;

            HashSet<String> joinVars = new HashSet<string>(join.Lhs.Variables.Intersect<String>(join.Rhs.Variables));
            StringBuilder vars = new StringBuilder();
            vars.Append("{");
            foreach (String var in joinVars)
            {
                vars.Append(" ?" + var + ",");
            }
            vars.Remove(vars.Length-1, 1);
            vars.Append(" }");

            if (join is IMinus)
            {
                if (joinVars.Count == 0)
                {
                    this.PrintExplanations("RHS of Minus is disjoint so has no effect and will not be evaluated");
                }
                else
                {
                    this.PrintExplanations("Minus on " + vars.ToString());
                }
            }
            else
            {
                if (joinVars.Count == 0)
                {
                    this.PrintExplanations("Cross Product");
                } 
                else 
                {
                    this.PrintExplanations("Join on " + vars.ToString());
                }
            }
        }

        /// <summary>
        /// Prints Expalantions
        /// </summary>
        /// <param name="output">StringBuilder to output to</param>
        private void PrintExplanations(StringBuilder output)
        {
            this.PrintExplanations(output.ToString());
        }

        /// <summary>
        /// Prints Explanations
        /// </summary>
        /// <param name="output">String to output</param>
        private void PrintExplanations(String output)
        {
            if (this.HasFlag(ExplanationLevel.OutputToConsoleStdErr))
            {
                Console.Error.WriteLine(output);
            }
            if (this.HasFlag(ExplanationLevel.OutputToConsoleStdOut))
            {
                Console.WriteLine(output);
            }
            if (this.HasFlag(ExplanationLevel.OutputToDebug))
            {
                System.Diagnostics.Debug.WriteLine(output);
            }
#if !SILVERLIGHT
            if (this.HasFlag(ExplanationLevel.OutputToTrace))
            {
                System.Diagnostics.Trace.WriteLine(output);
            }
#endif
        }

        /// <summary>
        /// Explains the start of evaluating some algebra operator
        /// </summary>
        /// <param name="algebra">Algebra</param>
        /// <param name="context">Context</param>
        private void ExplainEvaluationStart(ISparqlAlgebra algebra, SparqlEvaluationContext context)
        {
            if (this._level == ExplanationLevel.None) return;

            this._depthCounter.Value++;

            StringBuilder output = new StringBuilder();
            if (this.HasFlag(ExplanationLevel.ShowThreadID)) output.Append("[Thread ID " + Thread.CurrentThread.ManagedThreadId + "] ");
            if (this.HasFlag(ExplanationLevel.ShowDepth)) output.Append("Depth " + this._depthCounter.Value + " ");
            if (this.HasFlag(ExplanationLevel.ShowOperator)) output.Append(algebra.GetType().FullName + " ");
            if (this.HasFlag(ExplanationLevel.ShowAction)) output.Append("Start");

            this.PrintExplanations(output);
        }

        /// <summary>
        /// Explains the evaluation of some action
        /// </summary>
        /// <param name="algebra">Algebra</param>
        /// <param name="context">Context</param>
        /// <param name="action">Action</param>
        private void ExplainEvaluationAction(ISparqlAlgebra algebra, SparqlEvaluationContext context, String action)
        {
            if (this._level == ExplanationLevel.None) return;

            StringBuilder output = new StringBuilder();
            if (this.HasFlag(ExplanationLevel.ShowThreadID)) output.Append("[Thread ID " + Thread.CurrentThread.ManagedThreadId + "] ");
            if (this.HasFlag(ExplanationLevel.ShowDepth)) output.Append("Depth " + this._depthCounter.Value + " ");
            if (this.HasFlag(ExplanationLevel.ShowOperator)) output.Append(algebra.GetType().FullName + " ");
            if (this.HasFlag(ExplanationLevel.ShowAction)) output.Append(action);

            this.PrintExplanations(output);
        }

        /// <summary>
        /// Explains the end of evaluating some algebra operator
        /// </summary>
        /// <param name="algebra">Algebra</param>
        /// <param name="context">Context</param>
        private void ExplainEvaluationEnd(ISparqlAlgebra algebra, SparqlEvaluationContext context)
        {
            if (this._level == ExplanationLevel.None) return;

            this._depthCounter.Value--;

            StringBuilder output = new StringBuilder();
            if (this.HasFlag(ExplanationLevel.ShowThreadID)) output.Append("[Thread ID " + Thread.CurrentThread.ManagedThreadId + "] ");
            if (this.HasFlag(ExplanationLevel.ShowDepth)) output.Append("Depth " + this._depthCounter.Value + " ");
            if (this.HasFlag(ExplanationLevel.ShowOperator)) output.Append(algebra.GetType().FullName + " ");
            if (this.HasFlag(ExplanationLevel.ShowAction)) output.Append("End");

            this.PrintExplanations(output);
        }

        /// <summary>
        /// Explains and evaluates some algebra operator
        /// </summary>
        /// <typeparam name="T">Algebra Operator Type</typeparam>
        /// <param name="algebra">Algebra</param>
        /// <param name="context">Context</param>
        /// <param name="evaluator">Evaluator Function</param>
        /// <returns></returns>
        private BaseMultiset ExplainAndEvaluate<T>(T algebra, SparqlEvaluationContext context, Func<T, SparqlEvaluationContext, BaseMultiset> evaluator)
            where T : ISparqlAlgebra
        {
            //If explanation is disabled just evaluate and return
            if (this._level == ExplanationLevel.None) return evaluator(algebra, context);

            //Print the basic evaluation start information
            this.ExplainEvaluationStart(algebra, context);

            //Print analysis (if enabled)
            this.PrintAnalysis(algebra);

            //Start Timing (if enabled)
            if (this.HasFlag(ExplanationLevel.ShowTimings)) this._startTimes.Value.Push(DateTime.Now);

            //Do the actual Evaluation
            BaseMultiset results;// = evaluator(algebra, context);
            if (this.HasFlag(ExplanationLevel.Simulate))
            {
                results = (algebra is ITerminalOperator) ? new SingletonMultiset(algebra.Variables) : evaluator(algebra, context);
            }
            else
            {
                results = evaluator(algebra, context);
            }

            //End Timing and Print (if enabled)
            if (this.HasFlag(ExplanationLevel.ShowTimings))
            {
                DateTime start = this._startTimes.Value.Pop();
                TimeSpan elapsed = DateTime.Now - start;
                //this.PrintExplanations("Took " + elapsed.ToString());
                this.ExplainEvaluationAction(algebra, context, "Took " + elapsed.ToString());
            }
            //Show Intermediate Result Count (if enabled)
            if (this.HasFlag(ExplanationLevel.ShowIntermediateResultCount))
            {
                String result;
                if (results is NullMultiset)
                {
                    result = "Result is Null";
                }
                else if (results is IdentityMultiset)
                {
                    result = "Result is Identity";
                }
                else
                {
                    result = results.Count + " Results(s)";
                }
                this.ExplainEvaluationAction(algebra, context, result);
            }

            //Print the basic evaluation end information
            this.ExplainEvaluationEnd(algebra, context);

            //Result the results
            return results;
        }

        /// <summary>
        /// Processes an Ask
        /// </summary>
        /// <param name="ask">Ask</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public override BaseMultiset ProcessAsk(Ask ask, SparqlEvaluationContext context)
        {
            return this.ExplainAndEvaluate<Ask>(ask, context, base.ProcessAsk);
        }

        /// <summary>
        /// Processes a BGP
        /// </summary>
        /// <param name="bgp">BGP</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public override BaseMultiset ProcessBgp(IBgp bgp, SparqlEvaluationContext context)
        {
            return this.ExplainAndEvaluate<IBgp>(bgp, context, base.ProcessBgp);
        }

        /// <summary>
        /// Processes a Bindings modifier
        /// </summary>
        /// <param name="b">Bindings</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public override BaseMultiset ProcessBindings(Bindings b, SparqlEvaluationContext context)
        {
            return this.ExplainAndEvaluate<Bindings>(b, context, base.ProcessBindings);
        }

        /// <summary>
        /// Processes a Distinct modifier
        /// </summary>
        /// <param name="distinct">Distinct modifier</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public override BaseMultiset ProcessDistinct(Distinct distinct, SparqlEvaluationContext context)
        {
            return this.ExplainAndEvaluate<Distinct>(distinct, context, base.ProcessDistinct);
        }

        /// <summary>
        /// Processes an Exists Join
        /// </summary>
        /// <param name="existsJoin">Exists Join</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public override BaseMultiset ProcessExistsJoin(IExistsJoin existsJoin, SparqlEvaluationContext context)
        {
            return this.ExplainAndEvaluate<IExistsJoin>(existsJoin, context, base.ProcessExistsJoin);
        }

        /// <summary>
        /// Processes an Extend
        /// </summary>
        /// <param name="extend">Extend</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public override BaseMultiset ProcessExtend(Extend extend, SparqlEvaluationContext context)
        {
            return this.ExplainAndEvaluate<Extend>(extend, context, base.ProcessExtend);
        }

        /// <summary>
        /// Processes a Filter
        /// </summary>
        /// <param name="filter">Filter</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public override BaseMultiset ProcessFilter(IFilter filter, SparqlEvaluationContext context)
        {
            return this.ExplainAndEvaluate<IFilter>(filter, context, base.ProcessFilter);
        }

        /// <summary>
        /// Processes a Graph
        /// </summary>
        /// <param name="graph">Graph</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public override BaseMultiset ProcessGraph(Algebra.Graph graph, SparqlEvaluationContext context)
        {
            return this.ExplainAndEvaluate<Algebra.Graph>(graph, context, base.ProcessGraph);
        }

        /// <summary>
        /// Processes a Group By
        /// </summary>
        /// <param name="groupBy">Group By</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public override BaseMultiset ProcessGroupBy(GroupBy groupBy, SparqlEvaluationContext context)
        {
            return this.ExplainAndEvaluate<GroupBy>(groupBy, context, base.ProcessGroupBy);
        }

        /// <summary>
        /// Processes a Having
        /// </summary>
        /// <param name="having">Having</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public override BaseMultiset ProcessHaving(Having having, SparqlEvaluationContext context)
        {
            return this.ExplainAndEvaluate<Having>(having, context, base.ProcessHaving);
        }

        /// <summary>
        /// Processes a Join
        /// </summary>
        /// <param name="join">Join</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public override BaseMultiset ProcessJoin(IJoin join, SparqlEvaluationContext context)
        {
            return this.ExplainAndEvaluate<IJoin>(join, context, base.ProcessJoin);
        }

        /// <summary>
        /// Processes a LeftJoin
        /// </summary>
        /// <param name="leftJoin">Left Join</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public override BaseMultiset ProcessLeftJoin(ILeftJoin leftJoin, SparqlEvaluationContext context)
        {
            return this.ExplainAndEvaluate<ILeftJoin>(leftJoin, context, base.ProcessLeftJoin);
        }

        /// <summary>
        /// Processes a Minus
        /// </summary>
        /// <param name="minus">Minus</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public override BaseMultiset ProcessMinus(IMinus minus, SparqlEvaluationContext context)
        {
            return this.ExplainAndEvaluate<IMinus>(minus, context, base.ProcessMinus);
        }

        /// <summary>
        /// Processes a Negated Property Set
        /// </summary>
        /// <param name="negPropSet">Negated Property Set</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        /// <returns></returns>
        public override BaseMultiset ProcessNegatedPropertySet(NegatedPropertySet negPropSet, SparqlEvaluationContext context)
        {
            return this.ExplainAndEvaluate<NegatedPropertySet>(negPropSet, context, base.ProcessNegatedPropertySet);
        }

        /// <summary>
        /// Processes a Null Operator
        /// </summary>
        /// <param name="nullOp">Null Operator</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        /// <returns></returns>
        public override BaseMultiset ProcessNullOperator(NullOperator nullOp, SparqlEvaluationContext context)
        {
            return this.ExplainAndEvaluate<NullOperator>(nullOp, context, base.ProcessNullOperator);
        }

        /// <summary>
        /// Processes a One or More Path
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        /// <returns></returns>
        public override BaseMultiset ProcessOneOrMorePath(OneOrMorePath path, SparqlEvaluationContext context)
        {
            return this.ExplainAndEvaluate<OneOrMorePath>(path, context, base.ProcessOneOrMorePath);
        }

        /// <summary>
        /// Processes an Order By
        /// </summary>
        /// <param name="orderBy"></param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public override BaseMultiset ProcessOrderBy(OrderBy orderBy, SparqlEvaluationContext context)
        {
            return this.ExplainAndEvaluate<OrderBy>(orderBy, context, base.ProcessOrderBy);
        }

        /// <summary>
        /// Processes a Projection
        /// </summary>
        /// <param name="project">Projection</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public override BaseMultiset ProcessProject(Project project, SparqlEvaluationContext context)
        {
            return this.ExplainAndEvaluate<Project>(project, context, base.ProcessProject);
        }

        /// <summary>
        /// Processes a Property Path
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        /// <returns></returns>
        public override BaseMultiset ProcessPropertyPath(PropertyPath path, SparqlEvaluationContext context)
        {
            return this.ExplainAndEvaluate<PropertyPath>(path, context, base.ProcessPropertyPath);
        }

        /// <summary>
        /// Processes a Reduced modifier
        /// </summary>
        /// <param name="reduced">Reduced modifier</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public override BaseMultiset ProcessReduced(Reduced reduced, SparqlEvaluationContext context)
        {
            return this.ExplainAndEvaluate<Reduced>(reduced, context, base.ProcessReduced);
        }

        /// <summary>
        /// Processes a Select
        /// </summary>
        /// <param name="select">Select</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public override BaseMultiset ProcessSelect(Select select, SparqlEvaluationContext context)
        {
            return this.ExplainAndEvaluate<Select>(select, context, base.ProcessSelect);
        }

        /// <summary>
        /// Processes a Select Distinct Graphs
        /// </summary>
        /// <param name="selDistGraphs">Select Distinct Graphs</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public override BaseMultiset ProcessSelectDistinctGraphs(SelectDistinctGraphs selDistGraphs, SparqlEvaluationContext context)
        {
            return this.ExplainAndEvaluate<SelectDistinctGraphs>(selDistGraphs, context, base.ProcessSelectDistinctGraphs);
        }

        /// <summary>
        /// Processes a Service
        /// </summary>
        /// <param name="service">Service</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public override BaseMultiset ProcessService(Service service, SparqlEvaluationContext context)
        {
            return this.ExplainAndEvaluate<Service>(service, context, base.ProcessService);
        }

        /// <summary>
        /// Processes a Slice modifier
        /// </summary>
        /// <param name="slice">Slice modifier</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public override BaseMultiset ProcessSlice(Slice slice, SparqlEvaluationContext context)
        {
            return this.ExplainAndEvaluate<Slice>(slice, context, base.ProcessSlice);
        }

        /// <summary>
        /// Processes a Subquery
        /// </summary>
        /// <param name="subquery">Subquery</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        /// <returns></returns>
        public override BaseMultiset ProcessSubQuery(SubQuery subquery, SparqlEvaluationContext context)
        {
            return this.ExplainAndEvaluate<SubQuery>(subquery, context, base.ProcessSubQuery);
        }

        /// <summary>
        /// Processes a Union
        /// </summary>
        /// <param name="union">Union</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public override BaseMultiset ProcessUnion(IUnion union, SparqlEvaluationContext context)
        {
            return this.ExplainAndEvaluate<IUnion>(union, context, base.ProcessUnion);
        }

        /// <summary>
        /// Processes a Unknown Operator
        /// </summary>
        /// <param name="algebra">Unknown Operator</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public override BaseMultiset ProcessUnknownOperator(ISparqlAlgebra algebra, SparqlEvaluationContext context)
        {
            return this.ExplainAndEvaluate<ISparqlAlgebra>(algebra, context, base.ProcessUnknownOperator);
        }

        /// <summary>
        /// Processes a Zero Length Path
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        /// <returns></returns>
        public override BaseMultiset ProcessZeroLengthPath(ZeroLengthPath path, SparqlEvaluationContext context)
        {
            return this.ExplainAndEvaluate<ZeroLengthPath>(path, context, base.ProcessZeroLengthPath);
        }

        /// <summary>
        /// Processes a Zero or More Path
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        /// <returns></returns>
        public override BaseMultiset ProcessZeroOrMorePath(ZeroOrMorePath path, SparqlEvaluationContext context)
        {
            return this.ExplainAndEvaluate<ZeroOrMorePath>(path, context, base.ProcessZeroOrMorePath);
        }
    }
}
