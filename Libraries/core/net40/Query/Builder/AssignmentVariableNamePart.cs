using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Builder
{
    public sealed class AssignmentVariableNamePart<T>
    {
        private readonly ICommonQueryBuilder<T> _queryBuilder;
        private readonly SparqlExpression _assignment;

        internal AssignmentVariableNamePart(ICommonQueryBuilder<T> queryBuilder, SparqlExpression assignment)
        {
            _queryBuilder = queryBuilder;
            _assignment = assignment;
        }

        public T As(string variableName)
        {
            return _queryBuilder.Where(new BindPattern(variableName, _assignment.Expression));
        }
    }
}