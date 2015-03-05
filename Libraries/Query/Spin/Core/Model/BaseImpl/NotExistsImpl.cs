using VDS.RDF.Query.Spin.Core;
using VDS.RDF.Query.Spin.SparqlUtil;
using VDS.RDF.Query.Spin.Model.IO;

namespace VDS.RDF.Query.Spin.Model
{

    public class NotExistsImpl : ElementImpl, INotExistsResource
    {

        public NotExistsImpl(INode node, SpinProcessor spinModel)
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