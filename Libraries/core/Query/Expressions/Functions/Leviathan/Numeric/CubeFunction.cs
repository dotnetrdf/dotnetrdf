using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Expressions.Functions.Leviathan.Numeric
{
    /// <summary>
    /// Represents the Leviathan lfn:cube() function
    /// </summary>
    public class CubeFunction
        : BaseUnaryExpression
    {
        /// <summary>
        /// Creates a new Leviathan Cube Function
        /// </summary>
        /// <param name="expr">Expression</param>
        public CubeFunction(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Evaluates the expression
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            IValuedNode temp = this._expr.Evaluate(context, bindingID);
            if (temp == null) throw new RdfQueryException("Cannot square a null");

            switch (temp.NumericType)
            {
                case SparqlNumericType.Integer:
                    long l = temp.AsInteger();
                    return new LongNode(null, l * l * l);
                case SparqlNumericType.Decimal:
                    decimal d = temp.AsDecimal();
                    return new DecimalNode(null, d * d * d);
                case SparqlNumericType.Float:
                    float f = temp.AsFloat();
                    return new FloatNode(null, f * f * f);
                case SparqlNumericType.Double:
                    double dbl = temp.AsDouble();
                    return new DoubleNode(null, Math.Pow(dbl, 3));
                case SparqlNumericType.NaN:
                default:
                    throw new RdfQueryException("Cannot square a non-numeric argument");
            }
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.Cube + ">(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Type of this expression
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
                return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.Cube;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new CubeFunction(transformer.Transform(this._expr));
        }
    }
}
