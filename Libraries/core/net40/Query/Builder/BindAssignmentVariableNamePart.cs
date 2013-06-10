using System;
using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Builder
{
    sealed class BindAssignmentVariableNamePart : AssignmentVariableNamePart, IAssignmentVariableNamePart<IGraphPatternBuilder>, IAssignmentVariableNamePart<IQueryBuilder>
    {
        private readonly GraphPatternBuilder _graphPatternBuilder;
        private readonly QueryBuilder _queryBuilder;

        internal BindAssignmentVariableNamePart(QueryBuilder queryBuilder, Func<ExpressionBuilder, SparqlExpression> buildAssignmentExpression)
            : this(queryBuilder.RootGraphPatternBuilder, buildAssignmentExpression)
        {
            _queryBuilder = queryBuilder;
        }

        internal BindAssignmentVariableNamePart(GraphPatternBuilder graphPatternBuilder, Func<ExpressionBuilder, SparqlExpression> buildAssignmentExpression)
            :base(buildAssignmentExpression)
        {
            _graphPatternBuilder = graphPatternBuilder;
        }

        IGraphPatternBuilder IAssignmentVariableNamePart<IGraphPatternBuilder>.As(string variableName)
        {
            _graphPatternBuilder.Where(mapper => new ITriplePattern[] { new BindPattern(variableName, BuildAssignmentExpression(mapper)) });

            return _graphPatternBuilder;
        }

        IQueryBuilder IAssignmentVariableNamePart<IQueryBuilder>.As(string variableName)
        {
            _queryBuilder.RootGraphPatternBuilder.Where(mapper => new ITriplePattern[] { new BindPattern(variableName, BuildAssignmentExpression(mapper)) });

            return _queryBuilder;
        }
    }
}