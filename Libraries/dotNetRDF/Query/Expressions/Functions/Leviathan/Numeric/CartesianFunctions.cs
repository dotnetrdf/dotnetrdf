/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Expressions.Functions.Leviathan.Numeric
{
    /// <summary>
    /// Represents the Leviathan lfn:cartesian() function
    /// </summary>
    public class CartesianFunction
        : ISparqlExpression
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
        public CartesianFunction(ISparqlExpression x1, ISparqlExpression y1, ISparqlExpression x2, ISparqlExpression y2)
        {
            _x1 = x1;
            _y1 = y1;
            _x2 = x2;
            _y2 = y2;
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
        public CartesianFunction(ISparqlExpression x1, ISparqlExpression y1, ISparqlExpression z1, ISparqlExpression x2, ISparqlExpression y2, ISparqlExpression z2)
        {
            _x1 = x1;
            _y1 = y1;
            _z1 = z1;
            _x2 = x2;
            _y2 = y2;
            _z2 = z2;
            _3d = true;
        }


        /// <summary>
        /// Evaluates the expression
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            // Validate that all expressions are numeric expression
            if (_3d)
            {
                return CartesianDistance3D(context, bindingID);
            }
            else
            {
                return CartesianDistance2D(context, bindingID);
            }
        }

        /// <summary>
        /// Internal helper for calculating 2D Cartesian Distance
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        private IValuedNode CartesianDistance2D(SparqlEvaluationContext context, int bindingID)
        {
            IValuedNode x1 = _x1.Evaluate(context, bindingID);
            if (x1 == null) throw new RdfQueryException("Cannot calculate cartesian distance when a argument is null");
            IValuedNode y1 = _y1.Evaluate(context, bindingID);
            if (y1 == null) throw new RdfQueryException("Cannot calculate cartesian distance when a argument is null");
            IValuedNode x2 = _x2.Evaluate(context, bindingID);
            if (x2 == null) throw new RdfQueryException("Cannot calculate cartesian distance when a argument is null");
            IValuedNode y2 = _y2.Evaluate(context, bindingID);
            if (y2 == null) throw new RdfQueryException("Cannot calculate cartesian distance when a argument is null");

            double dX = x2.AsDouble() - x1.AsDouble();
            double dY = y2.AsDouble() - y1.AsDouble();

            return new DoubleNode(null, Math.Sqrt(Math.Pow(dX, 2) + Math.Pow(dY, 2)));
        }

        /// <summary>
        /// Internal helper for calculating 3D Cartesian Distance
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        private IValuedNode CartesianDistance3D(SparqlEvaluationContext context, int bindingID)
        {
            IValuedNode x1 = _x1.Evaluate(context, bindingID);
            if (x1 == null) throw new RdfQueryException("Cannot calculate cartesian distance when a argument is null");
            IValuedNode y1 = _y1.Evaluate(context, bindingID);
            if (y1 == null) throw new RdfQueryException("Cannot calculate cartesian distance when a argument is null");
            IValuedNode z1 = _z1.Evaluate(context, bindingID);
            if (z1 == null) throw new RdfQueryException("Cannot calculate cartesian distance when a argument is null");
            IValuedNode x2 = _x2.Evaluate(context, bindingID);
            if (x2 == null) throw new RdfQueryException("Cannot calculate cartesian distance when a argument is null");
            IValuedNode y2 = _y2.Evaluate(context, bindingID);
            if (y2 == null) throw new RdfQueryException("Cannot calculate cartesian distance when a argument is null");
            IValuedNode z2 = _z2.Evaluate(context, bindingID);
            if (z2 == null) throw new RdfQueryException("Cannot calculate cartesian distance when a argument is null");

            double dX = x2.AsDouble() - x1.AsDouble();
            double dY = y2.AsDouble() - y1.AsDouble();
            double dZ = z2.AsDouble() - z1.AsDouble();

            return new DoubleNode(null, Math.Sqrt(Math.Pow(dX, 2) + Math.Pow(dY, 2) + Math.Pow(dZ,2)));
        }

        /// <summary>
        /// Gets the Variables used in the function
        /// </summary>
        public IEnumerable<string> Variables
        {
            get
            {
                if (_3d)
                {
                    return _x1.Variables.Concat(_y1.Variables).Concat(_z1.Variables).Concat(_x2.Variables).Concat(_y2.Variables).Concat(_z2.Variables);
                }
                else
                {
                    return _x1.Variables.Concat(_y1.Variables).Concat(_x2.Variables).Concat(_y2.Variables);
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
            output.Append(_x1.ToString());
            output.Append(',');
            output.Append(_y1.ToString());
            output.Append(',');
            if (_3d)
            {
                output.Append(_z1.ToString());
                output.Append(',');
            }
            output.Append(_x2.ToString());
            output.Append(',');
            output.Append(_y2.ToString());
            if (_3d)
            {
                output.Append(',');
                output.Append(_z2.ToString());
            }
            output.Append(')');
            return output.ToString();
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
        public IEnumerable<ISparqlExpression> Arguments
        {
            get 
            {
                if (_3d)
                {
                    return new ISparqlExpression[] { _x1, _y1, _z1, _x2, _y2, _z2 };
                }
                else
                {
                    return new ISparqlExpression[] { _x1, _y1, _x2, _y2 };
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
                if (_3d)
                {
                    return _x1.CanParallelise && _y1.CanParallelise && _z1.CanParallelise && _x1.CanParallelise && _y2.CanParallelise && _z2.CanParallelise;
                }
                else
                {
                    return _x1.CanParallelise && _y1.CanParallelise && _x2.CanParallelise && _y2.CanParallelise;
                }
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            if (_3d)
            {
                return new CartesianFunction(transformer.Transform(_x1), transformer.Transform(_y1), transformer.Transform(_z1), transformer.Transform(_x2), transformer.Transform(_y2), transformer.Transform(_z2));
            }
            else
            {
                return new CartesianFunction(transformer.Transform(_x1), transformer.Transform(_y1), transformer.Transform(_x2), transformer.Transform(_y2));
            }
        }
    }
}
