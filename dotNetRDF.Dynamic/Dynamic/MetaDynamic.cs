namespace Dynamic
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Linq.Expressions;
    using VDS.RDF;

    internal class MetaDynamic : DynamicMetaObject
    {
        internal MetaDynamic(Expression parameter, IDynamicObject value) : base(parameter, BindingRestrictions.Empty, value) { }

        public override DynamicMetaObject BindGetIndex(GetIndexBinder binder, DynamicMetaObject[] indexes)
        {
            return this.ValidateIndexing(indexes, binder.ReturnType) ?? base.BindGetIndex(binder, indexes);
        }

        public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
        {
            return this.CreateMetaObject(
                Expression.Property(
                    Expression.Convert(
                        this.Expression,
                        this.RuntimeType),
                    "Item",
                    Expression.Constant(binder.Name, typeof(string))));
        }

        public override DynamicMetaObject BindSetIndex(SetIndexBinder binder, DynamicMetaObject[] indexes, DynamicMetaObject value)
        {
            return this.ValidateIndexing(indexes, binder.ReturnType) ?? base.BindSetIndex(binder, indexes, value);
        }

        public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
        {
            return this.CreateMetaObject(
                Expression.Assign(
                    Expression.Property(
                        Expression.Convert(
                            this.Expression,
                            this.RuntimeType),
                        "Item",
                        Expression.Constant(binder.Name, typeof(string))),
                        value.Expression));
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return (this.Value as IDynamicObject).GetDynamicMemberNames();
        }

        private DynamicMetaObject CreateMetaObject(Expression expression)
        {
            return new DynamicMetaObject(
                expression,
                BindingRestrictions.GetTypeRestriction(this.Expression, this.LimitType));
        }

        private DynamicMetaObject ValidateIndexing(DynamicMetaObject[] indexes, Type returnType)
        {
            if (indexes.Count() != 1)
            {
                return this.CreateMetaObject(
                    Expression.Throw(
                        Expression.New(
                            typeof(ArgumentException).GetConstructor(new[] { typeof(string) }),
                            Expression.Constant("only one index")),
                        returnType));
            }

            var value = indexes.Single().Value;

            if (value == null)
            {
                return this.CreateMetaObject(
                    Expression.Throw(
                        Expression.New(
                            typeof(ArgumentNullException).GetConstructor(new[] { typeof(string) }),
                            Expression.Constant("index can't be null")),
                        returnType));
            }

            if (!(value is INode || value is Uri || value is string))
            {
                return this.CreateMetaObject(
                    Expression.Throw(
                        Expression.New(
                            typeof(ArgumentException).GetConstructor(new[] { typeof(string) }),
                            Expression.Constant("only nodes, uris and string")),
                        returnType));
            }

            return null;
        }
    }
}
