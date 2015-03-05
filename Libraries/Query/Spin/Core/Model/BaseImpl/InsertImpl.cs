using System;
using VDS.RDF.Query.Spin.Core;
using VDS.RDF.Query.Spin.OntologyHelpers;
using VDS.RDF.Query.Spin.SparqlUtil;
using VDS.RDF.Query.Spin.Model.IO;

namespace VDS.RDF.Query.Spin.Model
{
    [Obsolete()]
    public class InsertImpl : UpdateImpl, IInsertResource
    {

        public InsertImpl(INode node, SpinProcessor graph)
            :base(node, graph)
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