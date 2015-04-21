using VDS.RDF.Query.Spin.Model.IO;
using VDS.RDF.Query.Spin.OntologyHelpers;

namespace VDS.RDF.Query.Spin.Model
{
    public class DeleteDataImpl : UpdateImpl, IDeleteDataResource
    {
        public DeleteDataImpl(INode node, SpinModel graph)
            : base(node, graph)
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