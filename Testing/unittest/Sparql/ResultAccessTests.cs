using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Test.Sparql
{
    [TestClass]
    public class ResultAccessTests
    {
        private ISparqlDataset _dataset;
        private LeviathanQueryProcessor _processor;
        private SparqlQueryParser _parser = new SparqlQueryParser();
        private NTriplesFormatter _formatter = new NTriplesFormatter();

        [TestInitialize]
        public void EnsureDataset()
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
            if (results is SparqlResultSet)
            {
                return (SparqlResultSet)results;
            }
            else
            {
                Assert.Fail("Did not get a Result Set as expected");
                return null;
            }
        }

        [TestMethod]
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

        [TestMethod,ExpectedException(typeof(RdfException))]
        public void SparqlResultAccessByNameError()
        {
            String query = "SELECT * WHERE { ?s a ?type . OPTIONAL { ?s rdfs:range ?range } }";
            SparqlQuery q = this.CreateQuery(query);
            SparqlResultSet results = this.GetResults(q);

            foreach (SparqlResult r in results)
            {
                Console.WriteLine("?s = " + r["s"].ToString(this._formatter) + " ?range = " + r["range"].ToString(this._formatter));
            }
        }

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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
    }
}
