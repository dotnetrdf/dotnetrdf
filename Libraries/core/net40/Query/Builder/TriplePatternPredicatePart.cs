using System;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Builder
{
    /// <summary>
    /// todo: extract interface?
    /// </summary>
    public sealed class TriplePatternPredicatePart
    {
        private readonly PatternItem _subjectPatternItem;
        private readonly TriplePatternBuilder _triplePatternBuilder;

        internal TriplePatternPredicatePart(TriplePatternBuilder triplePatternBuilder, PatternItem subjectPatternItem)
        {
            _subjectPatternItem = subjectPatternItem;
            _triplePatternBuilder = triplePatternBuilder;
        }

        public TriplePatternObjectPart Predicate(string variableName)
        {
            return new TriplePatternObjectPart(_triplePatternBuilder, _subjectPatternItem, new VariablePattern(variableName));
        }

        public TriplePatternObjectPart Predicate(Uri predicateUri)
        {
            throw new NotImplementedException();
        }

        public TriplePatternObjectPart PredicateUri(string predicateQName)
        {
            throw new NotImplementedException();
        }

        public TriplePatternObjectPart Predicate(INode predicateNode)
        {
            throw new NotImplementedException();
        }
    }
}