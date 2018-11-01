/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2018 dotNetRDF Project (http://dotnetrdf.org/)
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
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Builder
{
    internal class InlineDataBuilder : IInlineDataBuilder
    {
        private readonly BindingsPattern _bindingsPattern;
        private readonly List<string> _variables;

        public InlineDataBuilder(IEnumerable<string> variables)
        {
            _variables = variables.ToList();
            _bindingsPattern = new BindingsPattern(_variables);
        }

        public IInlineDataBuilder Values(params object[] values)
        { 
            return Values(builder => values.Aggregate(builder, CreateValue));
        }

        public IInlineDataBuilder Values(Action<IBindingTupleBuilder> buildWith)
        {
            var builder = new BindingTupleBuilder(_variables);
            buildWith(builder);
            _bindingsPattern.AddTuple(builder.GetTuple());
            return this;
        }

        public void AppendTo(GraphPattern graphPattern)
        {
            graphPattern.AddInlineData(_bindingsPattern);
        }

        public void AppendTo(SparqlQuery query)
        {
            query.Bindings = _bindingsPattern;
        }

        private static IBindingTupleBuilder CreateValue(IBindingTupleBuilder builder, object value)
        {
            if (value == null)
            {
                return builder.Undef();
            }

            if (value is Uri uri)
            {
                return builder.Value(uri);
            }

            return builder.Value(value);
        }
    }
}