/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Writing;

namespace VDS.RDF.Test.Writing
{
    [TestClass]
    public class SparqlXmlTests
    {
        [TestMethod]
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

                Assert.AreEqual(rset, rset2, "Result Sets should be equal");
            }
            else
            {
                Assert.Fail("Query did not return a Result Set as expected");
            }
        }

        [TestMethod]
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

                Assert.AreEqual(rset, rset2, "Result Sets should be equal");
            }
            else
            {
                Assert.Fail("Query did not return a Result Set as expected");
            }
        }
    }
}
