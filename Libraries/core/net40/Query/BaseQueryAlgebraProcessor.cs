using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Algebra;

namespace VDS.RDF.Query
{
    public abstract class BaseQueryAlgebraProcessor<TResult, TContext>
        : ISparqlQueryAlgebraProcessor<TResult, TContext>
    {
        /// <summary>
        /// Processes SPARQL Algebra
        /// </summary>
        /// <param name="algebra">Algebra</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public TResult ProcessAlgebra(ISparqlAlgebra algebra, TContext context)
        {
            if (algebra is Ask)
            {
                return this.ProcessAsk((Ask)algebra, context);
            }
            else if (algebra is IBgp)
            {
                return this.ProcessBgp((IBgp)algebra, context);
            }
            else if (algebra is Bindings)
            {
                return this.ProcessBindings((Bindings)algebra, context);
            }
            else if (algebra is Distinct)
            {
                return this.ProcessDistinct((Distinct)algebra, context);
            }
            else if (algebra is Extend)
            {
                return this.ProcessExtend((Extend)algebra, context);
            }
            else if (algebra is IExistsJoin)
            {
                return this.ProcessExistsJoin((IExistsJoin)algebra, context);
            }
            else if (algebra is IFilter)
            {
                return this.ProcessFilter((IFilter)algebra, context);
            }
            else if (algebra is Algebra.Graph)
            {
                return this.ProcessGraph((Algebra.Graph)algebra, context);
            }
            else if (algebra is GroupBy)
            {
                return this.ProcessGroupBy((GroupBy)algebra, context);
            }
            else if (algebra is Having)
            {
                return this.ProcessHaving((Having)algebra, context);
            }
            else if (algebra is IJoin)
            {
                return this.ProcessJoin((IJoin)algebra, context);
            }
            else if (algebra is ILeftJoin)
            {
                return this.ProcessLeftJoin((ILeftJoin)algebra, context);
            }
            else if (algebra is IMinus)
            {
                return this.ProcessMinus((IMinus)algebra, context);
            }
            else if (algebra is NegatedPropertySet)
            {
                return this.ProcessNegatedPropertySet((NegatedPropertySet)algebra, context);
            }
            else if (algebra is NullOperator)
            {
                return this.ProcessNullOperator((NullOperator)algebra, context);
            }
            else if (algebra is OneOrMorePath)
            {
                return this.ProcessOneOrMorePath((OneOrMorePath)algebra, context);
            }
            else if (algebra is OrderBy)
            {
                return this.ProcessOrderBy((OrderBy)algebra, context);
            }
            else if (algebra is PropertyPath)
            {
                return this.ProcessPropertyPath((PropertyPath)algebra, context);
            }
            else if (algebra is Reduced)
            {
                return this.ProcessReduced((Reduced)algebra, context);
            }
            else if (algebra is Select)
            {
                return this.ProcessSelect((Select)algebra, context);
            }
            else if (algebra is SelectDistinctGraphs)
            {
                return this.ProcessSelectDistinctGraphs((SelectDistinctGraphs)algebra, context);
            }
            else if (algebra is Service)
            {
                return this.ProcessService((Service)algebra, context);
            }
            else if (algebra is Slice)
            {
                return this.ProcessSlice((Slice)algebra, context);
            }
            else if (algebra is SubQuery)
            {
                return this.ProcessSubQuery((SubQuery)algebra, context);
            }
            else if (algebra is IUnion)
            {
                return this.ProcessUnion((IUnion)algebra, context);
            }
            else if (algebra is ZeroLengthPath)
            {
                return this.ProcessZeroLengthPath((ZeroLengthPath)algebra, context);
            }
            else if (algebra is ZeroOrMorePath)
            {
                return this.ProcessZeroOrMorePath((ZeroOrMorePath)algebra, context);
            }
            else
            {
                //Unknown Algebra
                return this.ProcessUnknownOperator(algebra, context);
            }
        }

        public abstract TResult ProcessAsk(Ask ask, TContext context);

        public abstract TResult ProcessBgp(IBgp bgp, TContext context);

        public abstract TResult ProcessBindings(Bindings b, TContext context);

        public abstract TResult ProcessDistinct(Distinct distinct, TContext context);

        public abstract TResult ProcessExistsJoin(IExistsJoin existsJoin, TContext context);

        public abstract TResult ProcessExtend(Extend extend, TContext context);

        public abstract TResult ProcessFilter(IFilter filter, TContext context);

        public abstract TResult ProcessGraph(Algebra.Graph graph, TContext context);

        public abstract TResult ProcessGroupBy(GroupBy groupBy, TContext context);

        public abstract TResult ProcessHaving(Having having, TContext context);

        public abstract TResult ProcessJoin(IJoin join, TContext context);

        public abstract TResult ProcessLeftJoin(ILeftJoin leftJoin, TContext context);

        public abstract TResult ProcessMinus(IMinus minus, TContext context);

        public abstract TResult ProcessNegatedPropertySet(NegatedPropertySet negPropSet, TContext context);

        public abstract TResult ProcessNullOperator(NullOperator nullOp, TContext context);

        public abstract TResult ProcessOneOrMorePath(OneOrMorePath path, TContext context);

        public abstract TResult ProcessOrderBy(OrderBy orderBy, TContext context);

        public abstract TResult ProcessPropertyPath(PropertyPath path, TContext context);

        public abstract TResult ProcessReduced(Reduced reduced, TContext context);

        public abstract TResult ProcessSelect(Select select, TContext context);

        public abstract TResult ProcessSelectDistinctGraphs(SelectDistinctGraphs selDistGraphs, TContext context);

        public abstract TResult ProcessService(Service service, TContext context);

        public abstract TResult ProcessSlice(Slice slice, TContext context);
        
        public abstract TResult ProcessSubQuery(SubQuery subquery, TContext context);

        public abstract TResult ProcessUnion(IUnion union, TContext context);

        public abstract TResult ProcessUnknownOperator(ISparqlAlgebra algebra, TContext context);
 
        public abstract TResult ProcessZeroLengthPath(ZeroLengthPath path, TContext context);

        public abstract TResult ProcessZeroOrMorePath(ZeroOrMorePath path, TContext context);
  
    }
}
