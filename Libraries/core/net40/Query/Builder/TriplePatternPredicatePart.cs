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
            var predicate = _triplePatternBuilder.PatternItemFactory.CreateVariablePattern(variableName);
            return CreateTriplePatternObjectPart(predicate);
        }

        public TriplePatternObjectPart PredicateUri(Uri predicateUri)
        {
            var predicate = _triplePatternBuilder.PatternItemFactory.CreateNodeMatchPattern(predicateUri);
            return CreateTriplePatternObjectPart(predicate);
        }

        public TriplePatternObjectPart PredicateUri(string predicateQName)
        {
            var predicate = _triplePatternBuilder.PatternItemFactory.CreateNodeMatchPattern(predicateQName);
            return CreateTriplePatternObjectPart(predicate);
        }

        public TriplePatternObjectPart PredicateUri(IUriNode predicateNode)
        {
            var predicate = _triplePatternBuilder.PatternItemFactory.CreateNodeMatchPattern(predicateNode);
            return CreateTriplePatternObjectPart(predicate);
        }

        private TriplePatternObjectPart CreateTriplePatternObjectPart(PatternItem predicate)
        {
            return new TriplePatternObjectPart(_triplePatternBuilder, _subjectPatternItem, predicate);
        }
    }
}