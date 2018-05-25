namespace Dynamic
{
    using System.Dynamic;
    using System.Linq.Expressions;

    internal class DelegatingMetaObject : DynamicMetaObject
    {
        private readonly DynamicMetaObject innerMetaObject;

        internal DelegatingMetaObject(Expression expression, IDynamicMetaObjectProviderContainer value) : base(expression, BindingRestrictions.Empty, value)
        {
            var parameter =
                Expression.Property(
                    Expression.Convert(
                        this.Expression,
                        this.LimitType),
                    nameof(IDynamicMetaObjectProviderContainer.InnerMetaObjectProvider));

            this.innerMetaObject = value.InnerMetaObjectProvider.GetMetaObject(parameter);
        }

        public override DynamicMetaObject BindGetIndex(GetIndexBinder binder, DynamicMetaObject[] indexes)
        {
            return binder.FallbackGetIndex(this, indexes, innerMetaObject.BindGetIndex(binder, indexes));
        }

        public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
        {
            return binder.FallbackGetMember(this, innerMetaObject.BindGetMember(binder));
        }

        public override DynamicMetaObject BindSetIndex(SetIndexBinder binder, DynamicMetaObject[] indexes, DynamicMetaObject value)
        {
            return binder.FallbackSetIndex(this, indexes, value, innerMetaObject.BindSetIndex(binder, indexes, value));
        }

        public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
        {
            return binder.FallbackSetMember(this, value, innerMetaObject.BindSetMember(binder, value));
        }
    }
}
