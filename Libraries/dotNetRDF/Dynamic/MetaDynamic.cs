namespace VDS.RDF.Dynamic
{
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Linq.Expressions;

    internal class MetaDynamic : DynamicMetaObject
    {
        internal MetaDynamic(Expression parameter, object value) : base(parameter, BindingRestrictions.Empty, value) { }

        public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
        {
            return this.CreateMetaObject(this.CreateIndexExpression(binder.Name));
        }

        public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
        {
            return this.CreateMetaObject(
                Expression.Assign(
                    this.CreateIndexExpression(binder.Name),
                    value.Expression));
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return (this.Value as IDictionary<string, object>)?.Keys ?? Enumerable.Empty<string>();
        }

        private IndexExpression CreateIndexExpression(string name)
        {
            return
                Expression.Property(
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
