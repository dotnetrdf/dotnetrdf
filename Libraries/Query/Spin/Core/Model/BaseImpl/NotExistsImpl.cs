using VDS.RDF.Query.Spin.Model.IO;

namespace VDS.RDF.Query.Spin.Model
{
    public class NotExistsImpl : ElementImpl, INotExistsResource
    {
        public NotExistsImpl(INode node, SpinModel spinModel)
            : base(node, spinModel)
        {
        }

        //override public void visit(IElementVisitor visitor)
        //{
        //    visitor.visit(this);
        //}

        override public void Print(ISparqlPrinter p)
        {
            p.printKeyword("NOT EXISTS");
            printNestedElementList(p);
        }
    }
}