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
using VDS.RDF.Parsing;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Aggregates.XPath
{
    /// <summary>
    /// Represents the XPath fn:string-join() aggregate
    /// </summary>
    public class StringJoinAggregate
        : BaseAggregate
    {
        /// <summary>
        /// Separator Expression
        /// </summary>
        protected ISparqlExpression _sep;
        private bool _customSep = true;

        /// <summary>
        /// Creates a new XPath String Join aggregate which uses no separator
        /// </summary>
        /// <param name="expr">Expression</param>
        public StringJoinAggregate(ISparqlExpression expr)
            : this(expr, new ConstantTerm(new LiteralNode(null, String.Empty)))
        {
            this._customSep = false;
        }

        /// <summary>
        /// Creates a new XPath String Join aggregate
        /// </summary>
        /// <param name="expr">Expression</param>
        /// <param name="sep">Separator Expression</param>
        public StringJoinAggregate(ISparqlExpression expr, ISparqlExpression sep)
            : base(expr)
        {
            this._sep = sep;
        }

        /// <summary>
        /// Applies the Aggregate in the given Context over the given Binding IDs
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingIDs">Binding IDs</param>
        /// <returns></returns>
        public override IValuedNode Apply(SparqlEvaluationContext context, IEnumerable<int> bindingIDs)
        {
            List<int> ids = bindingIDs.ToList();
            StringBuilder output = new StringBuilder();
            HashSet<String> values = new HashSet<string>();
            for (int i = 0; i < ids.Count; i++)
            {
                try
                {
                    String temp = this.ValueInternal(context, ids[i]);

                    //Apply DISTINCT modifer if required
                    if (this._distinct)
                    {
                        if (values.Contains(temp))
                        {
                            continue;
                        }
                        else
                        {
                            values.Add(temp);
                        }
                    }
                    output.Append(temp);
                }
                catch (RdfQueryException)
                {
                    output.Append(String.Empty);
                }

                //Append Separator if required
                if (i < ids.Count - 1)
                {
                    String sep = this.GetSeparator(context, ids[i]);
                    output.Append(sep);
                }
            }

            return new StringNode(null, output.ToString(), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
        }

        /// <summary>
        /// Gets the value of a member of the Group for concatenating as part of the result for the Group
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        private String ValueInternal(SparqlEvaluationContext context, int bindingID)
        {
            IValuedNode temp = this._expr.Evaluate(context, bindingID);
            if (temp == null) throw new RdfQueryException("Cannot do an XPath string-join on a null");
            if (temp.NodeType == NodeType.Literal)
            {
                ILiteralNode l = (ILiteralNode)temp;
                if (l.DataType != null)
                {
                    if (l.DataType.AbsoluteUri.Equals(XmlSpecsHelper.XmlSchemaDataTypeString))
                    {
                        return temp.AsString();
                    }
                    else
                    {
                        throw new RdfQueryException("Cannot do an XPath string-join on a Literal which is not typed as a String");
                    }
                }
                else
                {
                    return temp.AsString();
                }
            }
            else
            {
                throw new RdfQueryException("Cannot do an XPath string-join on a non-Literal Node");
            }
        }

        /// <summary>
        /// Gets the separator to use in the concatenation
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        private String GetSeparator(SparqlEvaluationContext context, int bindingID)
        {
            INode temp = this._sep.Evaluate(context, bindingID);
            if (temp == null)
            {
                return String.Empty;
            }
            else if (temp.NodeType == NodeType.Literal)
            {
                ILiteralNode l = (ILiteralNode)temp;
                if (l.DataType != null)
                {
                    if (l.DataType.AbsoluteUri.Equals(XmlSpecsHelper.XmlSchemaDataTypeString))
                    {
                        return l.Value;
                    }
                    else
                    {
                        throw new RdfQueryException("Cannot evaluate an XPath string-join since the separator expression returns a typed Literal which is not a String");
                    }
                }
                else
                {
                    return l.Value;
                }
            }
            else
            {
                throw new RdfQueryException("Cannot evaluate an XPath string-join since the separator expression does not return a Literal");
            }
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.Append('<');
            output.Append(XPathFunctionFactory.XPathFunctionsNamespace);
            output.Append(XPathFunctionFactory.StringJoin);
            output.Append(">(");
            if (this._distinct) output.Append("DISTINCT ");
            output.Append(this._expr.ToString());
            if (this._customSep)
            {
                output.Append(this._sep.ToString());
            }
            output.Append(')');
            return output.ToString();
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.StringJoin;
            }
        }
    }
}
