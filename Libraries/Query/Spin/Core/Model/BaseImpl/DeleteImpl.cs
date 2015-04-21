using System;
using VDS.RDF.Query.Spin.Model.IO;
using VDS.RDF.Query.Spin.OntologyHelpers;

namespace VDS.RDF.Query.Spin.Model
{
    [Obsolete()]
    public class DeleteImpl : UpdateImpl, IDeleteResource
    {
        public DeleteImpl(INode node, SpinModel graph)
            : base(node, graph)
        {
        }

        public override void printSPINRDF(ISparqlPrinter p)
        {
            printComment(p);
            printPrefixes(p);
            p.printIndentation(p.getIndentation());
            p.printKeyword("DELETE");
            printGraphIRIs(p, "FROM");
            printTemplates(p, SP.PropertyDeletePattern, null, true, null);
            printWhere(p);
        }
    }
}