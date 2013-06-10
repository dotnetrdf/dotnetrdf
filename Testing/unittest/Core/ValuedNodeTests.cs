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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using VDS.RDF.Nodes;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF
{
    [TestFixture]
    public class ValuedNodeTests : BaseTest
    {
        private Graph _graph;

        [SetUp]
        public void Setup()
        {
            _graph = new Graph();
        }

        [Test]
        public void NodeAsValuedTimeSpan()
        {
            INode orig = new TimeSpan(1, 0, 0).ToLiteral(_graph);
            IValuedNode valued = orig.AsValuedNode();

            Assert.AreEqual(((ILiteralNode)orig).Value, ((ILiteralNode)valued).Value);
            Assert.IsTrue(EqualityHelper.AreUrisEqual(((ILiteralNode)orig).DataType, ((ILiteralNode)valued).DataType));
            Assert.AreEqual(typeof(TimeSpanNode), valued.GetType());
        }

        [Test]
        public void ShouldCorrectlyPerformRoundtripConversionOfDecimalValuedNodesRegardlessOfCulture()
        {
            foreach (var ci in TestedCultureInfos)
            {
                TestTools.ExecuteWithChangedCulture(ci, () => RoundTripValuedNodeConversion(3.4m, n => n.AsDecimal()));
            }
        }

        [Test]
        public void ShouldCorrectlyPerformRoundtripConversionOfDoubleValuedNodesRegardlessOfCulture()
        {
            foreach (var ci in TestedCultureInfos)
            {
                TestTools.ExecuteWithChangedCulture(ci, () => RoundTripValuedNodeConversion(3.4d, n => n.AsDouble()));
            }
        }

        [Test]
        public void ShouldCorrectlyPerformRoundtripConversionOfSingleValuedNodesRegardlessOfCulture()
        {
            foreach (var ci in TestedCultureInfos)
            {
                TestTools.ExecuteWithChangedCulture(ci, () => RoundTripValuedNodeConversion(3.4f, n => n.AsFloat()));
            }
        }

        private void RoundTripValuedNodeConversion<T>(dynamic value, Func<IValuedNode, T> convertBack)
        {
            // given
            ILiteralNode literalNode = LiteralExtensions.ToLiteral(value, _graph);

            // when
            IValuedNode valuedNode = literalNode.AsValuedNode();
            T convertedBack = convertBack(valuedNode);

            // then
            Assert.AreEqual(value, convertedBack);
        }
    }
}
