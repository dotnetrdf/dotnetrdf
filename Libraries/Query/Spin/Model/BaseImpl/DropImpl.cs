using VDS.RDF;
using VDS.RDF.Query.Spin.SparqlUtil;
using VDS.RDF.Query.Spin;
using VDS.RDF.Query.Datasets;
namespace VDS.RDF.Query.Spin.Model
{


    public class DropImpl : UpdateImpl, IDrop
    {

        public DropImpl(INode node, SpinProcessor graph)
            : base(node, graph)
        {

        }


        override public void printSPINRDF(ISparqlPrinter p)
        {
            p.printKeyword("DROP");
            p.print(" ");
            printSilent(p);
            printGraphDefaultNamedOrAll(p);
        }
    }
}