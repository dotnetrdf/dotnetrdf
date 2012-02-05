using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Expressions.Primary
{
    /// <summary>
    /// Class for representing constant terms
    /// </summary>
    public class ConstantTerm
        : ISparqlExpression
    {
        /// <summary>
        /// Node this Term represents
        /// </summary>
        protected IValuedNode _node;

        public ConstantTerm(IValuedNode n)
        {
            this._node = n;
        }

        /// <summary>
        /// Creates a new Node Expression
        /// </summary>
        /// <param name="n">Node</param>
        public ConstantTerm(INode n)
            : this(n.AsValuedNode()) { }

        public IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            return this._node;
        }

        /// <summary>
        /// Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return SparqlSpecsHelper.Formatter.Format(this._node);
        }

        /// <summary>
        /// Gets an Empty Enumerable since a Node Term does not use variables
        /// </summary>
        public IEnumerable<String> Variables
        {
            get
            {
                return Enumerable.Empty<String>();
            }
        }

        /// <summary>
        /// Gets the Type of the Expression
        /// </summary>
        public SparqlExpressionType Type
        {
            get
            {
                return SparqlExpressionType.Primary;
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public String Functor
        {
            get
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// Gets the Arguments of the Expression
        /// </summary>
        public IEnumerable<ISparqlExpression> Arguments
        {
            get
            {
                return Enumerable.Empty<ISparqlExpression>();
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return transformer.Transform(this);
        }
    }
}
