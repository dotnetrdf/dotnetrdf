using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Builder
{
    sealed class SelectBuilder : ISelectBuilder
    {
        private SparqlQueryType _sparqlQueryType;
        private readonly IList<Func<INamespaceMapper, SparqlVariable>> _buildSelectVariables = new List<Func<INamespaceMapper, SparqlVariable>>();

        internal SelectBuilder(SparqlQueryType sparqlQueryType)
        {
            _sparqlQueryType = sparqlQueryType;
        }

        internal SparqlQueryType SparqlQueryType
        {
            get { return _sparqlQueryType; }
        }

        /// <summary>
        /// Adds additional SELECT <paramref name="variables"/>
        /// </summary>
        public ISelectBuilder And(params SparqlVariable[] variables)
        {
            foreach (var sparqlVariable in variables)
            {
                SparqlVariable variablelocalCopy = sparqlVariable;
                _buildSelectVariables.Add(prefixes => EnsureIsResultVariable(variablelocalCopy));
            }
            return this;
        }

        internal ISelectBuilder And(Func<INamespaceMapper, SparqlVariable> buildTriplePatternFunc)
        {
            _buildSelectVariables.Add(buildTriplePatternFunc);
            return this;
        }

        /// <summary>
        /// Adds additional SELECT expression
        /// </summary>
        public IAssignmentVariableNamePart<ISelectBuilder> And(Func<ExpressionBuilder, SparqlExpression> buildAssignmentExpression)
        {
            return new SelectAssignmentVariableNamePart(this, buildAssignmentExpression);
        }

        private static SparqlVariable EnsureIsResultVariable(SparqlVariable sparqlVariable)
        {
            if (sparqlVariable.IsResultVariable)
            {
                return sparqlVariable;
            }

            if (sparqlVariable.IsAggregate)
            {
                return new SparqlVariable(sparqlVariable.Name, sparqlVariable.Aggregate);
            }

            if (sparqlVariable.IsProjection)
            {
                return new SparqlVariable(sparqlVariable.Name, sparqlVariable.Projection);
            }

            return new SparqlVariable(sparqlVariable.Name, true);
        }

        /// <summary>
        /// Adds additional SELECT <paramref name="variables"/>
        /// </summary>
        public ISelectBuilder And(params string[] variables)
        {
            return And(variables.Select(var => new SparqlVariable(var, true)).ToArray());
        }

        /// <summary>
        /// Applies the DISTINCT modifier if the Query is a SELECT, otherwise leaves query unchanged (since results from any other query are DISTINCT by default)
        /// </summary>
        public ISelectBuilder Distinct()
        {
            switch (_sparqlQueryType)
            {
                case SparqlQueryType.Select:
                    _sparqlQueryType = SparqlQueryType.SelectDistinct;
                    break;
                case SparqlQueryType.SelectAll:
                    _sparqlQueryType = SparqlQueryType.SelectAllDistinct;
                    break;
                case SparqlQueryType.SelectReduced:
                    _sparqlQueryType = SparqlQueryType.SelectDistinct;
                    break;
                case SparqlQueryType.SelectAllReduced:
                    _sparqlQueryType = SparqlQueryType.SelectAllDistinct;
                    break;
            }
            return this;
        }

        internal IEnumerable<SparqlVariable> BuildVariables(INamespaceMapper prefixes)
        {
            foreach (var buildSelectVariable in _buildSelectVariables)
            {
                yield return buildSelectVariable(prefixes);
            }
        }

        public IQueryBuilder Where(params ITriplePattern[] triplePatterns)
        {
            return CreateQueryBuilder().Where(triplePatterns);
        }

        public IQueryBuilder Where(Action<ITriplePatternBuilder> buildTriplePatterns)
        {
            return CreateQueryBuilder().Where(buildTriplePatterns);
        }

        public IQueryBuilder Optional(Action<IGraphPatternBuilder> buildGraphPattern)
        {
            return CreateQueryBuilder().Optional(buildGraphPattern);
        }

        public IQueryBuilder Filter(Func<ExpressionBuilder, BooleanExpression> expr)
        {
            return CreateQueryBuilder().Filter(expr);
        }

        public IQueryBuilder Filter(ISparqlExpression expr)
        {
            return CreateQueryBuilder().Filter(expr);
        }

        public IQueryBuilder Minus(Action<IGraphPatternBuilder> buildGraphPattern)
        {
            return CreateQueryBuilder().Minus(buildGraphPattern);
        }

        public IAssignmentVariableNamePart<IQueryBuilder> Bind(Func<ExpressionBuilder, SparqlExpression> buildAssignmentExpression)
        {
            return CreateQueryBuilder().Bind(buildAssignmentExpression);
        }

        public IQueryBuilder Child(Action<IGraphPatternBuilder> buildGraphPattern)
        {
            return CreateQueryBuilder().Child(buildGraphPattern);
        }

        private QueryBuilder CreateQueryBuilder()
        {
            return new QueryBuilder(this);
        }
    }
}