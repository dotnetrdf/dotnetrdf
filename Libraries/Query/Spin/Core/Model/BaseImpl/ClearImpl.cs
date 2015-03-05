using VDS.RDF.Query.Spin.Core;
using VDS.RDF.Query.Spin.SparqlUtil;
using VDS.RDF.Query.Spin.Model.IO;

namespace VDS.RDF.Query.Spin.Model
{

    public class ClearImpl : UpdateImpl, IClearResource
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