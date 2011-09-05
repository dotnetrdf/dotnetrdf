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

namespace VDS.RDF.Query.Expressions.Functions
{
    /// <summary>
    /// Abstract Base Class for Unary Trigonometric Functions in the Leviathan Function Library
    /// </summary>
    public abstract class BaseUnaryLeviathanTrigonometricFunction
        : BaseUnaryLeviathanNumericFunction
    {
        /// <summary>
        /// Trigonometric function
        /// </summary>
        protected Func<double, double> _func;

        /// <summary>
        /// Creates a new Unary Trigonometric Function
        /// </summary>
        /// <param name="expr">Expression</param>
        public BaseUnaryLeviathanTrigonometricFunction(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Creates a new Unary Trigonometric Function
        /// </summary>
        /// <param name="expr">Expression</param>
        /// <param name="func">Trigonometric Function</param>
        public BaseUnaryLeviathanTrigonometricFunction(ISparqlExpression expr, Func<double, double> func)
            : base(expr)
        {
            this._func = func;
        }

        /// <summary>
        /// Gets the Integer value of the Function applied to an Integer
        /// </summary>
        /// <param name="l">Integer</param>
        /// <returns></returns>
        protected override object IntegerValueInternal(long l)
        {
            return this.DoubleValueInternal(Convert.ToDouble(l));
        }

        /// <summary>
        /// Gets the Decimal value of the Function applied to a Decimal
        /// </summary>
        /// <param name="d">Decimal</param>
        /// <returns></returns>
        protected override object DecimalValueInternal(decimal d)
        {
            return this.DoubleValueInternal(Convert.ToDouble(d));
        }

        /// <summary>
        /// Gets the Double value of the Function applied to a Double
        /// </summary>
        /// <param name="d">Double</param>
        /// <returns></returns>
        protected override object DoubleValueInternal(double d)
        {
            return this._func(d);
        }

        /// <summary>
        /// Gets the string representation of the Function
        /// </summary>
        /// <returns></returns>
        public abstract override string ToString();
    }

    /// <summary>
    /// Represents the Leviathan lfn:degrees-to-radians() function
    /// </summary>
    public class LeviathanDegreesToRadiansFunction 
        : BaseUnaryLeviathanNumericFunction
    {
        /// <summary>
        /// Creates a new Leviathan Degrees to Radians Function
        /// </summary>
        /// <param name="expr">Expression</param>
        public LeviathanDegreesToRadiansFunction(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Gets the Integer value of the Function applied to an Integer
        /// </summary>
        /// <param name="l">Integer</param>
        /// <returns></returns>
        protected override object IntegerValueInternal(long l)
        {
            return this.DoubleValueInternal(Convert.ToDouble(l));
        }

        /// <summary>
        /// Gets the Decimal value of the Function applied to a Decimal
        /// </summary>
        /// <param name="d">Decimal</param>
        /// <returns></returns>
        protected override object DecimalValueInternal(decimal d)
        {
            return this.DoubleValueInternal(Convert.ToDouble(d));
        }

        /// <summary>
        /// Gets the Double value of the Function applied to a Double
        /// </summary>
        /// <param name="d">Double</param>
        /// <returns></returns>
        protected override object DoubleValueInternal(double d)
        {
            return Math.PI * (d / 180.0d);
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.DegreesToRadians + ">(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get 
            {
                return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.DegreesToRadians; 
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new LeviathanDegreesToRadiansFunction(transformer.Transform(this._expr));
        }
    }

    /// <summary>
    /// Represents the Leviathan lfn:radians-to-degrees() function
    /// </summary>
    public class LeviathanRadiansToDegreesFunction
        : BaseUnaryLeviathanNumericFunction
    {
        /// <summary>
        /// Creates a new Leviathan Radians to Degrees Function
        /// </summary>
        /// <param name="expr">Expression</param>
        public LeviathanRadiansToDegreesFunction(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Gets the Integer value of the Function applied to an Integer
        /// </summary>
        /// <param name="l">Integer</param>
        /// <returns></returns>
        protected override object IntegerValueInternal(long l)
        {
            return this.DoubleValueInternal(Convert.ToDouble(l));
        }

        /// <summary>
        /// Gets the Decimal value of the Function applied to a Decimal
        /// </summary>
        /// <param name="d">Decimal</param>
        /// <returns></returns>
        protected override object DecimalValueInternal(decimal d)
        {
            return this.DoubleValueInternal(Convert.ToDouble(d));
        }

        /// <summary>
        /// Gets the Double value of the Function applied to a Double
        /// </summary>
        /// <param name="d">Double</param>
        /// <returns></returns>
        protected override object DoubleValueInternal(double d)
        {
            return d * (180.0d / Math.PI);
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.RadiansToDegrees + ">(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.RadiansToDegrees;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new LeviathanRadiansToDegreesFunction(transformer.Transform(this._expr));
        }
    }

    /// <summary>
    /// Represents the Leviathan lfn:sin() or lfn:sin-1 function
    /// </summary>
    public class LeviathanSineFunction 
        : BaseUnaryLeviathanTrigonometricFunction
    {
        private bool _inverse = false;

        /// <summary>
        /// Creates a new Leviathan Sine Function
        /// </summary>
        /// <param name="expr">Expression</param>
        public LeviathanSineFunction(ISparqlExpression expr)
            : base(expr, Math.Sin) { }

        /// <summary>
        /// Creates a new Leviathan Sine Function
        /// </summary>
        /// <param name="expr">Expression</param>
        /// <param name="inverse">Whether this should be the inverse function</param>
        public LeviathanSineFunction(ISparqlExpression expr, bool inverse)
            : base(expr)
        {
            this._inverse = inverse;
            if (this._inverse)
            {
                this._func = Math.Asin;
            }
            else
            {
                this._func = Math.Sin;
            }
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (this._inverse)
            {
                return "<" + LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.TrigSinInv + ">(" + this._expr.ToString() + ")";
            }
            else
            {
                return "<" + LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.TrigSin + ">(" + this._expr.ToString() + ")";
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                if (this._inverse)
                {
                    return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.TrigSinInv;
                }
                else
                {
                    return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.TrigSin;
                }
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new LeviathanSineFunction(transformer.Transform(this._expr), this._inverse);
        }
    }

    /// <summary>
    /// Represents the Leviathan lfn:cos() or lfn:cos-1 function
    /// </summary>
    public class LeviathanCosineFunction
        : BaseUnaryLeviathanTrigonometricFunction
    {
        private bool _inverse = false;

        /// <summary>
        /// Creates a new Leviathan Cosine Function
        /// </summary>
        /// <param name="expr">Expression</param>
        public LeviathanCosineFunction(ISparqlExpression expr)
            : base(expr, Math.Cos) { }

        /// <summary>
        /// Creates a new Leviathan Cosine Function
        /// </summary>
        /// <param name="expr">Expression</param>
        /// <param name="inverse">Whether this should be the inverse function</param>
        public LeviathanCosineFunction(ISparqlExpression expr, bool inverse)
            : base(expr)
        {
            this._inverse = inverse;
            if (this._inverse)
            {
                this._func = Math.Acos;
            }
            else
            {
                this._func = Math.Cos;
            }
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (this._inverse)
            {
                return "<" + LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.TrigCosInv + ">(" + this._expr.ToString() + ")";
            }
            else
            {
                return "<" + LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.TrigCos + ">(" + this._expr.ToString() + ")";
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                if (this._inverse)
                {
                    return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.TrigCosInv;
                }
                else
                {
                    return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.TrigCos;
                }
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new LeviathanCosineFunction(transformer.Transform(this._expr), this._inverse);
        }
    }

    /// <summary>
    /// Represents the Leviathan lfn:tan() or lfn:tan-1 function
    /// </summary>
    public class LeviathanTangentFunction
        : BaseUnaryLeviathanTrigonometricFunction
    {
        private bool _inverse = false;

        /// <summary>
        /// Creates a new Leviathan Tangent Function
        /// </summary>
        /// <param name="expr">Expression</param>
        public LeviathanTangentFunction(ISparqlExpression expr)
            : base(expr, Math.Tan) { }

        /// <summary>
        /// Creates a new Leviathan Tangent Function
        /// </summary>
        /// <param name="expr">Expression</param>
        /// <param name="inverse">Whether this should be the inverse function</param>
        public LeviathanTangentFunction(ISparqlExpression expr, bool inverse)
            : base(expr)
        {
            this._inverse = inverse;
            if (this._inverse)
            {
                this._func = Math.Atan;
            }
            else
            {
                this._func = Math.Tan;
            }
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (this._inverse)
            {
                return "<" + LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.TrigTanInv + ">(" + this._expr.ToString() + ")";
            }
            else
            {
                return "<" + LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.TrigTan + ">(" + this._expr.ToString() + ")";
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                if (this._inverse)
                {
                    return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.TrigTanInv;
                }
                else
                {
                    return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.TrigTan;
                }
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new LeviathanTangentFunction(transformer.Transform(this._expr), this._inverse);
        }
    }

    /// <summary>
    /// Represents the Leviathan lfn:sec() or lfn:sec-1 function
    /// </summary>
    public class LeviathanSecantFunction 
        : BaseUnaryLeviathanTrigonometricFunction
    {
        private bool _inverse = false;
        private static Func<double, double> _secant = ( d => (1 / Math.Cos(d)) );
        private static Func<double, double> _arcsecant = ( d => Math.Acos(1 / d) );

        /// <summary>
        /// Creates a new Leviathan Secant Function
        /// </summary>
        /// <param name="expr">Expression</param>
        public LeviathanSecantFunction(ISparqlExpression expr)
            : base(expr, _secant) { }

        /// <summary>
        /// Creates a new Leviathan Secant Function
        /// </summary>
        /// <param name="expr">Expression</param>
        /// <param name="inverse">Whether this should be the inverse function</param>
        public LeviathanSecantFunction(ISparqlExpression expr, bool inverse)
            : base(expr)
        {
            this._inverse = inverse;
            if (this._inverse)
            {
                this._func = _arcsecant;
            }
            else
            {
                this._func = _secant;
            }
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (this._inverse)
            {
                return "<" + LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.TrigSecInv + ">(" + this._expr.ToString() + ")";
            }
            else
            {
                return "<" + LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.TrigSec + ">(" + this._expr.ToString() + ")";
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                if (this._inverse)
                {
                    return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.TrigSecInv;
                }
                else
                {
                    return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.TrigSec;
                }
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new LeviathanSecantFunction(transformer.Transform(this._expr), this._inverse);
        }
    }

    /// <summary>
    /// Represents the Leviathan lfn:cosec() or lfn:cosec-1 function
    /// </summary>
    public class LeviathanCosecantFunction
        : BaseUnaryLeviathanTrigonometricFunction
    {
        private bool _inverse = false;
        private static Func<double, double> _cosecant = ( d => (1 / Math.Sin(d)) );
        private static Func<double, double> _arccosecant = ( d => Math.Asin(1 / d) );

        /// <summary>
        /// Creates a new Leviathan Cosecant Function
        /// </summary>
        /// <param name="expr">Expression</param>
        public LeviathanCosecantFunction(ISparqlExpression expr)
            : base(expr, _cosecant) { }

        /// <summary>
        /// Creates a new Leviathan Cosecant Function
        /// </summary>
        /// <param name="expr">Expression</param>
        /// <param name="inverse">Whether this should be the inverse function</param>
        public LeviathanCosecantFunction(ISparqlExpression expr, bool inverse)
            : base(expr)
        {
            this._inverse = inverse;
            if (this._inverse)
            {
                this._func = _arccosecant;
            }
            else
            {
                this._func = _cosecant;
            }
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (this._inverse)
            {
                return "<" + LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.TrigCosecInv + ">(" + this._expr.ToString() + ")";
            }
            else
            {
                return "<" + LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.TrigCosec + ">(" + this._expr.ToString() + ")";
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                if (this._inverse)
                {
                    return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.TrigCosecInv;
                }
                else
                {
                    return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.TrigCosec;
                }
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new LeviathanCosecantFunction(transformer.Transform(this._expr), this._inverse);
        }
    }

    /// <summary>
    /// Represents the Leviathan lfn:cot() or lfn:cot-1 function
    /// </summary>
    public class LeviathanCotangentFunction
        : BaseUnaryLeviathanTrigonometricFunction
    {
        private bool _inverse = false;
        private static Func<double, double> _cotangent = ( d => (Math.Cos(d) / Math.Sin(d)) );
        private static Func<double, double> _arccotangent = ( d => Math.Atan(1 / d) );

        /// <summary>
        /// Creates a new Leviathan Cotangent Function
        /// </summary>
        /// <param name="expr">Expression</param>
        public LeviathanCotangentFunction(ISparqlExpression expr)
            : base(expr, _cotangent) { }

        /// <summary>
        /// Creates a new Leviathan Cotangent Function
        /// </summary>
        /// <param name="expr">Expression</param>
        /// <param name="inverse">Whether this should be the inverse function</param>
        public LeviathanCotangentFunction(ISparqlExpression expr, bool inverse)
            : base(expr)
        {
            this._inverse = inverse;
            if (this._inverse)
            {
                this._func = _arccotangent;
            }
            else
            {
                this._func = _cotangent;
            }
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (this._inverse)
            {
                return "<" + LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.TrigCotanInv + ">(" + this._expr.ToString() + ")";
            }
            else
            {
                return "<" + LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.TrigCotan + ">(" + this._expr.ToString() + ")";
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                if (this._inverse)
                {
                    return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.TrigCotanInv;
                }
                else
                {
                    return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.TrigCotan;
                }
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new LeviathanCotangentFunction(transformer.Transform(this._expr), this._inverse);
        }
    }
}
