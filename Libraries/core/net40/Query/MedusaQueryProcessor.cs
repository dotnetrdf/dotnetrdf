using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query
{
    public class MedusaQueryProcessor
        : BaseQueryAlgebraProcessor<IEnumerable<ISet>, ISet>
    {
        private ISparqlDataset _data;

        public MedusaQueryProcessor(ISparqlDataset dataset)
        {
            if (dataset == null) throw new ArgumentNullException("dataset", "Dataset cannot be null");
            this._data = dataset;
        }

        #region ISparqlQueryAlgebraProcessor<IEnumerable<ISet>,ISet> Members

        public override IEnumerable<ISet> ProcessAsk(Ask ask, ISet context)
        {
            return this.ProcessAlgebra(ask.InnerAlgebra, context).Take(1);
        }

        public override IEnumerable<ISet> ProcessBgp(IBgp bgp, ISet context)
        {
            if (bgp.PatternCount == 0)
            {
                return new Set().AsEnumerable();
            }
            else if (bgp.PatternCount == 1)
            {
                if (bgp.TriplePatterns.First() is IMatchTriplePattern)
                {
                    IMatchTriplePattern match = (IMatchTriplePattern)bgp.TriplePatterns.First();
                    IEnumerable<Triple> ts;// = match.GetTriples(context); //TODO: Need an overload which accepts a ISet
                    //return (from t in ts
                    //        where match.Accepts(context, t)); //TODO: Need an overload which accepts an ISet
                }
            }
            throw new NotImplementedException();
        }

        public override IEnumerable<ISet> ProcessBindings(Bindings b, ISet context)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<ISet> ProcessDistinct(Distinct distinct, ISet context)
        {
            return this.ProcessAlgebra(distinct.InnerAlgebra, context).Distinct();
        }

        public override IEnumerable<ISet> ProcessExistsJoin(IExistsJoin existsJoin, ISet context)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<ISet> ProcessExtend(Extend extend, ISet context)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<ISet> ProcessFilter(IFilter filter, ISet context)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<ISet> ProcessGraph(Algebra.Graph graph, ISet context)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<ISet> ProcessGroupBy(GroupBy groupBy, ISet context)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<ISet> ProcessHaving(Having having, ISet context)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<ISet> ProcessJoin(IJoin join, ISet context)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<ISet> ProcessLeftJoin(ILeftJoin leftJoin, ISet context)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<ISet> ProcessMinus(IMinus minus, ISet context)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<ISet> ProcessNegatedPropertySet(NegatedPropertySet negPropSet, ISet context)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<ISet> ProcessNullOperator(NullOperator nullOp, ISet context)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<ISet> ProcessOneOrMorePath(OneOrMorePath path, ISet context)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<ISet> ProcessOrderBy(OrderBy orderBy, ISet context)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<ISet> ProcessPropertyPath(PropertyPath path, ISet context)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<ISet> ProcessReduced(Reduced reduced, ISet context)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<ISet> ProcessSelect(Select select, ISet context)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<ISet> ProcessSelectDistinctGraphs(SelectDistinctGraphs selDistGraphs, ISet context)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<ISet> ProcessService(Service service, ISet context)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<ISet> ProcessSlice(Slice slice, ISet context)
        {
            if (slice.Limit == 0) return Enumerable.Empty<ISet>();

            IEnumerable<ISet> sets = this.ProcessAlgebra(slice.InnerAlgebra, context);
            if (slice.Offset > 0) sets = sets.Skip(slice.Offset);
            if (slice.Limit > 0) sets = sets.Take(slice.Limit);

            return sets;
        }

        public override IEnumerable<ISet> ProcessSubQuery(SubQuery subquery, ISet context)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<ISet> ProcessUnion(IUnion union, ISet context)
        {
            return this.ProcessAlgebra(union.Lhs, context).Concat(this.ProcessAlgebra(union.Rhs, context));
        }

        public override IEnumerable<ISet> ProcessUnknownOperator(ISparqlAlgebra algebra, ISet context)
        {
            throw new RdfQueryException("Unsupported algebra operator - " + algebra.GetType().AssemblyQualifiedName);
        }

        public override IEnumerable<ISet> ProcessZeroLengthPath(ZeroLengthPath path, ISet context)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<ISet> ProcessZeroOrMorePath(ZeroOrMorePath path, ISet context)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
