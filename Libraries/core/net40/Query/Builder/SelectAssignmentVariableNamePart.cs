using System;
using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Builder
{
    sealed class SelectAssignmentVariableNamePart : AssignmentVariableNamePart, IAssignmentVariableNamePart<ISelectBuilder>
    {
        private readonly SelectBuilder _selectBuilder;

        internal SelectAssignmentVariableNamePart(SelectBuilder selectBuilder, Func<ExpressionBuilder, SparqlExpression> buildAssignmentExpression)
            : base(buildAssignmentExpression)
        {
            _selectBuilder = selectBuilder;
        }

        public ISelectBuilder As(string variableName)
        {
            _selectBuilder.And(mapper =>  new SparqlVariable(variableName, BuildAssignmentExpression(mapper)));
            return _selectBuilder;
        }
    }
}