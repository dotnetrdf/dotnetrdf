/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2013 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Builder
{
    internal class TriplePatternBuilder : ITriplePatternBuilder
    {
        private readonly IList<ITriplePattern> _patterns = new List<ITriplePattern>();
        private readonly PatternItemFactory _patternItemFactory;
        private readonly INamespaceMapper _prefixes;

        public TriplePatternBuilder(INamespaceMapper prefixes)
        {
            _prefixes = prefixes;
            _patternItemFactory = new PatternItemFactory();
        }

        public TriplePatternPredicatePart Subject(string subjectVariableName)
        {
            return Subject(PatternItemFactory.CreateVariablePattern(subjectVariableName));
        }

        public TriplePatternPredicatePart Subject<TNode>(string subject) where TNode : INode
        {
            return Subject(PatternItemFactory.CreatePatternItem(typeof(TNode), subject, _prefixes));
        }

        public TriplePatternPredicatePart Subject(INode subjectNode)
        {
            return Subject(PatternItemFactory.CreateNodeMatchPattern(subjectNode));
        }

        public TriplePatternPredicatePart Subject(Uri subject)
        {
            return Subject(PatternItemFactory.CreateNodeMatchPattern(subject));
        }

        public TriplePatternPredicatePart Subject(PatternItem subject)
        {
            return new TriplePatternPredicatePart(this, subject, _prefixes);
        }

        public ITriplePattern[] Patterns
        {
            get { return _patterns.ToArray(); }
        }

        public PatternItemFactory PatternItemFactory
        {
            get { return _patternItemFactory; }
        }

        internal void AddPattern(TriplePattern triplePattern)
        {
            _patterns.Add(triplePattern);
        }
    }
}