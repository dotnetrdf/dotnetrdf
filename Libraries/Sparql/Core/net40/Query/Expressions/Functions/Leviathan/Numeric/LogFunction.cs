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
    /// Represents the Leviathan lfn:log() function
    /// </summary>
    public class LogFunction
        : BaseBinaryExpression
    {
        private readonly bool _log10 = false;

        /// <summary>
        /// Creates a new Leviathan Log Function
        /// </summary>
        /// <param name="arg">Expression</param>
        public LogFunction(IExpression arg)
            : base(arg, new ConstantTerm(new DoubleNode(10)))
        {
            this._log10 = true;
        }

        /// <summary>
        /// Creates a new Leviathan Log Function
        /// </summary>
        /// <param name="arg">Expression</param>
        /// <param name="logBase">Log Base Expression</param>
        public LogFunction(IExpression arg, IExpression logBase)
            : base(arg, logBase) { }

        /// <summary>
        /// Evaluates the expression
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(ISolution solution, IExpressionContext context)
        {
            IValuedNode arg = this.FirstArgument.Evaluate(solution, context);
            if (arg == null) throw new RdfQueryException("Cannot log a null");
            IValuedNode logBase = this.SecondArgument.Evaluate(solution, context);
            if (logBase == null) throw new RdfQueryException("Cannot log to a null base");

            if (arg.NumericType == EffectiveNumericType.NaN || logBase.NumericType == EffectiveNumericType.NaN) throw new RdfQueryException("Cannot log when one/both arguments are non-numeric");

            return new DoubleNode(Math.Log(arg.AsDouble(), logBase.AsDouble()));
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
    }
}
