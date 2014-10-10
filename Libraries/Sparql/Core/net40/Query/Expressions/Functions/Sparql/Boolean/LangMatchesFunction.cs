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
using VDS.RDF.Specifications;

namespace VDS.RDF.Query.Expressions.Functions.Sparql.Boolean
{
    /// <summary>
    /// Class representing the Sparql LangMatches() function
    /// </summary>
    public class LangMatchesFunction
        : BaseBinaryExpression
    {
        /// <summary>
        /// Creates a new LangMatches() function expression
        /// </summary>
        /// <param name="term">Expression to obtain the Language of</param>
        /// <param name="langRange">Expression representing the Language Range to match</param>
        public LangMatchesFunction(IExpression term, IExpression langRange)
            : base(term, langRange) { }

        public override IExpression Copy(IExpression arg1, IExpression arg2)
        {
            return new LangMatchesFunction(arg1, arg2);
        }

        /// <summary>
        /// Computes the Effective Boolean Value of this Expression as evaluated for a given Binding
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(ISolution solution, IExpressionContext context)
        {
            INode result = this.FirstArgument.Evaluate(solution, context);
            INode langRange = this.SecondArgument.Evaluate(solution, context);

            if (result == null)
            {
                return new BooleanNode(false);
            }
            if (result.NodeType != NodeType.Literal) return new BooleanNode(false);
            if (langRange == null)
            {
                return new BooleanNode(false);
            }
            if (langRange.NodeType != NodeType.Literal) return new BooleanNode(false);
            string range = (langRange).Value;
            string lang = (result).Value;

            if (range.Equals("*"))
            {
                return new BooleanNode(!lang.Equals(string.Empty));
            }
            return new BooleanNode(lang.Equals(range, StringComparison.OrdinalIgnoreCase) || lang.StartsWith(range + "-", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordLangMatches;
            }
        }
    }
}
