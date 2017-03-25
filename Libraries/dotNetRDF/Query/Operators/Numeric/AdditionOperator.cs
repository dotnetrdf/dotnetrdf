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

using System.Linq;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Operators.Numeric
{
    /// <summary>
    /// Represents the numeric addition operator
    /// </summary>
    public class AdditionOperator
        : BaseNumericOperator
    {
        /// <summary>
        /// Gets the operator type
        /// </summary>
        public override SparqlOperatorType Operator
        {
            get
            {
                return SparqlOperatorType.Add;
            }
        }

        /// <summary>
        /// Applies the operator
        /// </summary>
        /// <param name="ns">Arguments</param>
        /// <returns></returns>
        public override IValuedNode Apply(params IValuedNode[] ns)
        {
            if (ns == null) throw new RdfQueryException("Cannot apply to null arguments");
            if (ns.Any(n => n == null)) throw new RdfQueryException("Cannot apply addition when any arguments are null");

            SparqlNumericType type = (SparqlNumericType)ns.Max(n => (int)n.NumericType);

            switch (type)
            {
                case SparqlNumericType.Integer:
                    return new LongNode(null, ns.Select(n => n.AsInteger()).Sum());
                case SparqlNumericType.Decimal:
                    return new DecimalNode(null, ns.Select(n => n.AsDecimal()).Sum());
                case SparqlNumericType.Float:
                    return new FloatNode(null, ns.Select(n => n.AsFloat()).Sum());
                case SparqlNumericType.Double:
                    return new DoubleNode(null, ns.Select(n => n.AsDouble()).Sum());
                default:
                    throw new RdfQueryException("Cannot evalute an Arithmetic Expression when the Numeric Type of the operation cannot be determined");
            }
        }
    }
}
