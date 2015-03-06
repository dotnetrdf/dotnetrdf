using VDS.RDF.Query.Spin.Core;
using VDS.RDF.Query.Spin.SparqlUtil;
using VDS.RDF.Query.Spin.Model.IO;

namespace VDS.RDF.Query.Spin.Model
{
    public class TripleTemplateImpl : TripleImpl, ITripleTemplateResource
    {

        public TripleTemplateImpl(INode node, SpinModel spinModel)
            : base(node, spinModel)
        {
        }


        override public void Print(ISparqlPrinter p)
        {
            p.setNamedBNodeMode(true);
            base.Print(p);
            p.setNamedBNodeMode(false);
        }
    }
}