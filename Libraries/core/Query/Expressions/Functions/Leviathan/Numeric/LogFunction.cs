using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Expressions.Nodes;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Expressions.Functions.Leviathan.Numeric
{
    /// <summary>
    /// Represents the Leviathan lfn:log() function
    /// </summary>
    public class LogFunction
        : BaseBinaryExpression
    {
        private bool _log10 = false;

        /// <summary>
        /// Creates a new Leviathan Log Function
        /// </summary>
        /// <param name="arg">Expression</param>
        public LogFunction(ISparqlExpression arg)
            : base(arg, new ConstantTerm(new DoubleNode(null, 10)))
        {
            this._log10 = true;
        }

        public LogFunction(ISparqlExpression arg, ISparqlExpression logBase)
            : base(arg, logBase) { }

        public override IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            IValuedNode arg = this._leftExpr.Evaluate(context, bindingID);
            if (arg == null) throw new RdfQueryException("Cannot log a null");
            IValuedNode logBase = this._rightExpr.Evaluate(context, bindingID);
            if (logBase == null) throw new RdfQueryException("Cannot log to a null base");

            if (arg.NumericType == SparqlNumericType.NaN || logBase.NumericType == SparqlNumericType.NaN) throw new RdfQueryException("Cannot log when one/both arguments are non-numeric");

            return new DoubleNode(null, Math.Log(arg.AsDouble(), logBase.AsDouble()));
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (this._log10)
            {
                return "<" + LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.Log + ">(" + this._leftExpr.ToString() + ")";
            }
            else
            {
                return "<" + LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.Log + ">(" + this._leftExpr.ToString() + "," + this._rightExpr.ToString() + ")";
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.Log;
            }
        }
        
        /// <summary>
        /// Gets the type of the expression
        /// </summary>
        public override SparqlExpressionType Type
        {
            get
            {
                return SparqlExpressionType.Function;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            if (this._log10)
            {
                return new LogFunction(transformer.Transform(this._leftExpr));
            }
            else
            {
                return new LogFunction(transformer.Transform(this._leftExpr), transformer.Transform(this._rightExpr));
            }
        }
    }
}
