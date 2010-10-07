using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.Alexandria.Documents;

namespace alexandria_tests
{
    [TestClass]
    public class ManagerTests
    {
        [TestMethod]
        public void MongoHasDocument()
        {
            MongoDBDocumentManager manager = new MongoDBDocumentManager("test-empty");
            String name = "default-graph";
            Assert.IsFalse(manager.HasDocument(name), "Database should not have contained the Graph");
            manager.Dispose();
        }
    }
}
