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
using System.Collections.Generic;
using System.Linq;
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
        : BaseNAryExpression
    {
        private readonly Dictionary<string, IExpression> _functionCache = new Dictionary<string, IExpression>();

        /// <summary>
        /// Creates a new COALESCE function with the given expressions as its arguments
        /// </summary>
        /// <param name="expressions">Argument expressions</param>
        public CallFunction(IEnumerable<IExpression> expressions)
            : base(expressions) { }

        public override IExpression Copy(IEnumerable<IExpression> args)
        {
            return new CallFunction(args);
        }

        /// <summary>
        /// Gets the value of the expression as evaluated in the given Context for the given Binding ID
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(ISolution solution, IExpressionContext context)
        {
            if (this.Arguments.Count == 0) return null;

            IValuedNode funcIdent = this.Arguments[0].Evaluate(solution, context);
            if (funcIdent == null) throw new RdfQueryException("Function identifier is unbound");
            if (funcIdent.NodeType != NodeType.Uri) throw new RdfQueryException("Function identifier is not a URI");
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
                    func = ExpressionFactory.CreateExpression(funcUri, this.Arguments.Skip(1).ToList());
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

      

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordCall;
            }
        }

        /// <summary>
        /// Gets whether an expression can safely be evaluated in parallel
        /// </summary>
        public override bool CanParallelise
        {
            get
            {
                return false;
            }
        }
    }
}
