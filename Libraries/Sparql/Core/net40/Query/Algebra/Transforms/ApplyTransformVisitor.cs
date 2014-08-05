using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            filter.InnerAlgebra.Accept(this);
            this.ResultingAlgebra = this.AlgebraTransform.Transform(filter, this.ResultingAlgebra);
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
            leftJoin.Lhs.Accept(this);
            IAlgebra lhs = this.ResultingAlgebra;
            leftJoin.Rhs.Accept(this);
            IAlgebra rhs = this.ResultingAlgebra;
            this.ResultingAlgebra = this.AlgebraTransform.Transform(leftJoin, lhs, rhs);
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
            orderBy.InnerAlgebra.Accept(this);
            this.ResultingAlgebra = this.AlgebraTransform.Transform(orderBy, this.ResultingAlgebra);
        }

        public void Visit(Extend extend)
        {
            extend.InnerAlgebra.Accept(this);
            this.ResultingAlgebra = this.AlgebraTransform.Transform(extend, this.ResultingAlgebra);
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
