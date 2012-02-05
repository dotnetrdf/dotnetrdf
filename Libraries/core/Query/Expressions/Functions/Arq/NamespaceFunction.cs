using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Expressions.Functions.Arq
{
    /// <summary>
    /// Represents the ARQ namespace() function
    /// </summary>
    public class NamespaceFunction
        : BaseUnaryExpression
    {
        /// <summary>
        /// Creates a new ARQ Namespace function
        /// </summary>
        /// <param name="expr">Expression</param>
        public NamespaceFunction(ISparqlExpression expr)
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
                        return new StringNode(null, u.Uri.ToString().Substring(0, u.Uri.ToString().LastIndexOf('#') + 1));
                    }
                    else
                    {
                        return new StringNode(null, u.Uri.ToString().Substring(0, u.Uri.ToString().LastIndexOf('/') + 1));
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
            return new NamespaceFunction(transformer.Transform(this._expr));
        }
    }
}
