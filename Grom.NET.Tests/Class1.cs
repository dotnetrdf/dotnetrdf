using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Grom.NET.Tests
{
    [TestClass]
    public class Class1
    {
        [TestMethod]
        public void MyTestMethod()
        {
            // solves callsite caching bug in current implementation
            // enables using DynamicObject alongside WrapperGraph etc

            dynamic a1 = new DynamicNode("1");
            dynamic a2 = new DynamicNode("2");

            var r1 = a1.x;
            var r2 = a2.x;

            Console.WriteLine(r1);
            Console.WriteLine(r2);
        }
    }

    public class DynamicNode : IDynamicMetaObjectProvider, IDynamicMetaObjectProviderContainer
    {
        internal string value;

        public DynamicNode(string value)
        {
            this.value = value;
            this.InnerMetaObjectProvider = new DynamicNodeDispatcher(this);
        }

        public DynamicMetaObject GetMetaObject(Expression parameter) => new DelegatingMetaObject(parameter, this);

        public IDynamicMetaObjectProvider InnerMetaObjectProvider { get; private set; }
    }

    internal class DynamicNodeDispatcher : DynamicObject
    {
        private DynamicNode a;

        internal DynamicNodeDispatcher(DynamicNode a)
        {
            this.a = a;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            result = this.a.value;
            return true;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = (this.a as dynamic)[0];
            return true;
        }
    }

    internal class DelegatingMetaObject : DynamicMetaObject
    {
        private readonly DynamicMetaObject innerMetaObject;

        internal DelegatingMetaObject(Expression expression, IDynamicMetaObjectProviderContainer value) : base(expression, BindingRestrictions.Empty, value)
        {
            var parameter =
                Expression.Property(
                    Expression.Convert(
                        this.Expression, 
                        this.LimitType), 
                    nameof(IDynamicMetaObjectProviderContainer.InnerMetaObjectProvider));

            this.innerMetaObject = value.InnerMetaObjectProvider.GetMetaObject(parameter);
        }

        public override DynamicMetaObject BindGetIndex(GetIndexBinder binder, DynamicMetaObject[] indexes)
        {
            return binder.FallbackGetIndex(this, indexes, innerMetaObject.BindGetIndex(binder, indexes));
        }

        public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
        {
            return binder.FallbackGetMember(this, innerMetaObject.BindGetMember(binder));
        }

        public override DynamicMetaObject BindSetIndex(SetIndexBinder binder, DynamicMetaObject[] indexes, DynamicMetaObject value)
        {
            return binder.FallbackSetIndex(this, indexes, value, innerMetaObject.BindSetIndex(binder, indexes, value));
        }

        public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
        {
            return binder.FallbackSetMember(this, value, innerMetaObject.BindSetMember(binder, value));
        }
    }

    internal interface IDynamicMetaObjectProviderContainer
    {
        IDynamicMetaObjectProvider InnerMetaObjectProvider { get; }
    }
}
