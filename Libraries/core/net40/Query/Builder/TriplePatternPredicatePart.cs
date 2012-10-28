using System;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Builder
{
    /// <summary>
    /// Class responsible for setting the predicate part of triple patterns
    /// </summary>
    // todo: extract interface
    public sealed class TriplePatternPredicatePart
    {
        private readonly PatternItem _subjectPatternItem;
        private readonly TriplePatternBuilder _triplePatternBuilder;

        internal TriplePatternPredicatePart(TriplePatternBuilder triplePatternBuilder, PatternItem subjectPatternItem)
        {
            _subjectPatternItem = subjectPatternItem;
            _triplePatternBuilder = triplePatternBuilder;
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
            return new TriplePatternObjectPart(_triplePatternBuilder, _subjectPatternItem, predicate);
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
        /// <remarks>A relevant prefix/base URI must be added to <see cref="ICommonQueryBuilder.Prefixes"/></remarks>
        public TriplePatternObjectPart PredicateUri(string predicateQName)
        {
            var predicate = _triplePatternBuilder.PatternItemFactory.CreateNodeMatchPattern(predicateQName);
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
            return new TriplePatternObjectPart(_triplePatternBuilder, _subjectPatternItem, predicate);
        }
    }
}