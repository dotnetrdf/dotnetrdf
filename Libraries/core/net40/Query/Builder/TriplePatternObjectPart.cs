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