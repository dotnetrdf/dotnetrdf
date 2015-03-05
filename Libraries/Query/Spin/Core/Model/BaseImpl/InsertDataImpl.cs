using VDS.RDF.Query.Spin.Core;
using VDS.RDF.Query.Spin.OntologyHelpers;
using VDS.RDF.Query.Spin.SparqlUtil;
using VDS.RDF.Query.Spin.Model.IO;

namespace VDS.RDF.Query.Spin.Model
{


    public class InsertDataImpl : UpdateImpl, IInsertDataResource
    {

        public InsertDataImpl(INode node, SpinProcessor graph)
            : base(node, graph)
        {

        }


        override public void printSPINRDF(ISparqlPrinter p)
        {
            p.printKeyword("INSERT");
            p.print(" ");
            p.printKeyword("DATA");
            printTemplates(p, SP.PropertyData, null, true, null);
        }


    }
}