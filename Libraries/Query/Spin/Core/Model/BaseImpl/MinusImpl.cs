using VDS.RDF.Query.Spin.Model.IO;

namespace VDS.RDF.Query.Spin.Model
{
    public class MinusImpl : ElementImpl, IMinusResource
    {
        public MinusImpl(INode node, SpinModel spinModel)
            : base(node, spinModel)
        {
        }

        //override public void visit(IElementVisitor visitor)
        //{
        //    visitor.visit(this);
        //}

        // TODO PRINT CONTEXT SHOULD DEPEND ON THE MODEL TO AVOID ADDING RESOURCE-CONSUMMING ATTERNS WHERE NOT NEEDED
        override public void Print(ISparqlPrinter p)
        {
            p.printKeyword("MINUS");
            printNestedElementList(p);
        }
    }
}