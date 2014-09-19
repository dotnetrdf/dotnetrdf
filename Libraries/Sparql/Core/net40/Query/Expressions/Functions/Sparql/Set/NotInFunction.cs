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
using System.Linq;
using System.Text;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Engine;
using VDS.RDF.Specifications;

namespace VDS.RDF.Query.Expressions.Functions.Sparql.Set
{
    /// <summary>
    /// Class representing the SPARQL NOT IN set function
    /// </summary>
    public class NotInFunction
        : BaseSetFunction
    {
        /// <summary>
        /// Creates a new SPARQL NOT IN function
        /// </summary>
        /// <param name="expr">Expression</param>
        /// <param name="set">Set</param>
        public NotInFunction(IExpression expr, IEnumerable<IExpression> set)
            : base(expr, set) { }

        public override IExpression Copy(IEnumerable<IExpression> args)
        {
            IEnumerable<IExpression> expressions = args as IList<IExpression> ?? args.ToList();
            return new NotInFunction(expressions.First(), expressions.Skip(1));
        }

        /// <summary>
        /// Evaluates the expression
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(ISolution solution, IExpressionContext context)
        {
            INode result = this.Arguments[0].Evaluate(solution, context);
            if (result == null) return new BooleanNode(true);
            if (this.Arguments.Count == 1) return new BooleanNode(true);

            //Have to use SPARQL Value Equality here
            //If any expressions error and nothing in the set matches then an error is thrown
            bool errors = false;
            foreach (IExpression expr in this.Arguments.Skip(1))
            {
                try
                {
                    INode temp = expr.Evaluate(solution, context);
                    if (SparqlSpecsHelper.Equality(result, temp)) return new BooleanNode(false);
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
            return new BooleanNode(true);
        }

        public override bool Equals(IExpression other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            if (!(other is NotInFunction)) return false;

            NotInFunction func = (NotInFunction) other;
            if (this.Arguments.Count != func.Arguments.Count) return false;
            for (int i = 0; i < this.Arguments.Count; i++)
            {
                if (!this.Arguments[i].Equals(func.Arguments[i])) return false;
            }
            return true;
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordNotIn;
            }
        }
    }
}
