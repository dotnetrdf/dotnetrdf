using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Aggregates.Sparql
{
    /// <summary>
    /// Class representing the SAMPLE aggregate
    /// </summary>
    public class SampleAggregate
        : BaseAggregate
    {
        /// <summary>
        /// Creates a new SAMPLE Aggregate
        /// </summary>
        /// <param name="expr">Expression</param>
        public SampleAggregate(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Applies the SAMPLE Aggregate
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingIDs">Binding IDs</param>
        /// <returns></returns>
        public override IValuedNode Apply(SparqlEvaluationContext context, IEnumerable<int> bindingIDs)
        {
            //Try the expression with each member of the Group until we find a non-null
            foreach (int id in bindingIDs)
            {
                try
                {

                    //First non-null result we find is returned
                    IValuedNode temp = this._expr.Evaluate(context, id);
                    if (temp != null) return temp;
                }
                catch (RdfQueryException)
                {
                    //Ignore errors - we'll loop round and try the next
                }
            }

            //If the Group is Empty of the Expression fails to evaluate for the entire Group then the result is null
            return null;
        }

        /// <summary>
        /// Gets the String representation
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (this._distinct)
            {
                return "SAMPLE(DISTINCT " + this._expr.ToString() + ")";
            }
            else
            {
                return "SAMPLE(" + this._expr.ToString() + ")";
            }
        }

        /// <summary>
        /// Gets the Functor of the Aggregate
        /// </summary>
        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordSample;
            }
        }
    }
}
