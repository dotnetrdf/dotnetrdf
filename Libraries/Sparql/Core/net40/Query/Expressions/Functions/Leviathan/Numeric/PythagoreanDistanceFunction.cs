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
using VDS.RDF.Nodes;
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Expressions.Factories;

namespace VDS.RDF.Query.Expressions.Functions.Leviathan.Numeric
{
    /// <summary>
    /// Represents the Leviathan lfn:pythagoras() function
    /// </summary>
    public class PythagoreanDistanceFunction
        : BaseBinaryExpression
    {
        /// <summary>
        /// Creates a new Leviathan Pythagorean Distance Function
        /// </summary>
        /// <param name="arg1">First Argument</param>
        /// <param name="arg2">Second Argument</param>
        public PythagoreanDistanceFunction(IExpression arg1, IExpression arg2)
            : base(arg1, arg2) { }

        public override IExpression Copy(IExpression arg1, IExpression arg2)
        {
            return new PythagoreanDistanceFunction(arg1, arg2);
        }

        /// <summary>
        /// Evaluates the expression
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(ISolution solution, IExpressionContext context)
        {
            IValuedNode x = this.FirstArgument.Evaluate(solution, context);
            if (x == null) throw new RdfQueryException("Cannot calculate distance of a null");
            IValuedNode y = this.SecondArgument.Evaluate(solution, context);
            if (y == null) throw new RdfQueryException("Cannot calculate distance of a null");

            if (x.NumericType == EffectiveNumericType.NaN || y.NumericType == EffectiveNumericType.NaN) throw new RdfQueryException("Cannot calculate distance when one/both arguments are non-numeric");

            return new DoubleNode(Math.Sqrt(Math.Pow(x.AsDouble(), 2) + Math.Pow(y.AsDouble(), 2)));
        }

        public override bool Equals(IExpression other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            if (!(other is PythagoreanDistanceFunction)) return false;

            PythagoreanDistanceFunction func = (PythagoreanDistanceFunction) other;
            return this.FirstArgument.Equals(func.FirstArgument) && this.SecondArgument.Equals(func.SecondArgument);
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
    }
}
