using VDS.RDF.Query.Spin.Model.IO;
using VDS.RDF.Query.Spin.OntologyHelpers;

namespace VDS.RDF.Query.Spin.Model
{
    public class LoadImpl : UpdateImpl, ILoadResource
    {
        public LoadImpl(INode node, SpinModel spinModel)
            : base(node, spinModel)
        {
        }

        override public void printSPINRDF(ISparqlPrinter p)
        {
            p.printKeyword("LOAD");
            p.print(" ");
            printSilent(p);
            IResource document = GetResource(SP.PropertyDocument);
            p.printURIResource(document);
            IResource into = GetResource(SP.PropertyInto);
            if (into != null)
            {
                p.print(" ");
                p.printKeyword("INTO");
                p.print(" ");
                p.printKeyword("GRAPH");
                p.print(" ");
                p.printURIResource(into);
            }
        }
    }
}