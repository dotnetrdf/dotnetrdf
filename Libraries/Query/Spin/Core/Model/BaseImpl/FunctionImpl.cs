using VDS.RDF.Query.Spin.Core;
using VDS.RDF.Query.Spin.OntologyHelpers;
using VDS.RDF.Query.Spin.Utility;

namespace VDS.RDF.Query.Spin.Model
{


    public class FunctionImpl : ModuleImpl, IFunctionResource
    {

        public FunctionImpl(INode node, SpinModel spinModel)
            : base(node, spinModel)
        {
        }


        public INode getReturnType()
        {
            return GetObject(SPIN.PropertyReturnType);
        }


        public bool isMagicProperty()
        {
            return HasProperty(RDF.PropertyType, SPIN.ClassMagicProperty);
        }


        public bool isPrivate()
        {
            return HasProperty(SPIN.PropertyPrivate, RDFHelper.TRUE);
        }
    }
}