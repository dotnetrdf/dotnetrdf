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
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Expressions.Functions.XPath.Cast
{
    /// <summary>
    /// Class representing an XPath Float Cast Function
    /// </summary>
    public class FloatCast
        : BaseCast
    {
        /// <summary>
        /// Creates a new XPath Float Cast Function Expression
        /// </summary>
        /// <param name="expr">Expression to be cast</param>
        public FloatCast(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Casts the value of the inner Expression to a Float
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Vinding ID</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            IValuedNode n = this._expr.Evaluate(context, bindingID);//.CoerceToFloat();

            if (n == null)
            {
                throw new RdfQueryException("Cannot cast a Null to a xsd:float");
            }

            //New method should be much faster
            //if (n is FloatNode) return n;
            //return new FloatNode(null, n.AsFloat());

            switch (n.NodeType)
            {
                case NodeType.Blank:
                case NodeType.GraphLiteral:
                case NodeType.Uri:
                    throw new RdfQueryException("Cannot cast a Blank/URI/Graph Literal Node to a xsd:float");

                case NodeType.Literal:
                    if (n is FloatNode) return n;
                    //See if the value can be cast
                    ILiteralNode lit = (ILiteralNode)n;
                    if (lit.DataType != null)
                    {
                        if (lit.DataType.AbsoluteUri.Equals(XmlSpecsHelper.XmlSchemaDataTypeFloat))
                        {
                            float f;
                            if (Single.TryParse(lit.Value, out f))
                            {
                                //Parsed OK
                                return new FloatNode(lit.Graph, f);
                            }
                            else
                            {
                                throw new RdfQueryException("Invalid lexical form for a xsd:float");
                            }
                        }
                        else if (lit.DataType.AbsoluteUri.Equals(XmlSpecsHelper.XmlSchemaDataTypeDateTime))
                        {
                            //DateTime cast forbidden
                            throw new RdfQueryException("Cannot cast a xsd:dateTime to a xsd:float");
                        }
                        else
                        {
                            float f;
                            if (Single.TryParse(lit.Value, out f))
                            {
                                //Parsed OK
                                return new FloatNode(lit.Graph, f);
                            }
                            else
                            {
                                throw new RdfQueryException("Cannot cast the value '" + lit.Value + "' to a xsd:float");
                            }
                        }
                    }
                    else
                    {
                        float f;
                        if (Single.TryParse(lit.Value, out f))
                        {
                            //Parsed OK
                            return new FloatNode(lit.Graph, f);
                        }
                        else
                        {
                            throw new RdfQueryException("Cannot cast the value '" + lit.Value + "' to a xsd:float");
                        }
                    }
                default:
                    throw new RdfQueryException("Cannot cast an Unknown Node to a xsd:float");
            }
        }

        /// <summary>
        /// Gets the String representation of the Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + XmlSpecsHelper.XmlSchemaDataTypeFloat + ">(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return XmlSpecsHelper.XmlSchemaDataTypeFloat;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new FloatCast(transformer.Transform(this._expr));
        }
    }
}
