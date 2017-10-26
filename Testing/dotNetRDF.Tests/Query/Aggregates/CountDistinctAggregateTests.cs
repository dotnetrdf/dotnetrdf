/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System.Linq;
using Xunit;
using VDS.RDF.Query.Aggregates.Sparql;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Aggregates
{

    public class CountDistinctAggregateTests
    {
        [Fact]
        public void SparqlCountDistinctWhenToStringCalled()
        {
            // given
            var aggregate = new CountDistinctAggregate(new VariableTerm("var"));

            // when
            string aggregateString = aggregate.ToString();

            // then
            Assert.Equal("COUNT(DISTINCT ?var)", aggregateString);
        }

        [Fact]
        public void SparqlCountDistinctHasDistinctModifier()
        {
            // given
            var term = new VariableTerm("var");
            var aggregate = new CountDistinctAggregate(term);

            // when
            var arguments = aggregate.Arguments.ToArray();

            // then
            Assert.Equal(2, arguments.Length);
            Assert.True(arguments[0] is DistinctModifier);
            Assert.Same(term, arguments[1]);
        }
    }
}