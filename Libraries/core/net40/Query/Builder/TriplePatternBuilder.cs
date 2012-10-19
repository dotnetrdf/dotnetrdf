using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Builder
{
    internal class TriplePatternBuilder : ITriplePatternBuilder
    {
        private readonly IList<TriplePattern> _patterns = new List<TriplePattern>();

        public TriplePatternPredicatePart Subject(string subjectVariableName)
        {
            return new TriplePatternPredicatePart(this, new VariablePattern(subjectVariableName));
        }

        public TriplePatternPredicatePart Subject<TNode>(string subject) where TNode : INode
        {
            throw new NotImplementedException();
        }

        public TriplePatternPredicatePart Subject(INode subjectNode)
        {
            throw new NotImplementedException();
        }

        public TriplePattern[] Patterns
        {
            get { return _patterns.ToArray(); }
        }

        internal void AddPattern(TriplePattern triplePattern)
        {
            _patterns.Add(triplePattern);
        }
    }
}