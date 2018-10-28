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
using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Builder
{
    internal class BindingTupleBuilder : IBindingTupleBuilder
    {
        private readonly List<string> _variables;
        private readonly List<PatternItem> _patternItems = new List<PatternItem>();
        private readonly PatternItemFactory _patternItemFactory = new PatternItemFactory();

        public BindingTupleBuilder(List<string> variables)
        {
            _variables = variables;
        }

        public IBindingTupleBuilder Value(object literal)
        {
            var node = LiteralExpressionExtensions.ToLiteral(literal);
            _patternItems.Add(_patternItemFactory.CreateNodeMatchPattern(node));
            return this;
        }

        public IBindingTupleBuilder Value(string literal, Uri dataType)
        {
            _patternItems.Add(_patternItemFactory.CreateLiteralNodeMatchPattern(literal, dataType));
            return this;
        }

        public IBindingTupleBuilder Value(string literal, string languageTag)
        {
            _patternItems.Add(_patternItemFactory.CreateLiteralNodeMatchPattern(literal, languageTag));
            return this;
        }

        public IBindingTupleBuilder Value(Uri uri)
        {
            _patternItems.Add(_patternItemFactory.CreateNodeMatchPattern(uri));
            return this;
        }

        public IBindingTupleBuilder Undef()
        {
            _patternItems.Add(null);
            return this;
        }

        public BindingTuple GetTuple()
        {
            if (_variables.Count != _patternItems.Count)
            {
                throw new InvalidOperationException("The number of values does not match the number of variables");
            }

            return new BindingTuple(_variables, _patternItems);
        }
    }
}