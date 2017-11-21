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
using System.Text.RegularExpressions;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;

namespace VDS.RDF.Query.Expressions.Functions.XPath.DateTime
{
    /// <summary>
    /// Represents the XPath timezone-from-dateTime() function
    /// </summary>
    public class TimezoneFromDateTimeFunction
        : ISparqlExpression
    {
        /// <summary>
        /// Expression that the Function applies to
        /// </summary>
        protected ISparqlExpression _expr;

        /// <summary>
        /// Creates a new XPath Timezone from Date Time function
        /// </summary>
        /// <param name="expr">Expression</param>
        public TimezoneFromDateTimeFunction(ISparqlExpression expr)
        {
            _expr = expr;
        }

        /// <summary>
        /// Calculates the value of the function in the given Evaluation Context for the given Binding ID
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public virtual IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            IValuedNode temp = _expr.Evaluate(context, bindingID);
            if (temp != null)
            {
                DateTimeOffset dt = temp.AsDateTimeOffset();
                // Regex based check to see if the value has a Timezone component
                // If not then the result is a null
                if (!Regex.IsMatch(temp.AsString(), "(Z|[+-]\\d{2}:\\d{2})$")) return null;

                // Now we have a DateTime we can try and return the Timezone
                if (dt.Offset.Equals(TimeSpan.Zero))
                {
                    // If Zero it was specified as Z (which means UTC so zero offset)
                    return new StringNode(null, "PT0S", UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDayTimeDuration));
                }
                else
                {
                    // If the Offset is outside the range -14 to 14 this is considered invalid
                    if (dt.Offset.Hours < -14 || dt.Offset.Hours > 14) return null;

                    // Otherwise it has an offset which is a given number of hours and minutse
                    string offset = "PT" + Math.Abs(dt.Offset.Hours) + "H";
                    if (dt.Offset.Hours < 0) offset = "-" + offset;
                    if (dt.Offset.Minutes != 0) offset = offset + Math.Abs(dt.Offset.Minutes) + "M";
                    if (dt.Offset.Hours == 0 && dt.Offset.Minutes < 0) offset = "-" + offset;

                    return new StringNode(null, offset, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDayTimeDuration));
                }
            }
            else
            {
                throw new RdfQueryException("Unable to evaluate an XPath Date Time function on a null argument");
            }
        }

        /// <summary>
        /// Calculates the effective boolean value of the function in the given Evaluation Context for the given Binding ID
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            throw new RdfQueryException("Cannot calculate the Effective Boolean Value of an XML Schema Duration");
        }

        /// <summary>
        /// Gets the Variables used in the function
        /// </summary>
        public IEnumerable<string> Variables
        {
            get
            {
                return _expr.Variables;
            }
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.TimezoneFromDateTime + ">(" + _expr.ToString() + ")";
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
        public virtual string Functor
        {
            get
            {
                return XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.TimezoneFromDateTime;
            }
        }

        /// <summary>
        /// Gets the Arguments of the Expression
        /// </summary>
        public IEnumerable<ISparqlExpression> Arguments
        {
            get
            {
                return _expr.AsEnumerable();
            }
        }

        /// <summary>
        /// Gets whether an expression can safely be evaluated in parallel
        /// </summary>
        public virtual bool CanParallelise
        {
            get
            {
                return _expr.CanParallelise;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public virtual ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new TimezoneFromDateTimeFunction(transformer.Transform(_expr));
        }
    }
}
