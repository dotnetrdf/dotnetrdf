using VDS.RDF;
using VDS.RDF.Query.Spin.SparqlUtil;
using VDS.RDF.Query.Spin;
using VDS.RDF.Query.Datasets;
namespace VDS.RDF.Query.Spin.Model
{

    public class ClearImpl : UpdateImpl, IClear
    {

        public ClearImpl(INode node, SpinProcessor graph)
            :base(node, graph)
        {
            
        }


        override public void printSPINRDF(ISparqlPrinter p)
        {
            p.printKeyword("CLEAR");
            p.print(" ");
            printSilent(p);
            printGraphDefaultNamedOrAll(p);
        }
    }
}