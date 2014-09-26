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
using VDS.RDF.Nodes;
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Expressions.Factories;
using VDS.RDF.Specifications;

namespace VDS.RDF.Query.Expressions.Functions.Sparql.String
{
    /// <summary>
    /// Represents the SPARQL STRBEFORE function
    /// </summary>
    public class StrBeforeFunction
        : BaseBinaryExpression
    {
        /// <summary>
        /// Creates a new STRBEFORE Function
        /// </summary>
        /// <param name="stringExpr">String Expression</param>
        /// <param name="startsExpr">Starts Expression</param>
        public StrBeforeFunction(IExpression stringExpr, IExpression startsExpr)
            : base(stringExpr, startsExpr) { }

        public override IExpression Copy(IExpression arg1, IExpression arg2)
        {
            return new StrBeforeFunction(arg1, arg2);
        }

        /// <summary>
        /// Returns the value of the Expression as evaluated for a given Binding as a Literal Node
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(ISolution solution, IExpressionContext context)
        {
            INode input = this.CheckArgument(this.FirstArgument, solution, context);
            INode ends = this.CheckArgument(this.SecondArgument, solution, context);

            if (!SparqlSpecsHelper.IsValidStringArgumentPair(input, ends)) throw new RdfQueryException("The Literals provided as arguments to this SPARQL String function are not of valid forms (see SPARQL spec for acceptable combinations)");

            if (input.Value.Contains(ends.Value))
            {
                // TODO This won't match the possibly user defined culture
                int endIndex = input.Value.IndexOf(ends.Value, StringComparison.InvariantCulture);
                string resultValue = (endIndex == 0 ? string.Empty : input.Value.Substring(0, endIndex));

                if (input.HasLanguage)
                {
                    return new StringNode(resultValue, input.Language);
                }
                return input.HasDataType ? new StringNode(resultValue, input.DataType) : new StringNode(resultValue);
            }
            if (ends.Value.Equals(string.Empty))
            {
                if (input.HasLanguage)
                {
                    return new StringNode(string.Empty/*, lang*/);
                }
                return input.HasDataType ? new StringNode(string.Empty, input.DataType) : new StringNode(string.Empty);
            }
            return new StringNode(string.Empty);
        }

        private INode CheckArgument(IExpression expr, ISolution solution, IExpressionContext context)
        {
            return this.CheckArgument(expr, solution, context, XPathFunctionFactory.AcceptStringArguments);
        }

        private INode CheckArgument(IExpression expr, ISolution solution, IExpressionContext context, Func<Uri, bool> argumentTypeValidator)
        {
            INode temp = expr.Evaluate(solution, context);
            if (temp == null) throw new RdfQueryException("Unable to evaluate as one of the argument expressions evaluated to null");
            if (temp.NodeType != NodeType.Literal) throw new RdfQueryException("Unable to evaluate as one of the argument expressions returned a non-literal");
            INode lit = temp;
            if (lit.DataType != null)
            {
                if (argumentTypeValidator(lit.DataType))
                {
                    //Appropriately typed literals are fine
                    return lit;
                }
                throw new RdfQueryException("Unable to evaluate as one of the argument expressions returned a typed literal with an invalid type");
            }
            if (argumentTypeValidator(UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString)))
            {
                // Untyped Literals are treated as Strings and may be returned when the argument allows strings
                return lit;
            }
            throw new RdfQueryException("Unable to evalaute as one of the argument expressions returned an untyped literal");
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordStrBefore;
            }
        }
    }
}
