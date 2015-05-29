/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-develop@lists.sf.net)

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
using System.Globalization;
using NUnit.Framework;
using VDS.RDF.Graphs;
using VDS.RDF.Specifications;

namespace VDS.RDF.Nodes
{
    [TestFixture]
    public class ValuedNodeTests
        : BaseTest
    {
        private IGraph _graph;

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

            Assert.AreEqual(orig.Value, valued.Value);
            Assert.IsTrue(EqualityHelper.AreUrisEqual(orig.DataType, valued.DataType));
            Assert.AreEqual(typeof(TimeSpanNode), valued.GetType());
        }

        [Test]
        public void NodeAsValuedDateTime1()
        {
            INode orig = DateTime.Now.ToLiteral(_graph);
            IValuedNode valued = orig.AsValuedNode();

            Assert.AreEqual(orig.Value, valued.Value);
            Assert.AreEqual(typeof(DateTimeNode), valued.GetType());
        }

        [Test]
        public void NodeAsValuedDateTime2()
        {
            INode orig = _graph.CreateLiteralNode("2013-06-19T09:58:00", UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDateTime));
            IValuedNode valued = orig.AsValuedNode();
            Assert.AreEqual(DateTimeKind.Unspecified, valued.AsDateTime().Kind);
        }

        [Test]
        public void NodeAsValuedDateTime3()
        {
            INode orig = _graph.CreateLiteralNode("2013-06-19T09:58:00-07:00", UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDateTime));
            IValuedNode valued = orig.AsValuedNode();
            Assert.AreEqual(16, valued.AsDateTime().Hour);
            Assert.AreEqual(DateTimeKind.Utc, valued.AsDateTime().Kind);
        }

        [Test]
        public void NodeDateTimeParsing()
        {
            String input = "2013-06-19T09:58:00";
            DateTime dt = DateTime.Parse(input, null, DateTimeStyles.AdjustToUniversal);
            Assert.AreEqual(DateTimeKind.Unspecified, dt.Kind);

            input = "2013-06-19T09:58:00-07:00";
            dt = DateTime.Parse(input, null, DateTimeStyles.AdjustToUniversal);
            Assert.AreEqual(DateTimeKind.Utc, dt.Kind);
            Assert.AreEqual(16, dt.Hour);

            input = "2013-06-19T09:58:00Z";
            dt = DateTime.Parse(input, null, DateTimeStyles.AdjustToUniversal);
            Assert.AreEqual(DateTimeKind.Utc, dt.Kind);
        }

        [Test]
        public void NodeDecimalValueRoundTripRegardlessOfCulture()
        {
            foreach (var ci in TestedCultureInfos)
            {
                TestTools.ExecuteWithChangedCulture(ci, () => RoundTripValuedNodeConversion(3.4m, n => n.AsDecimal()));
            }
        }

        [Test]
        public void NodeDoubleValueRoundTripRegardlessOfCulture()
        {
            foreach (var ci in TestedCultureInfos)
            {
                TestTools.ExecuteWithChangedCulture(ci, () => RoundTripValuedNodeConversion(3.4d, n => n.AsDouble()));
            }
        }

        [Test]
        public void NodeSingleValueRoundTripRegardlessOfCulture()
        {
            foreach (var ci in TestedCultureInfos)
            {
                TestTools.ExecuteWithChangedCulture(ci, () => RoundTripValuedNodeConversion(3.4f, n => n.AsFloat()));
            }
        }

        private void RoundTripValuedNodeConversion<T>(dynamic value, Func<IValuedNode, T> convertBack)
        {
            // given
            INode literalNode = LiteralExtensions.ToLiteral(value, _graph);

            // when
            IValuedNode valuedNode = literalNode.AsValuedNode();
            T convertedBack = convertBack(valuedNode);

            // then
            Assert.AreEqual(value, convertedBack);
        }
    }
}
