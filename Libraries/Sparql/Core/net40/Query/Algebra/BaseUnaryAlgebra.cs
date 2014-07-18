using System;
using System.Collections.Generic;
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Engine.Algebra;

namespace VDS.RDF.Query.Algebra
{
    public abstract class BaseUnaryAlgebra 
        : IUnaryAlgebra
    {
        protected BaseUnaryAlgebra(IAlgebra innerAlgebra)
        {
            if (innerAlgebra == null) throw new ArgumentNullException("innerAlgebra", "Inner Algebra cannot be null");
            this.InnerAlgebra = innerAlgebra;
        }

        public IAlgebra InnerAlgebra { get; private set; }

        public virtual IEnumerable<string> ProjectedVariables
        {
            get { return this.InnerAlgebra.ProjectedVariables; }
        }

        public virtual IEnumerable<string> FixedVariables
        {
            get { return this.InnerAlgebra.FixedVariables; }
        }

        public virtual IEnumerable<string> FloatingVariables
        {
            get { return this.InnerAlgebra.FloatingVariables; }
        }

        public abstract void Accept(IAlgebraVisitor visitor);

        public abstract IEnumerable<ISet> Execute(IAlgebraExecutor executor, IExecutionContext context);

        public abstract bool Equals(IAlgebra other);
    }
}