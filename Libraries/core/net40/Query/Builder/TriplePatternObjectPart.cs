using System;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Builder
{
    /// <summary>
    /// todo: extract interface?
    /// </summary>
    public sealed class TriplePatternObjectPart
    {
        private readonly TriplePatternBuilder _triplePatternBuilder;
        private readonly PatternItem _subjectPatternItem;
        private readonly PatternItem _predicatePatternItem;

        internal TriplePatternObjectPart(TriplePatternBuilder triplePatternBuilder, PatternItem subjectPatternItem, PatternItem predicatePatternItem)
        {
            _subjectPatternItem = subjectPatternItem;
            _predicatePatternItem = predicatePatternItem;
            _triplePatternBuilder = triplePatternBuilder;
        }

        public ITriplePatternBuilder Object(string variableName)
        {
            _triplePatternBuilder.AddPattern(new TriplePattern(_subjectPatternItem, _predicatePatternItem, new VariablePattern(variableName)));
            return _triplePatternBuilder;
        }

        public ITriplePatternBuilder Object<TNode>(string @object) where TNode : INode
        {
            var objectPattern = _triplePatternBuilder.PatternItemFactory.CreatePatternItem<TNode>(@object);
            _triplePatternBuilder.AddPattern(new TriplePattern(_subjectPatternItem, _predicatePatternItem, objectPattern));
            return _triplePatternBuilder;
        }

        public ITriplePatternBuilder Object(INode objectNode)
        {
            throw new NotImplementedException();
        }
    }
}