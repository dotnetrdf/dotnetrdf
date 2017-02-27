using VDS.RDF;
using VDS.RDF.Query.Spin.SparqlUtil;
using VDS.RDF.Query.Spin;
using VDS.RDF.Query.Spin.LibraryOntology;
using VDS.RDF.Query.Datasets;

namespace VDS.RDF.Query.Spin.Model
{

    public class DeleteDataImpl : UpdateImpl, IDeleteData
    {

        public DeleteDataImpl(INode node, SpinProcessor graph)
            :base(node, graph)
        {
            
        }


        override public void printSPINRDF(ISparqlPrinter p)
        {
            p.printKeyword("DELETE");
            p.print(" ");
            p.printKeyword("DATA");
            printTemplates(p, SP.PropertyData, null, true, null);
        }

        override public void PrintEnhancedSPARQL(ISparqlPrinter p)
        {
            p.PrintEnhancedSPARQL(this);
        }
    }
}