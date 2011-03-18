using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Storage;

namespace VDS.RDF.Test.Storage
{
    [TestClass]
    public class AdoStoreBasic
    {
        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void StorageAdoMicrosoftBadInstantiation()
        {
            MicrosoftAdoManager manager = new MicrosoftAdoManager(null, null, null, null);
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void StorageAdoMicrosoftBadInstantiation2()
        {
            MicrosoftAdoManager manager = new MicrosoftAdoManager(null, null, null);
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void StorageAdoMicrosoftBadInstantiation3()
        {
            MicrosoftAdoManager manager = new MicrosoftAdoManager("adostore", "user", null);
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void StorageAdoMicrosoftBadInstantiation4()
        {
            MicrosoftAdoManager manager = new MicrosoftAdoManager("adostore", null, "password");
        }

        [TestMethod]
        public void StorageAdoMicrosoftCheckVersion()
        {
            MicrosoftAdoManager manager = new MicrosoftAdoManager("adostore", "example", "password");
            int version = manager.CheckVersion();
            Console.WriteLine("Version: " + version);
            manager.Dispose();

            Assert.AreEqual(1, version, "Expected Version 1 to be reported");
        }
    }
}
