using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.RDF.Storage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Alexandria;

namespace alexandria_tests
{
    [TestClass]
    public class CreateTests
    {
        [TestMethod]
        public void CreateFileStore()
        {
            String[] testDirs = new String[]
            {
                "test",
                "test2",
                "random"
            };

            foreach (String dir in testDirs)
            {
                Console.WriteLine(Path.GetFullPath(dir));
                AlexandriaFileManager manager = new AlexandriaFileManager(dir);
            }
        }
    }
}
