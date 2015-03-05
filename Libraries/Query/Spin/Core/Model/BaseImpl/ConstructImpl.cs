using System.Collections.Generic;
using VDS.RDF.Query.Spin.Core;
using VDS.RDF.Query.Spin.OntologyHelpers;
using VDS.RDF.Query.Spin.SparqlUtil;
using VDS.RDF.Query.Spin.Model.IO;

namespace VDS.RDF.Query.Spin.Model
{
    public class ConstructImpl : QueryImpl, IConstructResource
    {

        public ConstructImpl(INode node, SpinProcessor spinModel)
            : base(node, spinModel)
        {
        }


        public List<ITripleTemplateResource> getTemplates()
        {
            List<ITripleTemplateResource> results = new List<ITripleTemplateResource>();
            foreach (IResource next in getList(SP.PropertyTemplates))
            {
                if (next != null && !(next.IsLiteral()))
                {
                    results.Add((ITripleTemplateResource)next.As(typeof(TripleTemplateImpl)));
                }
            }
            return results;
        }

        override public void Print(ISparqlPrinter p)
        {
            // TODO Auto-generated method stub
        }

        override public void printSPINRDF(ISparqlPrinter context)
        {
            printComment(context);
            printPrefixes(context);
            context.printIndentation(context.getIndentation());
            context.printKeyword("CONSTRUCT");
            context.print(" {");
            context.println();
            foreach (ITripleTemplateResource template in getTemplates())
            {
                context.printIndentation(context.getIndentation() + 1);
                template.Print(context);
                context.print(" .");
                context.println();
            }
            context.printIndentation(context.getIndentation());
            context.print("}");
            printStringFrom(context);
            context.println();
            printWhere(context);
            printSolutionModifiers(context);
            printValues(context);
        }
    }
}