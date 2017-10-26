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

namespace VDS.RDF
{

    public class ComparisonHelperTests : BaseTest
    {
        private Graph _graph;

        public ComparisonHelperTests()
        {
            _graph = new Graph();
        }

        [Fact]
        public void ShouldSuccesfullyCompareDecimalNodeRegardlessOfCulture()
        {
            foreach (var ci in TestedCultureInfos)
            {
                TestTools.ExecuteWithChangedCulture(ci, () =>
                {
                    const decimal left = 1.4m;
                    const decimal right = 3.55m;
                    Assert.Equal(-1, ComparisonHelper.CompareLiterals(left.ToLiteral(_graph), right.ToLiteral(_graph)));
                });
            }
        }

        [Fact]
        public void ShouldSuccesfullyCompareFloatNodeRegardlessOfCulture()
        {
            foreach (var ci in TestedCultureInfos)
            {
                TestTools.ExecuteWithChangedCulture(ci, () =>
                {
                    const float left = 1.4f;
                    const float right = 3.55f;
                    Assert.Equal(-1, ComparisonHelper.CompareLiterals(left.ToLiteral(_graph), right.ToLiteral(_graph)));
                });
            }
        }

        [Fact]
        public void ShouldSuccesfullyCompareDoubleNodeRegardlessOfCulture()
        {
            foreach (var ci in TestedCultureInfos)
            {
                TestTools.ExecuteWithChangedCulture(ci, () =>
                {
                    const double left = 1.4d;
                    const double right = 3.55d;
                    Assert.Equal(-1, ComparisonHelper.CompareLiterals(left.ToLiteral(_graph), right.ToLiteral(_graph)));
                });
            }
        }
    }
}