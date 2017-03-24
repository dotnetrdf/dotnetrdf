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
using VDS.RDF.Query;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Writing.Formatting
{

    /// <summary>
    /// Interface for classes which can format SPARQL Queries into Strings
    /// </summary>
    public interface IQueryFormatter : INodeFormatter, IUriFormatter
    {
        /// <summary>
        /// Formats a SPARQL Query into a String
        /// </summary>
        /// <param name="query">SPARQL Query</param>
        /// <returns></returns>
        String Format(SparqlQuery query);

        /// <summary>
        /// Formats a Graph Pattern into a String
        /// </summary>
        /// <param name="gp">Graph Pattern</param>
        /// <returns></returns>
        String Format(GraphPattern gp);

        /// <summary>
        /// Formats a Triple Pattern into a String
        /// </summary>
        /// <param name="tp">Triple Pattern</param>
        /// <returns></returns>
        String Format(ITriplePattern tp);

        /// <summary>
        /// Formats a Triple Pattern item into a String
        /// </summary>
        /// <param name="item">Pattern Item</param>
        /// <param name="segment">Segment of the Triple Pattern in which the Item appears</param>
        /// <returns></returns>
        String Format(PatternItem item, TripleSegment? segment);
    }
}
