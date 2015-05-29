/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-develop@lists.sf.net)

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
using System.Globalization;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Expressions.Factories;

namespace VDS.RDF.Query.Expressions.Functions.XPath.Numeric
{
    /// <summary>
    /// Represents the XPath fn:round() function
    /// </summary>
    public class RoundFunction
        : BaseUnaryExpression
    {
        /// <summary>
        /// Creates a new XPath Round function
        /// </summary>
        /// <param name="expr">Expression</param>
        public RoundFunction(IExpression expr)
            : base(expr) {}

        public override IExpression Copy(IExpression argument)
        {
            return new RoundFunction(argument);
        }

        /// <summary>
        /// Gets the Numeric Value of the function as evaluated in the given Context for the given Binding ID
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(ISolution solution, IExpressionContext context)
        {
            IValuedNode a = this.Argument.Evaluate(solution, context);
            if (a == null) throw new RdfQueryException("Cannot calculate an arithmetic expression on a null");

            switch (a.NumericType)
            {
                case EffectiveNumericType.Integer:
                    //Rounding an Integer has no effect
                    return a;

                case EffectiveNumericType.Decimal:
#if !SILVERLIGHT
                    return new DecimalNode(Math.Round(a.AsDecimal(), MidpointRounding.AwayFromZero));
#else
                    return new DecimalNodeMath.Round(a.AsDecimal()));
#endif

                case EffectiveNumericType.Float:
                    try
                    {
#if !SILVERLIGHT
                        return new FloatNode(Convert.ToSingle(Math.Round(a.AsDouble(), MidpointRounding.AwayFromZero), CultureInfo.InvariantCulture));
#else
                        return new FloatNode(Convert.ToSingle(Math.Round(a.AsDouble()), CultureInfo.InvariantCulture));
#endif
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
#if !SILVERLIGHT
                    return new DoubleNode(Math.Round(a.AsDouble(), MidpointRounding.AwayFromZero));
#else
                    return new DoubleNode(Math.Round(a.AsDouble()));
#endif

                default:
                    throw new RdfQueryException("Cannot evalute an Arithmetic Expression when the Numeric Type of the expression cannot be determined");
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get { return XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.Round; }
        }
    }
}
