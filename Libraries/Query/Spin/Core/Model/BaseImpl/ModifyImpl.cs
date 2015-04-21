using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Query.Spin.Model.IO;
using VDS.RDF.Query.Spin.OntologyHelpers;

namespace VDS.RDF.Query.Spin.Model
{
    public class ModifyImpl : UpdateImpl, IModifyResource
    {
        public ModifyImpl(INode node, SpinModel graph)
            : base(node, graph)
        {
        }

        public IEnumerable<Uri> getUsing()
        {
            return ListProperties(SP.PropertyUsing).Select(t => ((IUriNode)t.Object).Uri);
        }

        public IEnumerable<Uri> getUsingNamed()
        {
            return ListProperties(SP.PropertyUsingNamed).Select(t => ((IUriNode)t.Object).Uri);
        }

        public override void printSPINRDF(ISparqlPrinter p)
        {
            printComment(p);
            printPrefixes(p);

            IResource iri = GetResource(SP.PropertyGraphIRI);

            IResource with = GetResource(SP.PropertyWith);
            if (with != null)
            {
                p.printIndentation(p.getIndentation());
                p.printKeyword("WITH");
                p.print(" ");
                p.printURIResource(with);
                p.println();
            }

            // TODO add a INSERT/CONSTRUCT pattern before the delete is effective
            if (printTemplates(p, SP.PropertyDeletePattern, "DELETE", HasProperty(SP.PropertyDeletePattern), iri))
            {
                p.print("\n");
            }
            if (printTemplates(p, SP.PropertyInsertPattern, "INSERT", HasProperty(SP.PropertyInsertPattern), iri))
            {
                p.print("\n");
            }

            foreach (Uri _using in getUsing())
            {
                p.printKeyword("USING");
                p.print(" <");
                p.print(_using.ToString());
                p.print(">");
                p.println();
            }

            foreach (Uri usingNamed in getUsingNamed())
            {
                p.printKeyword("USING");
                p.print(" ");
                p.printKeyword("NAMED");
                p.print(" <");
                p.print(usingNamed.ToString());
                p.print(">");
                p.println();
            }
            printWhere(p);
        }
    }
}