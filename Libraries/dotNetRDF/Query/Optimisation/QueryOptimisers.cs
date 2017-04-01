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

using System.Collections.Generic;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Optimisation
{
    /// <summary>
    /// Default SPARQL Query Optimiser
    /// </summary>
    public class DefaultOptimiser
        : BaseQueryOptimiser
    {
        /// <summary>
        /// Gets the Default Comparer for Triple Patterns to rank them
        /// </summary>
        /// <returns></returns>
        protected override IComparer<ITriplePattern> GetRankingComparer()
        {
            // Triple Patterns have a CompareTo defined that orders them based on what is considered to be 
            // an optimal order
            // This order is only an approximation and may not be effective depending on the underlying dataset
            return Comparer<ITriplePattern>.Default;
        }
    }

    /// <summary>
    /// SPARQL Query Optimiser which does no reordering
    /// </summary>
    public class NoReorderOptimiser
        : BaseQueryOptimiser
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
    public class NoReorderComparer 
        : IComparer<ITriplePattern>
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
