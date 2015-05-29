/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-develop@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

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
