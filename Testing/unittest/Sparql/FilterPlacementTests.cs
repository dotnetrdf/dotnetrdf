using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace VDS.RDF.Test.Sparql
{
    [TestClass]
    public class FilterPlacementTests
    {
        [TestMethod]
        public void SparqlFilterOptionalNotBound()
        {
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");

            SparqlParameterizedString query = new SparqlParameterizedString();
            query.Namespaces.AddNamespace("rdf", new Uri(NamespaceMapper.RDF));
            query.Namespaces.AddNamespace("rdfs", new Uri(NamespaceMapper.RDFS));
            query.CommandText = "SELECT * WHERE { ?property a rdf:Property . OPTIONAL { ?property rdfs:range ?range } FILTER (!BOUND(?range)) }";

            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);
            SparqlResultSet results = g.ExecuteQuery(q) as SparqlResultSet;
            if (results != null)
            {
                TestTools.ShowResults(results);

                Assert.IsTrue(results.All(r => !r.HasValue("range") || r["range"] == null), "There should be no values for ?range returned");
            }
            else
            {
                Assert.Fail("Did not get a SparqlResultSet as expected");
            }
        }
    }
}
