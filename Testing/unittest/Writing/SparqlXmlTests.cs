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
using System.IO;
using System.Linq;
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace VDS.RDF.Writing
{

    public class SparqlXmlTests
    {
        private readonly SparqlXmlParser _parser = new SparqlXmlParser();

        [Fact]
        public void WritingSparqlXmlWithNulls()
        {
            TripleStore store = new TripleStore();
            store.Add(new Graph());
            Graph g = new Graph();
            g.BaseUri = new Uri("http://example.org/graph");
            store.Add(g);

            Object results = store.ExecuteQuery("SELECT DISTINCT ?g WHERE { GRAPH ?g { ?s ?p ?o } }");
            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;
                SparqlXmlWriter writer = new SparqlXmlWriter();
                writer.Save(rset, "temp.srx");

                SparqlXmlParser parser = new SparqlXmlParser();
                SparqlResultSet rset2 = new SparqlResultSet();
                parser.Load(rset2, "temp.srx");

                rset.Trim();
                Console.WriteLine("Original Results");
                TestTools.ShowResults(rset);
                Console.WriteLine();

                rset2.Trim();
                Console.WriteLine("Serializes and Parsed Results");
                TestTools.ShowResults(rset2);
                Console.WriteLine();

                Assert.Equal(rset, rset2);
            }
            else
            {
                Assert.True(false, "Query did not return a Result Set as expected");
            }
        }

        [Fact]
        public void WritingSparqlXmlWithSpecialCharacters()
        {
            TripleStore store = new TripleStore();
            store.Add(new Graph());
            Graph g = new Graph();
            g.BaseUri = new Uri("http://example.org/graph");
            INode subj = g.CreateBlankNode();
            INode pred = g.CreateUriNode(new Uri("http://example.org/predicate"));
            INode obj1 = g.CreateLiteralNode("with & ampersand");
            INode obj2 = g.CreateLiteralNode("with < tags > ");
            g.Assert(subj, pred, obj1);
            g.Assert(subj, pred, obj2);
            store.Add(g);

            Object results = store.ExecuteQuery("SELECT * WHERE { ?s ?p ?o }");
            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;
                SparqlXmlWriter writer = new SparqlXmlWriter();
                writer.Save(rset, "temp.srx");

                SparqlXmlParser parser = new SparqlXmlParser();
                SparqlResultSet rset2 = new SparqlResultSet();
                parser.Load(rset2, "temp.srx");

                rset.Trim();
                Console.WriteLine("Original Results");
                TestTools.ShowResults(rset);
                Console.WriteLine();

                rset2.Trim();
                Console.WriteLine("Serializes and Parsed Results");
                TestTools.ShowResults(rset2);
                Console.WriteLine();

                Assert.Equal(rset, rset2);
            }
            else
            {
                Assert.True(false, "Query did not return a Result Set as expected");
            }
        }

        [Fact]
        public void ParsingSparqlXmlCore432_01()
        {
            // Test case based off of CORE-432 - relative URI in XML
            const String data = @"<?xml version=""1.0""?>
<sparql xmlns=""http://www.w3.org/2005/sparql-results#"">
  <head>
    <variable name=""x"" />
    <variable name=""y"" />
  </head>
  <results>
    <result><binding name=""x""><uri>relative</uri></binding></result>
  </results>
</sparql>";
            SparqlResultSet results = new SparqlResultSet();

            Assert.Throws<RdfParseException>(() => this._parser.Load(results, new StringReader(data)));
        }

        [Fact]
        public void ParsingSparqlXmlCore432_02()
        {
            // Test case based off of CORE-432 - relative URI in XML
            const String data = @"<?xml version=""1.0""?>
<sparql xmlns=""http://www.w3.org/2005/sparql-results#"">
  <head>
    <variable name=""x"" />
    <variable name=""y"" />
  </head>
  <results>
    <result><binding name=""x""><literal datatype=""relative"">value</literal></binding></result>
  </results>
</sparql>";
            SparqlResultSet results = new SparqlResultSet();

            Assert.Throws<RdfParseException>(() => this._parser.Load(results, new StringReader(data)));
        }

        [Fact]
        public void ParsingSparqlXmlCore432_03()
        {
            // Test case based off of CORE-432 - invalid URI in XML
            const String data = @"<?xml version=""1.0""?>
<sparql xmlns=""http://www.w3.org/2005/sparql-results#"">
  <head>
    <variable name=""x"" />
    <variable name=""y"" />
  </head>
  <results>
    <result><binding name=""x""><uri>http://an invalid uri</uri></binding></result>
  </results>
</sparql>";
            SparqlResultSet results = new SparqlResultSet();

            Assert.Throws<RdfParseException>(() => this._parser.Load(results, new StringReader(data)));
        }

        [Fact]
        public void ParsingSparqlXmlCore432_04()
        {
            // Test case based off of CORE-432 - invalid URI in XML
            const String data = @"<?xml version=""1.0""?>
<sparql xmlns=""http://www.w3.org/2005/sparql-results#"">
  <head>
    <variable name=""x"" />
    <variable name=""y"" />
  </head>
  <results>
    <result><binding name=""x""><literal datatype=""http://an invalid uri"">Literal with invalid datatype URI</literal></binding></result>
  </results>
</sparql>";
            SparqlResultSet results = new SparqlResultSet();

            Assert.Throws<RdfParseException>(() => this._parser.Load(results, new StringReader(data)));
        }
    }
}
