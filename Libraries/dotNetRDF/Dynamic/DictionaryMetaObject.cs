namespace VDS.RDF.Dynamic
{
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq.Expressions;

    internal class DictionaryMetaObject : EnumerableMetaObject
    {
        internal DictionaryMetaObject(Expression parameter, object value) : base(parameter, value) { }

        public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
        {
            return binder.FallbackGetMember(
                this,
                CreateMetaObject(
                    this.CreateIndexExpression(
                        binder.Name)));
        }

        public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
        {
            return binder.FallbackSetMember(
                this,
                value,
                CreateMetaObject(
                    Expression.Assign(
                        this.CreateIndexExpression(binder.Name),
                        value.Expression)));
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return ((IDictionary<string, object>)Value).Keys;
        }

        private IndexExpression CreateIndexExpression(string name)
        {
            return Expression.Property(
                Expression.Convert(
                    this.Expression,
                    this.RuntimeType),
                "Item",
                new[] {
                    Expression.Constant(name) });
        }

        private DynamicMetaObject CreateMetaObject(Expression expression)
        {
            return new DynamicMetaObject(
                expression,
                BindingRestrictions.GetTypeRestriction(
                    this.Expression,
                    this.LimitType));
        }
    }
}
