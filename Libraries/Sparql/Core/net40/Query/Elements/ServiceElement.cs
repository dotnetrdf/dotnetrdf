using System;
using System.Collections.Generic;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Elements
{
    public class ServiceElement
        : IElement
    {
        public ServiceElement(IElement innerElement, Uri endpointUri, bool silent)
        {
            if (innerElement == null) throw new ArgumentNullException("innerElement");
            if (endpointUri == null) throw new ArgumentNullException("endpointUri");
            this.InnerElement = innerElement;
            this.EndpointUri = endpointUri;
            this.IsSilent = silent;
        }

        public IElement InnerElement { get; private set; }

        public Uri EndpointUri { get; private set; }

        public bool IsSilent { get; private set; }

        public bool Equals(IElement other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            if (!(other is ServiceElement)) return false;

            ServiceElement svc = (ServiceElement) other;
            return EqualityHelper.AreUrisEqual(this.EndpointUri, svc.EndpointUri) && this.IsSilent == svc.IsSilent && this.InnerElement.Equals(svc.InnerElement);
        }

        public void Accept(IElementVisitor visitor)
        {
            visitor.Visit(this);
        }

        public IEnumerable<string> Variables
        {
            get { return this.InnerElement.Variables; }
        }

        public IEnumerable<string> ProjectedVariables
        {
            get { return this.InnerElement.ProjectedVariables; }
        }
    }
}