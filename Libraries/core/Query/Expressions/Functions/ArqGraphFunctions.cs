/*

Copyright Robert Vesse 2009-10
rvesse@vdesign-studios.com

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

namespace VDS.RDF.Query.Expressions.Functions
{
    /// <summary>
    /// Represents the ARQ afn:bnode() function
    /// </summary>
    public class ArqBNodeFunction 
        : BaseUnaryExpression
    {
        /// <summary>
        /// Creates a new ARQ BNode function
        /// </summary>
        /// <param name="expr">Expression</param>
        public ArqBNodeFunction(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Gets the value of the function in the given Evaluation Context for the given Binding ID
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override INode Value(SparqlEvaluationContext context, int bindingID)
        {
            INode temp = this._expr.Value(context, bindingID);
            if (temp != null)
            {
                if (temp.NodeType == NodeType.Blank)
                {
                    IBlankNode b = (IBlankNode)temp;
                    return new LiteralNode(null, b.InternalID);
                }
                else
                {
                    throw new RdfQueryException("Cannot find the BNode Label for a non-Blank Node");
                }
            }
            else
            {
                throw new RdfQueryException("Cannot find the BNode Label for a null");
            }
        }

        /// <summary>
        /// Gets the effective boolean value of the function in the given Evaluation Context for the given Binding ID
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            return SparqlSpecsHelper.EffectiveBooleanValue(this.Value(context, bindingID));
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + ArqFunctionFactory.ArqFunctionsNamespace + ArqFunctionFactory.BNode + ">(" + this._expr.ToString() + ")";
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
                return ArqFunctionFactory.ArqFunctionsNamespace + ArqFunctionFactory.BNode;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new ArqBNodeFunction(transformer.Transform(this._expr));
        }
    }

    /// <summary>
    /// Represents the ARQ afn:localname() function
    /// </summary>
    public class ArqLocalNameFunction 
        : BaseUnaryExpression
    {
        /// <summary>
        /// Creates a new ARQ Local Name function
        /// </summary>
        /// <param name="expr">Expression</param>
        public ArqLocalNameFunction(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Gets the value of the function in the given Evaluation Context for the given Binding ID
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override INode Value(SparqlEvaluationContext context, int bindingID)
        {
            INode temp = this._expr.Value(context, bindingID);
            if (temp != null)
            {
                if (temp.NodeType == NodeType.Uri)
                {
                    IUriNode u = (IUriNode)temp;
                    if (!u.Uri.Fragment.Equals(String.Empty))
                    {
                        return new LiteralNode(null, u.Uri.Fragment.Substring(1));
                    }
                    else
                    {
#if SILVERLIGHT
                        return new LiteralNode(null, u.Uri.Segments().Last());
#else
                        return new LiteralNode(null, u.Uri.Segments.Last());
#endif
                    }
                }
                else
                {
                    throw new RdfQueryException("Cannot find the Local Name for a non-URI Node");
                }
            }
            else
            {
                throw new RdfQueryException("Cannot find the Local Name for a null");
            }
        }

        /// <summary>
        /// Gets the effective boolean value of the function in the given Evaluation Context for the given Binding ID
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            return SparqlSpecsHelper.EffectiveBooleanValue(this.Value(context, bindingID));
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + ArqFunctionFactory.ArqFunctionsNamespace + ArqFunctionFactory.LocalName + ">(" + this._expr.ToString() + ")";
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
                return ArqFunctionFactory.ArqFunctionsNamespace + ArqFunctionFactory.LocalName;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new ArqLocalNameFunction(transformer.Transform(this._expr));
        }
    }

    /// <summary>
    /// Represents the ARQ namespace() function
    /// </summary>
    public class ArqNamespaceFunction 
        : BaseUnaryExpression
    {
        /// <summary>
        /// Creates a new ARQ Namespace function
        /// </summary>
        /// <param name="expr">Expression</param>
        public ArqNamespaceFunction(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Gets the value of the function in the given Evaluation Context for the given Binding ID
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override INode Value(SparqlEvaluationContext context, int bindingID)
        {
            INode temp = this._expr.Value(context, bindingID);
            if (temp != null)
            {
                if (temp.NodeType == NodeType.Uri)
                {
                    IUriNode u = (IUriNode)temp;
                    if (!u.Uri.Fragment.Equals(String.Empty))
                    {
                        return new LiteralNode(null, u.Uri.ToString().Substring(0, u.Uri.ToString().LastIndexOf('#')+1));
                    }
                    else
                    {
                        return new LiteralNode(null, u.Uri.ToString().Substring(0, u.Uri.ToString().LastIndexOf('/')+1));
                    }
                }
                else
                {
                    throw new RdfQueryException("Cannot find the Local Name for a non-URI Node");
                }
            }
            else
            {
                throw new RdfQueryException("Cannot find the Local Name for a null");
            }
        }

        /// <summary>
        /// Gets the effective boolean value of the function in the given Evaluation Context for the given Binding ID
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            return SparqlSpecsHelper.EffectiveBooleanValue(this.Value(context, bindingID));
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + ArqFunctionFactory.ArqFunctionsNamespace + ArqFunctionFactory.Namespace + ">(" + this._expr.ToString() + ")";
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
                return ArqFunctionFactory.ArqFunctionsNamespace + ArqFunctionFactory.Namespace;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new ArqNamespaceFunction(transformer.Transform(this._expr));
        }
    }
}
