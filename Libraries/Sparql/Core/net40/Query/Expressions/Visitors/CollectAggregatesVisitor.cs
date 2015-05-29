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
