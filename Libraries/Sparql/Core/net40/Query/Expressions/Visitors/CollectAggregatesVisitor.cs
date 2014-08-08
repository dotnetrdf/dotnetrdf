using System.Collections.Generic;

namespace VDS.RDF.Query.Expressions.Visitors
{
    public class CollectAggregatesVisitor
        : IExpressionVisitor
    {
        public CollectAggregatesVisitor()
        {
            this.Aggregates = new HashSet<IAggregateExpression>();
        }

        private ISet<IAggregateExpression> Aggregates { get; set; }

        public ISet<IAggregateExpression> Collect(IEnumerable<IExpression> expressions)
        {
            this.Aggregates = new HashSet<IAggregateExpression>();
            foreach (IExpression expr in expressions)
            {
                expr.Accept(this);
            }
            return this.Aggregates;
        }

        public void Visit(INullaryExpression nullaryExpression)
        {}

        public void Visit(IUnaryExpression unaryExpression)
        {
            unaryExpression.Argument.Accept(this);
        }

        public void Visit(IBinaryExpression binaryExpression)
        {
            binaryExpression.FirstArgument.Accept(this);
            binaryExpression.SecondArgument.Accept(this);
        }

        public void Visit(ITernayExpression ternayExpression)
        {
            ternayExpression.FirstArgument.Accept(this);
            ternayExpression.SecondArgument.Accept(this);
            ternayExpression.ThirdArgument.Accept(this);
        }

        public void Visit(INAryExpression nAryExpression)
        {
            foreach (IExpression arg in nAryExpression.Arguments)
            {
                arg.Accept(this);
            }
        }

        public void Visit(IAlgebraExpression algebraExpression)
        {
            // TODO Do we need to collect inside algebra?
        }

        public void Visit(IAggregateExpression aggregateExpression)
        {
            this.Aggregates.Add(aggregateExpression);
            foreach (IExpression arg in aggregateExpression.Arguments)
            {
                arg.Accept(this);
            }
        }
    }
}
