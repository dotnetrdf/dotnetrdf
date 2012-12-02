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
        private readonly IList<Func<INamespaceMapper, ISparqlExpression>> _filterBuilders = new List<Func<INamespaceMapper, ISparqlExpression>>();
        private readonly IList<Func<INamespaceMapper, ITriplePattern[]>> _triplePatterns = new List<Func<INamespaceMapper, ITriplePattern[]>>();
        private readonly GraphPatternType _graphPatternType;

        /// <summary>
        /// Creates a builder of a normal graph patterns
        /// </summary>
        internal GraphPatternBuilder()
            : this(GraphPatternType.Normal)
        {
        }

        /// <summary>
        /// Creates a builder of a graph pattern
        /// </summary>
        /// <param name="graphPatternType">MINUS, GRAPH, SERVICE etc.</param>
        private GraphPatternBuilder(GraphPatternType graphPatternType)
        {
            _graphPatternType = graphPatternType;
        }

        internal GraphPattern BuildGraphPattern(INamespaceMapper prefixes)
        {
            var graphPattern = CreateGraphPattern();

            foreach (var triplePattern in _triplePatterns.SelectMany(getTriplePatterns => getTriplePatterns(prefixes)))
            {
                AddTriplePattern(graphPattern, triplePattern);
            }
            foreach (var graphPatternBuilder in _childGraphPatternBuilders)
            {
                graphPattern.AddGraphPattern(graphPatternBuilder.BuildGraphPattern(prefixes));
            }
            foreach (var buildFilter in _filterBuilders)
            {
                graphPattern.AddFilter(new UnaryExpressionFilter(buildFilter(prefixes)));
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
                case GraphPatternType.Union:
                    graphPattern.IsUnion = true;
                    break;
            }
            return graphPattern;
        }

        #region Implementation of IGraphPatternBuilder

        public IGraphPatternBuilder Where(params ITriplePattern[] triplePatterns)
        {
            _triplePatterns.Add(prefixes => triplePatterns);
            return this;
        }

        public IGraphPatternBuilder Where(Action<ITriplePatternBuilder> buildTriplePatterns)
        {
            return Where(prefixes =>
                {
                    var builder = new TriplePatternBuilder(prefixes);
                    buildTriplePatterns(builder);
                    return builder.Patterns;
                });
        }

        internal IGraphPatternBuilder Where(Func<INamespaceMapper, ITriplePattern[]> buildTriplePatternFunc)
        {
            _triplePatterns.Add(buildTriplePatternFunc);
            return this;
        }

        public IGraphPatternBuilder Optional(Action<IGraphPatternBuilder> buildGraphPattern)
        {
            AddChildGraphPattern(buildGraphPattern, GraphPatternType.Optional);
            return this;
        }

        public IGraphPatternBuilder Minus(Action<IGraphPatternBuilder> buildGraphPattern)
        {
            AddChildGraphPattern(buildGraphPattern, GraphPatternType.Minus);
            return this;
        }

        public IGraphPatternBuilder Union(Action<IGraphPatternBuilder> buildGraphPattern)
        {
            GraphPatternBuilder union;
            if (_graphPatternType == GraphPatternType.Union)
            {
                union = this;
            }
            else
            {
                union = new GraphPatternBuilder(GraphPatternType.Union);
                union._childGraphPatternBuilders.Add(this);
            }

            union.AddChildGraphPattern(buildGraphPattern, GraphPatternType.Normal);
            return union;
        }

        public IAssignmentVariableNamePart<IGraphPatternBuilder> Bind(Func<ExpressionBuilder, SparqlExpression> buildAssignmentExpression)
        {
            return new BindAssignmentVariableNamePart(this, buildAssignmentExpression);
        }

        public IGraphPatternBuilder Child(Action<IGraphPatternBuilder> buildGraphPattern)
        {
            AddChildGraphPattern(buildGraphPattern, GraphPatternType.Normal);
            return this;
        }

        public IGraphPatternBuilder Filter(Func<ExpressionBuilder, BooleanExpression> buildExpression)
        {
            _filterBuilders.Add(namespaceMapper =>
                {
                    var builder = new ExpressionBuilder(namespaceMapper);
                    return buildExpression(builder).Expression;
                });
            return this;
        }

        public IGraphPatternBuilder Filter(ISparqlExpression expr)
        {
            _filterBuilders.Add(namespaceMapper => expr);
            return this;
        }

        #endregion

        private void AddChildGraphPattern(Action<IGraphPatternBuilder> buildGraphPattern, GraphPatternType graphPatternType)
        {
            var childBuilder = new GraphPatternBuilder(graphPatternType);
            buildGraphPattern(childBuilder);
            _childGraphPatternBuilders.Add(childBuilder);
        }
    }
}