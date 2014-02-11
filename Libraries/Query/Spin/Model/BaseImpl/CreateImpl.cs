using VDS.RDF.Query.Spin.SparqlUtil;
using VDS.RDF.Query.Spin.LibraryOntology;
using VDS.RDF;
using VDS.RDF.Query.Spin;
using VDS.RDF.Query.Datasets;

namespace VDS.RDF.Query.Spin.Model
{

    public class CreateImpl : UpdateImpl, ICreate
    {

        public CreateImpl(INode node, SpinProcessor graph)
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
            p.printURIResource(getResource(SP.PropertyGraphIRI));
        }
    }
}