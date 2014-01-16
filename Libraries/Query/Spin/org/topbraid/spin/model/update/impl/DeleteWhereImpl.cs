using VDS.RDF.Query.Spin.SparqlUtil;
using org.topbraid.spin.vocabulary;
using VDS.RDF;
using VDS.RDF.Query.Spin;
using VDS.RDF.Query.Datasets;

namespace org.topbraid.spin.model.update.impl
{

    public class DeleteWhereImpl : UpdateImpl, IDeleteWhere
    {

        public DeleteWhereImpl(INode node, SpinProcessor graph)
            : base(node, graph)
        {

        }


        override public void printSPINRDF(IContextualSparqlPrinter p)
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