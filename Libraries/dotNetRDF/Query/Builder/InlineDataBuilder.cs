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

using System.Collections.Generic;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Builder
{
    internal class InlineDataBuilder : IInlineDataBuilder
    {
        private readonly PatternItemFactory _patternItemFactory = new PatternItemFactory();
        private readonly string _variable;
        private List<int> _values = new List<int>();

        public InlineDataBuilder(string variable)
        {
            _variable = variable;
        }

        public IInlineDataBuilder Values(int value)
        {
            _values.Add(value);
            return this;
        }

        public void AppendTo(GraphPattern graphPattern)
        {
            var bindingsPattern = new BindingsPattern(new[] { _variable });
            _values.ForEach(value => {
                var bindingTuple = new BindingTuple(new List<string>  { _variable }, new List<PatternItem>
                {
                    _patternItemFactory.CreateLiteralNodeMatchPattern(value),
                });
                bindingsPattern.AddTuple(bindingTuple);
            });

            graphPattern.AddInlineData(bindingsPattern);
        }
    }
}