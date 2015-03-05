using VDS.RDF.Query.Spin.Core;
using VDS.RDF.Query.Spin.OntologyHelpers;
using VDS.RDF.Query.Spin.SparqlUtil;
using VDS.RDF.Query.Spin.Model.IO;

namespace VDS.RDF.Query.Spin.Model
{

    public class DeleteDataImpl : UpdateImpl, IDeleteDataResource
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

    }
}