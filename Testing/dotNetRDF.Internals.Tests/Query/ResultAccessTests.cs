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
using System.Linq;
using System.Text;
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query
{

    public class ResultAccessTests
    {
        private ISparqlDataset _dataset;
        private LeviathanQueryProcessor _processor;
        private SparqlQueryParser _parser = new SparqlQueryParser();
        private NTriplesFormatter _formatter = new NTriplesFormatter();

        public ResultAccessTests()
        {
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            TripleStore store = new TripleStore();
            store.Add(g);
            this._dataset = new InMemoryDataset(store);
            this._processor = new LeviathanQueryProcessor(this._dataset);
        }

        private SparqlQuery CreateQuery(String query)
        {
            SparqlParameterizedString queryStr = new SparqlParameterizedString(query);
            queryStr.Namespaces = new NamespaceMapper();
            return this._parser.ParseFromString(queryStr);
        }

        private SparqlResultSet GetResults(SparqlQuery query)
        {
            Object results = this._processor.ProcessQuery(query);
            Assert.IsAssignableFrom<SparqlResultSet>(results);
            return results as SparqlResultSet;
        }

        [Fact]
        public void SparqlResultAccessByName()
        {
            String query = "SELECT * WHERE { ?s a ?type ; rdfs:comment ?comment }";
            SparqlQuery q = this.CreateQuery(query);
            SparqlResultSet results = this.GetResults(q);

            foreach (SparqlResult r in results)
            {
                Console.WriteLine("?s = " + r["s"].ToString(this._formatter) + " ?comment = " + r["comment"].ToString(this._formatter));
            }
        }

        [Fact]
        public void SparqlResultAccessByNameError()
        {
            Assert.Throws<RdfException>(() =>
            {
                String query = "SELECT * WHERE { ?s a ?type . OPTIONAL { ?s ex:range ?range } }";
                SparqlQuery q = this.CreateQuery(query);
                SparqlResultSet results = this.GetResults(q);

                foreach (SparqlResult r in results)
                {
                    Console.WriteLine("?s = " + r["s"].ToString(this._formatter) + " ?range = " +
                                      r["range"].ToString(this._formatter));
                }
            });
        }

        [Fact]
        public void SparqlResultAccessByNameSafeHasValue()
        {
            String query = "SELECT * WHERE { ?s a ?type . OPTIONAL { ?s rdfs:range ?range } }";
            SparqlQuery q = this.CreateQuery(query);
            SparqlResultSet results = this.GetResults(q);

            List<String> vars = results.Variables.ToList();
            foreach (SparqlResult r in results)
            {
                foreach (String var in vars)
                {
                    if (r.HasValue(var) && r[var] != null)
                    {
                        Console.Write("?" + var + " = " + r[var].ToString(this._formatter));
                    }
                }
                Console.WriteLine();
            }
        }

        [Fact]
        public void SparqlResultAccessByNameSafeTryGetValue()
        {
            String query = "SELECT * WHERE { ?s a ?type . OPTIONAL { ?s rdfs:range ?range } }";
            SparqlQuery q = this.CreateQuery(query);
            SparqlResultSet results = this.GetResults(q);

            List<String> vars = results.Variables.ToList();
            foreach (SparqlResult r in results)
            {
                foreach (String var in vars)
                {
                    INode value;
                    if (r.TryGetValue(var, out value) && value != null)
                    {
                        Console.Write("?" + var + " = " + value.ToString(this._formatter));
                    }
                }
                Console.WriteLine();
            }
        }

        [Fact]
        public void SparqlResultAccessByNameSafeTryGetBoundValue()
        {
            String query = "SELECT * WHERE { ?s a ?type . OPTIONAL { ?s rdfs:range ?range } }";
            SparqlQuery q = this.CreateQuery(query);
            SparqlResultSet results = this.GetResults(q);

            List<String> vars = results.Variables.ToList();
            foreach (SparqlResult r in results)
            {
                foreach (String var in vars)
                {
                    INode value;
                    if (r.TryGetBoundValue(var, out value))
                    {
                        Console.Write("?" + var + " = " + value.ToString(this._formatter));
                    }
                }
                Console.WriteLine();
            }
        }

        [Fact]
        public void SparqlResultAccessByIndex()
        {
            String query = "SELECT * WHERE { ?s a ?type ; rdfs:comment ?comment }";
            SparqlQuery q = this.CreateQuery(query);
            SparqlResultSet results = this.GetResults(q);

            List<String> vars = results.Variables.ToList();
            foreach (SparqlResult r in results)
            {
                for (int i = 0; i < vars.Count; i++)
                {
                    Console.Write("?" + vars[i] + " = " + r[i].ToString(this._formatter));
                }
                Console.WriteLine();
            }
        }

        [Fact]
        public void SparqlResultSetVariableOrder1()
        {
            String query = "SELECT ?s ?type ?comment WHERE { ?s a ?type ; rdfs:comment ?comment }";
            SparqlQuery q = this.CreateQuery(query);
            SparqlResultSet results = this.GetResults(q);

            this.TestVariableOrder(results, new List<String>() { "s", "type", "comment" });
        }

        [Fact]
        public void SparqlResultSetVariableOrder2()
        {
            String query = "SELECT ?s ?comment ?type WHERE { ?s a ?type ; rdfs:comment ?comment }";
            SparqlQuery q = this.CreateQuery(query);
            SparqlResultSet results = this.GetResults(q);

            this.TestVariableOrder(results, new List<String>() { "s", "comment", "type" });
        }

        [Fact]
        public void SparqlResultSetVariableOrder3()
        {
            String query = "SELECT ?comment ?type ?s WHERE { ?s a ?type ; rdfs:comment ?comment }";
            SparqlQuery q = this.CreateQuery(query);
            SparqlResultSet results = this.GetResults(q);

            this.TestVariableOrder(results, new List<String>() { "comment", "type", "s"});
        }

        [Fact]
        public void SparqlResultSetVariableOrder4()
        {
            String query = "SELECT ?comment ?type WHERE { ?s a ?type ; rdfs:comment ?comment }";
            SparqlQuery q = this.CreateQuery(query);
            SparqlResultSet results = this.GetResults(q);

            this.TestVariableOrder(results, new List<String>() { "comment", "type" });
        }

        [Fact]
        public void SparqlResultSetVariableOrder5()
        {
            String data = @"<?xml version=""1.0"" encoding=""utf-8""?>
<sparql xmlns=""" + SparqlSpecsHelper.SparqlNamespace + @""">
    <head>
        <variable name=""a"" />
        <variable name=""b"" />
    </head>
    <results />
</sparql>";

            SparqlResultSet results = new SparqlResultSet();
            StringParser.ParseResultSet(results, data, new SparqlXmlParser());

            this.TestVariableOrder(results, new List<string>() { "a", "b" });
        }

        [Fact]
        public void SparqlResultSetVariableOrder6()
        {
            String data = @"<?xml version=""1.0"" encoding=""utf-8""?>
<sparql xmlns=""" + SparqlSpecsHelper.SparqlNamespace + @""">
    <head>
        <variable name=""b"" />
        <variable name=""a"" />
    </head>
    <results />
</sparql>";

            SparqlResultSet results = new SparqlResultSet();
            StringParser.ParseResultSet(results, data, new SparqlXmlParser());

            this.TestVariableOrder(results, new List<string>() { "b", "a" });
        }

        [Fact]
        public void SparqlResultSetVariableOrder7()
        {
            String data = @"<?xml version=""1.0"" encoding=""utf-8""?>
<sparql xmlns=""" + SparqlSpecsHelper.SparqlNamespace + @""">
    <head>
        <variable name=""b"" />
        <variable name=""a"" />
    </head>
    <results>
        <result>
            <binding name=""a""><uri>http://example</uri></binding>
            <binding name=""b""><uri>http://example</uri></binding>
        </result>
        <result>
            <binding name=""b""><uri>http://example</uri></binding>
            <binding name=""a""><uri>http://example</uri></binding>
        </result>
        <result>
            <binding name=""a""><uri>http://example</uri></binding>
        </result>
        <result>
            <binding name=""b""><uri>http://example</uri></binding>
        </result>
    </results>
</sparql>";

            SparqlResultSet results = new SparqlResultSet();
            StringParser.ParseResultSet(results, data, new SparqlXmlParser());

            this.TestVariableOrder(results, new List<string>() { "b", "a" });
        }

        private void TestVariableOrder(SparqlResultSet results, List<String> expected)
        {
            List<String> vars = results.Variables.ToList();
            this.TestVariableOrder(expected, vars);

            foreach (SparqlResult r in results)
            {
                vars = r.Variables.ToList();
                this.TestVariableOrder(expected, vars);
            }
        }

        private void TestVariableOrder(List<String> expected, List<String> actual)
        {
            Assert.Equal(expected.Count, actual.Count);
            for (int i = 0; i < expected.Count; i++)
            {
                Assert.Equal(expected[i], actual[i]);
            }
        }
    }
}
