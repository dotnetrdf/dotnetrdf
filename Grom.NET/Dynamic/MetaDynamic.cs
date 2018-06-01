namespace Dynamic
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Linq.Expressions;

    // TODO: Remove fallback handling?
    // TODO: 1. Fallback isn't "simple".
    // TODO: 2. No point in our cases (node & graph)
    internal class MetaDynamic : DynamicMetaObject
    {
        internal MetaDynamic(Expression parameter, ISimpleDynamicObject value) : base(parameter, BindingRestrictions.Empty, value) { }

        public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
        {
            var expression =
                Expression.Call(
                    Expression.Convert(
                        this.Expression,
                        typeof(ISimpleDynamicObject)),
                    "GetMember",
                    new Type[0],
                    Expression.Constant(binder.Name)
                );

            var suggestion = new DynamicMetaObject(expression, BindingRestrictions.GetTypeRestriction(this.Expression, this.LimitType));

            return binder.FallbackGetMember(this, suggestion);
        }

        public override DynamicMetaObject BindGetIndex(GetIndexBinder binder, DynamicMetaObject[] indexes)
        {
            var expression =
                Expression.Call(
                    Expression.Convert(
                        this.Expression,
                        typeof(ISimpleDynamicObject)),
                    "GetIndex",
                    new Type[0],
                    Expression.NewArrayInit(
                        typeof(object),
                        indexes.Select(i =>
                            Expression.Convert(
                                i.Expression,
                                typeof(object))))
                );

            var suggestion = new DynamicMetaObject(expression, BindingRestrictions.GetTypeRestriction(this.Expression, this.LimitType));

            return binder.FallbackGetIndex(this, indexes, suggestion);
        }

        public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
        {
            var expression =
                Expression.Call(
                    Expression.Convert(
                        this.Expression,
                        typeof(ISimpleDynamicObject)),
                    "SetMember",
                    new Type[0],
                    Expression.Constant(binder.Name),
                    value.Expression
                );

            var suggestion = new DynamicMetaObject(expression, BindingRestrictions.GetTypeRestriction(this.Expression, this.LimitType));

            return binder.FallbackSetMember(this, value, suggestion);
        }

        public override DynamicMetaObject BindSetIndex(SetIndexBinder binder, DynamicMetaObject[] indexes, DynamicMetaObject value)
        {
            var expression =
                Expression.Call(
                    Expression.Convert(
                        this.Expression,
                        typeof(ISimpleDynamicObject)),
                    "SetIndex",
                    new Type[0],
                    Expression.NewArrayInit(
                        typeof(object),
                        indexes.Select(i =>
                            Expression.Convert(
                                i.Expression,
                                typeof(object)))),
                    value.Expression
                );

            var suggestion = new DynamicMetaObject(expression, BindingRestrictions.GetTypeRestriction(this.Expression, this.LimitType));

            return binder.FallbackSetIndex(this, indexes, value, suggestion);
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return (this.Value as ISimpleDynamicObject).GetDynamicMemberNames();
        }
    }
}
