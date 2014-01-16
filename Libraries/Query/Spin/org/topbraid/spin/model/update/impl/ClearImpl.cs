using VDS.RDF;
using VDS.RDF.Query.Spin.SparqlUtil;
using VDS.RDF.Query.Spin;
using VDS.RDF.Query.Datasets;
namespace org.topbraid.spin.model.update.impl
{

    public class ClearImpl : UpdateImpl, IClear
    {

        public ClearImpl(INode node, SpinProcessor graph)
            :base(node, graph)
        {
            
        }


        override public void printSPINRDF(IContextualSparqlPrinter p)
        {
            p.printKeyword("CLEAR");
            p.print(" ");
            printSilent(p);
            printGraphDefaultNamedOrAll(p);
        }
    }
}