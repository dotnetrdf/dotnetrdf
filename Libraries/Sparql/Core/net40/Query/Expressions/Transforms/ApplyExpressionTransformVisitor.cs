using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Algebra.Transforms;

namespace VDS.RDF.Query.Expressions.Transforms
{
    public class ApplyExpressionTransformVisitor
        : IExpressionVisitor
    {
        public ApplyExpressionTransformVisitor(IExpressionTransform algebraTransform)
        {
            if (algebraTransform == null) throw new ArgumentNullException("algebraTransform");
            this.ExpressionTransform = algebraTransform;
            this.Expressions = new Stack<IExpression>();
        }

        private IExpressionTransform ExpressionTransform { get; set; }

        private Stack<IExpression> Expressions { get; set; }

        private IExpression ResultingExpression
        {
            get
            {
                if (this.Expressions.Count == 0) throw new InvalidOperationException("No resulting expression at this point");
                return this.Expressions.Pop();
            }
            set
            {
                this.Expressions.Push(value);
            }
        }

        public IExpression Transform(IExpression expression)
        {
            expression.Accept(this);
            if (this.Expressions.Count != 1) throw new InvalidOperationException("Unexpected transformation state at end of transform");
            return this.ResultingExpression;
        }

        public void Visit(INullaryExpression nullaryExpression)
        {
            this.ResultingExpression = this.ExpressionTransform.Transform(nullaryExpression);
        }

        public void Visit(IUnaryExpression unaryExpression)
        {
            unaryExpression.Argument.Accept(this);
            this.ResultingExpression = this.ExpressionTransform.Transform(unaryExpression, this.ResultingExpression);
        }

        public void Visit(IBinaryExpression binaryExpression)
        {
            binaryExpression.FirstArgument.Accept(this);
            IExpression firstArg = this.ResultingExpression;
            binaryExpression.SecondArgument.Accept(this);
            IExpression secondArg = this.ResultingExpression;
            this.ResultingExpression = this.ExpressionTransform.Transform(binaryExpression, firstArg, secondArg);
        }

        public void Visit(ITernayExpression ternayExpression)
        {
            throw new NotImplementedException();
        }

        public void Visit(INAryExpression nAryExpression)
        {
            throw new NotImplementedException();
        }

        public void Visit(IAlgebraExpression algebraExpression)
        {
            throw new NotImplementedException();
        }

        public void Visit(IAggregateExpression aggregateExpression)
        {
            throw new NotImplementedException();
        }
    }
}
