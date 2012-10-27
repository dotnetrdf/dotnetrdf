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

        public TriplePatternBuilder(INamespaceMapper namespaceMapper)
        {
            _patternItemFactory = new PatternItemFactory(namespaceMapper);
        }

        public TriplePatternPredicatePart Subject(string subjectVariableName)
        {
            return new TriplePatternPredicatePart(this, PatternItemFactory.CreateVariablePattern(subjectVariableName));
        }

        public TriplePatternPredicatePart Subject<TNode>(string subject) where TNode : INode
        {
            return new TriplePatternPredicatePart(this, PatternItemFactory.CreatePatternItem(typeof(TNode), subject));
        }

        public TriplePatternPredicatePart Subject(INode subjectNode)
        {
            return new TriplePatternPredicatePart(this, PatternItemFactory.CreateNodeMatchPattern(subjectNode));
        }

        public TriplePatternPredicatePart Subject(Uri subject)
        {
            return new TriplePatternPredicatePart(this, PatternItemFactory.CreateNodeMatchPattern(subject));
        }

        public TriplePatternPredicatePart Subject(PatternItem subject)
        {
            return new TriplePatternPredicatePart(this, subject);
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