using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Filters;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Builder
{
    sealed class GraphPatternBuilder : IGraphPatternBuilder
    {
        private readonly IList<GraphPatternBuilder> _childGraphPatternBuilders = new List<GraphPatternBuilder>();
        private readonly IList<Func<ISparqlExpression>> _filterBuilders = new List<Func<ISparqlExpression>>();
        private readonly IList<Func<ITriplePattern[]>> _triplePatterns = new List<Func<ITriplePattern[]>>();
        private readonly INamespaceMapper _prefixes;
        private readonly GraphPatternType _graphPatternType;

        /// <summary>
        /// Creates a builder of a normal graph patterns
        /// </summary>
        internal GraphPatternBuilder(INamespaceMapper prefixes)
            : this(prefixes, GraphPatternType.Normal)
        {
        }

        /// <summary>
        /// Creates a builder of a graph pattern
        /// </summary>
        /// <param name="prefixes"></param>
        /// <param name="graphPatternType">MINUS, GRAPH, SERVICE etc.</param>
        private GraphPatternBuilder(INamespaceMapper prefixes, GraphPatternType graphPatternType)
        {
            _prefixes = prefixes;
            _graphPatternType = graphPatternType;
        }

        internal GraphPattern BuildGraphPattern()
        {
            if (!_triplePatterns.Any() && !_childGraphPatternBuilders.Any() && !_filterBuilders.Any())
            {
                return null;
            }
            
            var graphPattern = CreateGraphPattern();

            foreach (var triplePattern in _triplePatterns.SelectMany(getTriplePatterns => getTriplePatterns()))
            {
                AddTriplePattern(graphPattern, triplePattern);
            }
            foreach (var graphPatternBuilder in _childGraphPatternBuilders)
            {
                graphPattern.AddGraphPattern(graphPatternBuilder.BuildGraphPattern());
            }
            foreach (var buildFilter in _filterBuilders)
            {
                graphPattern.AddFilter(new UnaryExpressionFilter(buildFilter()));
            }

            return graphPattern;
        }

        private static void AddTriplePattern(GraphPattern graphPattern, ITriplePattern tp)
        {
            switch (tp.PatternType)
            {
                case TriplePatternType.Match:
                case TriplePatternType.Path:
                case TriplePatternType.PropertyFunction:
                case TriplePatternType.SubQuery:
                    graphPattern.AddTriplePattern(tp);
                    break;
                case TriplePatternType.LetAssignment:
                case TriplePatternType.BindAssignment:
                    graphPattern.AddAssignment((IAssignmentPattern)tp);
                    break;
                case TriplePatternType.Filter:
                    graphPattern.AddFilter(((IFilterPattern)tp).Filter);
                    break;
            }
        }

        private GraphPattern CreateGraphPattern()
        {
            var graphPattern = new GraphPattern();
            switch (_graphPatternType)
            {
                case GraphPatternType.Optional:
                    graphPattern.IsOptional = true;
                    break;
                case GraphPatternType.Minus:
                    graphPattern.IsMinus = true;
                    break;
            }
            return graphPattern;
        }

        #region Implementation of IGraphPatternBuilder

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

        public IGraphPatternBuilder Minus(Action<IGraphPatternBuilder> buildGraphPattern)
        {
            var optionalGraphPattern = new GraphPatternBuilder(Prefixes, GraphPatternType.Minus);
            buildGraphPattern(optionalGraphPattern);
            _childGraphPatternBuilders.Add(optionalGraphPattern);
            return this;
        }

        public AssignmentVariableNamePart<IGraphPatternBuilder> Bind(Func<ExpressionBuilder, SparqlExpression> buildAssignmentExpression)
        {
            return new AssignmentVariableNamePart<IGraphPatternBuilder>(this, buildAssignmentExpression);
        }

        public IGraphPatternBuilder Filter(Func<ExpressionBuilder, BooleanExpression> buildExpression)
        {
            _filterBuilders.Add(() =>
                {
                    var builder = new ExpressionBuilder(Prefixes);
                    return buildExpression(builder).Expression;
                });
            return this;
        }

        public IGraphPatternBuilder Filter(ISparqlExpression expr)
        {
            _filterBuilders.Add(() => expr);
            return this;
        }

        public INamespaceMapper Prefixes
        {
            get { return _prefixes; }
        }

        #endregion
    }
}