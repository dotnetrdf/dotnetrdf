using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Transforms;
using VDS.RDF.Query.Sorting;

namespace VDS.RDF.Query.Algebra.Transforms
{
    /// <summary>
    /// An algebra vistor that applies an algebra transform
    /// </summary>
    public sealed class ApplyTransformVisitor
        : IAlgebraVisitor
    {
        public ApplyTransformVisitor(IAlgebraTransform algebraTransform)
        {
            if (algebraTransform == null) throw new ArgumentNullException("algebraTransform");
            this.AlgebraTransform = algebraTransform;
            this.Algebras = new Stack<IAlgebra>();
        }

        public ApplyTransformVisitor(IAlgebraTransform algebraTransform, IExpressionTransform expressionTransform)
            : this(algebraTransform)
        {
            this.ExpressionTransform = expressionTransform;
        }

        /// <summary>
        /// Transforms the given algebra
        /// </summary>
        /// <param name="algebra">Algebra</param>
        /// <returns>Transformed algebra</returns>
        public IAlgebra Transform(IAlgebra algebra)
        {
            algebra.Accept(this);
            if (this.Algebras.Count != 1) throw new InvalidOperationException("Unexpected transformation state at end of transform");
            return this.ResultingAlgebra;
        }

        private IAlgebraTransform AlgebraTransform { get; set; }

        private IExpressionTransform ExpressionTransform { get; set; }

        private Stack<IAlgebra> Algebras { get; set; }

        private IAlgebra ResultingAlgebra
        {
            get
            {
                if (this.Algebras.Count == 0) throw new InvalidOperationException("No resulting algebra at this point");
                return this.Algebras.Pop();
            }
            set
            {
                this.Algebras.Push(value);
            }
        }

        private IEnumerable<IExpression> TransformExpressions(IEnumerable<IExpression> expressions)
        {
            // Return null if no transform available
            if (this.ExpressionTransform == null) return null;

            // Apply expression transform
            List<IExpression> transformedExpressions = new List<IExpression>();
            bool changed = false;
            foreach (IExpression expr in expressions)
            {
                ApplyExpressionTransformVisitor transform = new ApplyExpressionTransformVisitor(this.ExpressionTransform, this.AlgebraTransform);
                transformedExpressions.Add(transform.Transform(expr));

                // Set changed marker if expression was changed
                if (!expr.Equals(transformedExpressions[transformedExpressions.Count - 1])) changed = true;
            }
            // Return changes if any, otherwise return null
            return changed ? transformedExpressions : null;
        }

        private IEnumerable<KeyValuePair<string, IExpression>> TransformAssignments(IEnumerable<KeyValuePair<string, IExpression>> assignments)
        {
            // Return null if no transform available
            if (this.ExpressionTransform == null) return null;

            // Apply expression transform
            List<KeyValuePair<string, IExpression>> transformedAssignments = new List<KeyValuePair<string, IExpression>>();
            bool changed = false;
            foreach (KeyValuePair<string, IExpression> kvp in assignments)
            {
                ApplyExpressionTransformVisitor transform = new ApplyExpressionTransformVisitor(this.ExpressionTransform, this.AlgebraTransform);
                transformedAssignments.Add(new KeyValuePair<string, IExpression>(kvp.Key, transform.Transform(kvp.Value)));

                // Set changed marker if expression was changed
                if (!kvp.Value.Equals(transformedAssignments[transformedAssignments.Count - 1].Value)) changed = true;
            }

            // Return changes if any, otherwise return null
            return changed ? transformedAssignments : null;
        }

        public void Visit(Bgp bgp)
        {
            this.ResultingAlgebra = this.AlgebraTransform.Transform(bgp);
        }

        public void Visit(Slice slice)
        {
            slice.InnerAlgebra.Accept(this);
            this.ResultingAlgebra = this.AlgebraTransform.Transform(slice, this.ResultingAlgebra);
        }

        public void Visit(Union union)
        {
            union.Lhs.Accept(this);
            IAlgebra lhs = this.ResultingAlgebra;
            union.Rhs.Accept(this);
            IAlgebra rhs = this.ResultingAlgebra;
            this.ResultingAlgebra = this.AlgebraTransform.Transform(union, lhs, rhs);
        }

        public void Visit(NamedGraph namedGraph)
        {
            namedGraph.InnerAlgebra.Accept(this);
            this.ResultingAlgebra = this.AlgebraTransform.Transform(namedGraph, this.ResultingAlgebra);
        }

        public void Visit(Filter filter)
        {
            IEnumerable<IExpression> transformedExpressions = this.TransformExpressions(filter.Expressions);

            filter.InnerAlgebra.Accept(this);
            this.ResultingAlgebra = transformedExpressions == null ? this.AlgebraTransform.Transform(filter, this.ResultingAlgebra) : Filter.Create(this.ResultingAlgebra, transformedExpressions);
        }

        public void Visit(Table table)
        {
            this.ResultingAlgebra = this.AlgebraTransform.Transform(table);
        }

        public void Visit(Join join)
        {
            join.Lhs.Accept(this);
            IAlgebra lhs = this.ResultingAlgebra;
            join.Rhs.Accept(this);
            IAlgebra rhs = this.ResultingAlgebra;
            this.ResultingAlgebra = this.AlgebraTransform.Transform(join, lhs, rhs);
        }

        public void Visit(LeftJoin leftJoin)
        {
            IEnumerable<IExpression> transformedExpressions = this.TransformExpressions(leftJoin.Expressions);

            leftJoin.Lhs.Accept(this);
            IAlgebra lhs = this.ResultingAlgebra;
            leftJoin.Rhs.Accept(this);
            IAlgebra rhs = this.ResultingAlgebra;
            this.ResultingAlgebra = transformedExpressions == null ? this.AlgebraTransform.Transform(leftJoin, lhs, rhs) : new LeftJoin(lhs, rhs, transformedExpressions);
        }

        public void Visit(Minus minus)
        {
            minus.Lhs.Accept(this);
            IAlgebra lhs = this.ResultingAlgebra;
            minus.Rhs.Accept(this);
            IAlgebra rhs = this.ResultingAlgebra;
            this.ResultingAlgebra = this.AlgebraTransform.Transform(minus, lhs, rhs);
        }

        public void Visit(Distinct distinct)
        {
            distinct.InnerAlgebra.Accept(this);
            this.ResultingAlgebra = this.AlgebraTransform.Transform(distinct, this.ResultingAlgebra);
        }

        public void Visit(Reduced reduced)
        {
            reduced.InnerAlgebra.Accept(this);
            this.ResultingAlgebra = this.AlgebraTransform.Transform(reduced, this.ResultingAlgebra);
        }

        public void Visit(Project project)
        {
            project.InnerAlgebra.Accept(this);
            this.ResultingAlgebra = this.AlgebraTransform.Transform(project, this.ResultingAlgebra);
        }

        public void Visit(OrderBy orderBy)
        {
            IEnumerable<IExpression> rawTransformedExpressions = TransformExpressions(orderBy.SortConditions.Select(c => c.Expression));

            orderBy.InnerAlgebra.Accept(this);
            if (rawTransformedExpressions == null)
            {
                this.ResultingAlgebra = this.AlgebraTransform.Transform(orderBy, this.ResultingAlgebra);
            }
            else
            {
                List<IExpression> transformedExpressions = rawTransformedExpressions.ToList();
                List<ISortCondition> transformedSortConditions = new List<ISortCondition>();
                for (int i = 0; i < orderBy.SortConditions.Count; i++)
                {
                    transformedSortConditions.Add(new SortCondition(transformedExpressions[i], orderBy.SortConditions[i].IsAscending));
                }
                this.ResultingAlgebra = new OrderBy(this.ResultingAlgebra, transformedSortConditions);
            }
        }

        public void Visit(Extend extend)
        {
            IEnumerable<KeyValuePair<string, IExpression>> transformedAssignments = this.TransformAssignments(extend.Assignments);

            extend.InnerAlgebra.Accept(this);
            this.ResultingAlgebra = transformedAssignments == null ? this.AlgebraTransform.Transform(extend, this.ResultingAlgebra) : Extend.Create(this.ResultingAlgebra, transformedAssignments);
        }

        public void Visit(GroupBy groupBy)
        {
            

            groupBy.InnerAlgebra.Accept(this);
            this.ResultingAlgebra = this.AlgebraTransform.Transform(groupBy, this.ResultingAlgebra);
        }

        public void Visit(Service service)
        {
            service.InnerAlgebra.Accept(this);
            this.ResultingAlgebra = this.AlgebraTransform.Transform(service, this.ResultingAlgebra);
        }

        public void Visit(PropertyPath path)
        {
            path.InnerAlgebra.Accept(this);
            this.ResultingAlgebra = this.AlgebraTransform.Transform(path, this.ResultingAlgebra);
        }

        public void Visit(TopN topN)
        {
            topN.InnerAlgebra.Accept(this);
            this.ResultingAlgebra = this.AlgebraTransform.Transform(topN, this.ResultingAlgebra);
        }

        public void Visit(PropertyFunction propertyFunction)
        {
            propertyFunction.InnerAlgebra.Accept(this);
            this.ResultingAlgebra = this.AlgebraTransform.Transform(propertyFunction, this.ResultingAlgebra);
        }
    }
}
