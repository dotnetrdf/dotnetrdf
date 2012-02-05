using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Expressions.Functions.Arq
{
    /// <summary>
    /// Represents the ARQ afn:localname() function
    /// </summary>
    public class LocalNameFunction
        : BaseUnaryExpression
    {
        /// <summary>
        /// Creates a new ARQ Local Name function
        /// </summary>
        /// <param name="expr">Expression</param>
        public LocalNameFunction(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Gets the value of the function in the given Evaluation Context for the given Binding ID
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            INode temp = this._expr.Evaluate(context, bindingID);
            if (temp != null)
            {
                if (temp.NodeType == NodeType.Uri)
                {
                    IUriNode u = (IUriNode)temp;
                    if (!u.Uri.Fragment.Equals(String.Empty))
                    {
                        return new StringNode(null, u.Uri.Fragment.Substring(1));
                    }
                    else
                    {
#if SILVERLIGHT
                        return new LiteralNode(null, u.Uri.Segments().Last());
#else
                        return new StringNode(null, u.Uri.Segments.Last());
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
            return new LocalNameFunction(transformer.Transform(this._expr));
        }
    }
}
