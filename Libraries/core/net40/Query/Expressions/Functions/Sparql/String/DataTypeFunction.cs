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

namespace VDS.RDF.Query.Expressions.Functions.Sparql.String
{
    /// <summary>
    /// Class representing the SPARQL Datatype() function
    /// </summary>
    public class DataTypeFunction
        : BaseUnaryExpression
    {
        /// <summary>
        /// Creates a new Datatype() function expression
        /// </summary>
        /// <param name="expr">Expression to apply the function to</param>
        public DataTypeFunction(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Returns the value of the Expression as evaluated for a given Binding as a Literal Node
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            INode result = this._expr.Evaluate(context, bindingID);
            if (result == null)
            {
                throw new RdfQueryException("Cannot return the Data Type URI of a NULL");
            }
            else
            {
                switch (result.NodeType)
                {
                    case NodeType.Literal:
                        ILiteralNode lit = (ILiteralNode)result;
                        if (lit.DataType == null)
                        {
                            if (!lit.Language.Equals(string.Empty))
                            {
                                throw new RdfQueryException("Cannot return the Data Type URI of Language Specified Literals in SPARQL 1.0");
                            }
                            else
                            {
                                return new UriNode(UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
                            }
                        }
                        else
                        {
                            return new UriNode(lit.DataType);
                        }

                    default:
                        throw new RdfQueryException("Cannot return the Data Type URI of Nodes which are not Literal Nodes");
                }
            }
        }

        /// <summary>
        /// Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "DATATYPE(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Type of the Expression
        /// </summary>
        public override SparqlExpressionType Type
        {
            get
            {
                return SparqlExpressionType.Function;
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordDataType;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new DataTypeFunction(transformer.Transform(this._expr));
        }
    }

    /// <summary>
    /// Class representing the SPARQL Datatype() function in SPARQL 1.1
    /// </summary>
    /// <remarks>
    /// This is required because the changes to the function in SPARQL 1.1 are not backwards compatible with SPARQL 1.0
    /// </remarks>
    public class DataType11Function
        : DataTypeFunction
    {
        /// <summary>
        /// Creates a new DataType function
        /// </summary>
        /// <param name="expr">Expression</param>
        public DataType11Function(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Returns the value of the Expression as evaluated for a given Binding as a Literal Node
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            INode result = this._expr.Evaluate(context, bindingID);
            if (result == null)
            {
                throw new RdfQueryException("Cannot return the Data Type URI of a NULL");
            }
            else
            {
                switch (result.NodeType)
                {
                    case NodeType.Literal:
                        ILiteralNode lit = (ILiteralNode)result;
                        if (lit.DataType == null)
                        {
                            if (!lit.Language.Equals(string.Empty))
                            {
                                return new UriNode(UriFactory.Create(RdfSpecsHelper.RdfLangString));
                            }
                            else
                            {
                                return new UriNode(UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
                            }
                        }
                        else
                        {
                            return new UriNode(lit.DataType);
                        }

                    default:
                        throw new RdfQueryException("Cannot return the Data Type URI of Nodes which are not Literal Nodes");
                }
            }
        }
    }
}
