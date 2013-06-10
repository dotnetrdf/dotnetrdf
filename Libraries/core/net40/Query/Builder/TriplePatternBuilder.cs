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