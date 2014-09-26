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

using VDS.RDF.Nodes;
using VDS.RDF.Query.Engine;
using VDS.RDF.Specifications;

namespace VDS.RDF.Query.Expressions.Functions.Sparql.Boolean
{
    /// <summary>
    /// Class representing the Sparql SameTerm() function
    /// </summary>
    public class SameTermFunction
        : BaseBinaryExpression
    {
        /// <summary>
        /// Creates a new SameTerm() function expression
        /// </summary>
        /// <param name="term1">First Term</param>
        /// <param name="term2">Second Term</param>
        public SameTermFunction(IExpression term1, IExpression term2)
            : base(term1, term2) { }

        public override IExpression Copy(IExpression arg1, IExpression arg2)
        {
            return new SameTermFunction(arg1, arg2);
        }

        /// <summary>
        /// Computes the Effective Boolean Value of this Expression as evaluated for a given Binding
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(ISolution solution, IExpressionContext context)
        {
            INode a = this.FirstArgument.Evaluate(solution, context);
            INode b = this.SecondArgument.Evaluate(solution, context);

            return new BooleanNode(EqualityHelper.AreNodesEqual(a, b));
        }

        public override bool Equals(IExpression other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            if (!(other is SameTermFunction)) return false;

            SameTermFunction func = (SameTermFunction) other;
            return this.FirstArgument.Equals(func.FirstArgument) && this.SecondArgument.Equals(func.SecondArgument);
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordSameTerm;
            }
        }
    }
}
