/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

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
using VDS.RDF.Nodes;
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
            : base(arg, new ConstantTerm(new DoubleNode(10)))
        {
            this._log10 = true;
        }

        /// <summary>
        /// Creates a new Leviathan Log Function
        /// </summary>
        /// <param name="arg">Expression</param>
        /// <param name="logBase">Log Base Expression</param>
        public LogFunction(ISparqlExpression arg, ISparqlExpression logBase)
            : base(arg, logBase) { }

        /// <summary>
        /// Evaluates the expression
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            IValuedNode arg = this._leftExpr.Evaluate(context, bindingID);
            if (arg == null) throw new RdfQueryException("Cannot log a null");
            IValuedNode logBase = this._rightExpr.Evaluate(context, bindingID);
            if (logBase == null) throw new RdfQueryException("Cannot log to a null base");

            if (arg.NumericType == SparqlNumericType.NaN || logBase.NumericType == SparqlNumericType.NaN) throw new RdfQueryException("Cannot log when one/both arguments are non-numeric");

            return new DoubleNode(Math.Log(arg.AsDouble(), logBase.AsDouble()));
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
