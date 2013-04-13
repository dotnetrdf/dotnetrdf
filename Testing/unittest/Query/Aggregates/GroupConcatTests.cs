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
using VDS.RDF.Query.Datasets;

namespace VDS.RDF.Query.Aggregates
{
    [TestFixture]
    public class GroupConcatTests
    {
        private SparqlQueryParser _parser = new SparqlQueryParser();

        private void RunTest(IGraph g, String query, int expected, String var, bool expectNotNull, String expectMatch)
        {
            SparqlQuery q = this._parser.ParseFromString(query);
            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(new InMemoryDataset(g));

            SparqlResultSet results = processor.ProcessQuery(q) as SparqlResultSet;
            Assert.IsNotNull(results);
            TestTools.ShowResults(results);

            Assert.AreEqual(expected, results.Count);
            Assert.IsTrue(results.Variables.Contains(var));

            foreach (SparqlResult r in results)
            {
                Assert.IsTrue(r.HasValue(var));
                if (expectNotNull)
                {
                    Assert.IsTrue(r.HasBoundValue(var));
                    INode value = r[var];
                    Assert.AreEqual(NodeType.Literal, value.NodeType);
                    String lexValue = ((ILiteralNode)value).Value;
                    Assert.IsTrue(lexValue.Contains(expectMatch));
                }
                else
                {
                    Assert.IsFalse(r.HasBoundValue(var));
                }
            }
        }

        [Test]
        public void SparqlGroupConcat1()
        {
            IGraph g = new Graph();
            g.NamespaceMap.AddNamespace("ex", UriFactory.Create("http://example.org/ns#"));
            g.Assert(g.CreateUriNode("ex:subject"), g.CreateUriNode("ex:predicate"), g.CreateLiteralNode("object"));

            this.RunTest(g, "SELECT (GROUP_CONCAT(?o) AS ?concat) WHERE { ?s ?p ?o }", 1, "concat", true, "object");
        }

        [Test]
        public void SparqlGroupConcat2()
        {
            IGraph g = new Graph();
            g.NamespaceMap.AddNamespace("ex", UriFactory.Create("http://example.org/ns#"));
            g.Assert(g.CreateUriNode("ex:subject"), g.CreateUriNode("ex:predicate"), g.CreateLiteralNode("object"));

            this.RunTest(g, "SELECT (GROUP_CONCAT(?s) AS ?concat) WHERE { ?s ?p ?o }", 1, "concat", true, "subject");
        }

        [Test]
        public void SparqlGroupConcat3()
        {
            IGraph g = new Graph();

            String query = @"SELECT (GROUP_CONCAT(?x) AS ?concat)
WHERE
{
  VALUES ( ?x )
  {
    ( 'string' )
    ( 1234 )
    ( true )
  }
}";
            this.RunTest(g, query, 1, "concat", true, "1234");
        }

        [Test]
        public void SparqlGroupConcat4()
        {
            IGraph g = new Graph();

            String query = @"SELECT (GROUP_CONCAT(?x) AS ?concat)
WHERE
{
  VALUES ( ?x )
  {
    ( 'string' )
    ( 1234 )
    ( true )
    ( 'custom'^^<http://datatype> )
  }
}";
            this.RunTest(g, query, 1, "concat", true, "custom");
        }
    }
}
