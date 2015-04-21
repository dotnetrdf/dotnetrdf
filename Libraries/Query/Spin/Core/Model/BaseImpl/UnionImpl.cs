using System.Collections.Generic;
using VDS.RDF.Query.Spin.Model.IO;

namespace VDS.RDF.Query.Spin.Model
{
    public class UnionImpl : ElementImpl, IUnionResource
    {
        public UnionImpl(INode node, SpinModel spinModel)
            : base(node, spinModel)
        {
        }

        override public void Print(ISparqlPrinter p)
        {
            List<IElementResource> elements = getElements();
            for (IEnumerator<IElementResource> it = elements.GetEnumerator(); it.MoveNext(); )
            {
                IElementResource element = it.Current;
                p.print("{");
                p.println();
                p.setIndentation(p.getIndentation() + 1);
                element.Print(p);
                p.setIndentation(p.getIndentation() - 1);
                p.printIndentation(p.getIndentation());
                p.print("}");
                if (it.MoveNext())
                {
                    p.println();
                    p.printIndentation(p.getIndentation());
                    p.printKeyword("UNION");
                    p.println();
                    p.printIndentation(p.getIndentation());
                }
            }
        }

        //override public void visit(IElementVisitor visitor)
        //{
        //    visitor.visit(this);
        //}
    }
}