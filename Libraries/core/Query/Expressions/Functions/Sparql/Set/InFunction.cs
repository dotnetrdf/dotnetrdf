using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Expressions.Functions.Sparql.Set
{
    /// <summary>
    /// Class representing the SPARQL IN set function
    /// </summary>
    public class InFunction
        : BaseSetFunction
    {
        /// <summary>
        /// Creates a new SPARQL IN function
        /// </summary>
        /// <param name="expr">Expression</param>
        /// <param name="set">Set</param>
        public InFunction(ISparqlExpression expr, IEnumerable<ISparqlExpression> set)
            : base(expr, set) { }

        public override IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            IValuedNode result = this._expr.Evaluate(context, bindingID);
            if (result != null)
            {
                if (this._expressions.Count == 0) return new BooleanNode(null, false);

                //Have to use SPARQL Value Equality here
                //If any expressions error and nothing in the set matches then an error is thrown
                bool errors = false;
                foreach (ISparqlExpression expr in this._expressions)
                {
                    try
                    {
                        IValuedNode temp = expr.Evaluate(context, bindingID);
                        if (SparqlSpecsHelper.Equality(result, temp)) return new BooleanNode(null, true);
                    }
                    catch
                    {
                        errors = true;
                    }
                }

                if (errors)
                {
                    throw new RdfQueryException("One/more expressions in a Set function failed to evaluate");
                }
                else
                {
                    return new BooleanNode(null, false);
                }
            }
            else
            {
                return new BooleanNode(null, false);
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordIn;
            }
        }

        /// <summary>
        /// Gets the String representation of the Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            if (this._expr.Type == SparqlExpressionType.BinaryOperator || this._expr.Type == SparqlExpressionType.GraphOperator || this._expr.Type == SparqlExpressionType.SetOperator) output.Append('(');
            output.Append(this._expr.ToString());
            if (this._expr.Type == SparqlExpressionType.BinaryOperator || this._expr.Type == SparqlExpressionType.GraphOperator || this._expr.Type == SparqlExpressionType.SetOperator) output.Append(')');
            output.Append(" IN (");
            for (int i = 0; i < this._expressions.Count; i++)
            {
                output.Append(this._expressions[i].ToString());
                if (i < this._expressions.Count - 1)
                {
                    output.Append(" , ");
                }
            }
            output.Append(")");
            return output.ToString();
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new InFunction(transformer.Transform(this._expr), this._expressions.Select(e => transformer.Transform(e)));
        }
    }
}
