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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Engine;
using VDS.RDF.Specifications;

namespace VDS.RDF.Query.Expressions.Functions.Sparql
{
    /// <summary>
    /// Class representing the SPARQL IF function
    /// </summary>
    public class IfElseFunction 
        : BaseTernaryExpression
    {
        /// <summary>
        /// Creates a new IF function
        /// </summary>
        /// <param name="condition">Condition</param>
        /// <param name="ifBranch">Expression to evaluate if condition evaluates to true</param>
        /// <param name="elseBranch">Expression to evalaute if condition evaluates to false/error</param>
        public IfElseFunction(IExpression condition, IExpression ifBranch, IExpression elseBranch)
            : base(condition, ifBranch, elseBranch) { }

        public override IExpression Copy(IExpression arg1, IExpression arg2, IExpression arg3)
        {
            return new IfElseFunction(arg1, arg2, arg3);
        }

        /// <summary>
        /// Gets the value of the expression as evaluated in the given Context for the given Binding ID
        /// </summary>
        /// <param name="context">SPARQL Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(ISolution solution, IExpressionContext context)
        {
            IValuedNode result = this.FirstArgument.Evaluate(solution, context);

            //Condition evaluated without error so we go to the appropriate branch of the IF ELSE
            //depending on whether it evaluated to true or false
            return result.AsSafeBoolean() ? this.SecondArgument.Evaluate(solution, context) : this.ThirdArgument.Evaluate(solution, context);
        }

        /// <summary>
        /// Gets the Functor for the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordIf;
            }
        }
    }
}
