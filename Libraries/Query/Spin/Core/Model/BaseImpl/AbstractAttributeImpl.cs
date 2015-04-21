using System;
using VDS.RDF.Query.Spin.Model.IO;
using VDS.RDF.Query.Spin.OntologyHelpers;

namespace VDS.RDF.Query.Spin.Model
{
    public abstract class AbstractAttributeImpl : AbstractSPINResource, IAbstractAttributeResource
    {
        public AbstractAttributeImpl(INode node, SpinModel spinModel)
            : base(node, spinModel)
        {
        }

        public IResource getPredicate()
        {
            IResource r = GetResource(SPL.PropertyPredicate);
            if (r != null && r.IsUri())
            {
                return r;
            }
            else
            {
                return null;
            }
        }

        public IResource getValueType()
        {
            return GetObject(SPL.PropertyValueType);
        }

        public String getComment()
        {
            return GetString(RDFS.PropertyComment);
        }

        public bool IsOptional()
        {
            return (bool)GetBoolean(SPL.PropertyOptional);
        }

        override public void Print(ISparqlPrinter p)
        {
            // TODO Auto-generated method stub
        }
    }
}