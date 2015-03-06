using VDS.RDF.Query.Spin.Core;
using VDS.RDF.Query.Spin.OntologyHelpers;
using VDS.RDF.Query.Spin.SparqlUtil;
using VDS.RDF.Query.Spin.Model.IO;

namespace VDS.RDF.Query.Spin.Model
{

    public class DeleteWhereImpl : UpdateImpl, IDeleteWhereResource
    {

        public DeleteWhereImpl(INode node, SpinModel graph)
            : base(node, graph)
        {

        }


        override public void printSPINRDF(ISparqlPrinter p)
        {
            printComment(p);
            printPrefixes(p);
            p.printIndentation(p.getIndentation());
            p.printKeyword("DELETE");
            p.print(" ");
            p.printKeyword("WHERE");
            printNestedElementList(p, SP.PropertyWhere);
        }
    }
}