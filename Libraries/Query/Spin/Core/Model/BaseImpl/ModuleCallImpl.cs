using VDS.RDF.Query.Spin.Core;

namespace VDS.RDF.Query.Spin.Model
{
    public abstract class ModuleCallImpl : AbstractSPINResource, IModuleCallResource
    {

        public ModuleCallImpl(INode node, SpinProcessor spinModel)
            : base(node, spinModel)
        {
        }

        public abstract IModuleResource getModule();
    }
}