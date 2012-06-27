using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Storage;

namespace VDS.RDF.Test.Storage.Async
{
    [TestClass]
    public class DydraAsync
        : BaseAsyncTests
    {
        private String _apiKey;

        protected override IAsyncStorageProvider GetAsyncProvider()
        {
            if (this._apiKey == null)
            {
                //Read in API Key if not yet read
                if (File.Exists(DydraTests.DydraApiKeyFile))
                {
                    using (StreamReader reader = new StreamReader(DydraTests.DydraApiKeyFile))
                    {
                        this._apiKey = reader.ReadToEnd();
                        reader.Close();
                    }

                    Console.WriteLine("API Key:" + this._apiKey);
                }
                else
                {
                    Assert.Fail("You must specify your Dydra API Key in the " + DydraTests.DydraApiKeyFile + " file found in the resources directory");
                }
            }

            return new DydraConnector(DydraTests.DydraAccount, DydraTests.DydraRepository, this._apiKey);
        }
    }
}
