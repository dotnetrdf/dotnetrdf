using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Aggregates.Sparql
{
    /// <summary>
    /// Class representing COUNT Aggregate Function
    /// </summary>
    public class CountAggregate
        : BaseAggregate
    {
        private String _varname;

        /// <summary>
        /// Creates a new COUNT Aggregate
        /// </summary>
        /// <param name="expr">Variable Expression</param>
        public CountAggregate(VariableTerm expr)
            : base(expr)
        {
            this._varname = expr.ToString().Substring(1);
        }

        /// <summary>
        /// Creates a new Count Aggregate
        /// </summary>
        /// <param name="expr">Expression</param>
        public CountAggregate(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Counts the results
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingIDs">Binding IDs over which the Aggregate applies</param>
        /// <returns></returns>
        public override IValuedNode Apply(SparqlEvaluationContext context, IEnumerable<int> bindingIDs)
        {
            int c = 0;
            if (this._varname != null)
            {
                //Ensure the COUNTed variable is in the Variables of the Results
                if (!context.Binder.Variables.Contains(this._varname))
                {
                    throw new RdfQueryException("Cannot use the Variable " + this._expr.ToString() + " in a COUNT Aggregate since the Variable does not occur in a Graph Pattern");
                }

                //Just Count the number of results where the variable is bound
                VariableTerm varExpr = (VariableTerm)this._expr;
                foreach (int id in bindingIDs)
                {
                    if (varExpr.Evaluate(context, id) != null) c++;
                }
            }
            else
            {
                //Count the number of results where the result in not null/error
                foreach (int id in bindingIDs)
                {
                    try
                    {
                        if (this._expr.Evaluate(context, id) != null) c++;
                    }
                    catch
                    {
                        //Ignore errors
                    }
                }
            }

            return new LongNode(null, c);
        }

        /// <summary>
        /// Gets the String representation of the Aggregate
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.Append("COUNT(" + this._expr.ToString() + ")");
            return output.ToString();
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
    }
}
