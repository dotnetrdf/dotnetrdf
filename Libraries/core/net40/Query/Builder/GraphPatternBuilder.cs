using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Filters;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Builder
{
    class GraphPatternBuilder : IGraphPatternBuilder
    {
        private readonly IList<GraphPatternBuilder> _childGraphPatternBuilders = new List<GraphPatternBuilder>();
        private readonly IList<Func<ITriplePattern[]>> _triplePatterns = new List<Func<ITriplePattern[]>>();
        private readonly INamespaceMapper _prefixes;
        private readonly GraphPatternType _graphPatternType;

        public GraphPatternBuilder(INamespaceMapper prefixes)
            : this(prefixes, GraphPatternType.Normal)
        {
        }

        private GraphPatternBuilder(INamespaceMapper prefixes, GraphPatternType graphPatternType)
        {
            _prefixes = prefixes;
            _graphPatternType = graphPatternType;
        }

        public GraphPattern BuildGraphPattern()
        {
            if(!_triplePatterns.Any())
            {
                return null;
            }
            var graphPattern = CreateGraphPattern();

            foreach (var triplePattern in _triplePatterns.SelectMany(getTriplePatterns => getTriplePatterns()))
            {
                graphPattern.AddTriplePattern(triplePattern);
            }
            foreach (var graphPatternBuilder in _childGraphPatternBuilders)
            {
                graphPattern.AddGraphPattern(graphPatternBuilder.BuildGraphPattern());
            }

            return graphPattern;
        }

        private GraphPattern CreateGraphPattern()
        {
            var graphPattern = new GraphPattern();
            switch (_graphPatternType)
            {
                case GraphPatternType.Optional:
                    graphPattern.IsOptional = true;
                    break;
            }
            return graphPattern;
        }

        #region Implementation of IGraphPatternBuilder

        private void Where(ITriplePattern tp)
        {
            switch (tp.PatternType)
            {
                case TriplePatternType.Match:
                case TriplePatternType.Path:
                case TriplePatternType.PropertyFunction:
                case TriplePatternType.SubQuery:
                    BuildGraphPattern().AddTriplePattern(tp);
                    break;
                case TriplePatternType.LetAssignment:
                case TriplePatternType.BindAssignment:
                    BuildGraphPattern().AddAssignment((IAssignmentPattern)tp);
                    break;
                case TriplePatternType.Filter:
                    BuildGraphPattern().AddFilter(((IFilterPattern)tp).Filter);
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
            _triplePatterns.Add(() => triplePatterns);
            return this;
        }

        public IGraphPatternBuilder Where(Action<ITriplePatternBuilder> buildTriplePatterns)
        {
            _triplePatterns.Add(() =>
                {
                    var builder = new TriplePatternBuilder(Prefixes);
                    buildTriplePatterns(builder);
                    return builder.Patterns;
                });
            return this;
        }

        public IGraphPatternBuilder Optional(Action<IGraphPatternBuilder> buildGraphPattern)
        {
            var optionalGraphPattern = new GraphPatternBuilder(Prefixes, GraphPatternType.Optional);
            buildGraphPattern(optionalGraphPattern);
            _childGraphPatternBuilders.Add(optionalGraphPattern);
            return this;
        }

        public IGraphPatternBuilder Optional(params ITriplePattern[] triplePatterns)
        {
            var optionalGraphPattern = new GraphPatternBuilder(Prefixes, GraphPatternType.Optional);
            optionalGraphPattern.Where(triplePatterns);
            _childGraphPatternBuilders.Add(optionalGraphPattern);
            return this;
        }

        public IGraphPatternBuilder Filter(ISparqlExpression expr)
        {
            BuildGraphPattern().AddFilter(new UnaryExpressionFilter(expr));
            return this;
        }

        public INamespaceMapper Prefixes
        {
            get { return _prefixes; }
        }

        #endregion
    }
}