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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Expressions;
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
            _customSep = false;
        }

        /// <summary>
        /// Creates a new XPath String Join aggregate
        /// </summary>
        /// <param name="expr">Expression</param>
        /// <param name="sep">Separator Expression</param>
        public StringJoinAggregate(ISparqlExpression expr, ISparqlExpression sep)
            : base(expr)
        {
            _sep = sep;
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
                    String temp = ValueInternal(context, ids[i]);

                    // Apply DISTINCT modifer if required
                    if (_distinct)
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

                // Append Separator if required
                if (i < ids.Count - 1)
                {
                    String sep = GetSeparator(context, ids[i]);
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
        protected virtual String ValueInternal(SparqlEvaluationContext context, int bindingID)
        {
            IValuedNode temp = _expr.Evaluate(context, bindingID);
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
            INode temp = _sep.Evaluate(context, bindingID);
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
            if (_distinct) output.Append("DISTINCT ");
            output.Append(_expr.ToString());
            if (_customSep)
            {
                output.Append(_sep.ToString());
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
