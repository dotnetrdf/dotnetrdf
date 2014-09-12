/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Expressions.Factories;

namespace VDS.RDF.Query.Expressions.Functions.Leviathan.Numeric
{
    /// <summary>
    /// Represents the Leviathan lfn:cartesian() function
    /// </summary>
    public class CartesianFunction
        : IExpression
    {
        private IExpression _x1, _y1, _z1, _x2, _y2, _z2;
        private bool _3d = false;

        /// <summary>
        /// Creates a new 2D Cartesian Function
        /// </summary>
        /// <param name="x1">Expression for X Coordinate of 1st point</param>
        /// <param name="y1">Expression for Y Coordinate of 1st point</param>
        /// <param name="x2">Expression for X Coordinate of 2nd point</param>
        /// <param name="y2">Expression for Y Coordinate of 2nd point</param>
        public CartesianFunction(IExpression x1, IExpression y1, IExpression x2, IExpression y2)
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
        public CartesianFunction(IExpression x1, IExpression y1, IExpression z1, IExpression x2, IExpression y2, IExpression z2)
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
        /// Evaluates the expression
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public IValuedNode Evaluate(ISolution solution, IExpressionContext context)
        {
            //Validate that all expressions are numeric expression
            if (this._3d)
            {
                return this.CartesianDistance3D(solution, context);
            }
            else
            {
                return this.CartesianDistance2D(solution, context);
            }
        }

        /// <summary>
        /// Internal helper for calculating 2D Cartesian Distance
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        private IValuedNode CartesianDistance2D(ISolution solution, IExpressionContext context)
        {
            IValuedNode x1 = this._x1.Evaluate(solution, context);
            if (x1 == null) throw new RdfQueryException("Cannot calculate cartesian distance when a argument is null");
            IValuedNode y1 = this._y1.Evaluate(solution, context);
            if (y1 == null) throw new RdfQueryException("Cannot calculate cartesian distance when a argument is null");
            IValuedNode x2 = this._x2.Evaluate(solution, context);
            if (x2 == null) throw new RdfQueryException("Cannot calculate cartesian distance when a argument is null");
            IValuedNode y2 = this._y2.Evaluate(solution, context);
            if (y2 == null) throw new RdfQueryException("Cannot calculate cartesian distance when a argument is null");

            double dX = x2.AsDouble() - x1.AsDouble();
            double dY = y2.AsDouble() - y1.AsDouble();

            return new DoubleNode(Math.Sqrt(Math.Pow(dX, 2) + Math.Pow(dY, 2)));
        }

        /// <summary>
        /// Internal helper for calculating 3D Cartesian Distance
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        private IValuedNode CartesianDistance3D(ISolution solution, IExpressionContext context)
        {
            IValuedNode x1 = this._x1.Evaluate(solution, context);
            if (x1 == null) throw new RdfQueryException("Cannot calculate cartesian distance when a argument is null");
            IValuedNode y1 = this._y1.Evaluate(solution, context);
            if (y1 == null) throw new RdfQueryException("Cannot calculate cartesian distance when a argument is null");
            IValuedNode z1 = this._z1.Evaluate(solution, context);
            if (z1 == null) throw new RdfQueryException("Cannot calculate cartesian distance when a argument is null");
            IValuedNode x2 = this._x2.Evaluate(solution, context);
            if (x2 == null) throw new RdfQueryException("Cannot calculate cartesian distance when a argument is null");
            IValuedNode y2 = this._y2.Evaluate(solution, context);
            if (y2 == null) throw new RdfQueryException("Cannot calculate cartesian distance when a argument is null");
            IValuedNode z2 = this._z2.Evaluate(solution, context);
            if (z2 == null) throw new RdfQueryException("Cannot calculate cartesian distance when a argument is null");

            double dX = x2.AsDouble() - x1.AsDouble();
            double dY = y2.AsDouble() - y1.AsDouble();
            double dZ = z2.AsDouble() - z1.AsDouble();

            return new DoubleNode(Math.Sqrt(Math.Pow(dX, 2) + Math.Pow(dY, 2) + Math.Pow(dZ,2)));
        }

        /// <summary>
        /// Gets the Variables used in the function
        /// </summary>
        public IEnumerable<string> Variables
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
        /// Gets the Functor of the Expression
        /// </summary>
        public string Functor
        {
            get 
            {
                return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.Cartesian;
            }
        }

        /// <summary>
        /// Gets the Arguments of the Expression
        /// </summary>
        public IEnumerable<IExpression> Arguments
        {
            get 
            {
                if (this._3d)
                {
                    return new IExpression[] { this._x1, this._y1, this._z1, this._x2, this._y2, this._z2 };
                }
                else
                {
                    return new IExpression[] { this._x1, this._y1, this._x2, this._y2 };
                }
            }
        }

        /// <summary>
        /// Gets whether an expression can safely be evaluated in parallel
        /// </summary>
        public virtual bool CanParallelise
        {
            get
            {
                if (this._3d)
                {
                    return this._x1.CanParallelise && this._y1.CanParallelise && this._z1.CanParallelise && this._x1.CanParallelise && this._y2.CanParallelise && this._z2.CanParallelise;
                }
                else
                {
                    return this._x1.CanParallelise && this._y1.CanParallelise && this._x2.CanParallelise && this._y2.CanParallelise;
                }
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public IExpression Transform(IExpressionTransformer transformer)
        {
            if (this._3d)
            {
                return new CartesianFunction(transformer.Transform(this._x1), transformer.Transform(this._y1), transformer.Transform(this._z1), transformer.Transform(this._x2), transformer.Transform(this._y2), transformer.Transform(this._z2));
            }
            else
            {
                return new CartesianFunction(transformer.Transform(this._x1), transformer.Transform(this._y1), transformer.Transform(this._x2), transformer.Transform(this._y2));
            }
        }
    }
}
