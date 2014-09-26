/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using VDS.RDF.Query.Expressions.Factories;

namespace VDS.RDF.Query.Expressions.Functions.Leviathan.Numeric.Trigonometry
{
    /// <summary>
    /// Represents the Leviathan lfn:sec() or lfn:sec-1 function
    /// </summary>
    public class SecantFunction
        : BaseTrigonometricFunction
    {
        private readonly bool _inverse;
        private static readonly Func<double, double> _secant = (d => (1/Math.Cos(d)));
        private static readonly Func<double, double> _arcsecant = (d => Math.Acos(1/d));

        /// <summary>
        /// Creates a new Leviathan Secant Function
        /// </summary>
        /// <param name="expr">Expression</param>
        public SecantFunction(IExpression expr)
            : base(expr, _secant) {}

        /// <summary>
        /// Creates a new Leviathan Secant Function
        /// </summary>
        /// <param name="expr">Expression</param>
        /// <param name="inverse">Whether this should be the inverse function</param>
        public SecantFunction(IExpression expr, bool inverse)
            : base(expr)
        {
            this._inverse = inverse;
            this._func = this._inverse ? _arcsecant : _secant;
        }

        public override IExpression Copy(IExpression argument)
        {
            return new SecantFunction(argument, this._inverse);
        }

        public override bool Equals(IExpression other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            if (!(other is SecantFunction)) return false;

            SecantFunction func = (SecantFunction) other;
            // Include functor check because the same class can represent both the secant and inverse secant functions
            return this.Functor.Equals(func.Functor) && this.Argument.Equals(func.Argument);
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
                return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.TrigSec;
            }
        }
    }
}