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
