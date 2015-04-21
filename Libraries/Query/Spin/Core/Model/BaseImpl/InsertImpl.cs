using System;
using VDS.RDF.Query.Spin.Model.IO;
using VDS.RDF.Query.Spin.OntologyHelpers;

namespace VDS.RDF.Query.Spin.Model
{
    [Obsolete()]
    public class InsertImpl : UpdateImpl, IInsertResource
    {
        public InsertImpl(INode node, SpinModel graph)
            : base(node, graph)
        {
        }

        override public void printSPINRDF(ISparqlPrinter p)
        {
            printComment(p);
            printPrefixes(p);
            p.printIndentation(p.getIndentation());
            p.printKeyword("INSERT");
            printGraphIRIs(p, "INTO");
            printTemplates(p, SP.PropertyInsertPattern, null, true, null);
            printWhere(p);
        }
    }
}