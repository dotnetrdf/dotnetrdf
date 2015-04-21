using VDS.RDF.Query.Spin.Model.IO;

namespace VDS.RDF.Query.Spin.Model
{
    public class DropImpl : UpdateImpl, IDropResource
    {
        public DropImpl(INode node, SpinModel graph)
            : base(node, graph)
        {
        }

        override public void printSPINRDF(ISparqlPrinter p)
        {
            p.printKeyword("DROP");
            p.print(" ");
            printSilent(p);
            printGraphDefaultNamedOrAll(p);
        }
    }
}