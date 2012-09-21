/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

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
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Operators.DateTime
{
    /// <summary>
    /// Represents the time span subtraction operator
    /// </summary>
    /// <remarks>
    /// Allows queries to subtract time spans from each other
    /// </remarks>
    public class TimeSpanSubtraction
        : BaseTimeSpanOperator
    {
        /// <summary>
        /// Gets the operator type
        /// </summary>
        public override SparqlOperatorType Operator
        {
            get 
            {
                return SparqlOperatorType.Subtract;
            }
        }

        public override IValuedNode Apply(params IValuedNode[] ns)
        {
            if (ns == null) throw new RdfQueryException("Cannot apply to null arguments");
            if (ns.Any(n => n == null)) throw new RdfQueryException("Cannot apply operator when one/more arguments are null");

            return new TimeSpanNode(null, this.Subtract(ns.Select(n => n.AsTimeSpan())));
        }

        private TimeSpan Subtract(IEnumerable<TimeSpan> ts)
        {
            bool first = true;
            TimeSpan total = TimeSpan.Zero;
            foreach (TimeSpan t in ts)
            {
                if (first)
                {
                    total = t;
                    first = false;
                }
                else
                {
                    total -= t;
                }
            }
            return total;
        }
    }
}
