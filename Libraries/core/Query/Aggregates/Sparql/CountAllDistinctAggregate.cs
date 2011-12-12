using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Nodes;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Aggregates.Sparql
{
    /// <summary>
    /// Class representing COUNT(DISTINCT *) Aggregate Function
    /// </summary>
    public class CountAllDistinctAggregate
        : BaseAggregate
    {
        /// <summary>
        /// Creates a new COUNT(DISTINCT*) Aggregate
        /// </summary>
        public CountAllDistinctAggregate()
            : base(null)
        {
        }

        /// <summary>
        /// Counts the results
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingIDs">Binding IDs over which the Aggregate applies</param>
        /// <returns></returns>
        public override IValuedNode Apply(SparqlEvaluationContext context, IEnumerable<int> bindingIDs)
        {
            IEnumerable<ISet> cs = from id in bindingIDs
                                   select context.InputMultiset[id];
            return new LongNode(null, cs.Distinct().Count());
        }

        /// <summary>
        /// Gets the String representation of the Aggregate
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "COUNT(DISTINCT *)";
        }

        /// <summary>
        /// Gets the Functor of the Aggregate
        /// </summary>
        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordCount;
            }
        }

        /// <summary>
        /// Gets the Arguments of the Aggregate
        /// </summary>
        public override IEnumerable<ISparqlExpression> Arguments
        {
            get
            {
                return new ISparqlExpression[] { new DistinctModifier(), new AllModifier() };
            }
        }
    }
}
