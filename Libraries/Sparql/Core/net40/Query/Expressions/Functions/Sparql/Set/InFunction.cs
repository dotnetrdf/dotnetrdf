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

using System.Collections.Generic;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Engine;
using VDS.RDF.Specifications;

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
        public InFunction(IExpression expr, IEnumerable<IExpression> set)
            : base(expr, set) { }

        /// <summary>
        /// Evaluates the expression
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(ISolution solution, IExpressionContext context)
        {
            IValuedNode result = this._expr.Evaluate(solution, context);
            if (result == null) return new BooleanNode(false);
            if (this._expressions.Count == 0) return new BooleanNode(false);

            //Have to use SPARQL Value Equality here
            //If any expressions error and nothing in the set matches then an error is thrown
            bool errors = false;
            foreach (IExpression expr in this._expressions)
            {
                try
                {
                    IValuedNode temp = expr.Evaluate(solution, context);
                    if (SparqlSpecsHelper.Equality(result, temp)) return new BooleanNode(true);
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
            return new BooleanNode(false);
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
    }
}
