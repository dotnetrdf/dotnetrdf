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
                        binder.Name,
                        args),
                    BindingRestrictions.GetTypeRestriction(
                        this.Expression, 
                        this.LimitType
                    )
                )
            );
        }

        private Expression FindMethod(string methodName, DynamicMetaObject[] args)
        {
            InvalidOperationException invalid = null;

            for (var i = 1; i < 4; i++)
            {
                try
                {
                    return Expression.Call(typeof(Enumerable), methodName, Enumerable.Repeat(typeof(object), i).ToArray(), Expression.Convert(this.Expression, this.RuntimeType).AsEnumerable().Union(args.Select(arg => arg.Expression)).ToArray());
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
