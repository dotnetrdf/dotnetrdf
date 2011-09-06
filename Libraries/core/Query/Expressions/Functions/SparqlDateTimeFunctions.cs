/*

Copyright Robert Vesse 2009-10
rvesse@vdesign-studios.com

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

If this license is not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using VDS.RDF.Parsing;

namespace VDS.RDF.Query.Expressions.Functions
{
    /// <summary>
    /// Represents the SPARQL NOW() Function
    /// </summary>
    public class NowFunction 
        : ArqNowFunction
    {
        /// <summary>
        /// Creates a new SPARQL NOW() Function
        /// </summary>
        public NowFunction()
            : base() { }

        /// <summary>
        /// Gets the Functor of this Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordNow;
            }
        }

        /// <summary>
        /// Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordNow + "()";
        }
    }

    /// <summary>
    /// Represents the SPARQL YEAR() Function
    /// </summary>
    public class YearFunction
        : XPathYearFromDateTimeFunction
    {
        /// <summary>
        /// Creates a new SPARQL YEAR() Function
        /// </summary>
        /// <param name="expr">Argument Expression</param>
        public YearFunction(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Gets the Functor of this Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordYear;
            }
        }

        /// <summary>
        /// Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordYear + "(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new YearFunction(transformer.Transform(this._expr));
        }
    }

    /// <summary>
    /// Represents the SPARQL MONTH() Function
    /// </summary>
    public class MonthFunction 
        : XPathMonthFromDateTimeFunction
    {
        /// <summary>
        /// Creates a new SPARQL YEAR() Function
        /// </summary>
        /// <param name="expr">Argument Expression</param>
        public MonthFunction(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Gets the Functor of this Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordMonth;
            }
        }

        /// <summary>
        /// Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordMonth + "(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new MonthFunction(transformer.Transform(this._expr));
        }
    }

    /// <summary>
    /// Represents the SPARQL DAY() Function
    /// </summary>
    public class DayFunction 
        : XPathDayFromDateTimeFunction
    {
        /// <summary>
        /// Creates a new SPARQL DAY() Function
        /// </summary>
        /// <param name="expr">Argument Expression</param>
        public DayFunction(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Gets the Functor of this Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordDay;
            }
        }

        /// <summary>
        /// Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordDay + "(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new DayFunction(transformer.Transform(this._expr));
        }
    }

    /// <summary>
    /// Represents the SPARQL HOURS() Function
    /// </summary>
    public class HoursFunction 
        : XPathHoursFromDateTimeFunction
    {
        /// <summary>
        /// Creates a new SPARQL HOURS() Function
        /// </summary>
        /// <param name="expr">Argument Expression</param>
        public HoursFunction(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Gets the Functor of this Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordHours;
            }
        }

        /// <summary>
        /// Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordHours + "(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new HoursFunction(transformer.Transform(this._expr));
        }
    }

    /// <summary>
    /// Represents the SPARQL MINUTES() Function
    /// </summary>
    public class MinutesFunction 
        : XPathMinutesFromDateTimeFunction
    {
        /// <summary>
        /// Creates a new SPARQL MINUTES() Function
        /// </summary>
        /// <param name="expr">Argument Expression</param>
        public MinutesFunction(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Gets the Functor of this Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordMinutes;
            }
        }

        /// <summary>
        /// Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordMinutes + "(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new MinutesFunction(transformer.Transform(this._expr));
        }
    }

    /// <summary>
    /// Represents the SPARQL SECONDS() Function
    /// </summary>
    public class SecondsFunction 
        : XPathSecondsFromDateTimeFunction
    {
        /// <summary>
        /// Creates a new SPARQL SECONDS() Function
        /// </summary>
        /// <param name="expr">Argument Expression</param>
        public SecondsFunction(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Gets the Functor of this Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordSeconds;
            }
        }

        /// <summary>
        /// Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordSeconds + "(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new SecondsFunction(transformer.Transform(this._expr));
        }
    }

    /// <summary>
    /// Represents the SPARQL TIMEZONE() Function
    /// </summary>
    public class TimezoneFunction
        : XPathTimezoneFromDateTimeFunction
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
        public override INode Value(SparqlEvaluationContext context, int bindingID)
        {
            INode temp = base.Value(context, bindingID);

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

    /// <summary>
    /// Represents the SPARQL TZ() Function
    /// </summary>
    public class TZFunction 
        : BaseUnaryExpression
    {
        /// <summary>
        /// Creates a new SPARQL TZ() Function
        /// </summary>
        /// <param name="expr">Argument Expression</param>
        public TZFunction(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Gets the Timezone of the Argument Expression as evaluated for the given Binding in the given Context
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override INode Value(SparqlEvaluationContext context, int bindingID)
        {
            INode temp = this._expr.Value(context, bindingID);
            if (temp != null)
            {
                if (temp.NodeType == NodeType.Literal)
                {
                    ILiteralNode lit = (ILiteralNode)temp;
                    if (lit.DataType != null)
                    {
                        if (lit.DataType.ToString().Equals(XmlSpecsHelper.XmlSchemaDataTypeDateTime))
                        {
                            DateTimeOffset dt;
                            if (DateTimeOffset.TryParse(lit.Value, out dt))
                            {
                                //Regex based check to see if the value has a Timezone component
                                //If not then the result is a null
                                if (!Regex.IsMatch(lit.Value, "(Z|[+-]\\d{2}:\\d{2})$")) return new LiteralNode(null, String.Empty);

                                //Now we have a DateTime we can try and return the Timezone
                                if (dt.Offset.Equals(TimeSpan.Zero))
                                {
                                    //If Zero it was specified as Z (which means UTC so zero offset)
                                    return new LiteralNode(null, "Z");
                                }
                                else
                                {
                                    //If the Offset is outside the range -14 to 14 this is considered invalid
                                    if (dt.Offset.Hours < -14 || dt.Offset.Hours > 14) return null;

                                    //Otherwise it has an offset which is a given number of hours (and minutes)
                                    return new LiteralNode(null, dt.Offset.Hours.ToString("00") + ":" + dt.Offset.Minutes.ToString("00"));
                                }
                            }
                            else
                            {
                                throw new RdfQueryException("Unable to evaluate a Date Time function as the value of the Date Time typed literal couldn't be parsed as a Date Time");
                            }
                        }
                        else
                        {
                            throw new RdfQueryException("Unable to evaluate a Date Time function on a typed literal which is not a Date Time");
                        }
                    }
                    else
                    {
                        throw new RdfQueryException("Unable to evaluate a Date Time function on an untyped literal argument");
                    }
                }
                else
                {
                    throw new RdfQueryException("Unable to evaluate a Date Time function on a non-literal argument");
                }
            }
            else
            {
                throw new RdfQueryException("Unable to evaluate a Date Time function on a null argument");
            }
        }

        /// <summary>
        /// Gets the Effective Boolean Value of the Expression as evaluated for the given Binding in the given Context
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            return SparqlSpecsHelper.EffectiveBooleanValue(this.Value(context, bindingID));
        }

        /// <summary>
        /// Gets the Type of this Expression
        /// </summary>
        public override SparqlExpressionType Type
        {
            get 
            {
                return SparqlExpressionType.Function; 
            }
        }

        /// <summary>
        /// Gets the Functor of this Expression
        /// </summary>
        public override string Functor
        {
            get 
            {
                return SparqlSpecsHelper.SparqlKeywordTz; 
            }
        }

        /// <summary>
        /// Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordTz + "(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new TZFunction(transformer.Transform(this._expr));
        }
    }
}
