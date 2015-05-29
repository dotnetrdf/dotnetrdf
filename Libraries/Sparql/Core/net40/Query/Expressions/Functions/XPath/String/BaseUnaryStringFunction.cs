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

using VDS.RDF.Nodes;
using VDS.RDF.Query.Engine;
using VDS.RDF.Specifications;

namespace VDS.RDF.Query.Expressions.Functions.XPath.String
{
    /// <summary>
    /// Abstract Base Class for XPath Unary String functions
    /// </summary>
    public abstract class BaseUnaryStringFunction
        : BaseUnaryExpression
    {
        /// <summary>
        /// Creates a new XPath Unary String function
        /// </summary>
        /// <param name="stringExpr">Expression</param>
        protected BaseUnaryStringFunction(IExpression stringExpr)
            : base(stringExpr) { }

        /// <summary>
        /// Gets the Value of the function as evaluated in the given Context for the given Binding ID
        /// </summary>
        /// <param name="context">Context</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(ISolution solution, IExpressionContext context)
        {
            IValuedNode temp = this.Argument.Evaluate(solution, context);
            if (temp == null) throw new RdfQueryException("Unable to evaluate an XPath String function on a null input");
            if (temp.NodeType != NodeType.Literal) throw new RdfQueryException("Unable to evaluate an XPath String function on a non-Literal input");
            if (!temp.HasDataType || temp.HasLanguage) return this.EvaluateInternal(temp);
            if (temp.DataType.AbsoluteUri.Equals(XmlSpecsHelper.XmlSchemaDataTypeString))
            {
                return this.EvaluateInternal(temp);
            }
            throw new RdfQueryException("Unable to evalaute an XPath String function on a non-string typed Literal");
        }

        /// <summary>
        /// Gets the Value of the function as applied to the given String Literal
        /// </summary>
        /// <param name="stringLit">Simple/String typed Literal</param>
        /// <returns></returns>
        protected abstract IValuedNode EvaluateInternal(INode stringLit);
    }
}
