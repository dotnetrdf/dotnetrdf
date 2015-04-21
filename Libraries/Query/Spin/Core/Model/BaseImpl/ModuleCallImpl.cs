namespace VDS.RDF.Query.Spin.Model
{
    public abstract class ModuleCallImpl : AbstractSPINResource, IModuleCallResource
    {
        public ModuleCallImpl(INode node, SpinModel spinModel)
            : base(node, spinModel)
        {
        }

        public abstract IModuleResource getModule();
    }
}