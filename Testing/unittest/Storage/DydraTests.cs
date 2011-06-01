using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Storage;

namespace VDS.RDF.Test.Storage
{
    [TestClass]
    public class DydraTests
    {
        private String _apiKey;

        public const String DydraAccount = "rvesse",
                            DydraRepository = "test",
                            DydraApiKeyFIle = "dydra-api-key.txt";

        private DydraConnector GetConnection()
        {
            if (this._apiKey == null)
            {
                //Read in API Key if not yet read
                if (File.Exists(DydraApiKeyFIle))
                {
                    using (StreamReader reader = new StreamReader(DydraApiKeyFIle))
                    {
                        this._apiKey = reader.ReadToEnd();
                        reader.Close();
                    }
                }
                else
                {
                    Assert.Fail("You must specify your Dydra API Key in the " + DydraApiKeyFIle + " file found in the resources directory");
                }
            }

            return new DydraConnector(DydraAccount, DydraRepository, this._apiKey);
        }

        [TestMethod]
        public void StorageDydraSaveToDefaultGraph()
        {
            try
            {
                Graph orig = new Graph();
                orig.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
                orig.BaseUri = null;

                DydraConnector dydra = this.GetConnection();
                dydra.SaveGraph(orig);

                Graph g = new Graph();
                dydra.LoadGraph(g, (Uri)null);

                if (orig.Triples.Count == g.Triples.Count)
                {
                    Assert.AreEqual(orig, g, "Graphs should be equal");
                }
                else
                {
                    Assert.IsTrue(g.HasSubGraph(orig), "Original Graph should be a sub-graph of retrieved Graph");
                }
            }
            catch (Exception ex)
            {
                TestTools.ReportError("Error", ex, true);
            }
        }

        [TestMethod]
        public void StorageDydraSaveToNamedGraph()
        {
            try
            {
                Options.HttpDebugging = true;

                Graph orig = new Graph();
                orig.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");

                DydraConnector dydra = this.GetConnection();
                dydra.SaveGraph(orig);

                Graph g = new Graph();
                dydra.LoadGraph(g, orig.BaseUri);

                if (orig.Triples.Count == g.Triples.Count)
                {
                    Assert.AreEqual(orig, g, "Graphs should be equal");
                }
                else
                {
                    Assert.IsTrue(g.HasSubGraph(orig), "Original Graph should be a sub-graph of retrieved Graph");
                }
            }
            catch (Exception ex)
            {
                TestTools.ReportError("Error", ex, true);
            }
            finally
            {
                Options.HttpDebugging = false;
            }
        }
    }
}
