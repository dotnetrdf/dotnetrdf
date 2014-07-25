namespace VDS.RDF.Query.Expressions
{
    /// <summary>
    /// Interface for expression visitors
    /// </summary>
    public interface IExpressionVisitor
    {
        void Visit(IExpression expression);

        void Visit(INullaryExpression nullaryExpression);

        void Visit(IUnaryExpression unaryExpression);

        void Visit(IBinaryExpression binaryExpression);

        void Visit(ITernayExpression ternayExpression);

        void Visit(INAryExpression nAryExpression);

        void Visit(IAlgebraExpression algebraExpression);
    }
}
