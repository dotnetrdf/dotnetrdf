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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Expressions.Functions
{
    /// <summary>
    /// Represents an Unknown Function that is not supported by dotNetRDF
    /// </summary>
    /// <remarks>
    /// <para>
    /// This exists as a placeholder class so users may choose to parse Unknown Functions and have them appear in queries even if they cannot be evaluated.  This is useful when you wish to parse a query locally to check syntactic validity before passing it to an external query processor which may understand how to evaluate the function.  Using this placeholder also allows queries containing Unknown Functions to still be formatted properly.
    /// </para>
    /// </remarks>
    public class UnknownFunction
        : ISparqlExpression
    {
        private Uri _funcUri;
        private List<ISparqlExpression> _args = new List<ISparqlExpression>();

        /// <summary>
        /// Creates a new Unknown Function that has no Arguments
        /// </summary>
        /// <param name="funcUri">Function URI</param>
        public UnknownFunction(Uri funcUri)
        {
            _funcUri = funcUri;
        }

        /// <summary>
        /// Creates a new Unknown Function that has a Single Argument
        /// </summary>
        /// <param name="funcUri">Function URI</param>
        /// <param name="expr">Argument Expression</param>
        public UnknownFunction(Uri funcUri, ISparqlExpression expr)
            : this(funcUri)
        {
            _args.Add(expr);
        }

        /// <summary>
        /// Creates a new Unknown Function that has multiple Arguments
        /// </summary>
        /// <param name="funcUri">Function URI</param>
        /// <param name="exprs">Argument Expressions</param>
        public UnknownFunction(Uri funcUri, IEnumerable<ISparqlExpression> exprs)
            : this(funcUri)
        {
            _args.AddRange(exprs);
        }

        /// <summary>
        /// Gives null as the Value since dotNetRDF does not know how to evaluate Unknown Functions
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            return null;
        }

        /// <summary>
        /// Gets the Variables used in the Function
        /// </summary>
        public IEnumerable<string> Variables
        {
            get 
            {
                return (from arg in _args
                        from v in arg.Variables
                        select v).Distinct();
            }
        }

        /// <summary>
        /// Gets the Expression Type
        /// </summary>
        public SparqlExpressionType Type
        {
            get
            {
                return SparqlExpressionType.Function; 
            }
        }

        /// <summary>
        /// Gets the Function URI of the Expression
        /// </summary>
        public string Functor
        {
            get 
            {
                return _funcUri.ToString(); 
            }
        }

        /// <summary>
        /// Gets the Arguments of the Expression
        /// </summary>
        public IEnumerable<ISparqlExpression> Arguments
        {
            get
            {
                return _args; 
            }
        }

        /// <summary>
        /// Gets whether an expression can safely be evaluated in parallel
        /// </summary>
        public virtual bool CanParallelise
        {
            get
            {
                return _args.All(a => a.CanParallelise);
            }
        }

        /// <summary>
        /// Gets the String representation of the Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.Append('<');
            output.Append(_funcUri.AbsoluteUri.Replace(">", "\\>"));
            output.Append('>');
            output.Append('(');
            for (int i = 0; i < _args.Count; i++)
            {
                output.Append(_args[i].ToString());

                if (i < _args.Count - 1)
                {
                    output.Append(", ");
                }
            }
            output.Append(')');
            return output.ToString();
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new UnknownFunction(_funcUri, _args.Select(e => transformer.Transform(e)));
        }
    }
}
