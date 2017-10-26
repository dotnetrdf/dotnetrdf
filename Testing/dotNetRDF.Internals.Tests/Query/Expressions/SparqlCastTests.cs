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

using Xunit;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Query.Expressions.Functions.XPath.Cast;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Expressions
{
    public class SparqlCastTests : BaseTest
    {
        private INodeFactory _graph;

        public SparqlCastTests()
        {
            _graph = new Graph();
        }

        [Fact]
        public void ShouldSuccesfullyEvaluateDecimalCastRegardlessOfCulture()
        {
            foreach (var ci in TestedCultureInfos)
            {
                TestTools.ExecuteWithChangedCulture(ci, () =>
                {
                    // given
                    var cast = new DecimalCast(new ConstantTerm(3.4m.ToLiteral(_graph)));

                    // when
                    IValuedNode valuedNode = cast.Evaluate(new SparqlEvaluationContext(new SparqlQuery(), new InMemoryDataset()), 0);

                    // then
                    Assert.Equal(3.4m, valuedNode.AsDecimal());
                });
            }
        }

        [Fact]
        public void ShouldSuccesfullyEvaluateDoubleCastRegardlessOfCulture()
        {
            foreach (var ci in TestedCultureInfos)
            {
                TestTools.ExecuteWithChangedCulture(ci, () =>
                {
                    // given
                    var cast = new DoubleCast(new ConstantTerm(3.4d.ToLiteral(_graph)));

                    // when
                    IValuedNode valuedNode = cast.Evaluate(new SparqlEvaluationContext(new SparqlQuery(), new InMemoryDataset()), 0);

                    // then
                    Assert.Equal(3.4d, valuedNode.AsDouble());
                });
            }
        }

        [Fact]
        public void ShouldSuccesfullyEvaluateFloatCastRegardlessOfCulture()
        {
            foreach (var ci in TestedCultureInfos)
            {
                TestTools.ExecuteWithChangedCulture(ci, () =>
                {
                    // given
                    var cast = new FloatCast(new ConstantTerm(3.4f.ToLiteral(_graph)));

                    // when
                    IValuedNode valuedNode = cast.Evaluate(new SparqlEvaluationContext(new SparqlQuery(), new InMemoryDataset()), 0);

                    // then
                    Assert.Equal(3.4f, valuedNode.AsFloat());
                });
            }
        } 
    }
}