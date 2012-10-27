using System;
using System.Collections.Generic;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Filters;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Builder
{
    class GraphPatternBuilder : IGraphPatternBuilder
    {
        private readonly GraphPattern _graphPattern = new GraphPattern();

        public GraphPatternBuilder()
            : this(GraphPatternType.Normal)
        {
        }

        private GraphPatternBuilder(GraphPatternType graphPatternType)
        {
            switch (graphPatternType)
            {
                case GraphPatternType.Optional:
                    _graphPattern.IsOptional = true;
                    break;
            }
        }

        public GraphPattern GraphPattern
        {
            get { return _graphPattern; }
        }

        protected INamespaceMapper NamespaceMapper { get; set; }

        #region Implementation of IGraphPatternBuilder

        private void Where(ITriplePattern tp)
        {
            switch (tp.PatternType)
            {
                case TriplePatternType.Match:
                case TriplePatternType.Path:
                case TriplePatternType.PropertyFunction:
                case TriplePatternType.SubQuery:
                    GraphPattern.AddTriplePattern(tp);
                    break;
                case TriplePatternType.LetAssignment:
                case TriplePatternType.BindAssignment:
                    GraphPattern.AddAssignment((IAssignmentPattern)tp);
                    break;
                case TriplePatternType.Filter:
                    GraphPattern.AddFilter(((IFilterPattern)tp).Filter);
                    break;
            }
            return;
        }

        [Obsolete("Consider either leaving it here, adding a relevant method to triple pattern builder")]
        public IGraphPatternBuilder Where(IEnumerable<Triple> ts)
        {
            foreach (Triple t in ts)
            {
                Where(tpb => tpb.Subject(t.Subject).PredicateUri((IUriNode)t.Predicate).Object(t.Object));
            }
            return this;
        }

        public IGraphPatternBuilder Where(params ITriplePattern[] triplePatterns)
        {
            foreach (ITriplePattern tp in triplePatterns)
            {
                Where(tp);
            }
            return this;
        }

        public IGraphPatternBuilder Where(Action<ITriplePatternBuilder> buildTriplePatterns)
        {
            var builder = new TriplePatternBuilder(NamespaceMapper);
            buildTriplePatterns(builder);
            return Where(builder.Patterns);
        }

        public IGraphPatternBuilder Optional(Action<IGraphPatternBuilder> buildGraphPattern)
        {
            var optionalGraphPattern = new GraphPatternBuilder(GraphPatternType.Optional);
            buildGraphPattern(optionalGraphPattern);
            _graphPattern.AddGraphPattern(optionalGraphPattern.GraphPattern);
            return this;
        }

        public IGraphPatternBuilder Optional(params ITriplePattern[] triplePatterns)
        {
            var optionalGraphPattern = new GraphPatternBuilder(GraphPatternType.Optional);
            optionalGraphPattern.Where(triplePatterns);
            _graphPattern.AddGraphPattern(optionalGraphPattern.GraphPattern);
            return this;
        }

        public IGraphPatternBuilder Filter(ISparqlExpression expr)
        {
            GraphPattern.AddFilter(new UnaryExpressionFilter(expr));
            return this;
        }

        #endregion
    }
}