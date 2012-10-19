using System;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Builder
{
    internal class TriplePatternBuilder : ITriplePatternBuilder
    {
        public TriplePatternPredicatePart Subject(string subjectVariableName)
        {
            throw new NotImplementedException();
        }

        public TriplePatternPredicatePart Subject<TNode>(string subject) where TNode : INode
        {
            throw new NotImplementedException();
        }

        public TriplePatternPredicatePart Subject(INode subjectNode)
        {
            throw new NotImplementedException();
        }

        public ITriplePattern[] Patterns
        {
            get { throw new NotImplementedException(); }
        }
    }
}