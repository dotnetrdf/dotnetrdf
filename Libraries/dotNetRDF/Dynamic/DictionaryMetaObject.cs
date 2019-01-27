/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

namespace VDS.RDF.Dynamic
{
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq.Expressions;

    internal class DictionaryMetaObject : EnumerableMetaObject
    {
        internal DictionaryMetaObject(Expression parameter, object value)
            : base(parameter, value)
        {
        }

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
                new[] { Expression.Constant(name) });
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
