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

namespace VDS.RDF.Query.Expressions
{
    /// <summary>
    /// Class representing Arithmetic Addition expressions
    /// </summary>
    public class AdditionExpression 
        : BaseBinaryArithmeticExpression
    {
        /// <summary>
        /// Creates a new Addition Expression
        /// </summary>
        /// <param name="leftExpr">Left Hand Expression</param>
        /// <param name="rightExpr">Right Hand Expression</param>
        public AdditionExpression(ISparqlExpression leftExpr, ISparqlExpression rightExpr) : base(leftExpr, rightExpr) { }

        /// <summary>
        /// Calculates the Numeric Value of this Expression as evaluated for a given Binding
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override object NumericValue(SparqlEvaluationContext context, int bindingID)
        {
            if (this._leftExpr is ISparqlNumericExpression && this._rightExpr is ISparqlNumericExpression)
            {
                ISparqlNumericExpression a, b;
                a = (ISparqlNumericExpression)this._leftExpr;
                b = (ISparqlNumericExpression)this._rightExpr;

                SparqlNumericType type = (SparqlNumericType)Math.Max((int)a.NumericType(context, bindingID), (int)b.NumericType(context, bindingID));

                switch (type)
                {
                    case SparqlNumericType.Integer:
                        return a.IntegerValue(context, bindingID) + b.IntegerValue(context, bindingID);
                    case SparqlNumericType.Decimal:
                        return a.DecimalValue(context, bindingID) + b.DecimalValue(context, bindingID);
                    case SparqlNumericType.Float:
                        return a.FloatValue(context, bindingID) + b.FloatValue(context, bindingID);
                    case SparqlNumericType.Double:
                        return a.DoubleValue(context, bindingID) + b.DoubleValue(context, bindingID);
                    default:
                        throw new RdfQueryException("Cannot evalute an Arithmetic Expression when the Numeric Type of the expression cannot be determined");
                }
            }
            else
            {
                throw new RdfQueryException("Cannot evalute an Arithmetic Expression where the two sub-expressions are not Numeric Expressions");
            }
        }

        /// <summary>
        /// Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            if (this._leftExpr.Type == SparqlExpressionType.BinaryOperator)
            {
                output.Append("(" + this._leftExpr.ToString() + ")");
            }
            else
            {
                output.Append(this._leftExpr.ToString());
            }
            output.Append(" + ");
            if (this._rightExpr.Type == SparqlExpressionType.BinaryOperator)
            {
                output.Append("(" + this._rightExpr.ToString() + ")");
            }
            else
            {
                output.Append(this._rightExpr.ToString());
            }
            return output.ToString();
        }

        /// <summary>
        /// Gets the Type of the Expression
        /// </summary>
        public override SparqlExpressionType Type
        {
            get 
            {
                return SparqlExpressionType.BinaryOperator; 
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get 
            {
                return "+"; 
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new AdditionExpression(transformer.Transform(this._leftExpr), transformer.Transform(this._rightExpr));
        }
    }

    /// <summary>
    /// Class representing Arithmetic Subtraction expressions
    /// </summary>
    public class SubtractionExpression
        : BaseBinaryArithmeticExpression
    {
        /// <summary>
        /// Creates a new Subtraction Expression
        /// </summary>
        /// <param name="leftExpr">Left Hand Expression</param>
        /// <param name="rightExpr">Right Hand Expression</param>
        public SubtractionExpression(ISparqlExpression leftExpr, ISparqlExpression rightExpr) : base(leftExpr, rightExpr) { }

        /// <summary>
        /// Calculates the Numeric Value of this Expression as evaluated for a given Binding
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override Object NumericValue(SparqlEvaluationContext context, int bindingID)
        {
            if (this._leftExpr is ISparqlNumericExpression && this._rightExpr is ISparqlNumericExpression)
            {
                ISparqlNumericExpression a, b;
                a = (ISparqlNumericExpression)this._leftExpr;
                b = (ISparqlNumericExpression)this._rightExpr;

                SparqlNumericType type = (SparqlNumericType)Math.Max((int)a.NumericType(context, bindingID), (int)b.NumericType(context, bindingID));

                switch (type)
                {
                    case SparqlNumericType.Integer:
                        return a.IntegerValue(context, bindingID) - b.IntegerValue(context, bindingID);
                    case SparqlNumericType.Decimal:
                        return a.DecimalValue(context, bindingID) - b.DecimalValue(context, bindingID);
                    case SparqlNumericType.Float:
                        return a.FloatValue(context, bindingID) - b.FloatValue(context, bindingID);
                    case SparqlNumericType.Double:
                        return a.DoubleValue(context, bindingID) - b.DoubleValue(context, bindingID);
                    default:
                        throw new RdfQueryException("Cannot evalute an Arithmetic Expression when the Numeric Type of the expression cannot be determined");
                }
            }
            else
            {
                throw new RdfQueryException("Cannot evalute an Arithmetic Expression where the two sub-expressions are not Numeric Expressions");
            }
        }

        /// <summary>
        /// Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            if (this._leftExpr.Type == SparqlExpressionType.BinaryOperator)
            {
                output.Append("(" + this._leftExpr.ToString() + ")");
            }
            else
            {
                output.Append(this._leftExpr.ToString());
            }
            output.Append(" - ");
            if (this._rightExpr.Type == SparqlExpressionType.BinaryOperator)
            {
                output.Append("(" + this._rightExpr.ToString() + ")");
            }
            else
            {
                output.Append(this._rightExpr.ToString());
            }
            return output.ToString();
        }

        /// <summary>
        /// Gets the Type of the Expression
        /// </summary>
        public override SparqlExpressionType Type
        {
            get
            {
                return SparqlExpressionType.BinaryOperator;
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return "-";
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new SubtractionExpression(transformer.Transform(this._leftExpr), transformer.Transform(this._rightExpr));
        }
    }

    /// <summary>
    /// Class representing Arithmetic Multiplication expressions
    /// </summary>
    public class MultiplicationExpression 
        : BaseBinaryArithmeticExpression
    {
        /// <summary>
        /// Creates a new Multiplication Expression
        /// </summary>
        /// <param name="leftExpr">Left Hand Expression</param>
        /// <param name="rightExpr">Right Hand Expression</param>
        public MultiplicationExpression(ISparqlExpression leftExpr, ISparqlExpression rightExpr) : base(leftExpr, rightExpr) { }

        /// <summary>
        /// Calculates the Numeric Value of this Expression as evaluated for a given Binding
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override Object NumericValue(SparqlEvaluationContext context, int bindingID)
        {
            if (this._leftExpr is ISparqlNumericExpression && this._rightExpr is ISparqlNumericExpression)
            {
                ISparqlNumericExpression a, b;
                a = (ISparqlNumericExpression)this._leftExpr;
                b = (ISparqlNumericExpression)this._rightExpr;

                SparqlNumericType type = (SparqlNumericType)Math.Max((int)a.NumericType(context, bindingID), (int)b.NumericType(context, bindingID));

                switch (type)
                {
                    case SparqlNumericType.Integer:
                        return a.IntegerValue(context, bindingID) * b.IntegerValue(context, bindingID);
                    case SparqlNumericType.Decimal:
                        return a.DecimalValue(context, bindingID) * b.DecimalValue(context, bindingID);
                    case SparqlNumericType.Float:
                        return a.FloatValue(context, bindingID) * b.FloatValue(context, bindingID);
                    case SparqlNumericType.Double:
                        return a.DoubleValue(context, bindingID) * b.DoubleValue(context, bindingID);
                    default:
                        throw new RdfQueryException("Cannot evalute an Arithmetic Expression when the Numeric Type of the expression cannot be determined");
                }
            }
            else
            {
                throw new RdfQueryException("Cannot evalute an Arithmetic Expression where the two sub-expressions are not Numeric Expressions");
            }
        }

        /// <summary>
        /// Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            if (this._leftExpr.Type == SparqlExpressionType.BinaryOperator)
            {
                output.Append("(" + this._leftExpr.ToString() + ")");
            }
            else
            {
                output.Append(this._leftExpr.ToString());
            }
            output.Append(" * ");
            if (this._rightExpr.Type == SparqlExpressionType.BinaryOperator)
            {
                output.Append("(" + this._rightExpr.ToString() + ")");
            }
            else
            {
                output.Append(this._rightExpr.ToString());
            }
            return output.ToString();
        }

        /// <summary>
        /// Gets the Type of the Expression
        /// </summary>
        public override SparqlExpressionType Type
        {
            get
            {
                return SparqlExpressionType.BinaryOperator;
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return "*";
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new MultiplicationExpression(transformer.Transform(this._leftExpr), transformer.Transform(this._rightExpr));
        }
    }

    /// <summary>
    /// Class representing Arithmetic Division expressions
    /// </summary>
    public class DivisionExpression
        : BaseBinaryArithmeticExpression
    {
        /// <summary>
        /// Creates a new Division Expression
        /// </summary>
        /// <param name="leftExpr">Left Hand Expression</param>
        /// <param name="rightExpr">Right Hand Expression</param>
        public DivisionExpression(ISparqlExpression leftExpr, ISparqlExpression rightExpr) : base(leftExpr, rightExpr) { }

        /// <summary>
        /// Calculates the Numeric Value of this Expression as evaluated for a given Binding
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override Object NumericValue(SparqlEvaluationContext context, int bindingID)
        {
            if (this._leftExpr is ISparqlNumericExpression && this._rightExpr is ISparqlNumericExpression)
            {
                ISparqlNumericExpression a, b;
                a = (ISparqlNumericExpression)this._leftExpr;
                b = (ISparqlNumericExpression)this._rightExpr;

                SparqlNumericType type = (SparqlNumericType)Math.Max((int)a.NumericType(context, bindingID), (int)b.NumericType(context, bindingID));

                try
                {
                    switch (type)
                    {
                        case SparqlNumericType.Integer:
                        case SparqlNumericType.Decimal:
                            //For Division Integers are treated as decimals
                            decimal d = a.DecimalValue(context, bindingID) / b.DecimalValue(context, bindingID);
                            if (Decimal.Floor(d).Equals(d) && d >= Int64.MinValue && d <= Int64.MaxValue)
                            {
                                return Convert.ToInt64(d);
                            }
                            return d;
                        case SparqlNumericType.Float:
                            return a.FloatValue(context, bindingID) / b.FloatValue(context, bindingID);
                        case SparqlNumericType.Double:
                            return a.DoubleValue(context, bindingID) / b.DoubleValue(context, bindingID);
                        default:
                            throw new RdfQueryException("Cannot evalute an Arithmetic Expression when the Numeric Type of the expression cannot be determined");
                    }
                }
                catch (DivideByZeroException)
                {
                    throw new RdfQueryException("Cannot evaluate a Division Expression where the divisor is Zero");
                }
            }
            else
            {
                throw new RdfQueryException("Cannot evalute an Arithmetic Expression where the two sub-expressions are not Numeric Expressions");
            }
        }

        /// <summary>
        /// Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            if (this._leftExpr.Type == SparqlExpressionType.BinaryOperator)
            {
                output.Append("(" + this._leftExpr.ToString() + ")");
            }
            else
            {
                output.Append(this._leftExpr.ToString());
            }
            output.Append(" / ");
            if (this._rightExpr.Type == SparqlExpressionType.BinaryOperator)
            {
                output.Append("(" + this._rightExpr.ToString() + ")");
            }
            else
            {
                output.Append(this._rightExpr.ToString());
            }
            return output.ToString();
        }

        /// <summary>
        /// Gets the Type of the Expression
        /// </summary>
        public override SparqlExpressionType Type
        {
            get
            {
                return SparqlExpressionType.BinaryOperator;
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return "/";
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new DivisionExpression(transformer.Transform(this._leftExpr), transformer.Transform(this._rightExpr));
        }
    }

    /// <summary>
    /// Class representing Unary Minus expressions (sign of numeric expression is reversed)
    /// </summary>
    public class MinusExpression 
        : BaseUnaryArithmeticExpression
    {
        /// <summary>
        /// Creates a new Unary Minus Expression
        /// </summary>
        /// <param name="expr">Expression to apply the Minus operator to</param>
        public MinusExpression(ISparqlExpression expr) : base(expr) { }

        /// <summary>
        /// Calculates the Numeric Value of this Expression as evaluated for a given Binding
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override Object NumericValue(SparqlEvaluationContext context, int bindingID)
        {
            if (this._expr is ISparqlNumericExpression)
            {
                ISparqlNumericExpression a = (ISparqlNumericExpression)this._expr;

                switch (a.NumericType(context, bindingID))
                {
                    case SparqlNumericType.Integer:
                        return -1 * a.IntegerValue(context, bindingID);

                    case SparqlNumericType.Decimal:
                        decimal decvalue = a.DecimalValue(context, bindingID);
                        if (decvalue == 0)
                        {
                            return 0.0m;
                        }
                        else
                        {
                            return -1 * decvalue;
                        }
                    case SparqlNumericType.Float:
                        float fltvalue = a.FloatValue(context, bindingID);
                        if (Single.IsNaN(fltvalue))
                        {
                            return Single.NaN;
                        }
                        else if (Single.IsPositiveInfinity(fltvalue))
                        {
                            return Single.NegativeInfinity;
                        }
                        else if (Single.IsNegativeInfinity(fltvalue))
                        {
                            return Single.PositiveInfinity;
                        }
                        else
                        {
                            return -1.0 * fltvalue;
                        }
                    case SparqlNumericType.Double:
                        double dblvalue = a.DoubleValue(context, bindingID);
                        if (Double.IsNaN(dblvalue))
                        {
                            return Double.NaN;
                        }
                        else if (Double.IsPositiveInfinity(dblvalue))
                        {
                            return Double.NegativeInfinity;
                        }
                        else if (Double.IsNegativeInfinity(dblvalue))
                        {
                            return Double.PositiveInfinity;
                        }
                        else
                        {
                            return -1.0 * dblvalue;
                        }
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
        /// Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "-" + this._expr.ToString();
        }

        /// <summary>
        /// Gets the Type of the Expression
        /// </summary>
        public override SparqlExpressionType Type
        {
            get
            {
                return SparqlExpressionType.UnaryOperator;
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return "-";
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new MinusExpression(transformer.Transform(this._expr));
        }
    }
}
