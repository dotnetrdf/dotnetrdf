/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-develop@lists.sf.net)

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

namespace VDS.RDF.Query.Expressions.Functions.Arq
{
    /// <summary>
    /// Represents the ARQ max() function
    /// </summary>
    public class MaxFunction
        : BaseBinaryExpression
    {
        /// <summary>
        /// Creates a new ARQ max() function
        /// </summary>
        /// <param name="arg1">First Argument</param>
        /// <param name="arg2">Second Argument</param>
        public MaxFunction(IExpression arg1, IExpression arg2)
            : base(arg1, arg2) { }

        public override IExpression Copy(IExpression arg1, IExpression arg2)
        {
            return new MaxFunction(arg1, arg2);
        }

        /// <summary>
        /// Gets the numeric value of the function in the given Evaluation Context for the given Binding ID
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(ISolution solution, IExpressionContext context)
        {
            IValuedNode a = this.FirstArgument.Evaluate(solution, context);
            IValuedNode b = this.SecondArgument.Evaluate(solution, context);

            EffectiveNumericType type = (EffectiveNumericType)Math.Max((int)a.NumericType, (int)b.NumericType);

            switch (type)
            {
                case EffectiveNumericType.Integer:
                    return new LongNode(Math.Max(a.AsInteger(), b.AsInteger()));
                case EffectiveNumericType.Decimal:
                    return new DecimalNode(Math.Max(a.AsDecimal(), b.AsDecimal()));
                case EffectiveNumericType.Float:
                    return new FloatNode(Math.Max(a.AsFloat(), b.AsFloat()));
                case EffectiveNumericType.Double:
                    return new DoubleNode(Math.Max(a.AsDouble(), b.AsDouble()));
                default:
                    throw new RdfQueryException("Cannot evalute an Arithmetic Expression when the Numeric Type of the expression cannot be determined");
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return ArqFunctionFactory.ArqFunctionsNamespace + ArqFunctionFactory.Max;
            }
        }
    }
}
