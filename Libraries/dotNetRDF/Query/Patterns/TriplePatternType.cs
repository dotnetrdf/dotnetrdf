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

namespace VDS.RDF.Query.Patterns
{
    /// <summary>
    /// Possible Types of Triple Pattern
    /// </summary>
    public enum TriplePatternType
    {
        /// <summary>
        /// Simple pattern matching
        /// </summary>
        Match,
        /// <summary>
        /// FILTER application
        /// </summary>
        Filter,
        /// <summary>
        /// BIND assignment
        /// </summary>
        BindAssignment,
        /// <summary>
        /// LET assignment
        /// </summary>
        LetAssignment,
        /// <summary>
        /// Sub-query
        /// </summary>
        SubQuery,
        /// <summary>
        /// Property Path
        /// </summary>
        Path,
        /// <summary>
        /// Property Function
        /// </summary>
        PropertyFunction
    }

    /// <summary>
    /// Comparer for Triple Pattern Types
    /// </summary>
    public class TriplePatternTypeComparer
        : IComparer<TriplePatternType>
    {
        /// <summary>
        /// Compares two triple pattern types
        /// </summary>
        /// <param name="x">Pattern Type</param>
        /// <param name="y">Pattern Type</param>
        /// <returns></returns>
        public int Compare(TriplePatternType x, TriplePatternType y)
        {
            return ((int)x).CompareTo((int)y);
        }
    }
}
