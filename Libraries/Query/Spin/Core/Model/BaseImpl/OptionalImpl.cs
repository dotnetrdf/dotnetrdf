using VDS.RDF.Query.Spin.SparqlUtil;
using VDS.RDF.Query.Spin.Core;
using VDS.RDF.Query.Spin.Model.IO;

namespace VDS.RDF.Query.Spin.Model
{

    public class OptionalImpl : ElementImpl, IOptionalResource
    {

        public OptionalImpl(INode node, SpinModel spinModel)
            : base(node, spinModel)
        {

        }


        override public void Print(ISparqlPrinter p)
        {
            p.printKeyword("OPTIONAL");
            printNestedElementList(p);
        }


        //override public void visit(IElementVisitor visitor)
        //{
        //    visitor.visit(this);
        //}
    }
}