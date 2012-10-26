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
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Expressions.Functions.Leviathan.Numeric
{
    /// <summary>
    /// Represents the Leviathan lfn:rnd() function
    /// </summary>
    public class RandomFunction
        : BaseBinaryExpression
    {
        private Random _rnd = new Random();
        private int _args = 0;

        /// <summary>
        /// Creates a new Leviathan Random Function
        /// </summary>
        public RandomFunction()
            : base(new ConstantTerm(new DoubleNode(0)), new ConstantTerm(new DoubleNode(1))) { }

        /// <summary>
        /// Creates a new Leviathan Random Function
        /// </summary>
        /// <param name="max">Maximum</param>
        public RandomFunction(ISparqlExpression max)
            : base(new ConstantTerm(new DoubleNode(0)), max)
        {
            this._args = 1;
        }

        /// <summary>
        /// Creates a new Leviathan Random Function
        /// </summary>
        /// <param name="min">Minumum</param>
        /// <param name="max">Maximum</param>
        public RandomFunction(ISparqlExpression min, ISparqlExpression max)
            : base(min, max)
        {
            this._args = 2;
        }

        /// <summary>
        /// Evaluates the expression
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            IValuedNode min = this._leftExpr.Evaluate(context, bindingID);
            if (min == null) throw new RdfQueryException("Cannot randomize with a null minimum");
            IValuedNode max = this._rightExpr.Evaluate(context, bindingID);
            if (max == null) throw new RdfQueryException("Cannot randomize with a null maximum");

            if (min.NumericType == SparqlNumericType.NaN || max.NumericType == SparqlNumericType.NaN) throw new RdfQueryException("Cannot randomize when one/both arguments are non-numeric");

            double x = min.AsDouble();
            double y = max.AsDouble();

            if (x > y) throw new RdfQueryException("Cannot generate a random number in the given range since the minumum is greater than the maximum");
            double range = y - x;
            double rnd = this._rnd.NextDouble() * range;
            rnd += x;
            return new DoubleNode(rnd);
        }


        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.Append('<');
            output.Append(LeviathanFunctionFactory.LeviathanFunctionsNamespace);
            output.Append(LeviathanFunctionFactory.Random);
            output.Append(">(");
            switch (this._args)
            {
                case 1:
                    output.Append(this._rightExpr.ToString());
                    break;
                case 2:
                    output.Append(this._leftExpr.ToString());
                    output.Append(',');
                    output.Append(this._rightExpr.ToString());
                    break;
            }
            output.Append(')');
            return output.ToString();
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.Random;
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
            return new RandomFunction(transformer.Transform(this._leftExpr), transformer.Transform(this._rightExpr));
        }
    }
}
