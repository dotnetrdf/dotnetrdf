using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Expressions.Primary
{
    /// <summary>
    /// Class representing Variable value expressions
    /// </summary>
    public class VariableTerm
        : ISparqlExpression
    {
        private String _name;

        /// <summary>
        /// Creates a new Variable Expression
        /// </summary>
        /// <param name="name">Variable Name</param>
        public VariableTerm(String name)
        {
            if (name.StartsWith("?") || name.StartsWith("$"))
            {
                this._name = name.Substring(1);
            }
            else
            {
                this._name = name;
            }
        }

        public IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            INode value = context.Binder.Value(this._name, bindingID);
            return value.AsValuedNode();
        }

        /// <summary>
        /// Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "?" + this._name;
        }

        /// <summary>
        /// Gets the enumeration containing the single variable that this expression term represents
        /// </summary>
        public IEnumerable<String> Variables
        {
            get
            {
                return this._name.AsEnumerable();
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
            return this;
        }
    }
}
