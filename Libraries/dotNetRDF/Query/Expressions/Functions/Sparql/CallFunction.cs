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

namespace VDS.RDF.Query.Expressions.Functions.Sparql
{
    /// <summary>
    /// Class representing the SPARQL CALL() function
    /// </summary>
    public class CallFunction 
        : ISparqlExpression
    {
        private List<ISparqlExpression> _args = new List<ISparqlExpression>();
        private Dictionary<string, ISparqlExpression> _functionCache = new Dictionary<string, ISparqlExpression>();

        /// <summary>
        /// Creates a new COALESCE function with the given expressions as its arguments
        /// </summary>
        /// <param name="expressions">Argument expressions</param>
        public CallFunction(IEnumerable<ISparqlExpression> expressions)
        {
            _args.AddRange(expressions);
        }

        /// <summary>
        /// Gets the value of the expression as evaluated in the given Context for the given Binding ID
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            if (_args.Count == 0) return null;

            IValuedNode funcIdent = _args[0].Evaluate(context, bindingID);
            if (funcIdent == null) throw new RdfQueryException("Function identifier is unbound");
            if (funcIdent.NodeType == NodeType.Uri)
            {
                Uri funcUri = ((IUriNode)funcIdent).Uri;
                ISparqlExpression func;
                if (_functionCache.TryGetValue(funcUri.AbsoluteUri, out func))
                {
                    if (func == null) throw new RdfQueryException("Function identifier does not identify a known function");
                }
                else
                {
                    try
                    {
                        // Try to create the function and cache it - remember to respect the queries Expression Factories if present
                        func = SparqlExpressionFactory.CreateExpression(funcUri, _args.Skip(1).ToList(), (context.Query != null ? context.Query.ExpressionFactories : Enumerable.Empty<ISparqlCustomExpressionFactory>()));
                        _functionCache.Add(funcUri.AbsoluteUri, func);
                    }
                    catch
                    {
                        // If something goes wrong creating the function cache a null so we ignore this function URI for later calls
                        _functionCache.Add(funcUri.AbsoluteUri, null);
                    }
                }
                // Now invoke the function
                return func.Evaluate(context, bindingID);
            }
            else
            {
                throw new RdfQueryException("Function identifier is not a URI");
            }
        }

        /// <summary>
        /// Gets the Variables used in all the argument expressions of this function
        /// </summary>
        public IEnumerable<string> Variables
        {
            get 
            {
                return (from e in _args
                        from v in e.Variables
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
            output.Append("CALL(");
            for (int i = 0; i < _args.Count; i++)
            {
                output.Append(_args[i].ToString());
                if (i < _args.Count - 1)
                {
                    output.Append(", ");
                }
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
                return SparqlSpecsHelper.SparqlKeywordCall;
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
                return false;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new CallFunction(_args.Select(e => transformer.Transform(e)));
        }
    }
}
