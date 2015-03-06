using System;
using VDS.RDF.Query.Spin.Core;
using VDS.RDF.Query.Spin.OntologyHelpers;
using VDS.RDF.Query.Spin.SparqlUtil;
using VDS.RDF.Query.Spin.Utility;
using VDS.RDF.Query.Spin.Model.IO;

namespace VDS.RDF.Query.Spin.Model
{

    public class ServiceImpl : ElementImpl, IServiceResource
    {

        public ServiceImpl(INode node, SpinModel spinModel)
            : base(node, spinModel)
        {

        }


        public Uri getServiceURI()
        {
            IResource s = GetResource(SP.PropertyServiceURI);
            if (s != null && s.IsUri())
            {
                IVariableResource variable = ResourceFactory.asVariable(s);
                if (variable == null)
                {
                    return s.Uri;
                }
            }
            return null;
        }


        public IVariableResource getServiceVariable()
        {
            IResource s = GetResource(SP.PropertyServiceURI);
            if (s != null)
            {
                IVariableResource variable = ResourceFactory.asVariable(s);
                if (variable != null)
                {
                    return variable;
                }
            }
            return null;
        }


        override public void Print(ISparqlPrinter p)
        {
            p.printKeyword("SERVICE");
            IVariableResource var = getServiceVariable();
            if (var != null)
            {
                p.print(" ");
                p.printVariable(var.getName());
            }
            else
            {
                Uri uri = getServiceURI();
                if (uri != null)
                {
                    p.print(" ");
                    p.printURIResource(SpinResource.Get(RDFHelper.CreateUriNode(uri), GetModel()));
                }
            }
            printNestedElementList(p);
        }


        //override public void visit(IElementVisitor visitor)
        //{
        //    visitor.visit(this);
        //}
    }
}