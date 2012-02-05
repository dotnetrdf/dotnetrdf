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
    /// Class representing MIN Aggregate Functions
    /// </summary>
    public class MinAggregate 
        : BaseAggregate
    {
        private String _varname;

        /// <summary>
        /// Creates a new MIN Aggregate
        /// </summary>
        /// <param name="expr">Variable Expression</param>
        /// <param name="distinct">Whether a DISTINCT modifier applies</param>
        public MinAggregate(VariableTerm expr, bool distinct)
            : base(expr, distinct)
        {
            this._varname = expr.ToString().Substring(1);
        }

        /// <summary>
        /// Creates a new MIN Aggregate
        /// </summary>
        /// <param name="expr">Expression</param>
        /// <param name="distinct">Whether a DISTINCT modifier applies</param>
        public MinAggregate(ISparqlExpression expr, bool distinct)
            : base(expr, distinct) { }

        /// <summary>
        /// Creates a new MIN Aggregate
        /// </summary>
        /// <param name="expr">Variable Expression</param>
        public MinAggregate(VariableTerm expr)
            : this(expr, false) { }

        /// <summary>
        /// Creates a new MIN Aggregate
        /// </summary>
        /// <param name="expr">Expression</param>
        public MinAggregate(ISparqlExpression expr)
            : this(expr, false) { }

        /// <summary>
        /// Applies the Min Aggregate function to the results
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingIDs">Binding IDs over which the Aggregate applies</param>
        /// <returns></returns>
        public override IValuedNode Apply(SparqlEvaluationContext context, IEnumerable<int> bindingIDs)
        {
            if (this._varname != null)
            {
                //Ensured the MINed variable is in the Variables of the Results
                if (!context.Binder.Variables.Contains(this._varname))
                {
                    throw new RdfQueryException("Cannot use the Variable " + this._expr.ToString() + " in a MIN Aggregate since the Variable does not occur in a Graph Pattern");
                }
            }

            List<IValuedNode> values = new List<IValuedNode>();
            foreach (int id in bindingIDs)
            {
                try
                {
                    values.Add(this._expr.Evaluate(context, id));
                }
                catch
                {
                    //Ignore errors
                }
            }

            values.Sort(new SparqlOrderingComparer());
            return values.FirstOrDefault();
        }

        /// <summary>
        /// Gets the String representation of the Aggregate
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.Append("MIN(");
            if (this._distinct) output.Append("DISTINCT ");
            output.Append(this._expr.ToString() + ")");
            return output.ToString();
        }

        /// <summary>
        /// Gets the Functor of the Aggregate
        /// </summary>
        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordMin;
            }
        }
    }
}
