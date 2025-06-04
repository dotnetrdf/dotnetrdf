/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2025 dotNetRDF Project (http://dotnetrdf.org/)
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

using System;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;

namespace VDS.RDF.Dynamic;

internal class EnumerableMetaObject : DynamicMetaObject
{
    internal EnumerableMetaObject(Expression parameter, object value)
        : base(parameter, BindingRestrictions.Empty, value)
    {
    }

    public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args)
    {
        Expression expression = FindMethod(binder, args);
        var restrictions = BindingRestrictions.GetTypeRestriction(Expression, LimitType);
        var errorSuggestion = new DynamicMetaObject(expression, restrictions);

        return binder.FallbackInvokeMember(this, args, errorSuggestion);
    }

    private Expression FindMethod(InvokeMemberBinder binder, DynamicMetaObject[] args)
    {
        InvalidOperationException invalid = null;

        for (var i = 1; i < 4; i++)
        {
            try
            {
                Expression[] arguments = Expression.Convert(Expression, RuntimeType).AsEnumerable().Union(args.Select(arg => arg.Expression)).ToArray();
                Type[] typeArguments = Enumerable.Repeat(typeof(object), i).ToArray();
                MethodCallExpression expression = Expression.Call(typeof(Enumerable), binder.Name, typeArguments, arguments);

                return Expression.Convert(expression, binder.ReturnType);
            }
            catch (InvalidOperationException e)
            {
                invalid = e;
            }
        }

        return Expression.Throw(Expression.Constant(invalid), typeof(object));
    }
}
