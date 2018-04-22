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
    /// Class responsible for setting the predicate part of triple patterns
    /// </summary>
    public sealed class TriplePatternPredicatePart
    {
        private readonly PatternItem _subjectPatternItem;
        private readonly TriplePatternBuilder _triplePatternBuilder;
        private readonly INamespaceMapper _prefixes;

        internal TriplePatternPredicatePart(TriplePatternBuilder triplePatternBuilder, PatternItem subjectPatternItem, INamespaceMapper prefixes)
        {
            _subjectPatternItem = subjectPatternItem;
            _prefixes = prefixes;
            _triplePatternBuilder = triplePatternBuilder;
        }

        /// <summary>
        /// Sets a SPARQL variable as <see cref="IMatchTriplePattern.Predicate"/>
        /// </summary>
        public TriplePatternObjectPart Predicate(SparqlVariable variable)
        {
            var predicate = _triplePatternBuilder.PatternItemFactory.CreateVariablePattern(variable.Name);
            return CreateTriplePatternObjectPart(predicate);
        }

        /// <summary>
        /// Sets a SPARQL variable as <see cref="IMatchTriplePattern.Predicate"/>
        /// </summary>
        public TriplePatternObjectPart Predicate(string variableName)
        {
            var predicate = _triplePatternBuilder.PatternItemFactory.CreateVariablePattern(variableName);
            return CreateTriplePatternObjectPart(predicate);
        }

        /// <summary>
        /// Sets a <see cref="PatternItem"/> as <see cref="IMatchTriplePattern.Predicate"/>
        /// </summary>
        public TriplePatternObjectPart Predicate(PatternItem predicate)
        {
            return new TriplePatternObjectPart(_triplePatternBuilder, _subjectPatternItem, predicate, _prefixes);
        }

        /// <summary>
        /// Sets a <see cref="Uri"/> as <see cref="IMatchTriplePattern.Predicate"/>
        /// </summary>
        public TriplePatternObjectPart PredicateUri(Uri predicateUri)
        {
            var predicate = _triplePatternBuilder.PatternItemFactory.CreateNodeMatchPattern(predicateUri);
            return CreateTriplePatternObjectPart(predicate);
        }

        /// <summary>
        /// Sets a <see cref="Uri"/> as <see cref="IMatchTriplePattern.Predicate"/> using a QName
        /// </summary>
        /// <remarks>A relevant prefix/base URI must be added to <see cref="IQueryBuilder.Prefixes"/></remarks>
        public TriplePatternObjectPart PredicateUri(string predicateQName)
        {
            var predicate = _triplePatternBuilder.PatternItemFactory.CreateNodeMatchPattern(predicateQName, _prefixes);
            return CreateTriplePatternObjectPart(predicate);
        }

        /// <summary>
        /// Sets a <see cref="Uri"/> as <see cref="IMatchTriplePattern.Predicate"/> using a <see cref="IUriNode"/>
        /// </summary>
        public TriplePatternObjectPart PredicateUri(IUriNode predicateNode)
        {
            var predicate = _triplePatternBuilder.PatternItemFactory.CreateNodeMatchPattern(predicateNode);
            return CreateTriplePatternObjectPart(predicate);
        }

        private TriplePatternObjectPart CreateTriplePatternObjectPart(PatternItem predicate)
        {
            return new TriplePatternObjectPart(_triplePatternBuilder, _subjectPatternItem, predicate, _prefixes);
        }
    }
}