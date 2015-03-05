using System;
using VDS.RDF.Query.Spin.Core;
using VDS.RDF.Query.Spin.OntologyHelpers;
using VDS.RDF.Query.Spin.SparqlUtil;
using VDS.RDF.Query.Spin.Model.IO;

namespace VDS.RDF.Query.Spin.Model
{

    public abstract class AbstractAttributeImpl : AbstractSPINResource, IAbstractAttributeResource
    {

        public AbstractAttributeImpl(INode node, SpinProcessor spinModel)
            : base(node, spinModel)
        {
        }


        public IResource getPredicate()
        {
            IResource r = GetResource(SPL.PropertyPredicate);
            if (r!=null && r.IsUri())
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