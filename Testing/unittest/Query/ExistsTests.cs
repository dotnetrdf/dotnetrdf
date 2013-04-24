using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;

namespace VDS.RDF.Query
{
    [TestClass]
    public class ExistsTests
    {
        [TestMethod]
        public void SparqlExistsSimple1()
        {
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");

            String query = @"SELECT *
WHERE
{
  ?s ?p ?o .
  FILTER EXISTS { ?s a ?type }
}";

            SparqlQuery q = new SparqlQueryParser().ParseFromString(query);

            SparqlResultSet results = g.ExecuteQuery(q) as SparqlResultSet;
            Assert.IsNotNull(results);
            Assert.IsFalse(results.IsEmpty);
        }

        [TestMethod]
        public void SparqlExistsSimple2()
        {
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            int expected = g.GetTriplesWithObject(g.CreateUriNode(UriFactory.Create(ConfigurationLoader.ClassHttpHandler))).Count();

            String query = @"SELECT *
WHERE
{
  ?s ?p ?o .
  FILTER EXISTS { ?s ?property <" + ConfigurationLoader.ClassHttpHandler + @"> }
}";

            SparqlQuery q = new SparqlQueryParser().ParseFromString(query);

            SparqlResultSet results = g.ExecuteQuery(q) as SparqlResultSet;
            Assert.IsNotNull(results);
            Assert.IsFalse(results.IsEmpty);
            Assert.IsTrue(expected < results.Count, "Should be more triples found that just those matched by the EXISTS clause");
        }
    }
}
