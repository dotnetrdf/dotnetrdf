using VDS.RDF;
using VDS.RDF.Query.Spin.SparqlUtil;
using VDS.RDF.Query.Spin;
using VDS.RDF.Query.Spin.LibraryOntology;
using VDS.RDF.Query.Datasets;

namespace VDS.RDF.Query.Spin.Model
{


    public class InsertDataImpl : UpdateImpl, IInsertData
    {

        public InsertDataImpl(INode node, SpinProcessor graph)
            : base(node, graph)
        {

        }


        override public void printSPINRDF(ISparqlFactory p)
        {
            p.printKeyword("INSERT");
            p.print(" ");
            p.printKeyword("DATA");
            printTemplates(p, SP.PropertyData, null, true, null);
        }
    }
}