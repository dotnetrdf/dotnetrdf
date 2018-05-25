namespace Dynamic
{
    using System.Dynamic;

    internal interface IDynamicMetaObjectProviderContainer
    {
        IDynamicMetaObjectProvider InnerMetaObjectProvider { get; }
    }

}
