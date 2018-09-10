namespace Dynamic
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    internal class MetaDynamic : DynamicMetaObject
    {
        internal MetaDynamic(Expression parameter, object value) : base(parameter, BindingRestrictions.Empty, value) { }

        public override DynamicMetaObject BindGetIndex(GetIndexBinder binder, DynamicMetaObject[] indexes)
        {
            return this.BindGetIndex(indexes, binder.ReturnType);
        }

        public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
        {
            return this.BindGetIndex(
                new[] {
                    new DynamicMetaObject(
                        Expression.Constant(
                            binder.Name,
                            typeof(string)),
                        BindingRestrictions.Empty,
                        binder.Name) },
                binder.ReturnType);
        }

        public override DynamicMetaObject BindSetIndex(SetIndexBinder binder, DynamicMetaObject[] indexes, DynamicMetaObject value)
        {
            return this.BindSetIndex(indexes, binder.ReturnType, value);
        }

        public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
        {
            return this.BindSetIndex(
                new[] {
                    new DynamicMetaObject(
                        Expression.Constant(
                            binder.Name,
                            typeof(string)),
                        BindingRestrictions.Empty,
                        binder.Name) },
                binder.ReturnType,
                value);
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            if (this.Value is IDictionary<string, object> dictionary)
            {
                return dictionary.Keys;
            }

            return Enumerable.Empty<string>();
        }

        private DynamicMetaObject BindGetIndex(DynamicMetaObject[] indexes, Type type)
        {
            return
               this.ValidateIndexing(indexes, type, out var indexer) ??
               this.CreateMetaObject(this.CreateIndexingExpression(indexes, indexer));
        }

        private DynamicMetaObject BindSetIndex(DynamicMetaObject[] indexes, Type type, DynamicMetaObject value)
        {
            return
                this.ValidateIndexing(indexes, type, out var indexer) ??
                this.CreateMetaObject(
                    // ((DynamicGraph)$$arg0).Item[(string)$$arg1] = $$arg2
                    Expression.Assign(
                        this.CreateIndexingExpression(indexes, indexer),
                        value.Expression));
        }

        private IndexExpression CreateIndexingExpression(DynamicMetaObject[] indexes, PropertyInfo indexer)
        {
            // ((DynamicGraph)$$arg0).Item[(string)$$arg1]
            return Expression.Property(
                Expression.Convert(
                    this.Expression,
                    this.RuntimeType),
                indexer,
                indexes.Zip(
                    indexer.GetIndexParameters(),
                    (index, pi) => Expression.Convert(
                        index.Expression,
                        pi.ParameterType)));
        }

        private DynamicMetaObject CreateMetaObject(Expression expression)
        {
            return new DynamicMetaObject(
                expression,
                BindingRestrictions.GetTypeRestriction(this.Expression, this.LimitType));
        }

        private DynamicMetaObject ValidateIndexing(DynamicMetaObject[] indexes, Type returnType, out PropertyInfo indexer)
        {
            indexer = null;

            if (indexes.Count() != 1)
            {
                // throw new ArgumentException("only one index");
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
                // throw new ArgumentNullException("index can't be null");
                return this.CreateMetaObject(
                    Expression.Throw(
                        Expression.New(
                            typeof(ArgumentNullException).GetConstructor(new[] { typeof(string) }),
                            Expression.Constant("index can't be null")),
                        returnType));
            }

            indexer = this.RuntimeType.GetProperty(
                "Item",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                typeof(object),
                indexes.Select(index => index.RuntimeType).ToArray(),
                null);

            if (indexer == null)
            {
                // throw new ArgumentException("unknown type");
                return this.CreateMetaObject(
                    Expression.Throw(
                        Expression.New(
                            typeof(ArgumentException).GetConstructor(new[] { typeof(string) }),
                            Expression.Constant("unknown type")),
                        returnType));
            }

            return null;
        }
    }
}
