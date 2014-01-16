using VDS.RDF;
using VDS.RDF.Query.Spin.SparqlUtil;
using VDS.RDF.Query.Spin;
using org.topbraid.spin.vocabulary;
using VDS.RDF.Query.Datasets;

namespace org.topbraid.spin.model.update.impl
{


    public class InsertDataImpl : UpdateImpl, IInsertData
    {

        public InsertDataImpl(INode node, SpinProcessor graph)
            : base(node, graph)
        {

        }


        override public void printSPINRDF(IContextualSparqlPrinter p)
        {
            p.printKeyword("INSERT");
            p.print(" ");
            p.printKeyword("DATA");
            printTemplates(p, SP.PropertyData, null, true, null);
        }
    }
}