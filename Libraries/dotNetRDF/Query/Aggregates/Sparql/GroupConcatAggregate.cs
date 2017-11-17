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
using System.Text;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Aggregates.Sparql
{
    /// <summary>
    /// Class representing GROUP_CONCAT Aggregate
    /// </summary>
    public class GroupConcatAggregate
        : XPath.StringJoinAggregate
    {
        private bool _customSeparator = false;

        /// <summary>
        /// Creates a new GROUP_CONCAT aggregate
        /// </summary>
        /// <param name="expr">Expression</param>
        /// <param name="distinct">Should a distinct modifer be applied</param>
        public GroupConcatAggregate(ISparqlExpression expr, bool distinct)
            : base(expr, new ConstantTerm(new LiteralNode(null, " ")))
        {
            _distinct = distinct;
        }

        /// <summary>
        /// Creates a new GROUP_CONCAT aggregate
        /// </summary>
        /// <param name="expr">Expression</param>
        public GroupConcatAggregate(ISparqlExpression expr)
            : base(expr, new ConstantTerm(new LiteralNode(null, " "))) { }

        /// <summary>
        /// Creates a new GROUP_CONCAT aggregate
        /// </summary>
        /// <param name="expr">Expression</param>
        /// <param name="sepExpr">Separator Expression</param>
        /// <param name="distinct">Should a distinct modifer be applied</param>
        public GroupConcatAggregate(ISparqlExpression expr, ISparqlExpression sepExpr, bool distinct)
            : base(expr, sepExpr)
        {
            _distinct = distinct;
            _customSeparator = true;
        }

        /// <summary>
        /// Creates a new GROUP_CONCAT aggregate
        /// </summary>
        /// <param name="expr">Expression</param>
        /// <param name="sepExpr">Separator Expression</param>
        public GroupConcatAggregate(ISparqlExpression expr, ISparqlExpression sepExpr)
            : base(expr, sepExpr)
        {
            _customSeparator = true;
        }

        /// <summary>
        /// Applies the aggregate over the given bindings
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingIDs">Binding IDs</param>
        /// <returns></returns>
        public override IValuedNode Apply(SparqlEvaluationContext context, IEnumerable<int> bindingIDs)
        {
            IValuedNode n = base.Apply(context, bindingIDs);
            return new StringNode(n.Graph, n.AsString());
        }

        /// <summary>
        /// Gets the String representation of the Aggregate
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.Append("GROUP_CONCAT(");
            if (_distinct) output.Append("DISTINCT ");
            output.Append(_expr.ToString());
            if (_customSeparator)
            {
                output.Append(" ; SEPARATOR = ");
                output.Append(_sep.ToString());
            }
            output.Append(")");
            return output.ToString();
        }

        /// <summary>
        /// Gets the value of the aggregate for the given binding
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        protected override string ValueInternal(SparqlEvaluationContext context, int bindingID)
        {
            IValuedNode temp = _expr.Evaluate(context, bindingID);
            if (temp == null) throw new RdfQueryException("Cannot do an XPath string-join on a null");
            switch (temp.NodeType)
            {
                case NodeType.Literal:
                case NodeType.Uri:
                    return temp.AsString();
                default:
                    throw new RdfQueryException("Cannot do an XPath string-join on a non-Literal Node");
            }
        }

        /// <summary>
        /// Gets the Functor of the Aggregate
        /// </summary>
        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordGroupConcat;
            }
        }
    }
}
