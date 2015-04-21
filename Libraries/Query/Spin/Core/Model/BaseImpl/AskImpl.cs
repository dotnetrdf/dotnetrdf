using VDS.RDF.Query.Spin.Model.IO;

namespace VDS.RDF.Query.Spin.Model
{
    public class AskImpl : QueryImpl, IAskResource
    {
        public AskImpl(INode node, SpinModel spinModel)
            : base(node, spinModel)
        {
        }

        override public void printSPINRDF(ISparqlPrinter context)
        {
            printComment(context);
            printPrefixes(context);
            context.printIndentation(context.getIndentation());
            context.printKeyword("ASK");
            printStringFrom(context);
            context.print(" ");
            if (context.getIndentation() > 0)
            {
                // Avoid unnecessary whitespace after ASK -> put on extra row
                context.println();
            }
            printWhere(context);
            printValues(context);
        }
    }
}