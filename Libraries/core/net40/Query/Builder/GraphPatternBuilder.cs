/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2013 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Parsing.Tokens;
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
        private readonly IToken _graphSpecifier;

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

        internal GraphPatternBuilder(GraphPatternType graphPatternType, IToken graphSpecifier)
        {
            _graphPatternType = graphPatternType;
            _graphSpecifier = graphSpecifier;
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
                case GraphPatternType.Graph:
                    graphPattern.IsGraph = true;
                    graphPattern.GraphSpecifier = _graphSpecifier;
                    break;
                case GraphPatternType.Service:
                    graphPattern.IsService = true;
                    graphPattern.GraphSpecifier = _graphSpecifier;
                    break;
            }
            return graphPattern;
        }

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

        public IGraphPatternBuilder Graph(Uri graphUri, Action<IGraphPatternBuilder> buildGraphPattern)
        {
            AddChildGraphPattern(buildGraphPattern, GraphPatternType.Graph, new UriToken(string.Format("<{0}>", graphUri), 0, 0, 0));
            return this;
        }

        public IGraphPatternBuilder Graph(string graphVariable, Action<IGraphPatternBuilder> buildGraphPattern)
        {
            AddChildGraphPattern(buildGraphPattern, GraphPatternType.Graph, new VariableToken(graphVariable, 0, 0, 0));
            return this;
        }

        public IGraphPatternBuilder Service(Uri serviceUri, Action<IGraphPatternBuilder> buildGraphPattern)
        {
            AddChildGraphPattern(buildGraphPattern, GraphPatternType.Service,
                                 new UriToken(string.Format("<{0}>", serviceUri), 0, 0, 0));
            return this;
        }

        public IGraphPatternBuilder Union(Action<IGraphPatternBuilder> firstGraphPattern, params Action<IGraphPatternBuilder>[] unionedGraphPatternBuilders)
        {
            if (unionedGraphPatternBuilders == null || unionedGraphPatternBuilders.Length == 0)
            {
                return Child(firstGraphPattern);
            }

            var union = new GraphPatternBuilder(GraphPatternType.Union);
            union.AddChildGraphPattern(firstGraphPattern, GraphPatternType.Normal);

            foreach (var builder in unionedGraphPatternBuilders)
            {
                union.AddChildGraphPattern(builder, GraphPatternType.Normal);
            }

            _childGraphPatternBuilders.Add(union);
            return this;
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

        private void AddChildGraphPattern(Action<IGraphPatternBuilder> buildGraphPattern, GraphPatternType graphPatternType)
        {
            var childBuilder = new GraphPatternBuilder(graphPatternType);
            buildGraphPattern(childBuilder);
            _childGraphPatternBuilders.Add(childBuilder);
        }

        private void AddChildGraphPattern(Action<IGraphPatternBuilder> buildGraphPattern, GraphPatternType graphPatternType, IToken graphSpecifier)
        {
            var childBuilder = new GraphPatternBuilder(graphPatternType, graphSpecifier);
            buildGraphPattern(childBuilder);
            _childGraphPatternBuilders.Add(childBuilder);
        }
    }
}