/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using System;
using VDS.RDF.Nodes;

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
        public LangMatchesFunction(ISparqlExpression term, ISparqlExpression langRange)
            : base(term, langRange) { }

        /// <summary>
        /// Computes the Effective Boolean Value of this Expression as evaluated for a given Binding
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            INode result = _leftExpr.Evaluate(context, bindingID);
            INode langRange = _rightExpr.Evaluate(context, bindingID);

            if (result == null)
            {
                return new BooleanNode(null, false);
            }
            if (result.NodeType == NodeType.Literal)
            {
                if (langRange == null)
                {
                    return new BooleanNode(null, false);
                }
                if (langRange.NodeType == NodeType.Literal)
                {
                    string range = ((ILiteralNode)langRange).Value;
                    string lang = ((ILiteralNode)result).Value;

                    if (range.Equals("*"))
                    {
                        return new BooleanNode(null, !lang.Equals(string.Empty));
                    }
                    return new BooleanNode(null, lang.Equals(range, StringComparison.OrdinalIgnoreCase) || lang.StartsWith(range + "-", StringComparison.OrdinalIgnoreCase));
                }
                return new BooleanNode(null, false);
            }
            return new BooleanNode(null, false);
        }

        /// <summary>
        /// Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "LANGMATCHES(" + _leftExpr.ToString() + "," + _rightExpr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Type of the Expression
        /// </summary>
        public override SparqlExpressionType Type
        {
            get
            {
                return SparqlExpressionType.Function;
            }
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

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new LangMatchesFunction(transformer.Transform(_leftExpr), transformer.Transform(_rightExpr));
        }
    }
}
