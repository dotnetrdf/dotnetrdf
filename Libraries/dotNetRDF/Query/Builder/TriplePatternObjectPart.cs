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
    /// Class responsible for setting the object part of triple patterns
    /// </summary>
    public sealed class TriplePatternObjectPart
    {
        private readonly TriplePatternBuilder _triplePatternBuilder;
        private readonly PatternItem _subjectPatternItem;
        private readonly PatternItem _predicatePatternItem;
        private readonly INamespaceMapper _prefixes;

        internal TriplePatternObjectPart(TriplePatternBuilder triplePatternBuilder, PatternItem subjectPatternItem, PatternItem predicatePatternItem, INamespaceMapper prefixes)
        {
            _subjectPatternItem = subjectPatternItem;
            _predicatePatternItem = predicatePatternItem;
            _prefixes = prefixes;
            _triplePatternBuilder = triplePatternBuilder;
        }

        /// <summary>
        /// Sets a SPARQL variable as <see cref="IMatchTriplePattern.Object"/>
        /// </summary>
        public ITriplePatternBuilder Object(SparqlVariable variable)
        {
            var objectPattern = _triplePatternBuilder.PatternItemFactory.CreateVariablePattern(variable.Name);
            return Object(objectPattern);
        }

        /// <summary>
        /// Sets a SPARQL variable as <see cref="IMatchTriplePattern.Object"/>
        /// </summary>
        public ITriplePatternBuilder Object(string variableName)
        {
            var objectPattern = _triplePatternBuilder.PatternItemFactory.CreateVariablePattern(variableName);
            return Object(objectPattern);
        }

        /// <summary>
        /// Depending on the generic parameter type, sets a literal, a QName or a blank node as <see cref="IMatchTriplePattern.Object"/>
        /// </summary>
        /// <param name="object">Either a variable name, a literal, a QName or a blank node identifier</param>
        /// <remarks>A relevant prefix/base URI must be added to <see cref="IQueryBuilder.Prefixes"/> to accept a QName</remarks>
        public ITriplePatternBuilder Object<TNode>(string @object) where TNode : INode
        {
            var objectPattern = _triplePatternBuilder.PatternItemFactory.CreatePatternItem(typeof(TNode), @object, _prefixes);
            return Object(objectPattern);
        }

        /// <summary>
        /// Depending on the <paramref name="objectNode"/>'s type, sets a literal, a QName or a blank node as <see cref="IMatchTriplePattern.Object"/>
        /// </summary>
        public ITriplePatternBuilder Object(INode objectNode)
        {
            var objectPattern = _triplePatternBuilder.PatternItemFactory.CreateNodeMatchPattern(objectNode);
            return Object(objectPattern);
        }

        /// <summary>
        /// Sets a <see cref="Uri"/> as <see cref="IMatchTriplePattern.Object"/>
        /// </summary>
        public ITriplePatternBuilder Object(Uri objectUri)
        {
            PatternItem objectPattern = _triplePatternBuilder.PatternItemFactory.CreateNodeMatchPattern(objectUri);
            return Object(objectPattern);
        }

        /// <summary>
        /// Sets a plain literal as <see cref="IMatchTriplePattern.Object"/>
        /// </summary>
        public ITriplePatternBuilder ObjectLiteral(object literal)
        {
            PatternItem objectPattern = _triplePatternBuilder.PatternItemFactory.CreateLiteralNodeMatchPattern(literal);
            return Object(objectPattern);
        }

        /// <summary>
        /// Sets a literal with language tag as <see cref="IMatchTriplePattern.Object"/>
        /// </summary>
        public ITriplePatternBuilder ObjectLiteral(object literal, string langSpec)
        {
            PatternItem objectPattern = _triplePatternBuilder.PatternItemFactory.CreateLiteralNodeMatchPattern(literal, langSpec);
            return Object(objectPattern);
        }

        /// <summary>
        /// Sets a typed literal as <see cref="IMatchTriplePattern.Object"/>
        /// </summary>
        public ITriplePatternBuilder ObjectLiteral(object literal, Uri datatype)
        {
            PatternItem objectPattern = _triplePatternBuilder.PatternItemFactory.CreateLiteralNodeMatchPattern(literal, datatype);
            return Object(objectPattern);
        }

        /// <summary>
        /// Sets a <see cref="PatternItem"/> as <see cref="IMatchTriplePattern.Object"/>
        /// </summary>
        public ITriplePatternBuilder Object(PatternItem objectPattern)
        {
            _triplePatternBuilder.AddPattern(new TriplePattern(_subjectPatternItem, _predicatePatternItem, objectPattern));
            return _triplePatternBuilder;
        }
    }
}