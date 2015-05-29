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
using System.Text.RegularExpressions;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Expressions.Factories;
using VDS.RDF.Specifications;

namespace VDS.RDF.Query.Expressions.Functions.XPath.DateTime
{
    /// <summary>
    /// Represents the XPath timezone-from-dateTime() function
    /// </summary>
    public class TimezoneFromDateTimeFunction
        : BaseUnaryExpression
    {

        /// <summary>
        /// Creates a new XPath Timezone from Date Time function
        /// </summary>
        /// <param name="expr">Expression</param>
        public TimezoneFromDateTimeFunction(IExpression expr)
            : base(expr) { }

        public override IExpression Copy(IExpression argument)
        {
            return new TimezoneFromDateTimeFunction(argument);
        }

        /// <summary>
        /// Calculates the value of the function in the given Evaluation Context for the given Binding ID
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(ISolution solution, IExpressionContext context)
        {
            IValuedNode temp = this.Argument.Evaluate(solution, context);
            if (temp == null) throw new RdfQueryException("Unable to evaluate an XPath Date Time function on a null argument");

            DateTimeOffset dt = temp.AsDateTimeOffset();
            //Regex based check to see if the value has a Timezone component
            //If not then the result is a null
            if (!Regex.IsMatch(temp.AsString(), "(Z|[+-]\\d{2}:\\d{2})$")) return null;

            //Now we have a DateTime we can try and return the Timezone
            if (dt.Offset.Equals(TimeSpan.Zero))
            {
                //If Zero it was specified as Z (which means UTC so zero offset)
                return new StringNode("PT0S", UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDayTimeDuration));
            }

            //If the Offset is outside the range -14 to 14 this is considered invalid
            if (dt.Offset.Hours < -14 || dt.Offset.Hours > 14) return null;

            //Otherwise it has an offset which is a given number of hours and minutse
            string offset = "PT" + Math.Abs(dt.Offset.Hours) + "H";
            if (dt.Offset.Hours < 0) offset = "-" + offset;
            if (dt.Offset.Minutes != 0) offset = offset + Math.Abs(dt.Offset.Minutes) + "M";
            if (dt.Offset.Hours == 0 && dt.Offset.Minutes < 0) offset = "-" + offset;

            return new StringNode(offset, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDayTimeDuration));
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.TimezoneFromDateTime;
            }
        }
    }
}
