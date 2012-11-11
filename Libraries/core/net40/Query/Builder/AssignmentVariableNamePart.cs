using System;
using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Builder
{
    public sealed class AssignmentVariableNamePart<T> where T : ICommonQueryBuilder
    {
        private readonly ISelectQueryBuilder _selectBuilder;
        private readonly IGraphPatternBuilder _graphPatternBuilder;
        private readonly T _queryBuilder;
        private readonly Func<ExpressionBuilder, SparqlExpression> _buildAssignmentExpression;

        internal AssignmentVariableNamePart(ISelectQueryBuilder selectBuilder, Func<ExpressionBuilder, SparqlExpression> buildAssignmentExpression)
            : this((T)selectBuilder, buildAssignmentExpression)
        {
            _selectBuilder = selectBuilder;
        }

        internal AssignmentVariableNamePart(IGraphPatternBuilder graphPatternBuilder, Func<ExpressionBuilder, SparqlExpression> buildAssignmentExpression)
            : this((T)graphPatternBuilder, buildAssignmentExpression)
        {
            _graphPatternBuilder = graphPatternBuilder;
        }

        private AssignmentVariableNamePart(T builder, Func<ExpressionBuilder, SparqlExpression> buildAssignmentExpression)
        {
            _queryBuilder = builder;
            _buildAssignmentExpression = buildAssignmentExpression;
        }

        public T As(string variableName)
        {
            var expressionBuilder = new ExpressionBuilder(_queryBuilder.Prefixes);
            var assignment = _buildAssignmentExpression(expressionBuilder);
            var bindPattern = new BindPattern(variableName, assignment.Expression);
            if (typeof(T) == typeof(ISelectQueryBuilder))
            {
                _selectBuilder.And(new SparqlVariable(variableName, assignment.Expression));
            }
            else if (typeof(T) == typeof(IGraphPatternBuilder))
            {
                _graphPatternBuilder.Where(bindPattern);
            }
            else if (typeof(T) == typeof(IQueryBuilder))
            {
                ((IQueryBuilder)_queryBuilder).Where(bindPattern);
            }
            else
            {
                // todo: refactor as lookup table
                throw new InvalidOperationException(string.Format("Invalid type of T for creating assignment: {0}", typeof(T)));
            }
            return _queryBuilder;
        }
    }
}