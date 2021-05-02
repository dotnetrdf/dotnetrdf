using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;

namespace dotNetRDF.Query.Behemoth
{
    public class BehemothBuilder : ISparqlQueryAlgebraProcessor<IEvaluationBlock, BehemothEvaluationContext>
    {
        public IEvaluationBlock ProcessAlgebra(ISparqlAlgebra algebra, BehemothEvaluationContext context)
        {
            if (algebra is Ask)
            {
                return ProcessAsk((Ask)algebra, context);
            }
            else if (algebra is IBgp)
            {
                return ProcessBgp((IBgp)algebra, context);
            }
            else if (algebra is VDS.RDF.Query.Algebra.Bindings bindings)
            {
                return ProcessBindings(bindings, context);
            }
            else if (algebra is Distinct)
            {
                return ProcessDistinct((Distinct)algebra, context);
            }
            else if (algebra is Extend)
            {
                return ProcessExtend((Extend)algebra, context);
            }
            else if (algebra is IExistsJoin)
            {
                return ProcessExistsJoin((IExistsJoin)algebra, context);
            }
            else if (algebra is IFilter)
            {
                return ProcessFilter((IFilter)algebra, context);
            }
            else if (algebra is Graph)
            {
                return ProcessGraph((Graph)algebra, context);
            }
            else if (algebra is GroupBy)
            {
                return ProcessGroupBy((GroupBy)algebra, context);
            }
            else if (algebra is Having)
            {
                return ProcessHaving((Having)algebra, context);
            }
            else if (algebra is IJoin)
            {
                return ProcessJoin((IJoin)algebra, context);
            }
            else if (algebra is ILeftJoin)
            {
                return ProcessLeftJoin((ILeftJoin)algebra, context);
            }
            else if (algebra is IMinus)
            {
                return ProcessMinus((IMinus)algebra, context);
            }
            else if (algebra is NegatedPropertySet)
            {
                return ProcessNegatedPropertySet((NegatedPropertySet)algebra, context);
            }
            else if (algebra is NullOperator)
            {
                return ProcessNullOperator((NullOperator)algebra, context);
            }
            else if (algebra is OneOrMorePath)
            {
                return ProcessOneOrMorePath((OneOrMorePath)algebra, context);
            }
            else if (algebra is OrderBy)
            {
                return ProcessOrderBy((OrderBy)algebra, context);
            }
            else if (algebra is PropertyPath)
            {
                return ProcessPropertyPath((PropertyPath)algebra, context);
            }
            else if (algebra is Reduced)
            {
                return ProcessReduced((Reduced)algebra, context);
            }
            else if (algebra is Select)
            {
                return ProcessSelect((Select)algebra, context);
            }
            else if (algebra is SelectDistinctGraphs)
            {
                return ProcessSelectDistinctGraphs((SelectDistinctGraphs)algebra, context);
            }
            else if (algebra is Service)
            {
                return ProcessService((Service)algebra, context);
            }
            else if (algebra is Slice)
            {
                return ProcessSlice((Slice)algebra, context);
            }
            else if (algebra is SubQuery)
            {
                return ProcessSubQuery((SubQuery)algebra, context);
            }
            else if (algebra is IUnion)
            {
                return ProcessUnion((IUnion)algebra, context);
            }
            else if (algebra is ZeroLengthPath)
            {
                return ProcessZeroLengthPath((ZeroLengthPath)algebra, context);
            }
            else if (algebra is ZeroOrMorePath)
            {
                return ProcessZeroOrMorePath((ZeroOrMorePath)algebra, context);
            }
            else
            {
                // Unknown Algebra
                return ProcessUnknownOperator(algebra, context);
            }
        }

        public IEvaluationBlock ProcessAsk(Ask ask, BehemothEvaluationContext context)
        {
            return new AskBlock(ProcessAlgebra(ask.InnerAlgebra, context));
        }

        public IEvaluationBlock ProcessBgp(IBgp bgp, BehemothEvaluationContext context)
        {
            return new BgpBlock(bgp, context);
        }

        public IEvaluationBlock ProcessBindings(VDS.RDF.Query.Algebra.Bindings b, BehemothEvaluationContext context)
        {
            return new BindingsBlock(b.BindingsPattern, context);
        }

        public IEvaluationBlock ProcessDistinct(Distinct distinct, BehemothEvaluationContext context)
        {
            throw new NotImplementedException();
        }

        public IEvaluationBlock ProcessExistsJoin(IExistsJoin existsJoin, BehemothEvaluationContext context)
        {
            throw new NotImplementedException();
        }

        public IEvaluationBlock ProcessExtend(Extend extend, BehemothEvaluationContext context)
        {
            throw new NotImplementedException();
        }

        public IEvaluationBlock ProcessFilter(IFilter filter, BehemothEvaluationContext context)
        {
            throw new NotImplementedException();
        }

        public IEvaluationBlock ProcessGraph(Graph graph, BehemothEvaluationContext context)
        {
            throw new NotImplementedException();
        }

        public IEvaluationBlock ProcessGroupBy(GroupBy groupBy, BehemothEvaluationContext context)
        {
            throw new NotImplementedException();
        }

        public IEvaluationBlock ProcessHaving(Having having, BehemothEvaluationContext context)
        {
            throw new NotImplementedException();
        }

        public IEvaluationBlock ProcessJoin(IJoin @join, BehemothEvaluationContext context)
        {
            var joinVars = join.Lhs.Variables.Intersect(join.Rhs.Variables).ToList();
            if (joinVars.Any())
            {
                return new JoinBlock(ProcessAlgebra(join.Lhs, context), ProcessAlgebra(join.Rhs, context), joinVars);
            }

            throw new NotImplementedException();
        }

        public IEvaluationBlock ProcessLeftJoin(ILeftJoin leftJoin, BehemothEvaluationContext context)
        {
            var joinVars = leftJoin.Lhs.Variables.Intersect(leftJoin.Rhs.Variables).ToList();
            if (joinVars.Any())
            {
                return new LeftJoinBlock(ProcessAlgebra(leftJoin.Lhs, context), ProcessAlgebra(leftJoin.Rhs, context),
                    joinVars);
            }

            throw new NotImplementedException();
        }

        public IEvaluationBlock ProcessMinus(IMinus minus, BehemothEvaluationContext context)
        {
            throw new NotImplementedException();
        }

        public IEvaluationBlock ProcessNegatedPropertySet(NegatedPropertySet negPropSet, BehemothEvaluationContext context)
        {
            throw new NotImplementedException();
        }

        public IEvaluationBlock ProcessNullOperator(NullOperator nullOp, BehemothEvaluationContext context)
        {
            throw new NotImplementedException();
        }

        public IEvaluationBlock ProcessOneOrMorePath(OneOrMorePath path, BehemothEvaluationContext context)
        {
            throw new NotImplementedException();
        }

        public IEvaluationBlock ProcessOrderBy(OrderBy orderBy, BehemothEvaluationContext context)
        {
            throw new NotImplementedException();
        }

        public IEvaluationBlock ProcessPropertyPath(PropertyPath path, BehemothEvaluationContext context)
        {
            throw new NotImplementedException();
        }

        public IEvaluationBlock ProcessReduced(Reduced reduced, BehemothEvaluationContext context)
        {
            throw new NotImplementedException();
        }

        public IEvaluationBlock ProcessSelect(Select @select, BehemothEvaluationContext context)
        {
            return new SelectBlock(ProcessAlgebra(@select.InnerAlgebra, context), @select.IsSelectAll,
                @select.SparqlVariables.Select(x => x.Name).ToList());
        }

        public IEvaluationBlock ProcessSelectDistinctGraphs(SelectDistinctGraphs selDistGraphs, BehemothEvaluationContext context)
        {
            throw new NotImplementedException();
        }

        public IEvaluationBlock ProcessService(Service service, BehemothEvaluationContext context)
        {
            throw new NotImplementedException();
        }

        public IEvaluationBlock ProcessSlice(Slice slice, BehemothEvaluationContext context)
        {
            throw new NotImplementedException();
        }

        public IEvaluationBlock ProcessSubQuery(SubQuery subquery, BehemothEvaluationContext context)
        {
            throw new NotImplementedException();
        }

        public IEvaluationBlock ProcessUnion(IUnion union, BehemothEvaluationContext context)
        {
            throw new NotImplementedException();
        }

        public IEvaluationBlock ProcessUnknownOperator(ISparqlAlgebra algebra, BehemothEvaluationContext context)
        {
            throw new NotImplementedException();
        }

        public IEvaluationBlock ProcessZeroLengthPath(ZeroLengthPath path, BehemothEvaluationContext context)
        {
            throw new NotImplementedException();
        }

        public IEvaluationBlock ProcessZeroOrMorePath(ZeroOrMorePath path, BehemothEvaluationContext context)
        {
            throw new NotImplementedException();
        }
    }
}
