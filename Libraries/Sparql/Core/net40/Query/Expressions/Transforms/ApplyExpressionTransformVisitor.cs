using System;
using System.Collections.Generic;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Algebra.Transforms;

namespace VDS.RDF.Query.Expressions.Transforms
{
    public class ApplyExpressionTransformVisitor
        : IExpressionVisitor
    {
        public ApplyExpressionTransformVisitor(IExpressionTransform expressionTransform)
        {
            if (expressionTransform == null) throw new ArgumentNullException("expressionTransform");
            this.ExpressionTransform = expressionTransform;
            this.Expressions = new Stack<IExpression>();
        }

        public ApplyExpressionTransformVisitor(IExpressionTransform expressionTransform, IAlgebraTransform algebraTransform)
            : this(expressionTransform)
        {
            this.AlgebraTransform = algebraTransform;
        }

        private IExpressionTransform ExpressionTransform { get; set; }

        private IAlgebraTransform AlgebraTransform { get; set; }

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
            ternayExpression.FirstArgument.Accept(this);
            IExpression firstArg = this.ResultingExpression;
            ternayExpression.SecondArgument.Accept(this);
            IExpression secondArg = this.ResultingExpression;
            ternayExpression.ThirdArgument.Accept(this);
            IExpression thirdArg = this.ResultingExpression;
            this.ResultingExpression = this.ExpressionTransform.Transform(ternayExpression, firstArg, secondArg, thirdArg);
        }

        public void Visit(INAryExpression nAryExpression)
        {
            List<IExpression> transformedArgs = new List<IExpression>();
            foreach (IExpression arg in nAryExpression.Arguments)
            {
                arg.Accept(this);
                transformedArgs.Add(this.ResultingExpression);
            }
            this.ResultingExpression = this.ExpressionTransform.Transform(nAryExpression, transformedArgs);
        }

        public void Visit(IAlgebraExpression algebraExpression)
        {
            IAlgebra algebra = algebraExpression.Algebra;
            if (this.AlgebraTransform != null)
            {
                ApplyTransformVisitor transform = new ApplyTransformVisitor(this.AlgebraTransform, this.ExpressionTransform);
                algebra = transform.Transform(algebra);
            }
            this.ResultingExpression = this.ExpressionTransform.Transform(algebraExpression, algebra);
        }

        public void Visit(IAggregateExpression aggregateExpression)
        {
            List<IExpression> transformedArgs = new List<IExpression>();
            foreach (IExpression arg in aggregateExpression.Arguments)
            {
                arg.Accept(this);
                transformedArgs.Add(this.ResultingExpression);
            }
            this.ResultingExpression = this.ExpressionTransform.Transform(aggregateExpression, transformedArgs);
        }
    }
}
