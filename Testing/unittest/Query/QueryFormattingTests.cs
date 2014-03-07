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
using System.Linq;
using NUnit.Framework;
using VDS.RDF.Parsing;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query
{
    [TestFixture]
    public class QueryFormattingTests
    {
        private readonly SparqlFormatter _formatter = new SparqlFormatter();
        private readonly SparqlQueryParser _parser = new SparqlQueryParser();

        [Test]
        public void SparqlFormattingFilter1()
        {
            const string query = "SELECT * WHERE { { ?s ?p ?o } FILTER(ISURI(?o)) }";
            SparqlQuery q = this._parser.ParseFromString(query);

            Console.WriteLine("ToString() form:");
            String toString = q.ToString();
            Console.WriteLine(toString);
            Console.WriteLine();
            Console.WriteLine("Format() form:");
            String formatted = this._formatter.Format(q);
            Console.WriteLine(formatted);

            Assert.IsTrue(toString.Contains("FILTER"), "ToString() form should contain FILTER");
            Assert.IsTrue(formatted.Contains("FILTER"), "Format() form should contain FILTER");
        }

        [Test]
        public void SparqlFormattingFilter2()
        {
            const string query = "SELECT * WHERE { { ?s ?p ?o } FILTER(REGEX(?o, 'search', 'i')) }";
            SparqlQuery q = this._parser.ParseFromString(query);

            Console.WriteLine("ToString() form:");
            String toString = q.ToString();
            Console.WriteLine(toString);
            Console.WriteLine();
            Console.WriteLine("Format() form:");
            String formatted = this._formatter.Format(q);
            Console.WriteLine(formatted);

            Assert.IsTrue(toString.Contains("FILTER"), "ToString() form should contain FILTER");
            Assert.IsTrue(toString.Contains("i"), "ToString() form should contain i option");
            Assert.IsTrue(formatted.Contains("FILTER"), "Format() form should contain FILTER");
            Assert.IsTrue(toString.Contains("i"), "Format() form should contain i option");
        }

        private int CountOccurrences(String value, char c)
        {
            char[] cs = value.ToCharArray();
            return cs.Count(x => x == c);
        }

        [Test]
        public void SparqlFormattingUnion1()
        {
            const string query = "SELECT * WHERE { { ?s a ?type } UNION { ?s ?p ?o } }";
            SparqlQuery q = this._parser.ParseFromString(query);

            Console.WriteLine("ToString() form:");
            String toString = q.ToString();
            Console.WriteLine(toString);
            Console.WriteLine();
            Console.WriteLine("Format() form:");
            String formatted = this._formatter.Format(q);
            Console.WriteLine(formatted);

            Assert.IsTrue(toString.Contains("UNION"), "ToString() form should contain UNION");
            Assert.AreEqual(3, CountOccurrences(toString, '{'), "ToString() form should contain three opening braces");
            Assert.AreEqual(3, CountOccurrences(toString, '}'), "ToString() form should contain three closing braces");
            Assert.IsTrue(formatted.Contains("UNION"), "Formatted form should contain UNION");
            Assert.AreEqual(3, CountOccurrences(formatted, '{'), "Formatted form should contain three opening braces");
            Assert.AreEqual(3, CountOccurrences(formatted, '}'), "Formatted form should contain three closing braces");
        }

        [Test]
        public void SparqlFormattingUnion2()
        {
            const string query = "SELECT * WHERE { { GRAPH <http://x> { ?s a ?type } } UNION { GRAPH <http://y> { ?s ?p ?o } } }";
            SparqlQuery q = this._parser.ParseFromString(query);

            Console.WriteLine("ToString() form:");
            String toString = q.ToString();
            Console.WriteLine(toString);
            Console.WriteLine();
            Console.WriteLine("Format() form:");
            String formatted = this._formatter.Format(q);
            Console.WriteLine(formatted);

            Assert.IsTrue(toString.Contains("UNION"), "ToString() form should contain UNION");
            Assert.AreEqual(5, CountOccurrences(toString, '{'), "ToString() form should contain five opening braces");
            Assert.AreEqual(5, CountOccurrences(toString, '}'), "ToString() form should contain five closing braces");
            Assert.IsTrue(formatted.Contains("UNION"), "Formatted form should contain UNION");
            Assert.AreEqual(5, CountOccurrences(formatted, '{'), "Formatted form should contain five opening braces");
            Assert.AreEqual(5, CountOccurrences(formatted, '}'), "Formatted form should contain five closing braces");
        }
    }
}
