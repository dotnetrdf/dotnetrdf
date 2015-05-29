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

using System.Text;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Sorting;

namespace VDS.RDF.Query.Expressions.Comparison
{
    /// <summary>
    /// Class representing Relational Greater Than or Equal To Expressions
    /// </summary>
    public class GreaterThanOrEqualToExpression
        : BaseBinaryExpression
    {
        private readonly SparqlNodeComparer _comparer = new SparqlNodeComparer();

        /// <summary>
        /// Creates a new Greater Than or Equal To Relational Expression
        /// </summary>
        /// <param name="leftExpr">Left Hand Expression</param>
        /// <param name="rightExpr">Right Hand Expression</param>
        public GreaterThanOrEqualToExpression(IExpression leftExpr, IExpression rightExpr) : base(leftExpr, rightExpr) { }

        public override IExpression Copy(IExpression arg1, IExpression arg2)
        {
            return new GreaterThanOrEqualToExpression(arg1, arg2);
        }

        /// <summary>
        /// Evaluates the expression
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(ISolution solution, IExpressionContext context)
        {
            IValuedNode a, b;
            a = this.FirstArgument.Evaluate(solution, context);
            b = this.SecondArgument.Evaluate(solution, context);

            if (a == null)
            {
                if (b == null)
                {
                    return new BooleanNode(true);
                }
                throw new RdfQueryException("Cannot evaluate a >= when one argument is null");
            }

            int compare = this._comparer.Compare(a, b);
            return new BooleanNode(compare >= 0);
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return ">=";
            }
        }
    }
}
