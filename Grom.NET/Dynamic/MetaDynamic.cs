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
        internal MetaDynamic(Expression parameter, IDynamicObject value) : base(parameter, BindingRestrictions.Empty, value) { }

        public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
        {
            // Original:
            //
            // dyn.member;
            //
            //
            // Result:
            //
            // ((IDynamicObject)dyn).GetMember("member");

            var expression =
                Expression.Call(
                    Expression.Convert(
                        this.Expression,
                        typeof(IDynamicObject)),
                    "GetMember",
                    new Type[0],
                    Expression.Constant(binder.Name)
                );

            var suggestion = new DynamicMetaObject(expression, BindingRestrictions.GetTypeRestriction(this.Expression, this.LimitType));

            return binder.FallbackGetMember(this, suggestion);
        }

        public override DynamicMetaObject BindGetIndex(GetIndexBinder binder, DynamicMetaObject[] indexes)
        {
            // Original:
            //
            // dyn[index1, index2];
            //
            //
            // Result:
            //
            // ((IDynamicObject)dyn).GetIndex(new object[] { (object)index1, (object)index2 });

            var expression =
                Expression.Call(
                    Expression.Convert(
                        this.Expression,
                        typeof(IDynamicObject)),
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
            // Original:
            //
            // dyn.member = value;
            //
            //
            // Result:
            //
            // {
            //      ((IDynamicObject)dyn).SetMember("member", new [] { value });
            //      return value;
            // }

            var expression =
                Expression.Block(
                    Expression.Call(
                        Expression.Convert(
                            this.Expression,
                            typeof(IDynamicObject)),
                        "SetMember",
                        new Type[0],
                        Expression.Constant(binder.Name),
                        value.Expression
                    ),
                    value.Expression);

            var suggestion = new DynamicMetaObject(expression, BindingRestrictions.GetTypeRestriction(this.Expression, this.LimitType));

            return binder.FallbackSetMember(this, value, suggestion);
        }

        public override DynamicMetaObject BindSetIndex(SetIndexBinder binder, DynamicMetaObject[] indexes, DynamicMetaObject value)
        {
            // Original:
            //
            // dyn[index1, index2] = value
            //
            //
            // Result:
            //
            // {
            //     ((IDynamicObject)dyn).SetIndex(new object[] { (object)index1, (object)index2 }, value);
            //     return value;
            // }

            var expression =
                Expression.Block(
                    Expression.Call(
                        Expression.Convert(
                            this.Expression,
                            typeof(IDynamicObject)),
                        "SetIndex",
                        new Type[0],
                        Expression.NewArrayInit(
                            typeof(object),
                            indexes.Select(i =>
                                Expression.Convert(
                                    i.Expression,
                                    typeof(object)))),
                        value.Expression
                    ),
                    value.Expression);


            var suggestion = new DynamicMetaObject(expression, BindingRestrictions.GetTypeRestriction(this.Expression, this.LimitType));

            return binder.FallbackSetIndex(this, indexes, value, suggestion);
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return (this.Value as IDynamicObject).GetDynamicMemberNames();
        }
    }
}
