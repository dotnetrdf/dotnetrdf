using System;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Builder
{
    sealed class DescribeGraphPatternBuilder : IDescribeGraphPatternBuilder
    {
        private readonly GraphPatternBuilder _builder;

        internal DescribeGraphPatternBuilder(GraphPatternBuilder builder)
        {
            _builder = builder;
        }

        public IDescribeGraphPatternBuilder Where(params ITriplePattern[] triplePatterns)
        {
            _builder.Where(triplePatterns);
            return this;
        }

        public IDescribeGraphPatternBuilder Where(Action<ITriplePatternBuilder> buildTriplePatterns)
        {
            _builder.Where(buildTriplePatterns);
            return this;
        }

        public GraphPattern BuildGraphPattern()
        {
            return _builder.BuildGraphPattern();
        }
    }
}