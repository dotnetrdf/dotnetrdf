using VDS.RDF.Query.Spin.Core;
using VDS.RDF.Query.Spin.OntologyHelpers;
using VDS.RDF.Query.Spin.SparqlUtil;
using VDS.RDF.Query.Spin.Model.IO;

namespace VDS.RDF.Query.Spin.Model
{

    public class NamedGraphImpl : ElementImpl, INamedGraphResource
    {

        public NamedGraphImpl(INode node, SpinModel spinModel)
            : base(node, spinModel)
        {
        }


        public IResource getNameNode()
        {
            IResource r = GetObject(SP.PropertyGraphNameNode);
            if (r != null)
            {
                IVariableResource variable = ResourceFactory.asVariable(r);
                if (variable != null)
                {
                    return variable;
                }
                else
                {
                    return r;
                }
            }
            else
            {
                return null;
            }
        }


        override public void Print(ISparqlPrinter p)
        {
            p.printKeyword("GRAPH");
            p.print(" ");
            printVarOrResource(p, getNameNode());
            printNestedElementList(p);
        }

        //override public void visit(IElementVisitor visitor)
        //{
        //    visitor.visit(this);
        //}
    }
}
