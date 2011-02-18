using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Query;
using VDS.RDF.Update;

namespace VDS.RDF.Test.Sparql
{
    [TestClass]
    public class RemoteEndpoints
    {
        const String TestQueryUri = "http://localhost/demos/leviathan/";
        const String TestUpdateUri = "http://localhost/demos/server/update";

        [TestMethod]
        public void SparqlRemoteEndpointLongQuery()
        {
            try
            {
                Options.HttpDebugging = true;

                StringBuilder input = new StringBuilder();
                input.AppendLine("SELECT * WHERE {?s ?p ?o}");
                input.AppendLine(new String('#', 2048));

                SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new Uri(TestQueryUri));
                Object results = endpoint.QueryWithResultSet(input.ToString());
                if (results is SparqlResultSet)
                {
                    TestTools.ShowResults(results);
                }
                else
                {
                    Assert.Fail("Should have returned a SPARQL Result Set");
                }

            }
            finally
            {
                Options.HttpDebugging = false;
            }
        }

        [TestMethod]
        public void SparqlRemoteEndpointLongUpdate()
        {
            try
            {
                Options.HttpDebugging = true;

                StringBuilder input = new StringBuilder();
                input.AppendLine("LOAD <http://dbpedia.org/resource/Ilkeston>");
                input.AppendLine(new String('#', 2048));

                SparqlRemoteUpdateEndpoint endpoint = new SparqlRemoteUpdateEndpoint(new Uri(TestUpdateUri));
                endpoint.Update(input.ToString());
            }
            finally
            {
                Options.HttpDebugging = false;
            }
        }
    }
}
