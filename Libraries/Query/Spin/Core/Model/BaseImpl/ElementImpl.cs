using VDS.RDF.Query.Spin.Core;

namespace VDS.RDF.Query.Spin.Model
{

    public abstract class ElementImpl : AbstractSPINResource, IElementResource
    {

        public ElementImpl(INode node, SpinProcessor spinModel)
            : base(node, spinModel)
        {
        }

        //public abstract void visit(IElementVisitor visitor);
        
    }
}