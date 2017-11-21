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

using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Expressions.Functions.Sparql.Set
{
    /// <summary>
    /// Class representing the SPARQL IN set function
    /// </summary>
    public class InFunction
        : BaseSetFunction
    {
        /// <summary>
        /// Creates a new SPARQL IN function
        /// </summary>
        /// <param name="expr">Expression</param>
        /// <param name="set">Set</param>
        public InFunction(ISparqlExpression expr, IEnumerable<ISparqlExpression> set)
            : base(expr, set) { }

        /// <summary>
        /// Evaluates the expression
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            IValuedNode result = _expr.Evaluate(context, bindingID);
            if (result != null)
            {
                if (_expressions.Count == 0) return new BooleanNode(null, false);

                // Have to use SPARQL Value Equality here
                // If any expressions error and nothing in the set matches then an error is thrown
                bool errors = false;
                foreach (ISparqlExpression expr in _expressions)
                {
                    try
                    {
                        IValuedNode temp = expr.Evaluate(context, bindingID);
                        if (SparqlSpecsHelper.Equality(result, temp)) return new BooleanNode(null, true);
                    }
                    catch
                    {
                        errors = true;
                    }
                }

                if (errors)
                {
                    throw new RdfQueryException("One/more expressions in a Set function failed to evaluate");
                }
                else
                {
                    return new BooleanNode(null, false);
                }
            }
            else
            {
                return new BooleanNode(null, false);
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordIn;
            }
        }

        /// <summary>
        /// Gets the String representation of the Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            if (_expr.Type == SparqlExpressionType.BinaryOperator || _expr.Type == SparqlExpressionType.GraphOperator || _expr.Type == SparqlExpressionType.SetOperator) output.Append('(');
            output.Append(_expr.ToString());
            if (_expr.Type == SparqlExpressionType.BinaryOperator || _expr.Type == SparqlExpressionType.GraphOperator || _expr.Type == SparqlExpressionType.SetOperator) output.Append(')');
            output.Append(" IN (");
            for (int i = 0; i < _expressions.Count; i++)
            {
                output.Append(_expressions[i].ToString());
                if (i < _expressions.Count - 1)
                {
                    output.Append(" , ");
                }
            }
            output.Append(")");
            return output.ToString();
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new InFunction(transformer.Transform(_expr), _expressions.Select(e => transformer.Transform(e)));
        }
    }
}
