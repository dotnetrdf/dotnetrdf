using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using VDS.RDF.Parsing;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Expressions.Functions.XPath.DateTime
{
    /// <summary>
    /// Represents the XPath timezone-from-dateTime() function
    /// </summary>
    public class TimezoneFromDateTimeFunction
        : ISparqlExpression
    {
        /// <summary>
        /// Expression that the Function applies to
        /// </summary>
        protected ISparqlExpression _expr;

        /// <summary>
        /// Creates a new XPath Timezone from Date Time function
        /// </summary>
        /// <param name="expr">Expression</param>
        public TimezoneFromDateTimeFunction(ISparqlExpression expr)
        {
            this._expr = expr;
        }

        /// <summary>
        /// Calculates the value of the function in the given Evaluation Context for the given Binding ID
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public virtual IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            IValuedNode temp = this._expr.Evaluate(context, bindingID);
            if (temp != null)
            {
                DateTimeOffset dt = temp.AsDateTime();
                //Regex based check to see if the value has a Timezone component
                //If not then the result is a null
                if (!Regex.IsMatch(temp.AsString(), "(Z|[+-]\\d{2}:\\d{2})$")) return null;

                //Now we have a DateTime we can try and return the Timezone
                if (dt.Offset.Equals(TimeSpan.Zero))
                {
                    //If Zero it was specified as Z (which means UTC so zero offset)
                    return new StringNode(null, "PT0S", UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDayTimeDuration));
                }
                else
                {
                    //If the Offset is outside the range -14 to 14 this is considered invalid
                    if (dt.Offset.Hours < -14 || dt.Offset.Hours > 14) return null;

                    //Otherwise it has an offset which is a given number of hours and minutse
                    string offset = "PT" + Math.Abs(dt.Offset.Hours) + "H";
                    if (dt.Offset.Hours < 0) offset = "-" + offset;
                    if (dt.Offset.Minutes != 0) offset = offset + Math.Abs(dt.Offset.Minutes) + "M";
                    if (dt.Offset.Hours == 0 && dt.Offset.Minutes < 0) offset = "-" + offset;

                    return new StringNode(null, offset, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDayTimeDuration));
                }
            }
            else
            {
                throw new RdfQueryException("Unable to evaluate an XPath Date Time function on a null argument");
            }
        }

        /// <summary>
        /// Calculates the effective boolean value of the function in the given Evaluation Context for the given Binding ID
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            throw new RdfQueryException("Cannot calculate the Effective Boolean Value of an XML Schema Duration");
        }

        /// <summary>
        /// Gets the Variables used in the function
        /// </summary>
        public IEnumerable<string> Variables
        {
            get
            {
                return this._expr.Variables;
            }
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.TimezoneFromDateTime + ">(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Type of the Expression
        /// </summary>
        public SparqlExpressionType Type
        {
            get
            {
                return SparqlExpressionType.Function;
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public virtual string Functor
        {
            get
            {
                return XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.TimezoneFromDateTime;
            }
        }

        /// <summary>
        /// Gets the Arguments of the Expression
        /// </summary>
        public IEnumerable<ISparqlExpression> Arguments
        {
            get
            {
                return this._expr.AsEnumerable();
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public virtual ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new TimezoneFromDateTimeFunction(transformer.Transform(this._expr));
        }
    }
}
