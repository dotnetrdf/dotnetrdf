using VDS.RDF.Query.Spin.SparqlUtil;
using org.topbraid.spin.vocabulary;
using VDS.RDF;
using VDS.RDF.Query.Spin;
using VDS.RDF.Query.Datasets;

namespace org.topbraid.spin.model.update.impl
{

    public class CreateImpl : UpdateImpl, ICreate
    {

        public CreateImpl(INode node, SpinProcessor graph)
            : base(node, graph)
        {

        }


        override public void printSPINRDF(IContextualSparqlPrinter p)
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