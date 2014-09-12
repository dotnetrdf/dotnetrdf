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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Expressions.Factories;
using VDS.RDF.Specifications;

namespace VDS.RDF.Query.Expressions.Functions.Sparql
{
    /// <summary>
    /// Class representing the SPARQL CALL() function
    /// </summary>
    public class CallFunction 
        : IExpression
    {
        private List<IExpression> _args = new List<IExpression>();
        private Dictionary<string, IExpression> _functionCache = new Dictionary<string, IExpression>();

        /// <summary>
        /// Creates a new COALESCE function with the given expressions as its arguments
        /// </summary>
        /// <param name="expressions">Argument expressions</param>
        public CallFunction(IEnumerable<IExpression> expressions)
        {
            this._args.AddRange(expressions);
        }

        /// <summary>
        /// Gets the value of the expression as evaluated in the given Context for the given Binding ID
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public IValuedNode Evaluate(ISolution solution, IExpressionContext context)
        {
            if (this._args.Count == 0) return null;

            IValuedNode funcIdent = this._args[0].Evaluate(solution, context);
            if (funcIdent == null) throw new RdfQueryException("Function identifier is unbound");
            if (funcIdent.NodeType == NodeType.Uri)
            {
                Uri funcUri = funcIdent.Uri;
                IExpression func;
                if (this._functionCache.TryGetValue(funcUri.AbsoluteUri, out func))
                {
                    if (func == null) throw new RdfQueryException("Function identifier does not identify a known function");
                }
                else
                {
                    try
                    {
                        //Try to create the function and cache it - remember to respect the queries Expression Factories if present
                        func = ExpressionFactory.CreateExpression(funcUri, this._args.Skip(1).ToList(), (context.Query != null ? context.Query.ExpressionFactories : Enumerable.Empty<IExpressionFactory>()));
                        this._functionCache.Add(funcUri.AbsoluteUri, func);
                    }
                    catch
                    {
                        //If something goes wrong creating the function cache a null so we ignore this function URI for later calls
                        this._functionCache.Add(funcUri.AbsoluteUri, null);
                    }
                }
                //Now invoke the function
                if (func == null) throw new RdfQueryException("No function " + funcUri.AbsoluteUri + " available to call");
                return func.Evaluate(solution, context);
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
                return (from e in this._args
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
            for (int i = 0; i < this._args.Count; i++)
            {
                output.Append(this._args[i].ToString());
                if (i < this._args.Count - 1)
                {
                    output.Append(", ");
                }
            }
            output.Append(")");
            return output.ToString();
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
        public IEnumerable<IExpression> Arguments
        {
            get
            {
                return this._args;
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
    }
}
