using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Expressions.Functions.Arq
{
    /// <summary>
    /// Represents the ARQ afn:now() function
    /// </summary>
    public class NowFunction 
        : ISparqlExpression
    {
        private SparqlQuery _currQuery;
        private IValuedNode _node;

        /// <summary>
        /// Gets the value of the function in the given Evaluation Context for the given Binding ID
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns>
        /// Returns a constant Literal Node which is a Date Time typed Literal
        /// </returns>
        public IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            if (this._currQuery == null)
            {
                this._currQuery = context.Query;
            }
            if (this._node == null || !ReferenceEquals(this._currQuery, context.Query))
            {
                this._node = new DateTimeNode(null, DateTime.Now);
            }
            return this._node;
        }

        /// <summary>
        /// Gets the Type of the Expression
        /// </summary>
        public SparqlExpressionType Type
        {
            get
            {
                return SparqlExpressionType.Function;
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public virtual string Functor
        {
            get
            {
                return ArqFunctionFactory.ArqFunctionsNamespace + ArqFunctionFactory.Now;
            }
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + ArqFunctionFactory.ArqFunctionsNamespace + ArqFunctionFactory.Now + ">()";
        }

        public IEnumerable<string> Variables
        {
            get 
            {
                return Enumerable.Empty<string>();
            }
        }

        public IEnumerable<ISparqlExpression> Arguments
        {
            get 
            {
                return Enumerable.Empty<ISparqlExpression>(); 
            }
        }

        public ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return transformer.Transform(this);
        }
    }
}
