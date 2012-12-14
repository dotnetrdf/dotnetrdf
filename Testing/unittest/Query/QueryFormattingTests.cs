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
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query
{
    [TestClass]
    public class QueryFormattingTests
    {
        private SparqlFormatter _formatter = new SparqlFormatter();

        [TestMethod]
        public void SparqlFormattingFilter1()
        {
            String query = "SELECT * WHERE { { ?s ?p ?o } FILTER(ISURI(?o)) }";
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);

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

        [TestMethod]
        public void SparqlFormattingFilter2()
        {
            String query = "SELECT * WHERE { { ?s ?p ?o } FILTER(REGEX(?o, 'search', 'i')) }";
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);

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
    }
}
