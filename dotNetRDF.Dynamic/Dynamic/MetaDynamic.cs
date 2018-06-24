namespace Dynamic
{
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq.Expressions;

    internal class MetaDynamic : DynamicMetaObject
    {
        internal MetaDynamic(Expression parameter, IDynamicObject value) : base(parameter, BindingRestrictions.Empty, value) { }

        public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
        {
            // ((runtimeType)dyn).member -> dyn[(object)"member"]

            return new DynamicMetaObject(
                Expression.Property(
                    Expression.Convert(
                        this.Expression,
                        this.RuntimeType),
                    "Item",
                    Expression.Convert(
                        Expression.Constant(binder.Name),
                        typeof(object))),
                BindingRestrictions.GetTypeRestriction(this.Expression, this.LimitType));
        }

        public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
        {
            // ((runtimeType)dyn).member -> dyn[(object)"member"] = value

            return new DynamicMetaObject(
                Expression.Assign(
                    Expression.Property(
                        Expression.Convert(
                            this.Expression,
                            this.RuntimeType),
                        "Item",
                        Expression.Convert(
                            Expression.Constant(binder.Name),
                            typeof(object))),
                    value.Expression),
                BindingRestrictions.GetTypeRestriction(this.Expression, this.LimitType));
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return (this.Value as IDynamicObject).GetDynamicMemberNames();
        }
    }
}
