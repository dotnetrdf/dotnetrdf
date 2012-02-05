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
    /// Class representing COUNT(DISTINCT ?x) Aggregate Function
    /// </summary>
    public class CountDistinctAggregate
        : BaseAggregate
    {
        private String _varname;

        /// <summary>
        /// Creates a new COUNT(DISTINCT ?x) Aggregate
        /// </summary>
        /// <param name="expr">Variable Expression</param>
        public CountDistinctAggregate(VariableTerm expr)
            : base(expr)
        {
            this._varname = expr.ToString().Substring(1);
        }


        /// <summary>
        /// Creates a new COUNT DISTINCT Aggregate
        /// </summary>
        /// <param name="expr">Expression</param>
        public CountDistinctAggregate(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Counts the results
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingIDs">Binding IDs over which the Aggregate applies</param>
        /// <returns></returns>
        public override IValuedNode Apply(SparqlEvaluationContext context, IEnumerable<int> bindingIDs)
        {
            int c;
            HashSet<IValuedNode> values = new HashSet<IValuedNode>();

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
                    IValuedNode temp = varExpr.Evaluate(context, id);
                    if (temp != null)
                    {
                        values.Add(temp);
                    }
                }
                c = values.Count;
            }
            else
            {
                //Count the distinct non-null results
                foreach (int id in bindingIDs)
                {
                    try
                    {
                        IValuedNode temp = this._expr.Evaluate(context, id);
                        if (temp != null)
                        {
                            values.Add(temp);
                        }
                    }
                    catch
                    {
                        //Ignore errors
                    }
                }
                c = values.Count;
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

        /// <summary>
        /// Gets the Arguments of the Aggregate
        /// </summary>
        public override IEnumerable<ISparqlExpression> Arguments
        {
            get
            {
                return new ISparqlExpression[] { new DistinctModifier() }.Concat(base.Arguments);
            }
        }
    }
}
