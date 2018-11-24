namespace VDS.RDF.Dynamic
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Linq.Expressions;

    internal class EnumerableMetaObject : DynamicMetaObject
    {
        internal EnumerableMetaObject(Expression parameter, IEnumerable<object> value) : base(parameter, BindingRestrictions.Empty, value) { }

        public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args)
        {
            var enumerable = typeof(Enumerable);
            var @this = Expression.Convert(this.Expression, this.RuntimeType).AsEnumerable();
            var argParams = args.Select(arg => arg.Expression);
            var allParams = @this.Union(argParams).ToArray();
            var method = FindMethod(enumerable, binder.Name, allParams);
            var restrictions = BindingRestrictions.GetTypeRestriction(this.Expression, this.LimitType);
            return new DynamicMetaObject(method, restrictions);
        }

        private static MethodCallExpression FindMethod(Type type, string methodName, Expression[] arguments)
        {
            InvalidOperationException invalid = null;

            for (var i = 1; i < 4; i++)
            {
                var typeArguments = Enumerable.Repeat(typeof(object), i).ToArray();

                try
                {
                    return Expression.Call(type, methodName, typeArguments, arguments);
                }
                catch (InvalidOperationException e)
                {
                    invalid = e;
                }
            }

            throw invalid;
        }
    }
}
