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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Expressions.Arithmetic
{
    /// <summary>
    /// Class representing Arithmetic Multiplication expressions
    /// </summary>
    public class MultiplicationExpression
        : BaseBinaryExpression
    {
        /// <summary>
        /// Creates a new Multiplication Expression
        /// </summary>
        /// <param name="leftExpr">Left Hand Expression</param>
        /// <param name="rightExpr">Right Hand Expression</param>
        public MultiplicationExpression(ISparqlExpression leftExpr, ISparqlExpression rightExpr) 
            : base(leftExpr, rightExpr) { }

        /// <summary>
        /// Calculates the Numeric Value of this Expression as evaluated for a given Binding
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            IValuedNode a = this._leftExpr.Evaluate(context, bindingID);
            IValuedNode b = this._rightExpr.Evaluate(context, bindingID);
            if (a == null || b == null) throw new RdfQueryException("Cannot apply multiplication when one/both arguments are null");

            SparqlNumericType type = (SparqlNumericType)Math.Max((int)a.NumericType, (int)b.NumericType);

            switch (type)
            {
                case SparqlNumericType.Integer:
                    return new LongNode(null, a.AsInteger() * b.AsInteger());
                case SparqlNumericType.Decimal:
                    return new DecimalNode(null, a.AsDecimal() * b.AsDecimal());
                case SparqlNumericType.Float:
                    return new FloatNode(null, a.AsFloat() * b.AsFloat());
                case SparqlNumericType.Double:
                    return new DoubleNode(null, a.AsDouble() * b.AsDouble());
                default:
                    throw new RdfQueryException("Cannot evalute an Arithmetic Expression when the Numeric Type of the expression cannot be determined");
            }
        }

        /// <summary>
        /// Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            if (this._leftExpr.Type == SparqlExpressionType.BinaryOperator)
            {
                output.Append("(" + this._leftExpr.ToString() + ")");
            }
            else
            {
                output.Append(this._leftExpr.ToString());
            }
            output.Append(" * ");
            if (this._rightExpr.Type == SparqlExpressionType.BinaryOperator)
            {
                output.Append("(" + this._rightExpr.ToString() + ")");
            }
            else
            {
                output.Append(this._rightExpr.ToString());
            }
            return output.ToString();
        }

        /// <summary>
        /// Gets the Type of the Expression
        /// </summary>
        public override SparqlExpressionType Type
        {
            get
            {
                return SparqlExpressionType.BinaryOperator;
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return "*";
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new MultiplicationExpression(transformer.Transform(this._leftExpr), transformer.Transform(this._rightExpr));
        }
    }
}
