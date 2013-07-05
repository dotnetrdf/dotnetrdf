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
        private SparqlQueryParser _parser = new SparqlQueryParser();
        private SparqlXmlParser _resultsParser = new SparqlXmlParser();

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
    }
}
