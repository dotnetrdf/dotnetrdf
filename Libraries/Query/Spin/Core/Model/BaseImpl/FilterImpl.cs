using VDS.RDF.Query.Spin.Model.IO;
using VDS.RDF.Query.Spin.OntologyHelpers;

namespace VDS.RDF.Query.Spin.Model
{
    public class FilterImpl : ElementImpl, IFilterResource
    {
        public FilterImpl(INode node, SpinModel spinModel)
            : base(node, spinModel)
        {
        }

        public IResource getExpression()
        {
            IResource expr = GetObject(SP.PropertyExpression);
            if (expr != null)
            {
                return ResourceFactory.asExpression(expr);
            }
            else
            {
                return null;
            }
        }

        override public void Print(ISparqlPrinter context)
        {
            context.printKeyword("FILTER");
            context.print(" ");
            IResource expression = getExpression();
            if (expression == null)
            {
                context.print("<Exception: Missing expression>");
            }
            else
            {
                printNestedExpressionString(context, expression, true);
            }
        }

        //override public void visit(IElementVisitor visitor)
        //{
        //    visitor.visit(this);
        //}
    }
}