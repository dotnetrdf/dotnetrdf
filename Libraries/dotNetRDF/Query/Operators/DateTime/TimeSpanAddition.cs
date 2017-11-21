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
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Operators.DateTime
{
    /// <summary>
    /// Represents the time span addition operation
    /// </summary>
    /// <remarks>
    /// Allows queries to add time spans together
    /// </remarks>
    public class TimeSpanAddition
        : BaseTimeSpanOperator
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
            if (ns == null) throw new ArgumentNullException("Cannot apply to null arguments");
            if (ns.Any(n => n == null)) throw new RdfQueryException("Cannot apply operator when one/more arguments are null");

            return new TimeSpanNode(null, Add(ns.Select(n => n.AsTimeSpan())));
        }

        private TimeSpan Add(IEnumerable<TimeSpan> ts)
        {
            TimeSpan total = TimeSpan.Zero;
            foreach (TimeSpan t in ts)
            {
                total += t;
            }
            return total;
        }
    }
}
