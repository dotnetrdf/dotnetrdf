using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Engine.Algebra;

namespace VDS.RDF.Query.Algebra
{
    public class Service
        : IAlgebra
    {
        public bool Equals(IAlgebra other)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> ProjectedVariables
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<string> FixedVariables
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<string> FloatingVariables
        {
            get { throw new NotImplementedException(); }
        }

        public void Accept(IAlgebraVisitor visitor)
        {
            visitor.Visit(this);
        }

        public IEnumerable<ISet> Execute(IAlgebraExecutor executor, IExecutionContext context)
        {
            return executor.Execute(this, context);
        }
    }
}
