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
using System.Linq;
using System.Text;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Sorting
{
    public class ExpressionValueComparer
        : IComparer<ISolution>
    {
        public ExpressionValueComparer(IExpression expression, IExpressionContext context, IComparer<IValuedNode> nodeComparer)
        {
            if (expression == null) throw new ArgumentNullException("expression");
            if (context == null) throw new ArgumentNullException("context");
            if (nodeComparer == null) throw new ArgumentNullException("nodeComparer");
            this.Expression = expression;
            this.Context = context;
            this.NodeComparer = nodeComparer;
        }

        private IExpression Expression { get; set; }

        private IExpressionContext Context { get; set; }

        private IComparer<IValuedNode> NodeComparer { get; set; } 

        public IValuedNode SafeEvaluate(ISolution solution)
        {
            try
            {
                return this.Expression.Evaluate(solution, this.Context);
            }
            catch (RdfQueryException)
            {
                return null;
            }
        }

        public int Compare(ISolution x, ISolution y)
        {
            IValuedNode xNode = SafeEvaluate(x);
            IValuedNode yNode = SafeEvaluate(y);

            return this.NodeComparer.Compare(xNode, yNode);
        }
    }
}
