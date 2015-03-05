using VDS.RDF.Query.Spin.Core;
using VDS.RDF.Query.Spin.SparqlUtil;
using VDS.RDF.Query.Spin.Model.IO;

namespace VDS.RDF.Query.Spin.Model
{
    public class ExistsImpl : ElementImpl, IExistsResource
    {

        public ExistsImpl(INode node, SpinProcessor spinModel)
            : base(node, spinModel)
        {
        }


        //override public void visit(IElementVisitor visitor)
        //{
        //    visitor.visit(this);
        //}


        override public void Print(ISparqlPrinter p)
        {
            p.printKeyword("EXISTS");
            printNestedElementList(p);
        }
    }
}