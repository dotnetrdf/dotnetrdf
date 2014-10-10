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
using VDS.RDF.Nodes;
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Expressions.Factories;

namespace VDS.RDF.Query.Expressions.Functions.Leviathan.Numeric
{
    /// <summary>
    /// Represents the Leviathan lfn:cartesian() function
    /// </summary>
    public class CartesianFunction
        : BaseNAryExpression
    {

        /// <summary>
        /// Creates a new 2D Cartesian Function
        /// </summary>
        /// <param name="x1">Expression for X Coordinate of 1st point</param>
        /// <param name="y1">Expression for Y Coordinate of 1st point</param>
        /// <param name="x2">Expression for X Coordinate of 2nd point</param>
        /// <param name="y2">Expression for Y Coordinate of 2nd point</param>
        public CartesianFunction(IExpression x1, IExpression y1, IExpression x2, IExpression y2)
            : base(MakeArguments(x1, y1, x2, y2)) { }

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
            : base(MakeArguments(x1, y1, z1, x2, y2, z2)) { }

        private static IEnumerable<IExpression> MakeArguments(params IExpression[] args)
        {
            if (args.Length != 4 && args.Length != 6) throw new ArgumentException("Incorrect number of arguments for the cartesian function");

            return args;
        }


        public override IExpression Copy(IEnumerable<IExpression> args)
        {
            List<IExpression> arguments = args.ToList();
            return arguments.Count == 4 ? new CartesianFunction(arguments[0], arguments[1], arguments[2], arguments[3]) : new CartesianFunction(arguments[0], arguments[1], arguments[2], arguments[3], arguments[4], arguments[5]);
        }

        /// <summary>
        /// Evaluates the expression
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(ISolution solution, IExpressionContext context)
        {
            //Validate that all expressions are numeric expression
            if (this.Arguments.Count == 6)
            {
                return this.CartesianDistance3D(solution, context);
            }
            return this.CartesianDistance2D(solution, context);
        }

        /// <summary>
        /// Internal helper for calculating 2D Cartesian Distance
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        private IValuedNode CartesianDistance2D(ISolution solution, IExpressionContext context)
        {
            IValuedNode x1 = this.Arguments[0].Evaluate(solution, context);
            if (x1 == null) throw new RdfQueryException("Cannot calculate cartesian distance when a argument is null");
            IValuedNode y1 = this.Arguments[1].Evaluate(solution, context);
            if (y1 == null) throw new RdfQueryException("Cannot calculate cartesian distance when a argument is null");
            IValuedNode x2 = this.Arguments[2].Evaluate(solution, context);
            if (x2 == null) throw new RdfQueryException("Cannot calculate cartesian distance when a argument is null");
            IValuedNode y2 = this.Arguments[3].Evaluate(solution, context);
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
            IValuedNode x1 = this.Arguments[0].Evaluate(solution, context);
            if (x1 == null) throw new RdfQueryException("Cannot calculate cartesian distance when a argument is null");
            IValuedNode y1 = this.Arguments[1].Evaluate(solution, context);
            if (y1 == null) throw new RdfQueryException("Cannot calculate cartesian distance when a argument is null");
            IValuedNode z1 = this.Arguments[2].Evaluate(solution, context);
            if (z1 == null) throw new RdfQueryException("Cannot calculate cartesian distance when a argument is null");
            IValuedNode x2 = this.Arguments[3].Evaluate(solution, context);
            if (x2 == null) throw new RdfQueryException("Cannot calculate cartesian distance when a argument is null");
            IValuedNode y2 = this.Arguments[4].Evaluate(solution, context);
            if (y2 == null) throw new RdfQueryException("Cannot calculate cartesian distance when a argument is null");
            IValuedNode z2 = this.Arguments[5].Evaluate(solution, context);
            if (z2 == null) throw new RdfQueryException("Cannot calculate cartesian distance when a argument is null");

            double dX = x2.AsDouble() - x1.AsDouble();
            double dY = y2.AsDouble() - y1.AsDouble();
            double dZ = z2.AsDouble() - z1.AsDouble();

            return new DoubleNode(Math.Sqrt(Math.Pow(dX, 2) + Math.Pow(dY, 2) + Math.Pow(dZ,2)));
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
    }
}
