namespace VDS.RDF.Dynamic
{
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Linq.Expressions;

    public class EnumerableMetaObject : DynamicMetaObject
    {
        private readonly Expression parameter;
        private readonly DynamicObjectCollection dynamicObjectCollection;

        public EnumerableMetaObject(Expression parameter, IEnumerable<object> value) : base(parameter, BindingRestrictions.Empty, value) { }

        public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args)
        {
            var enumerableType = typeof(Enumerable);
            var objectType = new[] { typeof(object) };
            var thisParam = Expression.Convert(this.Expression, this.RuntimeType).AsEnumerable();
            var argParams = args.Select(arg => arg.Expression);
            var parameters = thisParam.Union(argParams).ToArray();
            var expression = Expression.Call(enumerableType, binder.Name, objectType, parameters);
            var restrictions = BindingRestrictions.GetTypeRestriction(this.Expression, this.LimitType);
            return new DynamicMetaObject(expression, restrictions);
        }
    }
}
