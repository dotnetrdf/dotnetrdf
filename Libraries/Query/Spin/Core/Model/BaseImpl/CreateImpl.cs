using VDS.RDF.Query.Spin.Model.IO;
using VDS.RDF.Query.Spin.OntologyHelpers;

namespace VDS.RDF.Query.Spin.Model
{
    public class CreateImpl : UpdateImpl, ICreateResource
    {
        public CreateImpl(INode node, SpinModel graph)
            : base(node, graph)
        {
        }

        override public void printSPINRDF(ISparqlPrinter p)
        {
            p.printKeyword("CREATE");
            p.print(" ");
            printSilent(p);
            p.printKeyword("GRAPH");
            p.print(" ");
            p.printURIResource(GetResource(SP.PropertyGraphIRI));
        }
    }
}