using System;
using System.Collections.Generic;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Engine.Algebra;

namespace VDS.RDF.Query.Algebra
{
    public class Service
        : BaseUnaryAlgebra
    {
        public Service(IAlgebra innerAlgebra, Uri endpointUri, bool isSilent)
            : base(innerAlgebra)
        {
            if (endpointUri == null) throw new ArgumentNullException("endpointUri");
            EndpointUri = endpointUri;
            IsSilent = isSilent;
        }

        public Uri EndpointUri { get; private set; }

        public bool IsSilent { get; private set; }

        public override bool Equals(IAlgebra other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            if (!(other is Service)) return false;

            Service svc = (Service) other;
            return EqualityHelper.AreUrisEqual(this.EndpointUri, svc.EndpointUri) && this.IsSilent == svc.IsSilent && this.InnerAlgebra.Equals(svc.InnerAlgebra);
        }

        public override void Accept(IAlgebraVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override IEnumerable<ISolution> Execute(IAlgebraExecutor executor, IExecutionContext context)
        {
            return executor.Execute(this, context);
        }
    }
}