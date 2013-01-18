using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Query.Patterns;

/**
 * This is proof of concept code designed to show a very basic and bare bones outline of how a query engine
 * that is streaming rather than block based can be constructed.
 * 
 * Currently this POC supports the following:
 * - Empty BGPs
 * - BGPs with a single match triple pattern in
 * - Filter
 * - Extend
 * - Union
 * - Distinct
 * - Slice i.e. LIMIT and OFFSET
 * - Join
 * 
 * Much of the POC is very hacky because of the various supporting APIs are quite closely tied to the Leviathan
 * engine.  Some APIs that it would be useful to change are as follows:
 * - Generally we want to move to a model where the query processor controls all evaluation i.e. not defer it
 *   to the ISparqlAlgebra classes.  Eventually they should all have their Evaluate() methods removed and that
 *   code migrated into LeviathanQueryProcessor.  This helps to have a cleaner separation between the structural
 *   representation of the algebra and the evaluation of it.
 * - Modify ISparqlExpression to operate directly on a ISet instance, may still need to provide some sort
 *   of Context object in order to support certain functions.  This context can be made a subset of the existing
 *   SparqlEvaluationContext
 * - Similarily modifying IMatchTriplePattern to allow retrieving triples based on an ISet or an IEnumerable<ISet>
 *   would help.  The better alternative may be to reimplement the functionality directly here
 **/

namespace VDS.RDF.Query
{
    /// <summary>
    /// An alternative query engine that is designed to be streaming and thus significantly more memory efficient
    /// </summary>
    public class MedusaQueryProcessor
        : BaseQueryAlgebraProcessor<IEnumerable<ISet>, ISet>, ISparqlQueryPatternProcessor<IEnumerable<ISet>, ISet>
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
                    return this.ProcessMatchPattern(match, context);
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
            //HACK: Once again we abuse the existing machinery for the sake of expidiency

            Func<ISet, ISet> tryExtend = (s =>
            {
                try
                {
                    SparqlEvaluationContext evalContext = new SparqlEvaluationContext(null, this._data);
                    evalContext.InputMultiset = new Multiset();
                    evalContext.InputMultiset.Add(s);

                    IValuedNode node = extend.AssignExpression.Evaluate(evalContext, s.ID);
                    s.Add(extend.VariableName, node);
                    return s;
                }
                catch (RdfQueryException)
                {
                    return s;
                }
            });

            return (from s in this.ProcessAlgebra(extend.InnerAlgebra, context)
                    select tryExtend(s));
        }

        public override IEnumerable<ISet> ProcessFilter(IFilter filter, ISet context)
        {
            //HACK: Here we are kinda abusing the existing machinery for the sake of proof of concept

            Func<ISet, bool> canAccept = (s =>
            {
                try
                {
                    SparqlEvaluationContext evalContext = new SparqlEvaluationContext(null, this._data);
                    evalContext.InputMultiset = new Multiset();
                    evalContext.InputMultiset.Add(s);
                    IValuedNode node = filter.SparqlFilter.Expression.Evaluate(evalContext, s.ID);
                    return node.AsSafeBoolean();
                }
                catch (RdfQueryException)
                {
                    return false;
                }
            });

            return (from s in this.ProcessAlgebra(filter.InnerAlgebra, context)
                    where canAccept(s)
                    select s);
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
            IEnumerable<ISet> lhs = this.ProcessAlgebra(join.Lhs, context);

            if (join.Lhs.Variables.IsDisjoint(join.Rhs.Variables))
            {
                IEnumerable<ISet> rhs = this.ProcessAlgebra(join.Rhs, context);

                //Do a product
                return (from x in lhs
                        from y in rhs
                        select x.Join(y));
            }
            else
            {
                //Do a join
                ISparqlAlgebra rhsAlgebra = join.Rhs;
                List<String> vars = join.Lhs.Variables.Where(v => join.Rhs.Variables.Contains(v)).ToList();
                return (from x in lhs
                        from y in this.ProcessAlgebra(rhsAlgebra, x)
                        where x.IsCompatibleWith(y, vars)
                        select x.Join(y));
            }
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

        #region ISparqlQueryPatternProcessor<IEnumerable<ISet>,ISet> Members

        public IEnumerable<ISet> ProcessTriplePattern(ITriplePattern pattern, ISet context)
        {
            switch (pattern.PatternType)
            {
                case TriplePatternType.BindAssignment:
                case TriplePatternType.LetAssignment:
                    return this.ProcessAssignmentPattern((IAssignmentPattern)pattern, context);
                case TriplePatternType.Filter:
                    return this.ProcessFilterPattern((IFilterPattern)pattern, context);
                case TriplePatternType.Match:
                    return this.ProcessMatchPattern((IMatchTriplePattern)pattern, context);
                case TriplePatternType.Path:
                    return this.ProcessPathPattern((IPropertyPathPattern)pattern, context);
                case TriplePatternType.PropertyFunction:
                    return this.ProcessFunctionPattern((IPropertyFunctionPattern)pattern, context);
                case TriplePatternType.SubQuery:
                    return this.ProcessSubQueryPattern((ISubQueryPattern)pattern, context);
                default:
                    throw new RdfQueryException("Unsupported Triple Pattern Type");
            }
        }

        public IEnumerable<ISet> ProcessMatchPattern(IMatchTriplePattern match, ISet context)
        {
            //HACK: Really we should have a proper implementation of this, right now we are just
            //calling into the existing Leviathan machinery
            //However this is just to prove the concept of a streaming engine so it doesn't matter too much

            SparqlEvaluationContext evalContext = new SparqlEvaluationContext(null, this._data);
            evalContext.InputMultiset = new Multiset();
            if (context != null) evalContext.InputMultiset.Add(context);

            IEnumerable<Triple> ts = match.GetTriples(evalContext);
            return (from t in ts
                    where match.Accepts(evalContext, t)
                    select match.CreateResult(t));
        }

        public IEnumerable<ISet> ProcessFilterPattern(IFilterPattern filter, ISet context)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ISet> ProcessAssignmentPattern(IAssignmentPattern assignment, ISet context)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ISet> ProcessPathPattern(IPropertyPathPattern path, ISet context)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ISet> ProcessFunctionPattern(IPropertyFunctionPattern function, ISet context)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ISet> ProcessSubQueryPattern(ISubQueryPattern subquery, ISet context)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
