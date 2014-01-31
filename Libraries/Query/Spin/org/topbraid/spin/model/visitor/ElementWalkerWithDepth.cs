namespace VDS.RDF.Query.Spin.Model.visitor
{

    /**
     * An ElementWalker that also keeps track of the depth inside of the element
     * structure.  This can be used to determine whether the currently visited
     * element is somewhere nested inside of other elements.
     * 
     * @author Holger Knublauch
     */
    public class ElementWalkerWithDepth : ElementWalker
    {

        private int depth;


        public ElementWalkerWithDepth(IElementVisitor elementVisitor, IExpressionVisitor expressionVisitor) :base(elementVisitor, expressionVisitor)
        {
            
        }


        public int getDepth()
        {
            return depth;
        }


        override protected void visitChildren(IElementGroup group)
        {
            depth++;
            base.visitChildren(group);
            depth--;
        }
    }
}