using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using VDS.RDF.LinkedData;
using VDS.RDF.LinkedData.Kasabi;
using VDS.RDF.LinkedData.Kasabi.Apis;
using VDS.RDF.Query;

namespace VDS.RDF.Test.LinkedData.Kasabi
{
    [TestClass]
    public abstract class BaseKasabiTests
    {
        private String _apiKey;

        protected String GetAuthKey()
        {
            if (this._apiKey != null) return this._apiKey;

            if (File.Exists("kasabi-api-key.txt"))
            {
                this._apiKey = new StreamReader("kasabi-api-key.txt").ReadToEnd().Trim();
                return this._apiKey;
            }
            else
            {
                Assert.Fail("Required kasabi-api-key.txt file was not found");
                return null;
            }
        }

        protected abstract String GetDatasetID();

        private void EnsureTest()
        {
            KasabiClient.AuthenticationKey = this.GetAuthKey();
            Console.WriteLine("Dataset ID = " + this.GetDatasetID());
        }

        [TestMethod]
        public void LinkedDataKasabiAttributionApiJavascript()
        {
            this.EnsureTest();
            AttributionApi api = KasabiClient.GetAttributionApi(this.GetDatasetID());

            Console.WriteLine(api.GetJavascript());
        }

        [TestMethod]
        public void LinkedDataKasabiAttributionApiJsonRaw()
        {
            this.EnsureTest();
            AttributionApi api = KasabiClient.GetAttributionApi(this.GetDatasetID());

            Console.WriteLine(api.GetRawJson());
        }

        [TestMethod]
        public void LinkedDataKasabiAttributionApiJsonObject()
        {
            this.EnsureTest();
            AttributionApi api = KasabiClient.GetAttributionApi(this.GetDatasetID());

            JToken token = api.GetJson();
            Console.WriteLine(token.ToString());
        }

        [TestMethod]
        public void LinkedDataKasabiSparqlAsk()
        {
            try
            {
                Options.HttpDebugging = true;

                this.EnsureTest();
                SparqlApi api = KasabiClient.GetSparqlApi(this.GetDatasetID());

                SparqlResultSet results = api.QueryWithResultSet("ASK WHERE { ?s ?p ?o }");
                TestTools.ShowResults(results);
                Assert.IsTrue(results.Result, "Expected Boolean Result to be true");
            }
            finally
            {
                Options.HttpDebugging = false;
            }
        }

        [TestMethod]
        public void LinkedDataKasabiSparqlSelect()
        {
            try
            {
                Options.HttpDebugging = true;

                this.EnsureTest();
                SparqlApi api = KasabiClient.GetSparqlApi(this.GetDatasetID());

                SparqlResultSet results = api.QueryWithResultSet("SELECT * WHERE { ?s a ?type } LIMIT 10");
                TestTools.ShowResults(results);
            }
            finally
            {
                Options.HttpDebugging = false;
            }
        }

        protected abstract Uri GetLookupUri();

        [TestMethod]
        public void LinkedDataKasabiLookup()
        {
            try
            {
                Options.HttpDebugging = true;
                this.EnsureTest();

                LookupApi api = KasabiClient.GetLookupApi(this.GetDatasetID());
                IGraph g = api.GetDescription(this.GetLookupUri());

                TestTools.ShowGraph(g);
            }
            finally
            {
                Options.HttpDebugging = false;
            }
        }
    }
}
