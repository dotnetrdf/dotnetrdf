using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Storage;

namespace VDS.RDF.Test.Storage.Async
{
    [TestClass]
    public class StardogAsync
        : BaseAsyncTests
    {
        protected override IAsyncStorageProvider GetAsyncProvider()
        {
            return new StardogConnector(StardogTests.StardogTestUri, StardogTests.StardogTestKB, StardogTests.StardogUser, StardogTests.StardogPassword);
        }
    }
}
