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
using VDS.RDF.Parsing;

namespace VDS.RDF.Query.Expressions.Functions.XPath.String
{
    /// <summary>
    /// Represents the XPath fn:concat() function
    /// </summary>
    public class ConcatFunction
        : ISparqlExpression
    {
        private List<ISparqlExpression> _exprs = new List<ISparqlExpression>();

        /// <summary>
        /// Creates a new XPath Concatenation function
        /// </summary>
        /// <param name="first">First Expression</param>
        /// <param name="second">Second Expression</param>
        public ConcatFunction(ISparqlExpression first, ISparqlExpression second)
        {
            _exprs.Add(first);
            _exprs.Add(second);
        }

        /// <summary>
        /// Creates a new XPath Concatenation function
        /// </summary>
        /// <param name="expressions">Enumeration of expressions</param>
        public ConcatFunction(IEnumerable<ISparqlExpression> expressions)
        {
            _exprs.AddRange(expressions);
        }

        /// <summary>
        /// Gets the Value of the function as evaluated in the given Context for the given Binding ID
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            StringBuilder output = new StringBuilder();
            foreach (ISparqlExpression expr in _exprs)
            {
                IValuedNode temp = expr.Evaluate(context, bindingID);
                if (temp == null) throw new RdfQueryException("Cannot evaluate the XPath concat() function when an argument evaluates to a Null");
                switch (temp.NodeType)
                {
                    case NodeType.Literal:
                        output.Append(temp.AsString());
                        break;
                    default:
                        throw new RdfQueryException("Cannot evaluate the XPath concat() function when an argument is not a Literal Node");
                }
            }

            return new StringNode(null, output.ToString(), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
        }

        /// <summary>
        /// Gets the Arguments the function applies to
        /// </summary>
        public IEnumerable<ISparqlExpression> Arguments
        {
            get
            {
                return _exprs;
            }
        }

        /// <summary>
        /// Gets whether an expression can safely be evaluated in parallel
        /// </summary>
        public virtual bool CanParallelise
        {
            get
            {
                return _exprs.All(e => e.CanParallelise);
            }
        }

        /// <summary>
        /// Gets the Variables used in the function
        /// </summary>
        public IEnumerable<string> Variables
        {
            get
            {
                return (from expr in _exprs
                        from v in expr.Variables
                        select v);
            }
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.Append('<');
            output.Append(XPathFunctionFactory.XPathFunctionsNamespace);
            output.Append(XPathFunctionFactory.Concat);
            output.Append(">(");
            for (int i = 0; i < _exprs.Count; i++)
            {
                output.Append(_exprs[i].ToString());
                if (i < _exprs.Count - 1) output.Append(", ");
            }
            output.Append(")");
            return output.ToString();
        }

        /// <summary>
        /// Gets the Type of the Expression
        /// </summary>
        public SparqlExpressionType Type
        {
            get
            {
                return SparqlExpressionType.Function;
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public string Functor
        {
            get
            {
                return XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.Concat;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new ConcatFunction(_exprs.Select(e => transformer.Transform(e)));
        }
    }
}
