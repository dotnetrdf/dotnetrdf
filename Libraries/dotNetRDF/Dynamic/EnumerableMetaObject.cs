namespace VDS.RDF.Dynamic
{
    using System;
    using System.Dynamic;
    using System.Linq;
    using System.Linq.Expressions;

    internal class EnumerableMetaObject : DynamicMetaObject
    {
        internal EnumerableMetaObject(Expression parameter, object value) : base(parameter, BindingRestrictions.Empty, value) { }

        public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args)
        {
            return binder.FallbackInvokeMember(
                this,
                args,
                new DynamicMetaObject(
                    this.FindMethod(
                        binder,
                        args),
                    BindingRestrictions.GetTypeRestriction(
                        this.Expression,
                        this.LimitType
                    )
                )
            );
        }

        private Expression FindMethod(InvokeMemberBinder binder, DynamicMetaObject[] args)
        {
            InvalidOperationException invalid = null;

            for (var i = 1; i < 4; i++)
            {
                try
                {
                    return
                        Expression.Convert(
                            Expression.Call(
                                typeof(Enumerable),
                                binder.Name,
                                Enumerable.Repeat(typeof(object), i).ToArray(),
                                Expression.Convert(
                                    this.Expression,
                                    this.RuntimeType).AsEnumerable().Union(args.Select(arg => arg.Expression)).ToArray()),
                            binder.ReturnType
                        );
                }
                catch (InvalidOperationException e)
                {
                    invalid = e;
                }
            }

            return Expression.Throw(Expression.Constant(invalid), typeof(object));
        }
    }
}
