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
using System.Globalization;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;

namespace VDS.RDF.Query.Expressions.Functions.XPath.Cast
{
    /// <summary>
    /// Class representing an XPath Double Cast Function
    /// </summary>
    public class DoubleCast
        : BaseCast
    {
        /// <summary>
        /// Creates a new XPath Double Cast Function Expression
        /// </summary>
        /// <param name="expr">Expression to be cast</param>
        public DoubleCast(ISparqlExpression expr) 
            : base(expr) { }

        /// <summary>
        /// Casts the value of the inner Expression to a Double
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            IValuedNode n = _expr.Evaluate(context, bindingID);//.CoerceToDouble();

            if (n == null)
            {
                throw new RdfQueryException("Cannot cast a Null to a xsd:double");
            }

            // New method should be much faster
            // if (n is DoubleNode) return n;
            // return new DoubleNode(null, n.AsDouble());

            switch (n.NodeType)
            {
                case NodeType.Blank:
                case NodeType.GraphLiteral:
                case NodeType.Uri:
                    throw new RdfQueryException("Cannot cast a Blank/URI/Graph Literal Node to a xsd:double");

                case NodeType.Literal:
                    if (n is DoubleNode) return n;
                    if (n is FloatNode) return new DoubleNode(n.Graph, n.AsDouble());
                    // See if the value can be cast
                    ILiteralNode lit = (ILiteralNode)n;
                    if (lit.DataType != null)
                    {
                        string dt = lit.DataType.ToString();
                        if (dt.Equals(XmlSpecsHelper.XmlSchemaDataTypeDouble) || dt.Equals(XmlSpecsHelper.XmlSchemaDataTypeFloat))
                        {
                            double d;
                            if (Double.TryParse(lit.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out d))
                            {
                                // Parsed OK
                                return new DoubleNode(lit.Graph, d);
                            }
                            else
                            {
                                throw new RdfQueryException("Invalid lexical form for xsd:double");
                            }
                        }
                        else if (dt.Equals(XmlSpecsHelper.XmlSchemaDataTypeDateTime))
                        {
                            // DateTime cast forbidden
                            throw new RdfQueryException("Cannot cast a xsd:dateTime to a xsd:double");
                        }
                        else
                        {
                            double d;
                            if (Double.TryParse(lit.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out d))
                            {
                                // Parsed OK
                                return new DoubleNode(lit.Graph, d);
                            }
                            else
                            {
                                throw new RdfQueryException("Cannot cast the value '" + lit.Value + "' to a xsd:double");
                            }
                        }
                    }
                    else
                    {
                        double d;
                        if (Double.TryParse(lit.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out d))
                        {
                            // Parsed OK
                            return new DoubleNode(lit.Graph, d);
                        }
                        else
                        {
                            throw new RdfQueryException("Cannot cast the value '" + lit.Value + "' to a xsd:double");
                        }
                    }
                default:
                    throw new RdfQueryException("Cannot cast an Unknown Node to a xsd:double");
            }
        }

        /// <summary>
        /// Gets the String representation of the Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + XmlSpecsHelper.XmlSchemaDataTypeDouble + ">(" + _expr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return XmlSpecsHelper.XmlSchemaDataTypeDouble;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new DoubleCast(transformer.Transform(_expr));
        }
    }
}
