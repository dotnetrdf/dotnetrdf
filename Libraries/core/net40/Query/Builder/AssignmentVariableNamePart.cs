using System;
using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Builder
{
    /// <summary>
    /// Exposes method for assigning a name to an expression variable
    /// </summary>
    public interface IAssignmentVariableNamePart<out T>
    {
        /// <summary>
        /// Set the expression's variable name
        /// </summary>
        /// <returns>the parent query or graph pattern builder</returns>
        T As(string variableName);

        //{
        //    var expressionBuilder = new ExpressionBuilder(_prefixes);
        //    var assignment = _buildAssignmentExpression(expressionBuilder);
        //    var bindPattern = new BindPattern(variableName, assignment.Expression);
        //    if (_selectBuilder != null)
        //    {
        //        _selectBuilder.And(new SparqlVariable(variableName, assignment.Expression));
        //        return (T)_selectBuilder;
        //    }

        //    if (_graphPatternBuilder != null)
        //    {
        //        _graphPatternBuilder.Where(bindPattern);
        //        return (T)_graphPatternBuilder;
        //    }

        //    // todo: refactor as lookup table
        //    throw new InvalidOperationException(string.Format("Invalid type of T for creating assignment: {0}", typeof(T)));
        //}
    }

    internal abstract class AssignmentVariableNamePart
    {
        private readonly Func<ExpressionBuilder, SparqlExpression> _buildAssignmentExpression;

        protected AssignmentVariableNamePart(Func<ExpressionBuilder, SparqlExpression> buildAssignmentExpression)
        {
            _buildAssignmentExpression = buildAssignmentExpression;
        }

        protected ISparqlExpression BuildAssignmentExpression(INamespaceMapper prefixes)
        {
            var expressionBuilder = new ExpressionBuilder(prefixes);
            var assignment = _buildAssignmentExpression(expressionBuilder);
            return assignment.Expression;
        }
    }
}