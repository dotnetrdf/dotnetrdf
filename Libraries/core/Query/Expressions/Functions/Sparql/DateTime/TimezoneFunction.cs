using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Expressions.Functions.Sparql.DateTime
{
    /// <summary>
    /// Represents the SPARQL TIMEZONE() Function
    /// </summary>
    public class TimezoneFunction
        : XPath.DateTime.TimezoneFromDateTimeFunction
    {
        /// <summary>
        /// Creates a new SPARQL TIMEZONE() Function
        /// </summary>
        /// <param name="expr">Argument Expression</param>
        public TimezoneFunction(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Gets the Timezone of the Argument Expression as evaluated for the given Binding in the given Context
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            IValuedNode temp = base.Evaluate(context, bindingID);

            if (temp == null)
            {
                //Unlike base function must error if no timezone component
                throw new RdfQueryException("Cannot get the Timezone from a Date Time that does not have a timezone component");
            }
            else
            {
                //Otherwise the base value is fine
                return temp;
            }
        }

        /// <summary>
        /// Gets the Functor of this Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordTimezone;
            }
        }

        /// <summary>
        /// Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordTimezone + "(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new TimezoneFunction(transformer.Transform(this._expr));
        }
    }
}
