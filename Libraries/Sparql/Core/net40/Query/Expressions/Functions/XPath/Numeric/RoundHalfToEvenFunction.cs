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

using System.Collections.Generic;
using System.Linq;
#if !SILVERLIGHT
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Expressions.Factories;
using System;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Expressions.Functions.XPath.Numeric
{
    /// <summary>
    /// Represents the XPath fn:round() function
    /// </summary>
    public class RoundHalfToEvenFunction
        : BaseNAryExpression
    {
        /// <summary>
        /// Creates a new XPath RoundHalfToEven function
        /// </summary>
        /// <param name="expr">Expression</param>
        public RoundHalfToEvenFunction(IExpression expr)
            : base(expr.AsEnumerable()) { }

        /// <summary>
        /// Creates a new XPath RoundHalfToEven function
        /// </summary>
        /// <param name="expr">Expression</param>
        /// <param name="precision">Precision</param>
        public RoundHalfToEvenFunction(IExpression expr, IExpression precision)
            : base(new IExpression[] {expr, precision}) { }

        public override IExpression Copy(IEnumerable<IExpression> args)
        {
            List<IExpression> arguments  = args.ToList();
            if (arguments.Count == 0 || arguments.Count > 2) throw new ArgumentException("Requires 1/2 arguments");
            return arguments.Count == 1 ? new RoundHalfToEvenFunction(arguments[0]) : new RoundHalfToEvenFunction(arguments[0], arguments[1]);
        }

        /// <summary>
        /// Gets the Numeric Value of the function as evaluated in the given Context for the given Binding ID
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(ISolution solution, IExpressionContext context)
        {
            IValuedNode a = this.Arguments[0].Evaluate(solution, context);
            if (a == null) throw new RdfQueryException("Cannot calculate an arithmetic expression on a null");

            int p = 0;
            if (this.Arguments.Count == 2)
            {
                IValuedNode precision = this.Arguments[2].Evaluate(solution, context);
                if (precision == null) throw new RdfQueryException("Cannot use a null precision for rounding");
                try
                {
                    p = Convert.ToInt32(precision.AsInteger());
                }
                catch
                {
                    throw new RdfQueryException("Unable to cast precision to an integer");
                }
            }

            switch (a.NumericType)
            {
                case EffectiveNumericType.Integer:
                    //Rounding an Integer has no effect
                    return a;

                case EffectiveNumericType.Decimal:
                    return new DecimalNode(Math.Round(a.AsDecimal(), p, MidpointRounding.AwayFromZero));

                case EffectiveNumericType.Float:
                    try
                    {
                        return new FloatNode(Convert.ToSingle(Math.Round(a.AsDouble(), p, MidpointRounding.AwayFromZero)));
                    }
                    catch (RdfQueryException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        throw new RdfQueryException("Unable to cast the float value of a round to a float", ex);
                    }

                case EffectiveNumericType.Double:
                    return new DoubleNode(Math.Round(a.AsDouble(), p, MidpointRounding.AwayFromZero));

                default:
                    throw new RdfQueryException("Cannot evalute an Arithmetic Expression when the Numeric Type of the expression cannot be determined");
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get { return XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.RoundHalfToEven; }
        }
    }
}

#endif