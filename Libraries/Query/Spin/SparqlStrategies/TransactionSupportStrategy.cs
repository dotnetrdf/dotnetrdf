using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Parsing.Tokens;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Functions.Sparql.Set;
using VDS.RDF.Query.Expressions.Functions.Sparql.String;
using VDS.RDF.Query.Expressions.Primary;
using VDS.RDF.Query.Filters;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Query.Spin.Core;
using VDS.RDF.Query.Spin.Core.Runtime;
using VDS.RDF.Query.Spin.Core.Transactions;
using VDS.RDF.Query.Spin.Utility;

namespace VDS.RDF.Query.Spin.SparqlStrategies
{
    // TODO this should be truly bound to the transaction log since it must be consistent accross the underlying store.
    // TODO make the transaction log a factory for this class
    // TODO DO NOT FORGET we need to maintain a CommandUnit additions/removals as well as global transaction assertions/removals as well as snapshot isolation for other transactions on non-transactional stores
    //      => snapshot isolation may be simpler maintained by duplicating graphs but there is a non-negligible volumetry issue doing this.
    //      => envisionned as a first solution we maintain both distinct snapshot and uncommitted changes
    //      => other solution : at first transaction's write, we force a refresh for the snapshot and we maintain here read/write "locks" that rollback the transaction whenever committed changes conflict with uncommitted changes => difficult
    // TODO check which performances are best between UNION and MINUS vs FILTER NOT EXISTS || FILTER EXISTS vs nested (FILTER NOT EXISTS)
    // TODO DO NOT FORGET to be consistent with the strategy : that this class must also handle transactions' Commit/Rollback events
    public sealed class TransactionSupportStrategy
        : BaseSparqlRewriteStrategy
    {
        private static String NON_CONFLICT_GRAPH_PREFIX = Guid.NewGuid().ToString().Substring(0, 4) + "_";

        private int __substitutionIndex = 0;

        private Stack<List<IToken>> _defaultGraphTokens = new Stack<List<IToken>>();
        private Stack<List<IToken>> _namedGraphTokens = new Stack<List<IToken>>();

        public TransactionSupportStrategy()
        {
        }

        /// <summary>
        /// Returns whether the strategy is required for the underlying storage
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override bool IsRequiredBy(Connection context)
        {
#if DEBUG
            return true;
#else
            return (context.UnderlyingStorage is ITransactionalStore);
#endif
        }

        internal override void Rewrite(SparqlCommandUnit command)
        {
            if (command.CommandType == SparqlCommandType.SparqlQuery)
            {
                RewriteQuery(command, command.Query);
            }
        }

        private void RewriteQuery(SparqlCommandUnit command, SparqlQuery query)
        {
            query.RootGraphPattern = RewriteWherePattern(command, query.RootGraphPattern);
        }

        /// <summary>
        /// Expands a graph pattern for simulated transactional support
        /// </summary>
        /// <example>
        /// 
        /// </example> 
        /// <param name="gp">The graph pattern to expand</param>
        /// <returns>the expanded graph pattern</returns>
        private GraphPattern RewriteWherePattern(SparqlCommandUnit command, GraphPattern gp)
        {
            if (!IsRequiredBy(command.Connection)) return gp;
            if (gp.IsService) return gp;
            if (_defaultGraphTokens.Count == 0)
            {
                _defaultGraphTokens.Push(new List<IToken>());
                _namedGraphTokens.Push(new List<IToken>());
            }
            GraphPattern output;
            if (gp.IsGraph || gp.IsSubQuery || !gp.HasModifier)
            { // If gp is neither a GraphGraphPattern nor a Bgp we use a shallow copy of gp and add gp rewritten childGraphPattern to it
                output = new GraphPattern();
                foreach (ITriplePattern pattern in gp.TriplePatterns.ToList())
                {
                    switch (pattern.PatternType)
                    {
                        case TriplePatternType.SubQuery:
                            // Provide correct graph restriction filter by re-initialize graph Tokens for the subquery;
                            _defaultGraphTokens.Push(new List<IToken>());
                            _namedGraphTokens.Push(new List<IToken>());
                            SparqlQuery subQuery = ((SubQueryPattern)pattern).SubQuery;
                            RewriteQuery(command, subQuery);
                            output.AddTriplePattern(pattern);
                            _defaultGraphTokens.Pop();
                            _namedGraphTokens.Pop();
                            break;

                        case TriplePatternType.Match:
                            IToken activeGraph = gp.ActiveGraph;
                            // Check wether the pattern use the default graph
                            if (activeGraph == null)
                            {
                                activeGraph = new VariableToken("?" + NON_CONFLICT_GRAPH_PREFIX + "g" + __substitutionIndex++.ToString(), 0, 0, 0);
                            }
                            // Assigns mappings for the graph additions and removals
                            IToken concurrentAssertionsGraph, concurrentRemovalsGraph;
                            IToken pendingAssertionsGraph=null, pendingRemovalsGraph=null;
                            switch (activeGraph.TokenType)
                            {
                                case Token.VARIABLE:
                                    AddGraphBindings(command, output, gp, (VariableToken)activeGraph, out concurrentAssertionsGraph, out concurrentRemovalsGraph, out pendingAssertionsGraph, out pendingRemovalsGraph);
                                    break;
                                case Token.URI:
                                    String graphUri = Uri.EscapeDataString(gp.GraphSpecifier.Value.Substring(1, gp.GraphSpecifier.Value.Length - 2));
                                    // TODO replace the TransactionLog.URI_PREFIX + "" by the current connection transaction id like done for the SparqlCommandUnitClass
                                    concurrentAssertionsGraph = new UriToken("<" + TransactionLog.URI_PREFIX + "" + TransactionLog.URI_INFIX_REMOVALS + Uri.EscapeDataString(graphUri) + ">", 0, 0, 0);
                                    concurrentRemovalsGraph = new UriToken("<" + TransactionLog.URI_PREFIX + "" + TransactionLog.URI_INFIX_ADDITIONS + graphUri + ">", 0, 0, 0);
                                    break;
                                default:
                                    throw new ArgumentException("Invalid token. Expected a graph IRI or variable");
                            }
                            // Create the replacement pattern
                            GraphPattern bgp = ((TriplePattern)pattern).AsGraphPattern();
                            GraphPattern concurrentRemovalsGp = new GraphPattern();
                            concurrentRemovalsGp.AddFilter(new BoundFilter(new VariableTerm(concurrentAssertionsGraph.Value.Substring(1))));
                            concurrentRemovalsGp.AddGraphPattern(bgp.WithinGraph(concurrentAssertionsGraph));
                            if (pendingAssertionsGraph != null)
                            {
                                GraphPattern pendingAdditionsGp = bgp.WithinGraph(pendingAssertionsGraph);
                                pendingAdditionsGp.AddFilter(new BoundFilter(new VariableTerm(pendingAssertionsGraph.Value.Substring(1))));
                                concurrentRemovalsGp.AddGraphPattern(pendingAdditionsGp.WithinMinus());
                            }
                            concurrentRemovalsGp = concurrentRemovalsGp.WithinMinus();

                            GraphPattern trimmedSourceGp = bgp.WithinGraph(activeGraph);
                            trimmedSourceGp.AddGraphPattern(concurrentRemovalsGp);
                            GraphPattern concurrentAssertionsGp = new GraphPattern();
                            concurrentAssertionsGp.AddFilter(new BoundFilter(new VariableTerm(concurrentRemovalsGraph.Value.Substring(1))));
                            concurrentAssertionsGp.AddGraphPattern(bgp.WithinGraph(concurrentRemovalsGraph));
                            if (pendingAssertionsGraph != null)
                            {
                                GraphPattern pendingRemovalsGp = bgp.WithinGraph(pendingRemovalsGraph);
                                pendingRemovalsGp.AddFilter(new BoundFilter(new VariableTerm(pendingRemovalsGraph.Value.Substring(1))));
                                concurrentAssertionsGp.AddGraphPattern(pendingRemovalsGp.WithinMinus());
                            }

                            List<GraphPattern> unions = new List<GraphPattern>() { trimmedSourceGp, concurrentAssertionsGp };
                            output.AddGraphPattern(unions.ToUnionGraphPattern());
                            break;

                        case TriplePatternType.Path:
                        // TODO split the path into it's components; depending on the nature of the pass, add a pre-processing command to the current set
                        case TriplePatternType.PropertyFunction:
                        // TODO check whether the property function requires SPARQL patterns or leave that to the evaluation process ?
                        default:
                            output.AddTriplePattern(pattern);
                            break;

                    }
                }
            }
            else
            {
                // Makes a shallow copy of the graphPattern (ie, only copy flags)
                output = gp.Clone(false);
            }
            // Rewrites the child graphPatterns
            foreach (GraphPattern cgp in gp.ChildGraphPatterns)
            {
                output.AddGraphPattern(RewriteWherePattern(command, cgp));
            }
            return output;
        }

        private GraphPattern RewriteUpdateTemplate(SparqlCommandUnit command, GraphPattern template, bool forInsert)
        {
            if (!IsRequiredBy(command.Connection)) return template;
            if (template.ActiveGraph == null) throw new NotSupportedException("Cannot currently write to the default graph.");
            GraphPattern output = template;
            if (template.IsGraph)
            {
                // TODO check whether we can use variables that map to the default graph
                if (template.GraphSpecifier is VariableToken && _defaultGraphTokens.Peek().Contains(template.GraphSpecifier)) throw new NotSupportedException("Cannot currently write to the default graph.");
                // TODO distinguish between INSERT and DELETE templates
                String outputGraph = forInsert ? command.GetAssertionsGraph(template.ActiveGraph) : command.GetRemovalsGraph(template.ActiveGraph);
                switch (template.ActiveGraph.TokenType)
                {
                    case Token.VARIABLE:
                        output = template.WithinGraph(outputGraph);
                        break;
                    case Token.URI:
                        output = template.WithinGraph(UriFactory.Create(outputGraph));
                        break;
                    default:
                        throw new ArgumentException("Invalid token. Expected a graph IRI or variable");
                }
            }
            return output;
        }

        #region Utilities

        private void AddGraphBindings(SparqlCommandUnit command, GraphPattern output, GraphPattern gp, VariableToken graphToken, out IToken concurrentAssertionsGraph, out IToken concurrentRemovalsGraph, out IToken pendingAssertionsGraph, out IToken pendingRemovalsGraph)
        {
            String varName = graphToken.Value.Substring(1);
            ISparqlFilter newGraphBindingFilter = null;
            if (gp.IsGraph)
            {
                _namedGraphTokens.Peek().Add(graphToken);
                IEnumerable<ISparqlExpression> targetGraphs = command.NamedGraphs.DefaultIfEmpty(UriFactory.Create("tag:null")).Select(u => new ConstantTerm(RDFHelper.CreateUriNode(u)));
                newGraphBindingFilter = new UnaryExpressionFilter(new InFunction(new VariableTerm(varName), targetGraphs));
            }
            else if (!_namedGraphTokens.Peek().Contains(graphToken))
            {
                _defaultGraphTokens.Peek().Add(graphToken);
                IEnumerable<ISparqlExpression> targetGraphs = command.DefaultGraphs.DefaultIfEmpty(UriFactory.Create("tag:null")).Select(u => new ConstantTerm(RDFHelper.CreateUriNode(u)));
                newGraphBindingFilter = new UnaryExpressionFilter(new InFunction(new VariableTerm(varName), targetGraphs));
            }
            bool isWriting = true /*command.Connection.IsWriting*/;
            concurrentAssertionsGraph = new VariableToken("?concurrentA_" + varName, 0, 0, 0);
            concurrentRemovalsGraph = new VariableToken("?concurrentR_" + varName, 0, 0, 0);
            pendingAssertionsGraph = !isWriting  ? null : new VariableToken("?pendingA_" + varName, 0, 0, 0);
            pendingRemovalsGraph = !isWriting ? null : new VariableToken("?pendingR_" + varName, 0, 0, 0);
            if (newGraphBindingFilter != null)
            {
                GraphPattern graphBindingsPattern = new GraphPattern();
                GraphPattern graphBinding = new TriplePattern(new VariablePattern(concurrentAssertionsGraph.Value.Substring(1)), new NodeMatchPattern(TransactionLog.ConcurrentAssertionsForGraph), new VariablePattern(varName)).AsGraphPattern();
                graphBinding.AddTriplePattern(new TriplePattern(new VariablePattern(concurrentAssertionsGraph.Value.Substring(1)), new NodeMatchPattern(DCTerms.PropertyIsRequiredBy), new NodeMatchPattern(RDFHelper.CreateUriNode(command.Connection.Uri))));
                graphBindingsPattern.AddGraphPattern(graphBinding.WithinOptional());
                graphBinding = new TriplePattern(new VariablePattern(concurrentRemovalsGraph.Value.Substring(1)), new NodeMatchPattern(TransactionLog.ConcurrentRemovalsForGraph), new VariablePattern(varName)).AsGraphPattern();
                graphBinding.AddTriplePattern(new TriplePattern(new VariablePattern(concurrentAssertionsGraph.Value.Substring(1)), new NodeMatchPattern(DCTerms.PropertyIsRequiredBy), new NodeMatchPattern(RDFHelper.CreateUriNode(command.Connection.Uri))));
                graphBindingsPattern.AddGraphPattern(graphBinding.WithinOptional());
                if (isWriting)
                {
                    graphBinding =new TriplePattern(new VariablePattern(pendingAssertionsGraph.Value.Substring(1)), new NodeMatchPattern(TransactionLog.PendingAssertionsForGraph), new VariablePattern(varName)).AsGraphPattern();
                    graphBinding.AddTriplePattern(new TriplePattern(new VariablePattern(concurrentAssertionsGraph.Value.Substring(1)), new NodeMatchPattern(DCTerms.PropertyIsRequiredBy), new NodeMatchPattern(RDFHelper.CreateUriNode(command.Connection.Uri))));
                    graphBindingsPattern.AddGraphPattern(graphBinding.WithinOptional());
                    graphBinding =new TriplePattern(new VariablePattern(pendingRemovalsGraph.Value.Substring(1)), new NodeMatchPattern(TransactionLog.PendingRemovalsForGraph), new VariablePattern(varName)).AsGraphPattern();
                    graphBinding.AddTriplePattern(new TriplePattern(new VariablePattern(concurrentAssertionsGraph.Value.Substring(1)), new NodeMatchPattern(DCTerms.PropertyIsRequiredBy), new NodeMatchPattern(RDFHelper.CreateUriNode(command.Connection.Uri))));
                    graphBindingsPattern.AddGraphPattern(graphBinding.WithinOptional());
                }
                graphBindingsPattern.AddFilter(newGraphBindingFilter);
                
                // In a SPARQL Update we need to track each individual command's inserted/deleted triples (for either SPIN post-processing or snapshot isolation for other transactions)
                // TODO find a way to unify the expression with the one used in SparqlCommandUnit class but for now there is no possiblity of subclassing so we may be safe
                if (command.CommandType == SparqlCommandType.SparqlUpdate)
                {
                    graphBindingsPattern.AddAssignment(new BindPattern(command.GetRemovalsGraph(graphToken), new ConcatFunction(new List<ISparqlExpression>() { new ConstantTerm(command.UnitRemovalsPrefix.ToLiteral(RDFHelper.nodeFactory)), new EncodeForUriFunction(new StrFunction(new VariableTerm(varName))) })));
                    graphBindingsPattern.AddAssignment(new BindPattern(command.GetAssertionsGraph(graphToken), new ConcatFunction(new List<ISparqlExpression>() { new ConstantTerm(command.UnitAssertionsPrefix.ToLiteral(RDFHelper.nodeFactory)), new EncodeForUriFunction(new StrFunction(new VariableTerm(varName))) })));
                }
                output.AddGraphPattern(graphBindingsPattern.WithinGraph(TransactionLog.TRANSACTION_LOG_URI));
            }
        }


        #endregion
    }
}
