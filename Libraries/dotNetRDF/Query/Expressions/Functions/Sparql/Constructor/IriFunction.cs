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
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;

namespace VDS.RDF.Query.Expressions.Functions.Sparql.Constructor
{
    /// <summary>
    /// Class representing the SPARQL IRI() function
    /// </summary>
    public class IriFunction
        : BaseUnaryExpression
    {
        /// <summary>
        /// Creates a new IRI() function expression
        /// </summary>
        /// <param name="expr">Expression to apply the function to</param>
        public IriFunction(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Returns the value of the Expression as evaluated for a given Binding as a Literal Node
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            IValuedNode result = _expr.Evaluate(context, bindingID);
            if (result == null)
            {
                throw new RdfQueryException("Cannot create an IRI from a null");
            }
            else
            {
                switch (result.NodeType)
                {
                    case NodeType.Literal:
                        ILiteralNode lit = (ILiteralNode)result;
                        string baseUri = string.Empty;
                        if (context.Query != null) baseUri = context.Query.BaseUri.ToSafeString();
                        string uri;
                        if (lit.DataType == null)
                        {
                            uri = Tools.ResolveUri(lit.Value, baseUri);
                            return new UriNode(null, UriFactory.Create(uri));
                        }
                        else
                        {
                            string dt = lit.DataType.AbsoluteUri;
                            if (dt.Equals(XmlSpecsHelper.XmlSchemaDataTypeString, StringComparison.Ordinal))
                            {
                                uri = Tools.ResolveUri(lit.Value, baseUri);
                                return new UriNode(null, UriFactory.Create(uri));
                            }
                            else
                            {
                                throw new RdfQueryException("Cannot create an IRI from a non-string typed literal");
                            }
                        }

                    case NodeType.Uri:
                        // Already a URI so nothing to do
                        return result;
                    default:
                        throw new RdfQueryException("Cannot create an IRI from a non-URI/String literal");
                }
            }
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "IRI(" + _expr.ToString() + ")";
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
                return SparqlSpecsHelper.SparqlKeywordIri;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new IriFunction(transformer.Transform(_expr));
        }
    }
}
