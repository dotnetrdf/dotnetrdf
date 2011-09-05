/*

Copyright Robert Vesse 2009-10
rvesse@vdesign-studios.com

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

namespace VDS.RDF.Query.Expressions.Functions
{
    /// <summary>
    /// Represents the Leviathan lfn:cartesian() function
    /// </summary>
    public class LeviathanCartesianFunction
        : BaseArithmeticExpression
    {
        private ISparqlExpression _x1, _y1, _z1, _x2, _y2, _z2;
        private bool _3d = false;

        /// <summary>
        /// Creates a new 2D Cartesian Function
        /// </summary>
        /// <param name="x1">Expression for X Coordinate of 1st point</param>
        /// <param name="y1">Expression for Y Coordinate of 1st point</param>
        /// <param name="x2">Expression for X Coordinate of 2nd point</param>
        /// <param name="y2">Expression for Y Coordinate of 2nd point</param>
        public LeviathanCartesianFunction(ISparqlExpression x1, ISparqlExpression y1, ISparqlExpression x2, ISparqlExpression y2)
        {
            this._x1 = x1;
            this._y1 = y1;
            this._x2 = x2;
            this._y2 = y2;
        }

        /// <summary>
        /// Creates a new 3D Cartesian Function
        /// </summary>
        /// <param name="x1">Expression for X Coordinate of 1st point</param>
        /// <param name="y1">Expression for Y Coordinate of 1st point</param>
        /// <param name="z1">Expression for Z Coordiante of 1st point</param>
        /// <param name="x2">Expression for X Coordinate of 2nd point</param>
        /// <param name="y2">Expression for Y Coordinate of 2nd point</param>
        /// <param name="z2">Expression for Z Coordinate of 2nd point</param>
        public LeviathanCartesianFunction(ISparqlExpression x1, ISparqlExpression y1, ISparqlExpression z1, ISparqlExpression x2, ISparqlExpression y2, ISparqlExpression z2)
        {
            this._x1 = x1;
            this._y1 = y1;
            this._z1 = z1;
            this._x2 = x2;
            this._y2 = y2;
            this._z2 = z2;
            this._3d = true;
        }

        /// <summary>
        /// Calculates the Numeric Value of the function as evaluated in the given Context for the given Binding ID
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override object NumericValue(SparqlEvaluationContext context, int bindingID)
        {
            //Validate that all expressions are numeric expression
            if (this._3d)
            {
                if (this._x1 is ISparqlNumericExpression && this._y1 is ISparqlNumericExpression && this._z1 is ISparqlNumericExpression && this._x2 is ISparqlNumericExpression && this._y2 is ISparqlNumericExpression && this._z2 is ISparqlNumericExpression)
                {
                    return this.CartesianDistance3D(context, bindingID);
                }
                else
                {
                    throw new RdfQueryException("Unable to evaluate the Leviathan Cartesian function since one/more of the arguments are non-numeric expressions");
                }
            }
            else
            {
                if (this._x1 is ISparqlNumericExpression && this._y1 is ISparqlNumericExpression && this._x2 is ISparqlNumericExpression && this._y2 is ISparqlNumericExpression)
                {
                    return this.CartesianDistance2D(context, bindingID);
                }
                else
                {
                    throw new RdfQueryException("Unable to evaluate the Leviathan Cartesian function since one/more of the arguments are non-numeric expressions");
                }
            }
        }

        /// <summary>
        /// Internal helper for calculating 2D Cartesian Distance
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        private object CartesianDistance2D(SparqlEvaluationContext context, int bindingID)
        {
            ISparqlNumericExpression x1 = (ISparqlNumericExpression)this._x1;
            ISparqlNumericExpression y1 = (ISparqlNumericExpression)this._y1;
            ISparqlNumericExpression x2 = (ISparqlNumericExpression)this._x2;
            ISparqlNumericExpression y2 = (ISparqlNumericExpression)this._y2;

            double dX = x2.DoubleValue(context, bindingID) - x1.DoubleValue(context, bindingID);
            double dY = y2.DoubleValue(context, bindingID) - y1.DoubleValue(context, bindingID);

            return Math.Sqrt(Math.Pow(dX, 2) + Math.Pow(dY, 2));
        }

        /// <summary>
        /// Internal helper for calculating 3D Cartesian Distance
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        private object CartesianDistance3D(SparqlEvaluationContext context, int bindingID)
        {
            ISparqlNumericExpression x1 = (ISparqlNumericExpression)this._x1;
            ISparqlNumericExpression y1 = (ISparqlNumericExpression)this._y1;
            ISparqlNumericExpression z1 = (ISparqlNumericExpression)this._z1;
            ISparqlNumericExpression x2 = (ISparqlNumericExpression)this._x2;
            ISparqlNumericExpression y2 = (ISparqlNumericExpression)this._y2;
            ISparqlNumericExpression z2 = (ISparqlNumericExpression)this._z2;

            double dX = x2.DoubleValue(context, bindingID) - x1.DoubleValue(context, bindingID);
            double dY = y2.DoubleValue(context, bindingID) - y1.DoubleValue(context, bindingID);
            double dZ = z2.DoubleValue(context, bindingID) - z1.DoubleValue(context, bindingID);

            return Math.Sqrt(Math.Pow(dX, 2) + Math.Pow(dY, 2) + Math.Pow(dZ,2));
        }

        /// <summary>
        /// Gets the Variables used in the function
        /// </summary>
        public override IEnumerable<string> Variables
        {
            get
            {
                if (this._3d)
                {
                    return this._x1.Variables.Concat(this._y1.Variables).Concat(this._z1.Variables).Concat(this._x2.Variables).Concat(this._y2.Variables).Concat(this._z2.Variables);
                }
                else
                {
                    return this._x1.Variables.Concat(this._y1.Variables).Concat(this._x2.Variables).Concat(this._y2.Variables);
                }
            }
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.Append("<" + LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.Cartesian + ">(");
            output.Append(this._x1.ToString());
            output.Append(',');
            output.Append(this._y1.ToString());
            output.Append(',');
            if (this._3d)
            {
                output.Append(this._z1.ToString());
                output.Append(',');
            }
            output.Append(this._x2.ToString());
            output.Append(',');
            output.Append(this._y2.ToString());
            if (this._3d)
            {
                output.Append(',');
                output.Append(this._z2.ToString());
            }
            output.Append(')');
            return output.ToString();
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
                return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.Cartesian;
            }
        }

        /// <summary>
        /// Gets the Arguments of the Expression
        /// </summary>
        public override IEnumerable<ISparqlExpression> Arguments
        {
            get 
            {
                if (this._3d)
                {
                    return new ISparqlExpression[] { this._x1, this._y1, this._z1, this._x2, this._y2, this._z2 };
                }
                else
                {
                    return new ISparqlExpression[] { this._x1, this._y1, this._x2, this._y2 };
                }
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            if (this._3d)
            {
                return new LeviathanCartesianFunction(transformer.Transform(this._x1), transformer.Transform(this._y1), transformer.Transform(this._z1), transformer.Transform(this._x2), transformer.Transform(this._y2), transformer.Transform(this._z2));
            }
            else
            {
                return new LeviathanCartesianFunction(transformer.Transform(this._x1), transformer.Transform(this._y1), transformer.Transform(this._x2), transformer.Transform(this._y2));
            }
        }
    }
}
