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
    /// Represents the XPath fn:abs() function
    /// </summary>
    public class XPathAbsoluteFunction
        : BaseUnaryArithmeticExpression
    {
        /// <summary>
        /// Creates a new XPath Absolute function
        /// </summary>
        /// <param name="expr">Expression</param>
        public XPathAbsoluteFunction(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Gets the Numeric Value of the function as evaluated in the given Context for the given Binding ID
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override object NumericValue(SparqlEvaluationContext context, int bindingID)
        {
            if (this._expr is ISparqlNumericExpression)
            {
                ISparqlNumericExpression a = (ISparqlNumericExpression)this._expr;

                switch (a.NumericType(context, bindingID))
                {
                    case SparqlNumericType.Integer:
                        return Math.Abs(a.IntegerValue(context, bindingID));

                    case SparqlNumericType.Decimal:
                        return Math.Abs(a.DecimalValue(context, bindingID));

                    case SparqlNumericType.Float:
                        return (float)Math.Abs(a.DoubleValue(context, bindingID));

                    case SparqlNumericType.Double:
                        return Math.Abs(a.DoubleValue(context, bindingID));
                        
                    default:
                        throw new RdfQueryException("Cannot evalute an Arithmetic Expression when the Numeric Type of the expression cannot be determined");
                }
            }
            else
            {
                throw new RdfQueryException("Cannot evalute an Arithmetic Expression where the sub-expression is not a Numeric Expressions");
            }
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.Absolute + ">(" + this._expr.ToString() + ")";
        }

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

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.Absolute; 
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new XPathAbsoluteFunction(transformer.Transform(this._expr));
        }
    }

    /// <summary>
    /// Represents the XPath fn:ceiling() function
    /// </summary>
    public class XPathCeilingFunction 
        : BaseUnaryArithmeticExpression
    {
        /// <summary>
        /// Creates a new XPath Ceiling function
        /// </summary>
        /// <param name="expr">Expression</param>
        public XPathCeilingFunction(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Gets the Numeric Value of the function as evaluated in the given Context for the given Binding ID
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override object NumericValue(SparqlEvaluationContext context, int bindingID)
        {
            if (this._expr is ISparqlNumericExpression)
            {
                ISparqlNumericExpression a = (ISparqlNumericExpression)this._expr;

                switch (a.NumericType(context, bindingID))
                {
#if !SILVERLIGHT
                    case SparqlNumericType.Integer:
                        return (long)Math.Ceiling((decimal)a.IntegerValue(context, bindingID));

                    case SparqlNumericType.Decimal:
                        return Math.Ceiling(a.DecimalValue(context, bindingID));
#else
                    case SparqlNumericType.Integer:
                    case SparqlNumericType.Decimal:
#endif
                    case SparqlNumericType.Float:
                        return (float)Math.Ceiling(a.DoubleValue(context, bindingID));

                    case SparqlNumericType.Double:
                        return Math.Ceiling(a.DoubleValue(context, bindingID));

                    default:
                        throw new RdfQueryException("Cannot evalute an Arithmetic Expression when the Numeric Type of the expression cannot be determined");
                }
            }
            else
            {
                throw new RdfQueryException("Cannot evalute an Arithmetic Expression where the sub-expression is not a Numeric Expressions");
            }
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.Ceiling + ">(" + this._expr.ToString() + ")";
        }

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

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.Ceiling;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new XPathCeilingFunction(transformer.Transform(this._expr));
        }
    }

    /// <summary>
    /// Represents the XPath fn:floor() function
    /// </summary>
    public class XPathFloorFunction
        : BaseUnaryArithmeticExpression
    {
        /// <summary>
        /// Creates a new XPath Floor function
        /// </summary>
        /// <param name="expr">Expression</param>
        public XPathFloorFunction(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Gets the Numeric Value of the function as evaluated in the given Context for the given Binding ID
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override object NumericValue(SparqlEvaluationContext context, int bindingID)
        {
            if (this._expr is ISparqlNumericExpression)
            {
                ISparqlNumericExpression a = (ISparqlNumericExpression)this._expr;

                switch (a.NumericType(context, bindingID))
                {
#if !SILVERLIGHT
                    case SparqlNumericType.Integer:
                        return (long)Math.Floor((decimal)a.IntegerValue(context, bindingID));

                    case SparqlNumericType.Decimal:
                        return Math.Floor(a.DecimalValue(context, bindingID));
#else
                    case SparqlNumericType.Integer:
                    case SparqlNumericType.Decimal:
#endif

                    case SparqlNumericType.Float:
                        return (float)Math.Floor(a.DoubleValue(context, bindingID));

                    case SparqlNumericType.Double:
                        return Math.Floor(a.DoubleValue(context, bindingID));

                    default:
                        throw new RdfQueryException("Cannot evalute an Arithmetic Expression when the Numeric Type of the expression cannot be determined");
                }
            }
            else
            {
                throw new RdfQueryException("Cannot evalute an Arithmetic Expression where the sub-expression is not a Numeric Expressions");
            }
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.Floor + ">(" + this._expr.ToString() + ")";
        }

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

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.Floor;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new XPathFloorFunction(transformer.Transform(this._expr));
        }
    }

    /// <summary>
    /// Represents the XPath fn:round() function
    /// </summary>
    public class XPathRoundFunction
        : BaseUnaryArithmeticExpression
    {
        /// <summary>
        /// Creates a new XPath Round function
        /// </summary>
        /// <param name="expr">Expression</param>
        public XPathRoundFunction(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Gets the Numeric Value of the function as evaluated in the given Context for the given Binding ID
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override object NumericValue(SparqlEvaluationContext context, int bindingID)
        {
            if (this._expr is ISparqlNumericExpression)
            {
                ISparqlNumericExpression a = (ISparqlNumericExpression)this._expr;

                switch (a.NumericType(context, bindingID))
                {
                    case SparqlNumericType.Integer:
                        //Rounding an Integer has no effect
                        return a.IntegerValue(context, bindingID);

                    case SparqlNumericType.Decimal:
#if !SILVERLIGHT
                        return Math.Round(a.DecimalValue(context, bindingID), MidpointRounding.AwayFromZero);
#else
                        return Math.Round(a.DecimalValue(context, bindingID));
#endif

                    case SparqlNumericType.Float:
#if !SILVERLIGHT
                        return (float)Math.Round(a.DoubleValue(context, bindingID), MidpointRounding.AwayFromZero);
#else
                        return (float)Math.Round(a.DoubleValue(context, bindingID));
#endif

                    case SparqlNumericType.Double:
#if !SILVERLIGHT
                        return Math.Round(a.DoubleValue(context, bindingID), MidpointRounding.AwayFromZero);
#else
                        return Math.Round(a.DoubleValue(context, bindingID));
#endif

                    default:
                        throw new RdfQueryException("Cannot evalute an Arithmetic Expression when the Numeric Type of the expression cannot be determined");
                }
            }
            else
            {
                throw new RdfQueryException("Cannot evalute an Arithmetic Expression where the sub-expression is not a Numeric Expressions");
            }
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.Round + ">(" + this._expr.ToString() + ")";
        }

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

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.Round;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new XPathRoundFunction(transformer.Transform(this._expr));
        }
    }

#if !SILVERLIGHT

    /// <summary>
    /// Represents the XPath fn:round-half-to-even() function
    /// </summary>
    public class XPathRoundHalfToEvenFunction
        : BaseUnaryArithmeticExpression
    {
        private ISparqlExpression _precisionExpr;

        /// <summary>
        /// Creates a new XPath Round Half to Even function
        /// </summary>
        /// <param name="expr">Expression</param>
        public XPathRoundHalfToEvenFunction(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Creates a new XPath Round Half to Even function
        /// </summary>
        /// <param name="expr">Expression</param>
        /// <param name="precision">Precision for the Rouding</param>
        public XPathRoundHalfToEvenFunction(ISparqlExpression expr, ISparqlExpression precision)
            : base(expr)
        {
            this._precisionExpr = precision;
        }

        /// <summary>
        /// Gets the Numeric Value of the function as evaluated in the given Context for the given Binding ID
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override object NumericValue(SparqlEvaluationContext context, int bindingID)
        {
            int precision = 0;
            if (this._expr is ISparqlNumericExpression)
            {
                ISparqlNumericExpression a = (ISparqlNumericExpression)this._expr;

                //Get the Precision setting (if any)
                if (this._precisionExpr != null)
                {
                    if (this._precisionExpr is ISparqlNumericExpression)
                    {
                        ISparqlNumericExpression b = (ISparqlNumericExpression)this._precisionExpr;
                        precision = Convert.ToInt32(b.IntegerValue(context, bindingID));
                    }
                }

                switch (a.NumericType(context, bindingID))
                {
                    case SparqlNumericType.Integer:
                        return Math.Round((decimal)a.IntegerValue(context, bindingID), precision, MidpointRounding.ToEven);

                    case SparqlNumericType.Decimal:
                        return Math.Round(a.DecimalValue(context, bindingID), precision, MidpointRounding.ToEven);

                    case SparqlNumericType.Float:
                        return (float)Math.Round(a.DoubleValue(context, bindingID), MidpointRounding.ToEven);

                    case SparqlNumericType.Double:
                        return Math.Round(a.DoubleValue(context, bindingID), precision, MidpointRounding.ToEven);

                    default:
                        throw new RdfQueryException("Cannot evalute an Arithmetic Expression when the Numeric Type of the expression cannot be determined");
                }
            }
            else
            {
                throw new RdfQueryException("Cannot evalute an Arithmetic Expression where the sub-expression is not a Numeric Expressions");
            }
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.Floor + ">(" + this._expr.ToString() + ")";
        }

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

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.RoundHalfToEven;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new XPathRoundHalfToEvenFunction(transformer.Transform(this._expr));
        }
    }

#endif

}