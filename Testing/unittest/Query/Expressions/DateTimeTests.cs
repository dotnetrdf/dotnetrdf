/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2013 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

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
using System.Linq;
using System.Text;
using NUnit.Framework;
using VDS.RDF.Parsing;

namespace VDS.RDF.Query.Expressions
{
    [TestFixture]
    public class DateTimeTests
    {
        private readonly SparqlQueryParser _parser = new SparqlQueryParser();
        private readonly SparqlXmlParser _resultsParser = new SparqlXmlParser();

        [TestCase("2013-07-05T12:00:00Z", "2013-07-05T04:00:00-08:00", true)]
        [TestCase("2013-07-05T12:00:00Z", "2013-07-05T12:00:00+14:00", false)]
        public void SparqlDateTimeEquality(String x, String y, bool equals)
        {
            IGraph g = new Graph();
            INode dateTime1 = g.CreateLiteralNode(x, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDateTime));
            INode dateTime2 = g.CreateLiteralNode(y, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDateTime));

            if (equals)
            {
                Assert.IsTrue(SparqlSpecsHelper.DateTimeEquality(dateTime1, dateTime2));
                Assert.IsTrue(SparqlSpecsHelper.DateTimeEquality(dateTime2, dateTime1));
                Assert.IsTrue(SparqlSpecsHelper.Equality(dateTime1, dateTime2));
                Assert.IsTrue(SparqlSpecsHelper.Equality(dateTime2, dateTime1));
                Assert.IsFalse(SparqlSpecsHelper.Inequality(dateTime1, dateTime2));
                Assert.IsFalse(SparqlSpecsHelper.Inequality(dateTime2, dateTime1));
            }
            else
            {
                Assert.IsFalse(SparqlSpecsHelper.DateTimeEquality(dateTime1, dateTime2));
                Assert.IsFalse(SparqlSpecsHelper.DateTimeEquality(dateTime2, dateTime1));
                Assert.IsFalse(SparqlSpecsHelper.Equality(dateTime1, dateTime2));
                Assert.IsFalse(SparqlSpecsHelper.Equality(dateTime2, dateTime1));
                Assert.IsTrue(SparqlSpecsHelper.Inequality(dateTime1, dateTime2));
                Assert.IsTrue(SparqlSpecsHelper.Inequality(dateTime2, dateTime1));
            }
        }

        [TestCase("2013-07-05", "2013-07-05", true)]
        [TestCase("2013-07-05-08:00", "2013-07-05Z", true)]
        [TestCase("2006-08-23", "2001-01-01", false)]
        [TestCase("2013-07-05", "2013-07-05Z", true)]
        [TestCase("2006-08-23", "2001-01-01Z", false)]
        public void SparqlDateEquality(String x, String y, bool equals)
        {
            IGraph g = new Graph();
            INode date1 = g.CreateLiteralNode(x, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDate));
            INode date2 = g.CreateLiteralNode(y, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDate));

            if (equals)
            {
                Assert.IsTrue(SparqlSpecsHelper.DateEquality(date1, date2));
                Assert.IsTrue(SparqlSpecsHelper.Equality(date1, date2));
                Assert.IsTrue(SparqlSpecsHelper.Equality(date2, date1));
                Assert.IsFalse(SparqlSpecsHelper.Inequality(date1, date2));
                Assert.IsFalse(SparqlSpecsHelper.Inequality(date2, date1));
            }
            else
            {
                Assert.IsFalse(SparqlSpecsHelper.DateEquality(date1, date2));
                Assert.IsFalse(SparqlSpecsHelper.Equality(date1, date2));
                Assert.IsFalse(SparqlSpecsHelper.Equality(date2, date1));
                Assert.IsTrue(SparqlSpecsHelper.Inequality(date1, date2));
                Assert.IsTrue(SparqlSpecsHelper.Inequality(date2, date1));
            }
        }

        [TestCase("2006-08-23T09:00:00+01:00", "2006-08-23Z", false)]
        public void SparqlDateTimeMixedEquality(String x, String y, bool equals)
        {
            IGraph g = new Graph();
            INode dateTime1 = g.CreateLiteralNode(x, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDateTime));
            INode date2 = g.CreateLiteralNode(y, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDateTime));

            if (equals)
            {
                Assert.IsTrue(SparqlSpecsHelper.DateTimeEquality(dateTime1, date2));
                Assert.IsTrue(SparqlSpecsHelper.Equality(dateTime1, date2));
                Assert.IsTrue(SparqlSpecsHelper.Equality(date2, dateTime1));
                Assert.IsFalse(SparqlSpecsHelper.Inequality(dateTime1, date2));
                Assert.IsFalse(SparqlSpecsHelper.Inequality(date2, dateTime1));
            }
            else
            {
                Assert.IsFalse(SparqlSpecsHelper.DateTimeEquality(dateTime1, date2));
                Assert.IsFalse(SparqlSpecsHelper.Equality(dateTime1, date2));
                Assert.IsFalse(SparqlSpecsHelper.Equality(date2, dateTime1));
                Assert.IsTrue(SparqlSpecsHelper.Inequality(dateTime1, date2));
                Assert.IsTrue(SparqlSpecsHelper.Inequality(date2, dateTime1));
            }
        }

        [TestCase("2013-07-05T12:00:00", "2013-07-05T12:00:00Z")]
        [ExpectedException(typeof(RdfQueryException))]
        public void SparqlDateTimeIncomparable(String x, String y)
        {
            IGraph g = new Graph();
            INode dateTime1 = g.CreateLiteralNode(x, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDateTime));
            INode dateTime2 = g.CreateLiteralNode(y, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDateTime));
            SparqlSpecsHelper.DateTimeEquality(dateTime1, dateTime2);
        }

        [ExpectedException(typeof(RdfQueryException))]
        public void SparqlDateIncomparable(String x, String y)
        {
            IGraph g = new Graph();
            INode dateTime1 = g.CreateLiteralNode(x, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDate));
            INode dateTime2 = g.CreateLiteralNode(y, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDate));
            SparqlSpecsHelper.DateEquality(dateTime1, dateTime2);
        }

        [TestCase("2006-08-23T09:00:00+01:00", "2006-08-23")]
        [ExpectedException(typeof(RdfQueryException))]
        public void SparqlDateTimeMixedIncomparable(String x, String y)
        {
            IGraph g = new Graph();
            INode dateTime1 = g.CreateLiteralNode(x, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDateTime));
            INode date2 = g.CreateLiteralNode(y, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDate));
            SparqlSpecsHelper.DateTimeEquality(dateTime1, date2);
        }

        [Test]
        public void SparqlDateTimeInequality()
        {
            IGraph g = new Graph();
            g.LoadFromFile(@"resources\sparql\data-3.ttl");
            Assert.IsFalse(g.IsEmpty);

            SparqlQuery q = this._parser.ParseFromFile(@"resources\sparql\date-2.rq");
            SparqlResultSet actual = g.ExecuteQuery(q) as SparqlResultSet;
            Assert.IsNotNull(actual);

            Console.WriteLine("Actual Results:");
            TestTools.ShowResults(actual);

            SparqlResultSet expected = new SparqlResultSet();
            this._resultsParser.Load(expected, @"resources\sparql\date-2-result.srx");

            Console.WriteLine("Expected Results:");
            TestTools.ShowResults(expected);

            Assert.IsTrue(expected.Equals(actual));
        }

        [Test]
        public void SparqlDateTimeGreaterThan()
        {
            IGraph g = new Graph();
            g.LoadFromFile(@"resources\sparql\data-3.ttl");
            Assert.IsFalse(g.IsEmpty);

            SparqlQuery q = this._parser.ParseFromFile(@"resources\sparql\date-3.rq");
            SparqlResultSet actual = g.ExecuteQuery(q) as SparqlResultSet;
            Assert.IsNotNull(actual);

            Console.WriteLine("Actual Results:");
            TestTools.ShowResults(actual);

            SparqlResultSet expected = new SparqlResultSet();
            this._resultsParser.Load(expected, @"resources\sparql\date-3-result.srx");

            Console.WriteLine("Expected Results:");
            TestTools.ShowResults(expected);

            Assert.IsTrue(expected.Equals(actual));
        }

        [TestCase("2013-01-01T03:00:00-08:00", "2013-01-01T12:00:00Z")]
        [TestCase("2013-01-01T12:00:00", "2013-01-01T12:00:00Z")]
        [TestCase("2013-01-01T12:00:00", "2013-01-01T12:00:00-08:00")]
        public void SparqlDateTimeCompare(String x, String y)
        {
            IGraph g = new Graph();
            INode dateTime1 = g.CreateLiteralNode(x, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDateTime));
            INode dateTime2 = g.CreateLiteralNode(y, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDateTime));

            SparqlOrderingComparer comparer = new SparqlOrderingComparer();

            // Expected result is that x < y so return should be -1
            Assert.AreEqual(-1, comparer.Compare(dateTime1, dateTime2));
            // Thus inverse should give 1
            Assert.AreEqual(1, comparer.Compare(dateTime2, dateTime1));

            // Also both should compare equal to self
            Assert.AreEqual(0, comparer.Compare(dateTime1, dateTime1));
            Assert.AreEqual(0, comparer.Compare(dateTime2, dateTime2));
        }

        [TestCase(new String[] { "2013-01-01T12:00:00Z", "2013-01-01T03:00:00-08:00" }, new String[] { "2013-01-01T03:00:00-08:00", "2013-01-01T12:00:00Z" })]
        [TestCase(new String[] { "2013-01-01T12:00:00Z", "2013-01-01T12:00:00-08:00" }, new String[] { "2013-01-01T12:00:00Z", "2013-01-01T12:00:00-08:00" })]
        public void SparqlDateTimeSorting(String[] input, String[] output)
        {
            IGraph g = new Graph();
            List<INode> orig =
                input.Select(x => g.CreateLiteralNode(x, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDateTime)))
                     .OfType<INode>().ToList();
            List<INode> sorted = new List<INode>(orig);
            sorted.Sort(new SparqlOrderingComparer());

            List<INode> expected =
                output.Select(x => g.CreateLiteralNode(x, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDateTime)))
                      .OfType<INode>()
                      .ToList();

            Assert.AreEqual(expected.Count, sorted.Count);

            for (int i = 0; i < expected.Count; i++)
            {
                Assert.AreEqual(expected[i], sorted[i], "Incorrect value at index " + i);
            }
        }
    }
}
