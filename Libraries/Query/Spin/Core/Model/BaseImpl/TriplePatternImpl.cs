using VDS.RDF.Query.Spin.Core;

namespace VDS.RDF.Query.Spin.Model
{

    public class TriplePatternImpl : TripleImpl, ITriplePatternResource
    {

        public TriplePatternImpl(INode node, SpinModel spinModel)
            : base(node, spinModel)
        {

        }

        //public void visit(IElementVisitor visitor)
        //{
        //    visitor.visit(this);
        //}
    }
}