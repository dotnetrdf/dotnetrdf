using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Parsing.Tokens;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Functions.Sparql.Set;
using VDS.RDF.Query.Expressions.Primary;
using VDS.RDF.Query.Filters;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Query.Spin.Core.Transactions;
using VDS.RDF.Query.Spin.Utility;

namespace VDS.RDF.Query.Spin.SparqlStrategies
{
    // TODO this should be truly bound to the transaction log since it must be consistent accross the underlying store.
    // TODO make the transaction log a factory for this...
    // TODO we need connection and query context for correct rewriting => try to determine how to make this class reusable for several queries over any context
    // TODO should we maintain here read "locks" for proper and efficient isolation ?
    public class TransactionSupportStrategy
        : BaseSparqlRewriteStrategy
    {

        private int __patternCount = 0;

        private List<Uri> _defaultGraphUris = new List<Uri>();
        private List<Uri> _namedGraphUris = new List<Uri>();

        private List<IToken> _defaultGraphTokens = new List<IToken>();
        private List<IToken> _namedGraphTokens = new List<IToken>();

        public TransactionSupportStrategy()
        {
        }

        /// <summary>
        /// Returns whether rewriting is required to work with the underlying storage
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

        // TODO we need to know whether the token (if variable) refers to a named or defaulted graph
        private void BuildTransactionGraphsMapping(IToken graphToken)
        {

        }


        /// <summary>
        /// Expands a graph pattern for simulated transactional support
        /// </summary>
        /// <example>
        /// 
        /// </example> 
        /// <param name="gp">The graph pattern to expand</param>
        /// <returns>the expanded graph pattern</returns>
        // TODO check whether it's best to output a new pattern or replace the current one
        public GraphPattern RewriteWherePattern(Connection context, GraphPattern gp)
        {
            if (!IsRequiredBy(context)) return gp;
            GraphPattern output;
            if (gp.IsGraph || !gp.HasModifier)
            { // If gp is neither a GraphGraphPattern nor a Bgp we use a shallow copy of gp and add gp rewritten childGraphPattern to it
                output = new GraphPattern();
                foreach (ITriplePattern pattern in gp.TriplePatterns.ToList())
                {
                    switch (pattern.PatternType)
                    {
                        case TriplePatternType.Match:
                            IToken activeGraph = gp.ActiveGraph;
                            if (activeGraph == null)
                            {
                                activeGraph = new VariableToken("?dfltG" + __patternCount++.ToString(), 0, 0, 0);
                            }

                            IToken removalsactiveGraph = null, additionsactiveGraph = null; // TODO replace this with a struct bound to current's query graphs
                            if (activeGraph is VariableToken)
                            {// TODO relocate this in a method that returns a struct activeGraph/removalsGraph/additionsGraph
                                String varName = activeGraph.Value.Substring(1);
                                // Handles mapping of the graph variables with the query's DEFAULT or NAMED graphs
                                // TODO add the pattern for binding rems_ and adds_ graph variables with the activeGraph
                                if (gp.IsGraph)
                                {
                                    _namedGraphTokens.Add(activeGraph);
                                    IEnumerable<ISparqlExpression> targetGraphs = _namedGraphUris.DefaultIfEmpty(UriFactory.Create("tag:null")).Select(u => new ConstantTerm(RDFHelper.CreateUriNode(u)));
                                    output.AddFilter(new UnaryExpressionFilter(new InFunction(new VariableTerm(varName), targetGraphs)));
                                }
                                else if (!_namedGraphTokens.Contains(activeGraph))
                                {
                                    _defaultGraphTokens.Add(activeGraph);
                                    IEnumerable<ISparqlExpression> targetGraphs = _defaultGraphUris.DefaultIfEmpty(UriFactory.Create("tag:null")).Select(u => new ConstantTerm(RDFHelper.CreateUriNode(u)));
                                    output.AddFilter(new UnaryExpressionFilter(new InFunction(new VariableTerm(varName), targetGraphs)));
                                }
                                // TODO replace the variables names with correct non-conflicting names
                                removalsactiveGraph = new VariableToken("?rems_" + activeGraph.Value.Substring(1), 0, 0, 0);
                                additionsactiveGraph = new VariableToken("?adds_" + activeGraph.Value.Substring(1), 0, 0, 0);

                            }
                            else
                            {
                                // TODO replace the TransactionLog.URI_PREFIX + "" by the current connection transaction id
                                removalsactiveGraph = new UriToken("<" + TransactionLog.URI_PREFIX + "" + TransactionLog.URI_INFIX_REMOVALS + Uri.EscapeDataString(gp.GraphSpecifier.Value) + ">", 0, 0, 0);
                                additionsactiveGraph = new UriToken("<" + TransactionLog.URI_PREFIX + "" + TransactionLog.URI_INFIX_ADDITIONS + Uri.EscapeDataString(gp.GraphSpecifier.Value) + ">", 0, 0, 0);
                            }

                            GraphPattern bgp = ((TriplePattern)pattern).ToGraphPattern();
                            GraphPattern removalsGp = bgp.AsGraph(removalsactiveGraph).AsMinus();

                            GraphPattern trimmedSourceGp = bgp.AsGraph(activeGraph);
                            trimmedSourceGp.AddGraphPattern(removalsGp);
                            GraphPattern additionsGp = bgp.AsGraph(additionsactiveGraph);

                            List<GraphPattern> unions = new List<GraphPattern>() { trimmedSourceGp, additionsGp };
                            output.AddGraphPattern(unions.AsUnion());
                            break;
                        default:
                            output.AddTriplePattern(pattern);
                            break;

                    }
                }
            }
            else
            {
                output = gp.Clone(false);
            }
            foreach (GraphPattern cgp in gp.ChildGraphPatterns)
            {
                output.AddGraphPattern(RewriteWherePattern(context, cgp));
            }
            return output;
        }

        // <remarks>
        // Tracking of underlying updates should be performed by another component.
        //</remarks>
        public GraphPattern RewriteUpdateTemplate(Connection context, GraphPattern template)
        {
            if (!IsRequiredBy(context)) return template;
            return template;
        }

    }
}
