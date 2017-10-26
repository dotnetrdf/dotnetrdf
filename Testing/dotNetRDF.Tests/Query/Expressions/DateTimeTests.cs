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
using Xunit;
using VDS.RDF.Parsing;

namespace VDS.RDF.Query.Expressions
{

    public class DateTimeTests
    {
        private readonly SparqlQueryParser _parser = new SparqlQueryParser();
        private readonly SparqlXmlParser _resultsParser = new SparqlXmlParser();

        [Theory]
        [InlineData("2013-07-05T12:00:00Z", "2013-07-05T04:00:00-08:00", true)]
        [InlineData("2013-07-05T12:00:00Z", "2013-07-05T12:00:00+14:00", false)]
        public void SparqlDateTimeEquality(String x, String y, bool equals)
        {
            IGraph g = new Graph();
            INode dateTime1 = g.CreateLiteralNode(x, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDateTime));
            INode dateTime2 = g.CreateLiteralNode(y, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDateTime));

            if (equals)
            {
                Assert.True(SparqlSpecsHelper.DateTimeEquality(dateTime1, dateTime2));
                Assert.True(SparqlSpecsHelper.DateTimeEquality(dateTime2, dateTime1));
                Assert.True(SparqlSpecsHelper.Equality(dateTime1, dateTime2));
                Assert.True(SparqlSpecsHelper.Equality(dateTime2, dateTime1));
                Assert.False(SparqlSpecsHelper.Inequality(dateTime1, dateTime2));
                Assert.False(SparqlSpecsHelper.Inequality(dateTime2, dateTime1));
            }
            else
            {
                Assert.False(SparqlSpecsHelper.DateTimeEquality(dateTime1, dateTime2));
                Assert.False(SparqlSpecsHelper.DateTimeEquality(dateTime2, dateTime1));
                Assert.False(SparqlSpecsHelper.Equality(dateTime1, dateTime2));
                Assert.False(SparqlSpecsHelper.Equality(dateTime2, dateTime1));
                Assert.True(SparqlSpecsHelper.Inequality(dateTime1, dateTime2));
                Assert.True(SparqlSpecsHelper.Inequality(dateTime2, dateTime1));
            }
        }

        [Theory]
        [InlineData("2013-07-05", "2013-07-05", true)]
        [InlineData("2013-07-05-08:00", "2013-07-05Z", true)]
        [InlineData("2006-08-23", "2001-01-01", false)]
        [InlineData("2013-07-05", "2013-07-05Z", true)]
        [InlineData("2006-08-23", "2001-01-01Z", false)]
        public void SparqlDateEquality(String x, String y, bool equals)
        {
            IGraph g = new Graph();
            INode date1 = g.CreateLiteralNode(x, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDate));
            INode date2 = g.CreateLiteralNode(y, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDate));

            if (equals)
            {
                Assert.True(SparqlSpecsHelper.DateEquality(date1, date2));
                Assert.True(SparqlSpecsHelper.Equality(date1, date2));
                Assert.True(SparqlSpecsHelper.Equality(date2, date1));
                Assert.False(SparqlSpecsHelper.Inequality(date1, date2));
                Assert.False(SparqlSpecsHelper.Inequality(date2, date1));
            }
            else
            {
                Assert.False(SparqlSpecsHelper.DateEquality(date1, date2));
                Assert.False(SparqlSpecsHelper.Equality(date1, date2));
                Assert.False(SparqlSpecsHelper.Equality(date2, date1));
                Assert.True(SparqlSpecsHelper.Inequality(date1, date2));
                Assert.True(SparqlSpecsHelper.Inequality(date2, date1));
            }
        }

        [Theory]
        [InlineData("2006-08-23T09:00:00+01:00", "2006-08-23Z", false)]
        public void SparqlDateTimeMixedEquality(String x, String y, bool equals)
        {
            IGraph g = new Graph();
            INode dateTime1 = g.CreateLiteralNode(x, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDateTime));
            INode date2 = g.CreateLiteralNode(y, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDateTime));

            if (equals)
            {
                Assert.True(SparqlSpecsHelper.DateTimeEquality(dateTime1, date2));
                Assert.True(SparqlSpecsHelper.Equality(dateTime1, date2));
                Assert.True(SparqlSpecsHelper.Equality(date2, dateTime1));
                Assert.False(SparqlSpecsHelper.Inequality(dateTime1, date2));
                Assert.False(SparqlSpecsHelper.Inequality(date2, dateTime1));
            }
            else
            {
                Assert.False(SparqlSpecsHelper.DateTimeEquality(dateTime1, date2));
                Assert.False(SparqlSpecsHelper.Equality(dateTime1, date2));
                Assert.False(SparqlSpecsHelper.Equality(date2, dateTime1));
                Assert.True(SparqlSpecsHelper.Inequality(dateTime1, date2));
                Assert.True(SparqlSpecsHelper.Inequality(date2, dateTime1));
            }
        }

        [Theory]
        [InlineData("2013-07-05T12:00:00", "2013-07-05T12:00:00Z")]
        public void SparqlDateTimeIncomparable(String x, String y)
        {
            IGraph g = new Graph();
            INode dateTime1 = g.CreateLiteralNode(x, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDateTime));
            INode dateTime2 = g.CreateLiteralNode(y, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDateTime));

            Assert.Throws<RdfQueryException>(() => SparqlSpecsHelper.DateTimeEquality(dateTime1, dateTime2));
        }

        [Theory]
        [InlineData(null, null)]
        public void SparqlDateIncomparable(String x, String y)
        {
            IGraph g = new Graph();
            INode dateTime1 = g.CreateLiteralNode(x, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDate));
            INode dateTime2 = g.CreateLiteralNode(y, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDate));

            Assert.Throws<RdfQueryException>(() => SparqlSpecsHelper.DateEquality(dateTime1, dateTime2));
        }

        [Theory]
        [InlineData("2006-08-23T09:00:00+01:00", "2006-08-23")]
        public void SparqlDateTimeMixedIncomparable(String x, String y)
        {
            IGraph g = new Graph();
            INode dateTime1 = g.CreateLiteralNode(x, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDateTime));
            INode date2 = g.CreateLiteralNode(y, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDate));

            Assert.Throws<RdfQueryException>(() => SparqlSpecsHelper.DateTimeEquality(dateTime1, date2));
        }

        [Fact]
        public void SparqlDateTimeInequality()
        {
            IGraph g = new Graph();
            g.LoadFromFile(@"resources\sparql\data-3.ttl");
            Assert.False(g.IsEmpty);

            SparqlQuery q = this._parser.ParseFromFile(@"resources\sparql\date-2.rq");
            SparqlResultSet actual = g.ExecuteQuery(q) as SparqlResultSet;
            Assert.NotNull(actual);

            Console.WriteLine("Actual Results:");
            TestTools.ShowResults(actual);

            SparqlResultSet expected = new SparqlResultSet();
            this._resultsParser.Load(expected, @"resources\sparql\date-2-result.srx");

            Console.WriteLine("Expected Results:");
            TestTools.ShowResults(expected);

            Assert.True(expected.Equals(actual));
        }

        [Fact]
        public void SparqlDateTimeGreaterThan()
        {
            IGraph g = new Graph();
            g.LoadFromFile(@"resources\sparql\data-3.ttl");
            Assert.False(g.IsEmpty);

            SparqlQuery q = this._parser.ParseFromFile(@"resources\sparql\date-3.rq");
            SparqlResultSet actual = g.ExecuteQuery(q) as SparqlResultSet;
            Assert.NotNull(actual);

            Console.WriteLine("Actual Results:");
            TestTools.ShowResults(actual);

            SparqlResultSet expected = new SparqlResultSet();
            this._resultsParser.Load(expected, @"resources\sparql\date-3-result.srx");

            Console.WriteLine("Expected Results:");
            TestTools.ShowResults(expected);

            Assert.True(expected.Equals(actual));
        }

        [Theory]
        [InlineData("2013-01-01T03:00:00-08:00", "2013-01-01T12:00:00Z")]
        [InlineData("2013-01-01T12:00:00", "2013-01-01T12:00:00Z")]
        [InlineData("2013-01-01T12:00:00", "2013-01-01T12:00:00-08:00")]
        public void SparqlDateTimeCompare(String x, String y)
        {
            IGraph g = new Graph();
            INode dateTime1 = g.CreateLiteralNode(x, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDateTime));
            INode dateTime2 = g.CreateLiteralNode(y, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDateTime));

            SparqlOrderingComparer comparer = new SparqlOrderingComparer();

            // Expected result is that x < y so return should be -1
            Assert.Equal(-1, comparer.Compare(dateTime1, dateTime2));
            // Thus inverse should give 1
            Assert.Equal(1, comparer.Compare(dateTime2, dateTime1));

            // Also both should compare equal to self
            Assert.Equal(0, comparer.Compare(dateTime1, dateTime1));
            Assert.Equal(0, comparer.Compare(dateTime2, dateTime2));
        }

        [Theory]
        [InlineData(new String[] { "2013-01-01T12:00:00Z", "2013-01-01T03:00:00-08:00" }, new String[] { "2013-01-01T03:00:00-08:00", "2013-01-01T12:00:00Z" })]
        [InlineData(new String[] { "2013-01-01T12:00:00Z", "2013-01-01T12:00:00-08:00" }, new String[] { "2013-01-01T12:00:00Z", "2013-01-01T12:00:00-08:00" })]
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

            Assert.Equal(expected.Count, sorted.Count);

            for (int i = 0; i < expected.Count; i++)
            {
                Assert.Equal(expected[i], sorted[i]);
            }
        }
    }
}
