using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.RDF.Storage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.Alexandria;

namespace alexandria_tests
{
    [TestClass]
    public class CreateTests
    {
        [TestMethod]
        public void FSCreateStore()
        {
            String[] testDirs = new String[]
            {
                "testdir",
                "testdir2",
                "random"
            };

            foreach (String dir in testDirs)
            {
                Console.WriteLine(Path.GetFullPath(dir));
                AlexandriaFileManager manager = new AlexandriaFileManager(dir);
                manager.Dispose();
            }
        }

        [TestMethod]
        public void MongoCreateStore()
        {
            AlexandriaMongoDBManager manager = new AlexandriaMongoDBManager("test");
            manager.Dispose();
        }
    }
}
