/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

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
            : base(expr, new ConstantTerm(new LiteralNode(" ")))
        {
            this._distinct = distinct;
        }

        /// <summary>
        /// Creates a new GROUP_CONCAT aggregate
        /// </summary>
        /// <param name="expr">Expression</param>
        public GroupConcatAggregate(ISparqlExpression expr)
            : base(expr, new ConstantTerm(new LiteralNode(" "))) { }

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
