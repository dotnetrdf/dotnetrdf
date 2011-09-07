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

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
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
    /// Abstract Base Class for functions which are Unary functions applied to Date Time objects in the XPath function library
    /// </summary>
    public abstract class BaseUnaryXPathDateTimeFunction 
        : BaseUnaryArithmeticExpression
    {
        /// <summary>
        /// Creates a new Unary XPath Date Time function
        /// </summary>
        /// <param name="expr"></param>
        public BaseUnaryXPathDateTimeFunction(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Gets the numeric value of the function in the given Evaluation Context for the given Binding ID
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override object NumericValue(SparqlEvaluationContext context, int bindingID)
        {
            INode temp = this._expr.Value(context, bindingID);
            if (temp != null)
            {
                if (temp.NodeType == NodeType.Literal)
                {
                    ILiteralNode lit = (ILiteralNode)temp;
                    if (lit.DataType != null)
                    {
                        if (lit.DataType.ToString().Equals(XmlSpecsHelper.XmlSchemaDataTypeDateTime) || lit.DataType.ToString().Equals(XmlSpecsHelper.XmlSchemaDataTypeDate))
                        {
                            DateTimeOffset dt;
                            if (DateTimeOffset.TryParse(lit.Value, out dt))
                            {
                                return this.NumericValueInternal(dt);
                            }
                            else
                            {
                                throw new RdfQueryException("Unable to evaluate an XPath Date Time function as the value of the Date Time typed literal couldn't be parsed as a Date Time");
                            }
                        }
                        else
                        {
                            throw new RdfQueryException("Unable to evaluate an XPath Date Time function on a typed literal which is not a Date Time");
                        }
                    }
                    else
                    {
                        throw new RdfQueryException("Unable to evaluate an XPath Date Time function on an untyped literal argument");
                    }
                }
                else
                {
                    throw new RdfQueryException("Unable to evaluate an XPath Date Time function on a non-literal argument");
                }
            }
            else
            {
                throw new RdfQueryException("Unable to evaluate an XPath Date Time function on a null argument");
            }
        }

        /// <summary>
        /// Abstract method which derived classes must implement to generate the actual numeric value for the function
        /// </summary>
        /// <param name="dateTime">Date Time</param>
        /// <returns></returns>
        protected abstract object NumericValueInternal(DateTimeOffset dateTime);

        /// <summary>
        /// Gets the String representation of the Function
        /// </summary>
        /// <returns></returns>
        public abstract override string ToString();

        /// <summary>
        /// Gets the Type of the Expression
        /// </summary>
        public override SparqlExpressionType Type
        {
            get 
            {
                return SparqlExpressionType.Function;
            }
        }
    }

    /// <summary>
    /// Represents the XPath year-from-dateTime() function
    /// </summary>
    public class XPathYearFromDateTimeFunction
        : BaseUnaryXPathDateTimeFunction
    {
        /// <summary>
        /// Creates a new XPath Year from Date Time function
        /// </summary>
        /// <param name="expr">Expression</param>
        public XPathYearFromDateTimeFunction(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Calculates the numeric value of the function from the given Date Time
        /// </summary>
        /// <param name="dateTime">Date Time</param>
        /// <returns></returns>
        protected override object NumericValueInternal(DateTimeOffset dateTime)
        {
            return Convert.ToInt64(dateTime.Year);
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.YearFromDateTime + ">(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get 
            {
                return XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.YearFromDateTime;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new XPathYearFromDateTimeFunction(transformer.Transform(this._expr));
        }
    }

    /// <summary>
    /// Represents the XPath month-from-dateTime() function
    /// </summary>
    public class XPathMonthFromDateTimeFunction 
        : BaseUnaryXPathDateTimeFunction
    {
        /// <summary>
        /// Creates a new XPath Month from Date Time function
        /// </summary>
        /// <param name="expr">Expression</param>
        public XPathMonthFromDateTimeFunction(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Calculates the numeric value of the function from the given Date Time
        /// </summary>
        /// <param name="dateTime">Date Time</param>
        /// <returns></returns>
        protected override object NumericValueInternal(DateTimeOffset dateTime)
        {
            return Convert.ToInt64(dateTime.Month);
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.MonthFromDateTime + ">(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.MonthFromDateTime;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new XPathMonthFromDateTimeFunction(transformer.Transform(this._expr));
        }
    }

    /// <summary>
    /// Represents the XPath day-from-dateTime() function
    /// </summary>
    public class XPathDayFromDateTimeFunction 
        : BaseUnaryXPathDateTimeFunction
    {
        /// <summary>
        /// Creates a new XPath Day from Date Time function
        /// </summary>
        /// <param name="expr">Expression</param>
        public XPathDayFromDateTimeFunction(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Calculates the numeric value of the function from the given Date Time
        /// </summary>
        /// <param name="dateTime">Date Time</param>
        /// <returns></returns>
        protected override object NumericValueInternal(DateTimeOffset dateTime)
        {
            return Convert.ToInt64(dateTime.Day);
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.DayFromDateTime + ">(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.DayFromDateTime;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new XPathDayFromDateTimeFunction(transformer.Transform(this._expr));
        }
    }

    /// <summary>
    /// Represents the XPath hours-from-dateTime() function
    /// </summary>
    public class XPathHoursFromDateTimeFunction 
        : BaseUnaryXPathDateTimeFunction
    {
        /// <summary>
        /// Creates a new XPath Hours from Date Time function
        /// </summary>
        /// <param name="expr">Expression</param>
        public XPathHoursFromDateTimeFunction(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Calculates the numeric value of the function from the given Date Time
        /// </summary>
        /// <param name="dateTime">Date Time</param>
        /// <returns></returns>
        protected override object NumericValueInternal(DateTimeOffset dateTime)
        {
            return Convert.ToInt64(dateTime.Hour);
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.HoursFromDateTime + ">(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.HoursFromDateTime;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new XPathHoursFromDateTimeFunction(transformer.Transform(this._expr));
        }
    }

    /// <summary>
    /// Represents the XPath minutes-from-dateTime() function
    /// </summary>
    public class XPathMinutesFromDateTimeFunction 
        : BaseUnaryXPathDateTimeFunction
    {
        /// <summary>
        /// Creates a new XPath Minutes from Date Time function
        /// </summary>
        /// <param name="expr">Expression</param>
        public XPathMinutesFromDateTimeFunction(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Calculates the numeric value of the function from the given Date Time
        /// </summary>
        /// <param name="dateTime">Date Time</param>
        /// <returns></returns>
        protected override object NumericValueInternal(DateTimeOffset dateTime)
        {
            return Convert.ToInt64(dateTime.Minute);
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.MinutesFromDateTime + ">(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.MinutesFromDateTime;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new XPathMinutesFromDateTimeFunction(transformer.Transform(this._expr));
        }
    }

    /// <summary>
    /// Represents the XPath seconds-from-dateTime() function
    /// </summary>
    public class XPathSecondsFromDateTimeFunction
        : BaseUnaryXPathDateTimeFunction
    {
        /// <summary>
        /// Creates a new XPath Seconds from Date Time function
        /// </summary>
        /// <param name="expr">Expression</param>
        public XPathSecondsFromDateTimeFunction(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Calculates the numeric value of the function from the given Date Time
        /// </summary>
        /// <param name="dateTime">Date Time</param>
        /// <returns></returns>
        protected override object NumericValueInternal(DateTimeOffset dateTime)
        {
            decimal seconds = Convert.ToDecimal(dateTime.Second);
            seconds += ((decimal)dateTime.Millisecond) / 1000m;

            return seconds;
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.SecondsFromDateTime + ">(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.SecondsFromDateTime;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new XPathSecondsFromDateTimeFunction(transformer.Transform(this._expr));
        }
    }

    /// <summary>
    /// Represents the XPath timezone-from-dateTime() function
    /// </summary>
    public class XPathTimezoneFromDateTimeFunction
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
        public XPathTimezoneFromDateTimeFunction(ISparqlExpression expr)
        {
            this._expr = expr;
        }

        /// <summary>
        /// Calculates the value of the function in the given Evaluation Context for the given Binding ID
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public virtual INode Value(SparqlEvaluationContext context, int bindingID)
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
                                if (!Regex.IsMatch(lit.Value, "(Z|[+-]\\d{2}:\\d{2})$")) return null;

                                //Now we have a DateTime we can try and return the Timezone
                                if (dt.Offset.Equals(TimeSpan.Zero))
                                {
                                    //If Zero it was specified as Z (which means UTC so zero offset)
                                    return new LiteralNode(null, "PT0S", new Uri(XmlSpecsHelper.XmlSchemaDataTypeDayTimeDuration));
                                }
                                else
                                {
                                    //If the Offset is outside the range -14 to 14 this is considered invalid
                                    if (dt.Offset.Hours < -14 || dt.Offset.Hours > 14) return null; 

                                    //Otherwise it has an offset which is a given number of hours and minutse
                                    String offset = "PT" + Math.Abs(dt.Offset.Hours) + "H";
                                    if (dt.Offset.Hours < 0) offset = "-" + offset;
                                    if (dt.Offset.Minutes != 0) offset = offset + Math.Abs(dt.Offset.Minutes) + "M";
                                    if (dt.Offset.Hours == 0 && dt.Offset.Minutes < 0) offset = "-" + offset;

                                    return new LiteralNode(null, offset, new Uri(XmlSpecsHelper.XmlSchemaDataTypeDayTimeDuration));
                                }
                            }
                            else
                            {
                                throw new RdfQueryException("Unable to evaluate an XPath Date Time function as the value of the Date Time typed literal couldn't be parsed as a Date Time");
                            }
                        }
                        else
                        {
                            throw new RdfQueryException("Unable to evaluate an XPath Date Time function on a typed literal which is not a Date Time");
                        }
                    }
                    else
                    {
                        throw new RdfQueryException("Unable to evaluate an XPath Date Time function on an untyped literal argument");
                    }
                }
                else
                {
                    throw new RdfQueryException("Unable to evaluate an XPath Date Time function on a non-literal argument");
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
            return new XPathTimezoneFromDateTimeFunction(transformer.Transform(this._expr));
        }
    }
}