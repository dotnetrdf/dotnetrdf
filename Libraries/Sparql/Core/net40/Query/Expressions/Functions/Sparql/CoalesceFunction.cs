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

using System.Collections.Generic;
using System.Text;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Engine;
using VDS.RDF.Specifications;

namespace VDS.RDF.Query.Expressions.Functions.Sparql
{
    /// <summary>
    /// Class representing the SPARQL COALESCE() function
    /// </summary>
    public class CoalesceFunction 
        : BaseNAryExpression
    {

        /// <summary>
        /// Creates a new COALESCE function with the given expressions as its arguments
        /// </summary>
        /// <param name="expressions">Argument expressions</param>
        public CoalesceFunction(IEnumerable<IExpression> expressions)
            : base(expressions) { }

        public override IExpression Copy(IEnumerable<IExpression> args)
        {
            return new CoalesceFunction(args);
        }

        /// <summary>
        /// Gets the value of the expression as evaluated in the given Context for the given Binding ID
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(ISolution solution, IExpressionContext context)
        {
            foreach (IExpression expr in this.Arguments)
            {
                try
                {
                    //Test the expression
                    IValuedNode temp = expr.Evaluate(solution, context);

                    //Don't return nulls
                    if (temp == null) continue;

                    //Otherwise return
                    return temp;
                }
                catch (RdfQueryException)
                {
                    //Ignore the error and try the next expression (if any)
                }
            }

            //Return error if all expressions are null/error
            throw new RdfQueryException("None of the arguments to the COALESCE function could be evaluated to give non-null/error responses for the given Binding");
        }
     
        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordCoalesce;
            }
        }
    }
}
