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

namespace VDS.RDF.Query.Expressions.Conditional
{
    /// <summary>
    /// Class representing Conditional Or expressions
    /// </summary>
    public class OrExpression
        : BaseBinaryExpression
    {
        /// <summary>
        /// Creates a new Conditional Or Expression
        /// </summary>
        /// <param name="leftExpr">Left Hand Expression</param>
        /// <param name="rightExpr">Right Hand Expression</param>
        public OrExpression(IExpression leftExpr, IExpression rightExpr) : base(leftExpr, rightExpr) { }

        public override IExpression Copy(IExpression arg1, IExpression arg2)
        {
            return new OrExpression(arg1, arg2);
        }

        /// <summary>
        /// Evaluates the expression
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(ISolution solution, IExpressionContext context)
        {
            //Lazy Evaluation for efficiency
            try
            {
                bool leftResult = this.FirstArgument.Evaluate(solution, context).AsBoolean();
                if (leftResult)
                {
                    //If the LHS is true it doesn't matter about any subsequent results
                    return new BooleanNode(true);
                }
                //If the LHS is false then we have to evaluate the RHS
                return new BooleanNode(this.SecondArgument.Evaluate(solution, context).AsBoolean());
            }
            catch (Exception ex)
            {
                //If there's an Error on the LHS we return true only if the RHS evaluates to true
                //Otherwise we throw the Error
                bool rightResult = this.SecondArgument.Evaluate(solution, context).AsSafeBoolean();
                if (rightResult)
                {
                    return new BooleanNode(true);
                }
                //Ensure the error we throw is a RdfQueryException so as not to cause issues higher up
                if (ex is RdfQueryException)
                {
                    throw;
                }
                throw new RdfQueryException("Error evaluating OR expression", ex);
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return "||";
            }
        }
    }
}
