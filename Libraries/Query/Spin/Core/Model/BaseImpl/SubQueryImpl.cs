using VDS.RDF.Query.Spin.Model.IO;
using VDS.RDF.Query.Spin.OntologyHelpers;

namespace VDS.RDF.Query.Spin.Model
{
    public class SubQueryImpl : ElementImpl, ISubQueryResource
    {
        public SubQueryImpl(INode node, SpinModel spinModel)
            : base(node, spinModel)
        {
        }

        public IQueryResource getQuery()
        {
            IResource r = GetResource(SP.PropertyQuery);
            if (r != null)
            {
                return ResourceFactory.asQuery(r);
            }
            else
            {
                return null;
            }
        }

        override public void Print(ISparqlPrinter p)
        {
            p.print("{");
            p.println();
            IQueryResource query = getQuery();
            if (query != null)
            {
                p.setIndentation(p.getIndentation() + 1);
                query.Print(p);
                p.setIndentation(p.getIndentation() - 1);
            }
            else
            {
                p.print("<Exception: Missing sub-query>");
            }
            p.println();
            p.printIndentation(p.getIndentation());
            p.print("}");
        }

        //override public void visit(IElementVisitor visitor)
        //{
        //    visitor.visit(this);
        //}
    }
}