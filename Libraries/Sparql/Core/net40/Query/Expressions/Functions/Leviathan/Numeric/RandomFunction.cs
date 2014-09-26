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
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Expressions.Functions.Leviathan.Numeric
{
    /// <summary>
    /// Represents the Leviathan lfn:rnd() function
    /// </summary>
    public class RandomFunction
        : BaseBinaryExpression
    {
        private Random _rnd = new Random();
        private int _args = 0;

        /// <summary>
        /// Creates a new Leviathan Random Function
        /// </summary>
        public RandomFunction()
            : base(new ConstantTerm(new DoubleNode(0)), new ConstantTerm(new DoubleNode(1))) { }

        /// <summary>
        /// Creates a new Leviathan Random Function
        /// </summary>
        /// <param name="max">Maximum</param>
        public RandomFunction(IExpression max)
            : base(new ConstantTerm(new DoubleNode(0)), max)
        {
            this._args = 1;
        }

        /// <summary>
        /// Creates a new Leviathan Random Function
        /// </summary>
        /// <param name="min">Minumum</param>
        /// <param name="max">Maximum</param>
        public RandomFunction(IExpression min, IExpression max)
            : base(min, max)
        {
            this._args = 2;
        }

        /// <summary>
        /// Evaluates the expression
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(ISolution solution, IExpressionContext context)
        {
            IValuedNode min = this.FirstArgument.Evaluate(solution, context);
            if (min == null) throw new RdfQueryException("Cannot randomize with a null minimum");
            IValuedNode max = this.SecondArgument.Evaluate(solution, context);
            if (max == null) throw new RdfQueryException("Cannot randomize with a null maximum");

            if (min.NumericType == EffectiveNumericType.NaN || max.NumericType == EffectiveNumericType.NaN) throw new RdfQueryException("Cannot randomize when one/both arguments are non-numeric");

            double x = min.AsDouble();
            double y = max.AsDouble();

            if (x > y) throw new RdfQueryException("Cannot generate a random number in the given range since the minumum is greater than the maximum");
            double range = y - x;
            double rnd = this._rnd.NextDouble() * range;
            rnd += x;
            return new DoubleNode(rnd);
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

        public override bool IsDeterministic
        {
            get { return false; }
        }

        public override bool CanParallelise
        {
            get { return false; }
        }

        public override bool IsConstant
        {
            get { return false; }
        }
    }
}
