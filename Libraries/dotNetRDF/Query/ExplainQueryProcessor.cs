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
using System.Linq;
using System.Text;
using System.Threading;
using VDS.Common.References;
using VDS.RDF.Parsing.Tokens;
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
        /// Shows an analysis of Named Graphs used by a Graph clause prior to evaluating them
        /// </summary>
        /// <remarks>
        /// This lets you see how many graphs a given Graph clause will operate over.  As the Graph clause in SPARQL is defined as the union of evaluating the inner operator over each named graph in the dataset graph clauses applied to datasets with many named graphs can be expensive.
        /// </remarks>
        AnalyseNamedGraphs = 4096,

        /// <summary>
        /// Sets whether Evaluation should be simulated (means timings will not be accurate but allows you to explain queries without needing actual data to evaluate them against)
        /// </summary>
        Simulate = 8092,

        /// <summary>
        /// Shows all analysis information
        /// </summary>
        AnalyseAll = AnalyseBgps | AnalyseJoins | AnalyseNamedGraphs,

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
        private readonly ThreadIsolatedValue<int> _depthCounter;
        private readonly ThreadIsolatedReference<Stack<DateTime>> _startTimes;
        private ExplanationLevel _level = ExplanationLevel.Default;
        private readonly SparqlFormatter _formatter = new SparqlFormatter();

        /// <summary>
        /// Creates a new Explain Query Processor that will use the Default Explanation Level
        /// </summary>
        /// <param name="dataset">Dataset</param>
        public ExplainQueryProcessor(ISparqlDataset dataset)
            : this(new ExplainDataset(dataset)) { }

        private ExplainQueryProcessor(ExplainDataset dataset)
            : base(dataset)
        {
            _depthCounter = new ThreadIsolatedValue<int>(() => 0);
            _startTimes = new ThreadIsolatedReference<Stack<DateTime>>(() => new Stack<DateTime>());
            dataset.Processor = this;
        }

        /// <summary>
        /// Creates a new Explain Query Processor with the desired Explanation Level
        /// </summary>
        /// <param name="dataset">Dataset</param>
        /// <param name="level">Explanation Level</param>
        public ExplainQueryProcessor(ISparqlDataset dataset, ExplanationLevel level)
            : this(dataset)
        {
            _level = level;
        }

        /// <summary>
        /// Creates a new Explain Query Processor that will use the Default Explanation Level
        /// </summary>
        /// <param name="store">Triple Store</param>
        public ExplainQueryProcessor(IInMemoryQueryableStore store)
            : this(new InMemoryDataset(store)) {}

        /// <summary>
        /// Creates a new Explain Query Processor with the desired Explanation Level
        /// </summary>
        /// <param name="store">Triple Store</param>
        /// <param name="level">Explanation Level</param>
        public ExplainQueryProcessor(IInMemoryQueryableStore store, ExplanationLevel level)
            : this(new InMemoryDataset(store), level) {}

        /// <summary>
        /// Gets/Sets the Explanation Level
        /// </summary>
        public ExplanationLevel ExplanationLevel
        {
            get { return _level; }
            set { _level = value; }
        }

        /// <summary>
        /// Determines whether a given Flag is present
        /// </summary>
        /// <param name="flag">Flag</param>
        /// <returns></returns>
        internal bool HasFlag(ExplanationLevel flag)
        {
            return ((_level & flag) == flag);
        }

        /// <summary>
        /// Prints Analysis
        /// </summary>
        /// <param name="algebra">Algebra</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        private void PrintAnalysis(ISparqlAlgebra algebra, SparqlEvaluationContext context)
        {
            if (algebra is IBgp)
            {
                PrintBgpAnalysis((IBgp) algebra);
            }
            else if (algebra is IAbstractJoin)
            {
                PrintJoinAnalysis((IAbstractJoin) algebra);
            }
            else if (algebra is Algebra.Graph)
            {
                PrintGraphAnalysis((Algebra.Graph) algebra, context);
            }
        }

        /// <summary>
        /// Prints BGP Analysis
        /// </summary>
        /// <param name="bgp">Analysis</param>
        private void PrintBgpAnalysis(IBgp bgp)
        {
            if (!HasFlag(ExplanationLevel.AnalyseBgps)) return;

            List<ITriplePattern> ps = bgp.TriplePatterns.ToList();
            if (ps.Count == 0)
            {
                PrintExplanations("Empty BGP");
            }
            else
            {
                HashSet<String> vars = new HashSet<string>();
                for (int i = 0; i < ps.Count; i++)
                {
                    StringBuilder output = new StringBuilder();

                    // Print what will happen
                    if (ps[i].PatternType == TriplePatternType.Filter)
                    {
                        output.Append("Apply ");
                    }
                    else if (ps[i].PatternType == TriplePatternType.BindAssignment || ps[i].PatternType == TriplePatternType.LetAssignment)
                    {
                        output.Append("Extend by Assignment with ");
                    }
                    else if (ps[i].PatternType == TriplePatternType.SubQuery)
                    {
                        output.Append("Sub-query ");
                    }
                    else if (ps[i].PatternType == TriplePatternType.Path)
                    {
                        output.Append("Property Path ");
                    }
                    else if (ps[i].PatternType == TriplePatternType.PropertyFunction)
                    {
                        output.Append("Property Function ");
                    }

                    // Print the type of Join to be performed
                    if (i > 0 && (ps[i].PatternType == TriplePatternType.Match || ps[i].PatternType == TriplePatternType.SubQuery || ps[i].PatternType == TriplePatternType.Path))
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

                    // Print the actual Triple Pattern
                    output.Append(_formatter.Format(ps[i]));

                    // Update variables seen so far unless a FILTER which cannot introduce new variables
                    if (ps[i].PatternType != TriplePatternType.Filter)
                    {
                        foreach (String var in ps[i].Variables)
                        {
                            vars.Add(var);
                        }
                    }

                    PrintExplanations(output);
                }
            }
        }

        /// <summary>
        /// Prints Join Analysis
        /// </summary>
        /// <param name="join">Join</param>
        private void PrintJoinAnalysis(IAbstractJoin join)
        {
            if (!HasFlag(ExplanationLevel.AnalyseJoins)) return;

            HashSet<String> joinVars = new HashSet<string>(join.Lhs.Variables.Intersect<String>(join.Rhs.Variables));
            StringBuilder vars = new StringBuilder();
            vars.Append("{");
            foreach (String var in joinVars)
            {
                vars.Append(" ?" + var + ",");
            }
            vars.Remove(vars.Length - 1, 1);
            vars.Append(" }");

            if (join is IMinus)
            {
                if (joinVars.Count == 0)
                {
                    PrintExplanations("RHS of Minus is disjoint so has no effect and will not be evaluated");
                }
                else
                {
                    PrintExplanations("Minus on " + vars.ToString());
                }
            }
            else
            {
                if (joinVars.Count == 0)
                {
                    PrintExplanations("Cross Product");
                }
                else
                {
                    PrintExplanations("Join on " + vars.ToString());
                }
            }
        }

        private void PrintGraphAnalysis(Algebra.Graph graph, SparqlEvaluationContext context)
        {
            if (!HasFlag(ExplanationLevel.AnalyseNamedGraphs)) return;

            switch (graph.GraphSpecifier.TokenType)
            {
                case Token.QNAME:
                case Token.URI:
                    // Only a single active graph
                    Uri activeGraphUri = UriFactory.Create(Tools.ResolveUriOrQName(graph.GraphSpecifier, context.Query.NamespaceMap, context.Query.BaseUri));
                    PrintExplanations("Graph clause accesses single named graph " + activeGraphUri.AbsoluteUri);
                    break;
                case Token.VARIABLE:
                    // Potentially many active graphs
                    List<String> activeGraphs = new List<string>();
                    String gvar = graph.GraphSpecifier.Value.Substring(1);

                    // Watch out for the case in which the Graph Variable is not bound for all Sets in which case
                    // we still need to operate over all Graphs
                    if (context.InputMultiset.ContainsVariable(gvar) && context.InputMultiset.Sets.All(s => s[gvar] != null))
                    {
                        // If there are already values bound to the Graph variable for all Input Solutions then we limit the Query to those Graphs
                        List<Uri> graphUris = new List<Uri>();
                        foreach (ISet s in context.InputMultiset.Sets)
                        {
                            INode temp = s[gvar];
                            if (temp == null) continue;
                            if (temp.NodeType != NodeType.Uri) continue;
                            activeGraphs.Add(temp.ToString());
                            graphUris.Add(((IUriNode) temp).Uri);
                        }
                    }
                    else
                    {
                        // Worth explaining that the graph variable is partially bound
                        if (context.InputMultiset.ContainsVariable(gvar))
                        {
                            PrintExplanations("Graph clause uses variable ?" + gvar + " to specify named graphs, this variable is only partially bound at this point in the query so can't be use to restrict list of named graphs to access");
                        }

                        // Nothing yet bound to the Graph Variable so the Query is over all the named Graphs
                        if (context.Query != null && context.Query.NamedGraphs.Any())
                        {
                            // Query specifies one/more named Graphs
                            PrintExplanations("Graph clause uses variable ?" + gvar + " which is restricted to graphs specified by the queries FROM NAMED clause(s)");
                            activeGraphs.AddRange(context.Query.NamedGraphs.Select(u => u.AbsoluteUri));
                        }
                        else if (context.Query != null && context.Query.DefaultGraphs.Any() && !context.Query.NamedGraphs.Any())
                        {
                            // Gives null since the query dataset does not include any named graphs
                            PrintExplanations("Graph clause uses variable ?" + gvar + " which will match no graphs because the queries dataset description does not include any named graphs i.e. there where FROM clauses but no FROM NAMED clauses");
                            return;
                        }
                        else
                        {
                            // Query is over entire dataset/default Graph since no named Graphs are explicitly specified
                            PrintExplanations("Graph clause uses variable ?" + gvar + " which accesses all named graphs provided by the dataset");
                            activeGraphs.AddRange(context.Data.GraphUris.Select(u => u.ToSafeString()));
                        }
                    }

                    // Remove all duplicates from Active Graphs to avoid duplicate results
                    activeGraphs = activeGraphs.Distinct().ToList();
                    activeGraphs.RemoveAll(x => x == null);
                    PrintExplanations("Graph clause will access the following " + activeGraphs.Count + " graphs:");
                    foreach (String uri in activeGraphs)
                    {
                        PrintExplanations(uri);
                    }

                    break;

                default:
                    throw new RdfQueryException("Cannot use a '" + graph.GraphSpecifier.GetType() + "' Token to specify the Graph for a GRAPH clause");
            }
        }

        /// <summary>
        /// Prints Expalantions
        /// </summary>
        /// <param name="output">StringBuilder to output to</param>
        internal void PrintExplanations(StringBuilder output)
        {
            PrintExplanations(output.ToString());
        }

        /// <summary>
        /// Prints Explanations
        /// </summary>
        /// <param name="output">String to output</param>
        internal void PrintExplanations(String output)
        {
            String indent = new string(' ', Math.Max(_depthCounter.Value-1, 1) * 2);
            if (HasFlag(ExplanationLevel.OutputToConsoleStdErr))
            {
                Console.Error.Write(indent);
                Console.Error.WriteLine(output);
            }
            if (HasFlag(ExplanationLevel.OutputToConsoleStdOut))
            {
                Console.Write(indent);
                Console.WriteLine(output);
            }
            if (HasFlag(ExplanationLevel.OutputToDebug))
            {
                System.Diagnostics.Debug.Write(indent);
                System.Diagnostics.Debug.WriteLine(output);
            }
#if !NETCORE
            if (HasFlag(ExplanationLevel.OutputToTrace))
            {
                System.Diagnostics.Trace.Write(indent);
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
            if (_level == ExplanationLevel.None) return;

            _depthCounter.Value++;

            StringBuilder output = new StringBuilder();
            if (HasFlag(ExplanationLevel.ShowThreadID)) output.Append("[Thread ID " + Thread.CurrentThread.ManagedThreadId + "] ");
            if (HasFlag(ExplanationLevel.ShowDepth)) output.Append("Depth " + _depthCounter.Value + " ");
            if (HasFlag(ExplanationLevel.ShowOperator)) output.Append(algebra.GetType().FullName + " ");
            if (HasFlag(ExplanationLevel.ShowAction)) output.Append("Start");

            PrintExplanations(output);
        }

        /// <summary>
        /// Explains the evaluation of some action
        /// </summary>
        /// <param name="algebra">Algebra</param>
        /// <param name="context">Context</param>
        /// <param name="action">Action</param>
        private void ExplainEvaluationAction(ISparqlAlgebra algebra, SparqlEvaluationContext context, String action)
        {
            if (_level == ExplanationLevel.None) return;

            StringBuilder output = new StringBuilder();
            if (HasFlag(ExplanationLevel.ShowThreadID)) output.Append("[Thread ID " + Thread.CurrentThread.ManagedThreadId + "] ");
            if (HasFlag(ExplanationLevel.ShowDepth)) output.Append("Depth " + _depthCounter.Value + " ");
            if (HasFlag(ExplanationLevel.ShowOperator)) output.Append(algebra.GetType().FullName + " ");
            if (HasFlag(ExplanationLevel.ShowAction)) output.Append(action);

            PrintExplanations(output);
        }

        /// <summary>
        /// Explains the end of evaluating some algebra operator
        /// </summary>
        /// <param name="algebra">Algebra</param>
        /// <param name="context">Context</param>
        private void ExplainEvaluationEnd(ISparqlAlgebra algebra, SparqlEvaluationContext context)
        {
            if (_level == ExplanationLevel.None) return;

            _depthCounter.Value--;

            StringBuilder output = new StringBuilder();
            if (HasFlag(ExplanationLevel.ShowThreadID)) output.Append("[Thread ID " + Thread.CurrentThread.ManagedThreadId + "] ");
            if (HasFlag(ExplanationLevel.ShowDepth)) output.Append("Depth " + _depthCounter.Value + " ");
            if (HasFlag(ExplanationLevel.ShowOperator)) output.Append(algebra.GetType().FullName + " ");
            if (HasFlag(ExplanationLevel.ShowAction)) output.Append("End");

            PrintExplanations(output);
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
            // If explanation is disabled just evaluate and return
            if (_level == ExplanationLevel.None) return evaluator(algebra, context);

            // Print the basic evaluation start information
            ExplainEvaluationStart(algebra, context);

            // Print analysis (if enabled)
            PrintAnalysis(algebra, context);

            // Start Timing (if enabled)
            if (HasFlag(ExplanationLevel.ShowTimings)) _startTimes.Value.Push(DateTime.Now);

            // Do the actual Evaluation
            BaseMultiset results; // = evaluator(algebra, context);
            if (HasFlag(ExplanationLevel.Simulate))
            {
                results = (algebra is ITerminalOperator) ? new SingletonMultiset(algebra.Variables) : evaluator(algebra, context);
            }
            else
            {
                results = evaluator(algebra, context);
            }

            // End Timing and Print (if enabled)
            if (HasFlag(ExplanationLevel.ShowTimings))
            {
                DateTime start = _startTimes.Value.Pop();
                TimeSpan elapsed = DateTime.Now - start;
                // this.PrintExplanations("Took " + elapsed.ToString());
                ExplainEvaluationAction(algebra, context, "Took " + elapsed.ToString());
            }
            // Show Intermediate Result Count (if enabled)
            if (HasFlag(ExplanationLevel.ShowIntermediateResultCount))
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
                ExplainEvaluationAction(algebra, context, result);
            }

            // Print the basic evaluation end information
            ExplainEvaluationEnd(algebra, context);

            // Result the results
            return results;
        }

        /// <summary>
        /// Processes an Ask
        /// </summary>
        /// <param name="ask">Ask</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public override BaseMultiset ProcessAsk(Ask ask, SparqlEvaluationContext context)
        {
            return ExplainAndEvaluate<Ask>(ask, context, base.ProcessAsk);
        }

        /// <summary>
        /// Processes a BGP
        /// </summary>
        /// <param name="bgp">BGP</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public override BaseMultiset ProcessBgp(IBgp bgp, SparqlEvaluationContext context)
        {
            return ExplainAndEvaluate<IBgp>(bgp, context, base.ProcessBgp);
        }

        /// <summary>
        /// Processes a Bindings modifier
        /// </summary>
        /// <param name="b">Bindings</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public override BaseMultiset ProcessBindings(Bindings b, SparqlEvaluationContext context)
        {
            return ExplainAndEvaluate<Bindings>(b, context, base.ProcessBindings);
        }

        /// <summary>
        /// Processes a Distinct modifier
        /// </summary>
        /// <param name="distinct">Distinct modifier</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public override BaseMultiset ProcessDistinct(Distinct distinct, SparqlEvaluationContext context)
        {
            return ExplainAndEvaluate<Distinct>(distinct, context, base.ProcessDistinct);
        }

        /// <summary>
        /// Processes an Exists Join
        /// </summary>
        /// <param name="existsJoin">Exists Join</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public override BaseMultiset ProcessExistsJoin(IExistsJoin existsJoin, SparqlEvaluationContext context)
        {
            return ExplainAndEvaluate<IExistsJoin>(existsJoin, context, base.ProcessExistsJoin);
        }

        /// <summary>
        /// Processes an Extend
        /// </summary>
        /// <param name="extend">Extend</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public override BaseMultiset ProcessExtend(Extend extend, SparqlEvaluationContext context)
        {
            return ExplainAndEvaluate<Extend>(extend, context, base.ProcessExtend);
        }

        /// <summary>
        /// Processes a Filter
        /// </summary>
        /// <param name="filter">Filter</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public override BaseMultiset ProcessFilter(IFilter filter, SparqlEvaluationContext context)
        {
            return ExplainAndEvaluate<IFilter>(filter, context, base.ProcessFilter);
        }

        /// <summary>
        /// Processes a Graph
        /// </summary>
        /// <param name="graph">Graph</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public override BaseMultiset ProcessGraph(Algebra.Graph graph, SparqlEvaluationContext context)
        {
            return ExplainAndEvaluate<Algebra.Graph>(graph, context, base.ProcessGraph);
        }

        /// <summary>
        /// Processes a Group By
        /// </summary>
        /// <param name="groupBy">Group By</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public override BaseMultiset ProcessGroupBy(GroupBy groupBy, SparqlEvaluationContext context)
        {
            return ExplainAndEvaluate<GroupBy>(groupBy, context, base.ProcessGroupBy);
        }

        /// <summary>
        /// Processes a Having
        /// </summary>
        /// <param name="having">Having</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public override BaseMultiset ProcessHaving(Having having, SparqlEvaluationContext context)
        {
            return ExplainAndEvaluate<Having>(having, context, base.ProcessHaving);
        }

        /// <summary>
        /// Processes a Join
        /// </summary>
        /// <param name="join">Join</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public override BaseMultiset ProcessJoin(IJoin join, SparqlEvaluationContext context)
        {
            return ExplainAndEvaluate<IJoin>(join, context, base.ProcessJoin);
        }

        /// <summary>
        /// Processes a LeftJoin
        /// </summary>
        /// <param name="leftJoin">Left Join</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public override BaseMultiset ProcessLeftJoin(ILeftJoin leftJoin, SparqlEvaluationContext context)
        {
            return ExplainAndEvaluate<ILeftJoin>(leftJoin, context, base.ProcessLeftJoin);
        }

        /// <summary>
        /// Processes a Minus
        /// </summary>
        /// <param name="minus">Minus</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public override BaseMultiset ProcessMinus(IMinus minus, SparqlEvaluationContext context)
        {
            return ExplainAndEvaluate<IMinus>(minus, context, base.ProcessMinus);
        }

        /// <summary>
        /// Processes a Negated Property Set
        /// </summary>
        /// <param name="negPropSet">Negated Property Set</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        /// <returns></returns>
        public override BaseMultiset ProcessNegatedPropertySet(NegatedPropertySet negPropSet, SparqlEvaluationContext context)
        {
            return ExplainAndEvaluate<NegatedPropertySet>(negPropSet, context, base.ProcessNegatedPropertySet);
        }

        /// <summary>
        /// Processes a Null Operator
        /// </summary>
        /// <param name="nullOp">Null Operator</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        /// <returns></returns>
        public override BaseMultiset ProcessNullOperator(NullOperator nullOp, SparqlEvaluationContext context)
        {
            return ExplainAndEvaluate<NullOperator>(nullOp, context, base.ProcessNullOperator);
        }

        /// <summary>
        /// Processes a One or More Path
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        /// <returns></returns>
        public override BaseMultiset ProcessOneOrMorePath(OneOrMorePath path, SparqlEvaluationContext context)
        {
            return ExplainAndEvaluate<OneOrMorePath>(path, context, base.ProcessOneOrMorePath);
        }

        /// <summary>
        /// Processes an Order By
        /// </summary>
        /// <param name="orderBy"></param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public override BaseMultiset ProcessOrderBy(OrderBy orderBy, SparqlEvaluationContext context)
        {
            return ExplainAndEvaluate<OrderBy>(orderBy, context, base.ProcessOrderBy);
        }

        /// <summary>
        /// Processes a Property Path
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        /// <returns></returns>
        public override BaseMultiset ProcessPropertyPath(PropertyPath path, SparqlEvaluationContext context)
        {
            return ExplainAndEvaluate<PropertyPath>(path, context, base.ProcessPropertyPath);
        }

        /// <summary>
        /// Processes a Reduced modifier
        /// </summary>
        /// <param name="reduced">Reduced modifier</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public override BaseMultiset ProcessReduced(Reduced reduced, SparqlEvaluationContext context)
        {
            return ExplainAndEvaluate<Reduced>(reduced, context, base.ProcessReduced);
        }

        /// <summary>
        /// Processes a Select
        /// </summary>
        /// <param name="select">Select</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public override BaseMultiset ProcessSelect(Select select, SparqlEvaluationContext context)
        {
            return ExplainAndEvaluate<Select>(select, context, base.ProcessSelect);
        }

        /// <summary>
        /// Processes a Select Distinct Graphs
        /// </summary>
        /// <param name="selDistGraphs">Select Distinct Graphs</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public override BaseMultiset ProcessSelectDistinctGraphs(SelectDistinctGraphs selDistGraphs, SparqlEvaluationContext context)
        {
            return ExplainAndEvaluate<SelectDistinctGraphs>(selDistGraphs, context, base.ProcessSelectDistinctGraphs);
        }

        /// <summary>
        /// Processes a Service
        /// </summary>
        /// <param name="service">Service</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public override BaseMultiset ProcessService(Service service, SparqlEvaluationContext context)
        {
            return ExplainAndEvaluate<Service>(service, context, base.ProcessService);
        }

        /// <summary>
        /// Processes a Slice modifier
        /// </summary>
        /// <param name="slice">Slice modifier</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public override BaseMultiset ProcessSlice(Slice slice, SparqlEvaluationContext context)
        {
            return ExplainAndEvaluate<Slice>(slice, context, base.ProcessSlice);
        }

        /// <summary>
        /// Processes a Subquery
        /// </summary>
        /// <param name="subquery">Subquery</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        /// <returns></returns>
        public override BaseMultiset ProcessSubQuery(SubQuery subquery, SparqlEvaluationContext context)
        {
            return ExplainAndEvaluate<SubQuery>(subquery, context, base.ProcessSubQuery);
        }

        /// <summary>
        /// Processes a Union
        /// </summary>
        /// <param name="union">Union</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public override BaseMultiset ProcessUnion(IUnion union, SparqlEvaluationContext context)
        {
            return ExplainAndEvaluate<IUnion>(union, context, base.ProcessUnion);
        }

        /// <summary>
        /// Processes a Unknown Operator
        /// </summary>
        /// <param name="algebra">Unknown Operator</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public override BaseMultiset ProcessUnknownOperator(ISparqlAlgebra algebra, SparqlEvaluationContext context)
        {
            return ExplainAndEvaluate<ISparqlAlgebra>(algebra, context, base.ProcessUnknownOperator);
        }

        /// <summary>
        /// Processes a Zero Length Path
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        /// <returns></returns>
        public override BaseMultiset ProcessZeroLengthPath(ZeroLengthPath path, SparqlEvaluationContext context)
        {
            return ExplainAndEvaluate<ZeroLengthPath>(path, context, base.ProcessZeroLengthPath);
        }

        /// <summary>
        /// Processes a Zero or More Path
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        /// <returns></returns>
        public override BaseMultiset ProcessZeroOrMorePath(ZeroOrMorePath path, SparqlEvaluationContext context)
        {
            return ExplainAndEvaluate<ZeroOrMorePath>(path, context, base.ProcessZeroOrMorePath);
        }
    }

    class ExplainDataset 
        : WrapperDataset
    {
        public ExplainDataset(ISparqlDataset dataset) 
            : base(dataset) {}

        public ExplainQueryProcessor Processor { get; set; }

        public override void SetActiveGraph(Uri graphUri)
        {
            if (Processor != null)
            {
                if (Processor.HasFlag(ExplanationLevel.AnalyseNamedGraphs))
                {
                    Processor.PrintExplanations("Switching to named graph " + graphUri.ToSafeString());
                }
            }
            base.SetActiveGraph(graphUri);
        }

        public override void SetActiveGraph(IEnumerable<Uri> graphUris)
        {
            IList<Uri> gs = graphUris as IList<Uri> ?? graphUris.ToList();
            if (Processor != null)
            {
                if (Processor.HasFlag(ExplanationLevel.AnalyseNamedGraphs))
                {
                    Processor.PrintExplanations("Switching to named graph as merge of the following graphs:");
                    foreach (Uri graphUri in gs)
                    {
                        Processor.PrintExplanations(graphUri.ToSafeString());
                        
                    }
                }
            }
            base.SetActiveGraph(gs);
        }
    }
}