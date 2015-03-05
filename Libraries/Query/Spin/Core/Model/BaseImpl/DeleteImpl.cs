using System;
using VDS.RDF.Query.Spin.Core;
using VDS.RDF.Query.Spin.OntologyHelpers;
using VDS.RDF.Query.Spin.SparqlUtil;
using VDS.RDF.Query.Spin.Model.IO;

namespace VDS.RDF.Query.Spin.Model
{
    [Obsolete()]

    public class DeleteImpl : UpdateImpl, IDeleteResource
    {

        public DeleteImpl(INode node, SpinProcessor graph)
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