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

namespace VDS.RDF.Query.Expressions.Functions
{
    /// <summary>
    /// Abstract Base class for unary numeric functions in the Leviathan Function Library
    /// </summary>
    public abstract class BaseUnaryLeviathanNumericFunction 
        : BaseUnaryArithmeticExpression
    {
        /// <summary>
        /// Creates a new Leviathan Unary Numeric function
        /// </summary>
        /// <param name="expr">Expression</param>
        public BaseUnaryLeviathanNumericFunction(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Gets the Numeric Value of the Function as evaluated in the given Context for the given Binding ID
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override object NumericValue(SparqlEvaluationContext context, int bindingID)
        {
            if (this._expr is ISparqlNumericExpression)
            {
                ISparqlNumericExpression a = (ISparqlNumericExpression)this._expr;

                SparqlNumericType type = a.NumericType(context, bindingID);
                switch (type)
                {
                    case SparqlNumericType.Integer:
                        return this.IntegerValueInternal(a.IntegerValue(context, bindingID));
                    case SparqlNumericType.Decimal:
                        return this.DecimalValueInternal(a.DecimalValue(context, bindingID));
                    case SparqlNumericType.Double:
                        return this.DoubleValueInternal(a.DoubleValue(context, bindingID));
                    case SparqlNumericType.NaN:
                    default:
                        throw new RdfQueryException("Unable to evaluate a Leviathan Numeric Expression since the inner expression did not evaluate to a Numeric Value");
                }
            }
            else
            {
                throw new RdfQueryException("Unable to evaluate a Leviathan Numeric Expression since the inner expression is not a Numeric Expression");
            }
        }

        /// <summary>
        /// Gets the Integer Value of the function as applied to an Integer
        /// </summary>
        /// <param name="l">Integer</param>
        /// <returns></returns>
        protected abstract object IntegerValueInternal(long l);

        /// <summary>
        /// Gets the Decimal Value of the function as applied to a Decimal
        /// </summary>
        /// <param name="d">Decimal</param>
        /// <returns></returns>
        protected abstract object DecimalValueInternal(decimal d);

        /// <summary>
        /// Gets the Double Value of the function as applied to a Double
        /// </summary>
        /// <param name="d">Double</param>
        /// <returns></returns>
        protected abstract object DoubleValueInternal(double d);

        /// <summary>
        /// Gets the String representation of the function
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
    /// Abstract Base class for binary numeric functions in the Leviathan Function Library
    /// </summary>
    public abstract class BaseBinaryLeviathanNumericFunction
        : BaseBinaryArithmeticExpression
    {
        /// <summary>
        /// Creates a new Leviathan Binary Numeric function
        /// </summary>
        /// <param name="arg1">First Argument</param>
        /// <param name="arg2">Second Argument</param>
        public BaseBinaryLeviathanNumericFunction(ISparqlExpression arg1, ISparqlExpression arg2)
            : base(arg1, arg2) { }

        /// <summary>
        /// Gets the Numeric Value of the Function as evaluated in the given Context for the given Binding ID
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override object NumericValue(SparqlEvaluationContext context, int bindingID)
        {
            if (this._leftExpr is ISparqlNumericExpression && this._rightExpr is ISparqlNumericExpression)
            {
                ISparqlNumericExpression a = (ISparqlNumericExpression)this._leftExpr;
                ISparqlNumericExpression b = (ISparqlNumericExpression)this._rightExpr;

                SparqlNumericType type = (SparqlNumericType)Math.Max((int)a.NumericType(context, bindingID), (int)b.NumericType(context, bindingID));
                switch (type)
                {
                    case SparqlNumericType.Integer:
                        return this.IntegerValueInternal(a.IntegerValue(context, bindingID), b.IntegerValue(context, bindingID));
                    case SparqlNumericType.Decimal:
                        return this.DecimalValueInternal(a.DecimalValue(context, bindingID), b.DecimalValue(context, bindingID));
                    case SparqlNumericType.Double:
                        return this.DoubleValueInternal(a.DoubleValue(context, bindingID), b.DoubleValue(context, bindingID));
                    case SparqlNumericType.NaN:
                    default:
                        throw new RdfQueryException("Unable to evaluate a Leviathan Numeric Expression since both arguments did not evaluate to a Numeric Value");
                }
            }
            else
            {
                throw new RdfQueryException("Unable to evaluate a Leviathan Numeric Expression since the one/both of the arguments are not Numeric Expressions");
            }
        }

        /// <summary>
        /// Gets the Integer Value of the function as applied to two Integers
        /// </summary>
        /// <param name="x">Integer</param>
        /// <param name="y">Integer</param>
        /// <returns></returns>
        protected abstract object IntegerValueInternal(long x, long y);

        /// <summary>
        /// Gets the Decimal Value of the function as applied to two Decimals
        /// </summary>
        /// <param name="x">Decimal</param>
        /// <param name="y">Decimal</param>
        /// <returns></returns>
        protected abstract object DecimalValueInternal(decimal x, decimal y);

        /// <summary>
        /// Gets the Double Value of the function as applied to two Doubles
        /// </summary>
        /// <param name="x">Double</param>
        /// <param name="y">Double</param>
        /// <returns></returns>
        protected abstract object DoubleValueInternal(double x, double y);

        /// <summary>
        /// Gets the String representation of the function
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
    /// Represents the Leviathan lfn:sq() function
    /// </summary>
    public class LeviathanSquareFunction 
        : BaseUnaryLeviathanNumericFunction
    {
        /// <summary>
        /// Creates a new Leviathan Square Function
        /// </summary>
        /// <param name="expr">Expression</param>
        public LeviathanSquareFunction(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Gets the Integer Value of the function as applied to an Integer
        /// </summary>
        /// <param name="l">Integer</param>
        /// <returns></returns>
        protected override object IntegerValueInternal(long l)
        {
            return Math.Pow(Convert.ToDouble(l), 2);
        }

        /// <summary>
        /// Gets the Decimal Value of the function as applied to a Decimal
        /// </summary>
        /// <param name="d">Decimal</param>
        /// <returns></returns>
        protected override object DecimalValueInternal(decimal d)
        {
            return Math.Pow(Convert.ToDouble(d), 2);
        }

        /// <summary>
        /// Gets the Double Value of the function as applied to a Double
        /// </summary>
        /// <param name="d">Double</param>
        /// <returns></returns>
        protected override object DoubleValueInternal(double d)
        {
            return Math.Pow(d, 2);
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.Square + ">(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get 
            {
                return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.Square;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new LeviathanSquareFunction(transformer.Transform(this._expr));
        }
    }

    /// <summary>
    /// Represents the Leviathan lfn:cube() function
    /// </summary>
    public class LeviathanCubeFunction
        : BaseUnaryLeviathanNumericFunction
    {
        /// <summary>
        /// Creates a new Leviathan Cube Function
        /// </summary>
        /// <param name="expr">Expression</param>
        public LeviathanCubeFunction(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Gets the Integer Value of the function as applied to an Integer
        /// </summary>
        /// <param name="l">Integer</param>
        /// <returns></returns>
        protected override object IntegerValueInternal(long l)
        {
            return Math.Pow(Convert.ToDouble(l), 3);
        }

        /// <summary>
        /// Gets the Decimal Value of the function as applied to a Decimal
        /// </summary>
        /// <param name="d">Decimal</param>
        /// <returns></returns>
        protected override object DecimalValueInternal(decimal d)
        {
            return Math.Pow(Convert.ToDouble(d), 3);
        }

        /// <summary>
        /// Gets the Double Value of the function as applied to a Double
        /// </summary>
        /// <param name="d">Double</param>
        /// <returns></returns>
        protected override object DoubleValueInternal(double d)
        {
            return Math.Pow(d, 3);
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.Cube + ">(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.Cube;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new LeviathanCubeFunction(transformer.Transform(this._expr));
        }
    }

    /// <summary>
    /// Represents the Leviathan lfn:sqrt() function
    /// </summary>
    public class LeviathanSquareRootFunction
        : BaseUnaryLeviathanNumericFunction
    {
        /// <summary>
        /// Creates a new Leviathan Square Root Function
        /// </summary>
        /// <param name="expr">Expression</param>
        public LeviathanSquareRootFunction(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Gets the Integer Value of the function as applied to an Integer
        /// </summary>
        /// <param name="l">Integer</param>
        /// <returns></returns>
        protected override object IntegerValueInternal(long l)
        {
            return Math.Sqrt(Convert.ToDouble(l));
        }

        /// <summary>
        /// Gets the Decimal Value of the function as applied to a Decimal
        /// </summary>
        /// <param name="d">Decimal</param>
        /// <returns></returns>
        protected override object DecimalValueInternal(decimal d)
        {
            return Math.Sqrt(Convert.ToDouble(d));
        }

        /// <summary>
        /// Gets the Double Value of the function as applied to a Double
        /// </summary>
        /// <param name="d">Double</param>
        /// <returns></returns>
        protected override object DoubleValueInternal(double d)
        {
            return Math.Sqrt(d);
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.SquareRoot + ">(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.SquareRoot;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new LeviathanSquareRootFunction(transformer.Transform(this._expr));
        }
    }

    /// <summary>
    /// Represents the Leviathan lfn:ln() function
    /// </summary>
    public class LeviathanNaturalLogFunction
        : BaseUnaryLeviathanNumericFunction
    {
        /// <summary>
        /// Creates a new Leviathan Natural Logarithm Function
        /// </summary>
        /// <param name="expr">Expression</param>
        public LeviathanNaturalLogFunction(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Gets the Integer Value of the function as applied to an Integer
        /// </summary>
        /// <param name="l">Integer</param>
        /// <returns></returns>
        protected override object IntegerValueInternal(long l)
        {
            return Math.Log(Convert.ToDouble(l), Math.E);
        }

        /// <summary>
        /// Gets the Decimal Value of the function as applied to a Decimal
        /// </summary>
        /// <param name="d">Decimal</param>
        /// <returns></returns>
        protected override object DecimalValueInternal(decimal d)
        {
            return Math.Log(Convert.ToDouble(d), Math.E);
        }

        /// <summary>
        /// Gets the Double Value of the function as applied to a Double
        /// </summary>
        /// <param name="d">Double</param>
        /// <returns></returns>
        protected override object DoubleValueInternal(double d)
        {
            return Math.Log(d, Math.E);
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.Ln + ">(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.Ln;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new LeviathanNaturalLogFunction(transformer.Transform(this._expr));
        }
    }

    /// <summary>
    /// Represents the Leviathan lfn:e() function
    /// </summary>
    public class LeviathanEFunction 
        : BaseUnaryLeviathanNumericFunction
    {
        /// <summary>
        /// Creates a new Leviathan E Function
        /// </summary>
        /// <param name="expr">Expression</param>
        public LeviathanEFunction(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Gets the Integer Value of the function as applied to an Integer
        /// </summary>
        /// <param name="l">Integer</param>
        /// <returns></returns>
        protected override object IntegerValueInternal(long l)
        {
            return Math.Pow(Math.E, Convert.ToDouble(l));
        }

        /// <summary>
        /// Gets the Decimal Value of the function as applied to a Decimal
        /// </summary>
        /// <param name="d">Decimal</param>
        /// <returns></returns>
        protected override object DecimalValueInternal(decimal d)
        {
            return Math.Pow(Math.E, Convert.ToDouble(d));
        }

        /// <summary>
        /// Gets the Double Value of the function as applied to a Double
        /// </summary>
        /// <param name="d">Double</param>
        /// <returns></returns>
        protected override object DoubleValueInternal(double d)
        {
            return Math.Pow(Math.E, d);
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.E + ">(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.E;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new LeviathanEFunction(transformer.Transform(this._expr));
        }
    }

    /// <summary>
    /// Represents the Leviathan lfn:ten() function
    /// </summary>
    public class LeviathanTenFunction 
        : BaseUnaryLeviathanNumericFunction
    {
        /// <summary>
        /// Creates a new Leviathan Ten Function
        /// </summary>
        /// <param name="expr">Expression</param>
        public LeviathanTenFunction(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Gets the Integer Value of the function as applied to an Integer
        /// </summary>
        /// <param name="l">Integer</param>
        /// <returns></returns>
        protected override object IntegerValueInternal(long l)
        {
            return Math.Pow(10, Convert.ToDouble(l));
        }

        /// <summary>
        /// Gets the Decimal Value of the function as applied to a Decimal
        /// </summary>
        /// <param name="d">Decimal</param>
        /// <returns></returns>
        protected override object DecimalValueInternal(decimal d)
        {
            return Math.Pow(10, Convert.ToDouble(d));
        }

        /// <summary>
        /// Gets the Double Value of the function as applied to a Double
        /// </summary>
        /// <param name="d">Double</param>
        /// <returns></returns>
        protected override object DoubleValueInternal(double d)
        {
            return Math.Pow(10, d);
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.Ten + ">(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.Ten;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new LeviathanTenFunction(transformer.Transform(this._expr));
        }
    }

    /// <summary>
    /// Represents the Leviathan lfn:factorial() function
    /// </summary>
    public class LeviathanFactorialFunction 
        : BaseUnaryLeviathanNumericFunction
    {
        /// <summary>
        /// Creates a new Leviathan Factorial Function
        /// </summary>
        /// <param name="expr">Expression</param>
        public LeviathanFactorialFunction(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Gets the Integer Value of the function as applied to an Integer
        /// </summary>
        /// <param name="l">Integer</param>
        /// <returns></returns>
        protected override object IntegerValueInternal(long l)
        {
            if (l == 0) return 0;
            long fac = 1;
            if (l > 0)
            {
                for (long i = l; i > 1; i--)
                {
                    fac = fac * i;
                }
            }
            else
            {
                for (long i = l; i < -1; i++)
                {
                    fac = fac * i;
                }
            }
            return fac;
        }

        /// <summary>
        /// Gets the Decimal Value of the function as applied to a Decimal
        /// </summary>
        /// <param name="d">Decimal</param>
        /// <returns></returns>
        protected override object DecimalValueInternal(decimal d)
        {
            return this.IntegerValueInternal(Convert.ToInt64(d));
        }

        /// <summary>
        /// Gets the Double Value of the function as applied to a Double
        /// </summary>
        /// <param name="d">Double</param>
        /// <returns></returns>
        protected override object DoubleValueInternal(double d)
        {
            return this.IntegerValueInternal(Convert.ToInt64(d));
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.Factorial + ">(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.Factorial;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new LeviathanFactorialFunction(transformer.Transform(this._expr));
        }
    }

    /// <summary>
    /// Represents the Leviathan lfn:reciprocal() function
    /// </summary>
    public class LeviathanReciprocalFunction
        : BaseUnaryLeviathanNumericFunction
    {
        /// <summary>
        /// Creates a new Leviathan Reciprocal Function
        /// </summary>
        /// <param name="expr">Expression</param>
        public LeviathanReciprocalFunction(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Gets the Integer Value of the function as applied to an Integer
        /// </summary>
        /// <param name="l">Integer</param>
        /// <returns></returns>
        protected override object IntegerValueInternal(long l)
        {
            return this.DoubleValueInternal(Convert.ToDouble(l));
        }

        /// <summary>
        /// Gets the Decimal Value of the function as applied to a Decimal
        /// </summary>
        /// <param name="d">Decimal</param>
        /// <returns></returns>
        protected override object DecimalValueInternal(decimal d)
        {
            return this.DoubleValueInternal(Convert.ToDouble(d));
        }

        /// <summary>
        /// Gets the Double Value of the function as applied to a Double
        /// </summary>
        /// <param name="d">Double</param>
        /// <returns></returns>
        protected override object DoubleValueInternal(double d)
        {
            return (1 / d);
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.Reciprocal + ">(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.Reciprocal;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new LeviathanReciprocalFunction(transformer.Transform(this._expr));
        }
    }

    /// <summary>
    /// Represents the Leviathan lfn:pow() function
    /// </summary>
    public class LeviathanPowerFunction
        : BaseBinaryLeviathanNumericFunction
    {
        /// <summary>
        /// Creates a new Leviathan Power Function
        /// </summary>
        /// <param name="arg1">First Argument</param>
        /// <param name="arg2">Second Argument</param>
        public LeviathanPowerFunction(ISparqlExpression arg1, ISparqlExpression arg2)
            : base(arg1, arg2) { }

        /// <summary>
        /// Gets the Integer Value of the function as applied to two Integers
        /// </summary>
        /// <param name="x">Integer</param>
        /// <param name="y">Integer</param>
        /// <returns></returns>
        protected override object IntegerValueInternal(long x, long y)
        {
            return Math.Pow(Convert.ToDouble(x), Convert.ToDouble(y));
        }

        /// <summary>
        /// Gets the Decimal Value of the function as applied to two Decimals
        /// </summary>
        /// <param name="x">Decimal</param>
        /// <param name="y">Decimal</param>
        /// <returns></returns>
        protected override object DecimalValueInternal(decimal x, decimal y)
        {
            return Math.Pow(Convert.ToDouble(x), Convert.ToDouble(y));
        }

        /// <summary>
        /// Gets the Double Value of the function as applied to two Doubles
        /// </summary>
        /// <param name="x">Double</param>
        /// <param name="y">Double</param>
        /// <returns></returns>
        protected override object DoubleValueInternal(double x, double y)
        {
            return Math.Pow(x, y);
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.Power + ">(" + this._leftExpr.ToString() + "," + this._rightExpr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.Power;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new LeviathanPowerFunction(transformer.Transform(this._leftExpr), transformer.Transform(this._rightExpr));
        }
    }

    /// <summary>
    /// Represents the Leviathan lfn:root() function
    /// </summary>
    public class LeviathanRootFunction 
        : BaseBinaryLeviathanNumericFunction
    {
        /// <summary>
        /// Creates a new Leviathan Root Function
        /// </summary>
        /// <param name="arg1">First Argument</param>
        /// <param name="arg2">Second Argument</param>
        public LeviathanRootFunction(ISparqlExpression arg1, ISparqlExpression arg2)
            : base(arg1, arg2) { }

        /// <summary>
        /// Gets the Integer Value of the function as applied to two Integers
        /// </summary>
        /// <param name="x">Integer</param>
        /// <param name="y">Integer</param>
        /// <returns></returns>
        protected override object IntegerValueInternal(long x, long y)
        {
            return Math.Pow(Convert.ToDouble(x), (1 / Convert.ToDouble(y)));
        }

        /// <summary>
        /// Gets the Decimal Value of the function as applied to two Decimals
        /// </summary>
        /// <param name="x">Decimal</param>
        /// <param name="y">Decimal</param>
        /// <returns></returns>
        protected override object DecimalValueInternal(decimal x, decimal y)
        {
            return Math.Pow(Convert.ToDouble(x), (1 / Convert.ToDouble(y)));
        }

        /// <summary>
        /// Gets the Double Value of the function as applied to two Doubles
        /// </summary>
        /// <param name="x">Double</param>
        /// <param name="y">Double</param>
        /// <returns></returns>
        protected override object DoubleValueInternal(double x, double y)
        {
            return Math.Pow(x, (1 / y));
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.Power + ">(" + this._leftExpr.ToString() + "," + this._rightExpr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.Root;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new LeviathanRootFunction(transformer.Transform(this._leftExpr), transformer.Transform(this._rightExpr));
        }
    }

    /// <summary>
    /// Represents the Leviathan lfn:log() function
    /// </summary>
    public class LeviathanLogFunction 
        : BaseBinaryLeviathanNumericFunction
    {
        private bool _log10 = false;

        /// <summary>
        /// Creates a new Leviathan Log Function
        /// </summary>
        /// <param name="arg">Expression</param>
        public LeviathanLogFunction(ISparqlExpression arg)
            : base(arg, new NumericExpressionTerm(10)) 
        {
            this._log10 = true;
        }

        /// <summary>
        /// Creates a new Leviathan Log Function
        /// </summary>
        /// <param name="arg1">First Argument</param>
        /// <param name="arg2">Second Argument</param>
        public LeviathanLogFunction(ISparqlExpression arg1, ISparqlExpression arg2)
            : base(arg1, arg2) { }

        /// <summary>
        /// Gets the Integer Value of the function as applied to two Integers
        /// </summary>
        /// <param name="x">Integer</param>
        /// <param name="y">Integer</param>
        /// <returns></returns>
        protected override object IntegerValueInternal(long x, long y)
        {
            return this.DoubleValueInternal(Convert.ToDouble(x), Convert.ToDouble(y));
        }

        /// <summary>
        /// Gets the Decimal Value of the function as applied to two Decimals
        /// </summary>
        /// <param name="x">Decimal</param>
        /// <param name="y">Decimal</param>
        /// <returns></returns>
        protected override object DecimalValueInternal(decimal x, decimal y)
        {
            return this.DoubleValueInternal(Convert.ToDouble(x), Convert.ToDouble(y));
        }

        /// <summary>
        /// Gets the Double Value of the function as applied to two Doubles
        /// </summary>
        /// <param name="x">Double</param>
        /// <param name="y">Double</param>
        /// <returns></returns>
        protected override object DoubleValueInternal(double x, double y)
        {
            return Math.Log(x, y);
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (this._log10)
            {
                return "<" + LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.Log + ">(" + this._leftExpr.ToString() + ")";
            }
            else
            {
                return "<" + LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.Log + ">(" + this._leftExpr.ToString() + "," + this._rightExpr.ToString() + ")";
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.Log;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            if (this._log10)
            {
                return new LeviathanLogFunction(transformer.Transform(this._leftExpr));
            }
            else
            {
                return new LeviathanLogFunction(transformer.Transform(this._leftExpr), transformer.Transform(this._rightExpr));
            }
        }
    }

    /// <summary>
    /// Represents the Leviathan lfn:pythagoras() function
    /// </summary>
    public class LeviathanPyathagoreanDistanceFunction 
        : BaseBinaryLeviathanNumericFunction
    {
        /// <summary>
        /// Creates a new Leviathan Pythagorean Distance Function
        /// </summary>
        /// <param name="arg1">First Argument</param>
        /// <param name="arg2">Second Argument</param>
        public LeviathanPyathagoreanDistanceFunction(ISparqlExpression arg1, ISparqlExpression arg2)
            : base(arg1, arg2) { }

        /// <summary>
        /// Gets the Integer Value of the function as applied to two Integers
        /// </summary>
        /// <param name="x">Integer</param>
        /// <param name="y">Integer</param>
        /// <returns></returns>
        protected override object IntegerValueInternal(long x, long y)
        {
            return this.DoubleValueInternal(Convert.ToDouble(x), Convert.ToDouble(y));
        }

        /// <summary>
        /// Gets the Decimal Value of the function as applied to two Decimals
        /// </summary>
        /// <param name="x">Decimal</param>
        /// <param name="y">Decimal</param>
        /// <returns></returns>
        protected override object DecimalValueInternal(decimal x, decimal y)
        {
            return this.DoubleValueInternal(Convert.ToDouble(x), Convert.ToDouble(y));
        }

        /// <summary>
        /// Gets the Double Value of the function as applied to two Doubles
        /// </summary>
        /// <param name="x">Double</param>
        /// <param name="y">Double</param>
        /// <returns></returns>
        protected override object DoubleValueInternal(double x, double y)
        {
            return Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.Pythagoras + ">(" + this._leftExpr.ToString() + "," + this._rightExpr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.Pythagoras;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new LeviathanPyathagoreanDistanceFunction(transformer.Transform(this._leftExpr), transformer.Transform(this._rightExpr));
        }
    }

    /// <summary>
    /// Represents the Leviathan lfn:rnd() function
    /// </summary>
    public class LeviathanRandomFunction
        : BaseBinaryLeviathanNumericFunction
    {
        private Random _rnd = new Random();
        private int _args = 0;

        /// <summary>
        /// Creates a new Leviathan Random Function
        /// </summary>
        public LeviathanRandomFunction()
            : base(new NumericExpressionTerm(0), new NumericExpressionTerm(1)) { }

        /// <summary>
        /// Creates a new Leviathan Random Function
        /// </summary>
        /// <param name="max">Maximum</param>
        public LeviathanRandomFunction(ISparqlExpression max)
            : base(new NumericExpressionTerm(0), max) 
        {
            this._args = 1;
        }

        /// <summary>
        /// Creates a new Leviathan Random Function
        /// </summary>
        /// <param name="min">Minumum</param>
        /// <param name="max">Maximum</param>
        public LeviathanRandomFunction(ISparqlExpression min, ISparqlExpression max)
            : base(min, max) 
        {
            this._args = 2;
        }

        /// <summary>
        /// Gets the Integer Value of the function as applied to two Integers
        /// </summary>
        /// <param name="x">Integer</param>
        /// <param name="y">Integer</param>
        /// <returns></returns>
        protected override object IntegerValueInternal(long x, long y)
        {
            return this.DoubleValueInternal(Convert.ToDouble(x), Convert.ToDouble(y));
        }

        /// <summary>
        /// Gets the Decimal Value of the function as applied to two Decimals
        /// </summary>
        /// <param name="x">Decimal</param>
        /// <param name="y">Decimal</param>
        /// <returns></returns>
        protected override object DecimalValueInternal(decimal x, decimal y)
        {
            return this.DoubleValueInternal(Convert.ToDouble(x), Convert.ToDouble(y));
        }

        /// <summary>
        /// Gets the Double Value of the function as applied to two Doubles
        /// </summary>
        /// <param name="x">Double</param>
        /// <param name="y">Double</param>
        /// <returns></returns>
        protected override object DoubleValueInternal(double x, double y)
        {
            if (x > y) throw new RdfQueryException("Cannot generate a random number in the given range since the minumum is greater than the maximum");
            double range = y - x;
            double rnd = this._rnd.NextDouble() * range;
            rnd += x;
            return rnd;
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.Append('<');
            output.Append(LeviathanFunctionFactory.LeviathanFunctionsNamespace);
            output.Append(LeviathanFunctionFactory.Random);
            output.Append(">(");
            switch (this._args)
            {
                case 1:
                    output.Append(this._rightExpr.ToString());
                    break;
                case 2:
                    output.Append(this._leftExpr.ToString());
                    output.Append(',');
                    output.Append(this._rightExpr.ToString());
                    break;
            }
            output.Append(')');
            return output.ToString();
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.Random;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new LeviathanRandomFunction(transformer.Transform(this._leftExpr), transformer.Transform(this._rightExpr));
        }
    }

}


