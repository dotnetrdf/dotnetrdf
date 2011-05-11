/*

Copyright Robert Vesse 2009-11
rvesse@vdesign-studios.com

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
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Optimisation
{
    /// <summary>
    /// Default SPARQL Query Optimiser
    /// </summary>
    public class DefaultOptimiser : BaseQueryOptimiser
    {
        /// <summary>
        /// Gets the Default Comparer for Triple Patterns to rank them
        /// </summary>
        /// <returns></returns>
        protected override IComparer<ITriplePattern> GetRankingComparer()
        {
            //Triple Patterns have a CompareTo defined that orders them based on what is considered to be 
            //an optimal order
            //This order is only an approximation and may not be effective depending on the underlying dataset
            return Comparer<ITriplePattern>.Default;
        }
    }

    /// <summary>
    /// SPARQL Query Optimiser which does no reordering
    /// </summary>
    public class NoReorderOptimiser : BaseQueryOptimiser
    {
        /// <summary>
        /// Gets that Triple Patterns should not be reordered
        /// </summary>
        protected override bool ShouldReorder
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a comparer which does not change the order of Triple Patterns
        /// </summary>
        /// <returns></returns>
        protected override IComparer<ITriplePattern> GetRankingComparer()
        {
            return new NoReorderComparer();
        }
    }

    /// <summary>
    /// A Comparer which ranks all Triple Patterns as equal
    /// </summary>
    public class NoReorderComparer : IComparer<ITriplePattern>
    {
        /// <summary>
        /// Compares two Triple Patterns are always returns that they are ranking equal
        /// </summary>
        /// <param name="x">First Triple Pattern</param>
        /// <param name="y">Second Triple Pattern</param>
        /// <returns></returns>
        public int Compare(ITriplePattern x, ITriplePattern y)
        {
            return 0;
        }
    }

}
