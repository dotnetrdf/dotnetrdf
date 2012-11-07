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
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Primary;
using VDS.RDF.Query.Expressions.Functions.XPath.String;

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
            this._distinct = distinct;
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
            this._distinct = distinct;
            this._customSeparator = true;
        }

        /// <summary>
        /// Creates a new GROUP_CONCAT aggregate
        /// </summary>
        /// <param name="expr">Expression</param>
        /// <param name="sepExpr">Separator Expression</param>
        public GroupConcatAggregate(ISparqlExpression expr, ISparqlExpression sepExpr)
            : base(expr, sepExpr)
        {
            this._customSeparator = true;
        }

        /// <summary>
        /// Gets the String representation of the Aggregate
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.Append("GROUP_CONCAT(");
            if (this._distinct) output.Append("DISTINCT ");
            if (this._expr is ConcatFunction)
            {
                ConcatFunction concatFunc = (ConcatFunction)this._expr;
                for (int i = 0; i < concatFunc.Arguments.Count(); i++)
                {
                    output.Append(concatFunc.Arguments.Skip(i).First().ToString());
                    if (i < concatFunc.Arguments.Count() - 1)
                    {
                        output.Append(", ");
                    }
                }
            }
            else
            {
                output.Append(this._expr.ToString());
            }
            if (this._customSeparator)
            {
                output.Append(" ; SEPARATOR = ");
                output.Append(this._sep.ToString());
            }
            output.Append(")");
            return output.ToString();
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
