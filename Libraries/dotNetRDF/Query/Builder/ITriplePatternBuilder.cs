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
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Builder
{
    /// <summary>
    /// Provides methods for building triple patterns
    /// </summary>
    public interface ITriplePatternBuilder
    {
        /// <summary>
        /// Sets a variable as <see cref="IMatchTriplePattern.Subject"/>
        /// </summary>
        TriplePatternPredicatePart Subject(SparqlVariable variable);

        /// <summary>
        /// Sets a variable as <see cref="IMatchTriplePattern.Subject"/>
        /// </summary>
        TriplePatternPredicatePart Subject(string subjectVariableName);
        
        /// <summary>
        /// Depending on the generic parameter type, sets a literal, a QName or a blank node as <see cref="IMatchTriplePattern.Subject"/>
        /// </summary>
        /// <param name="subject">Either a variable name, a literal, a QName or a blank node identifier</param>
        /// <remarks>A relevant prefix/base URI must be added to <see cref="IQueryBuilder.Prefixes"/> to accept a QName</remarks>
        TriplePatternPredicatePart Subject<TNode>(string subject) where TNode : INode;

        /// <summary>
        /// Depending on the <paramref name="subjectNode"/>'s type, sets a literal, a QName or a blank node as <see cref="IMatchTriplePattern.Subject"/>
        /// </summary>
        /// <remarks>A relevant prefix/base URI must be added to <see cref="IQueryBuilder.Prefixes"/> to accept a QName</remarks>
        TriplePatternPredicatePart Subject(INode subjectNode);
        
        /// <summary>
        /// Sets a <see cref="Uri"/> as <see cref="IMatchTriplePattern.Subject"/>
        /// </summary>
        TriplePatternPredicatePart Subject(Uri subject);
        
        /// <summary>
        /// Sets a <see cref="PatternItem"/> as <see cref="IMatchTriplePattern.Subject"/>
        /// </summary>
        TriplePatternPredicatePart Subject(PatternItem subject);
    }
}