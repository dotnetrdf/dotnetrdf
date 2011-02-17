using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Storage;

namespace VDS.RDF.Test.Storage
{
    [TestClass]
    public class SqlIOTests
    {
        [TestMethod]
        public void StorageSqlIOManagerDispose()
        {
            ISqlIOManager manager = new NonNativeVirtuosoManager("test", "test", "test");
            Console.WriteLine("Created a Manager which we will now dipose of twice, second dispose should throw an error");

            manager.Dispose();
            try
            {
                manager.Dispose();
                Assert.Fail("Second dispose should have thrown an error");
            }
            catch (RdfStorageException ex)
            {
                Console.WriteLine("Error thrown as expected: " + ex.Message);
            }

            Console.WriteLine();
            Console.WriteLine("If we try and save a Triple we should also now get an error");
            try
            {
                manager.SaveTriple(null, "id");
                Assert.Fail("Trying to save a Triple once the Manager is disposed of should fail");
            }
            catch (RdfStorageException ex)
            {
                Console.WriteLine("Error thrown as expected: " + ex.Message);
            }

            manager = new NonNativeVirtuosoManager("test", "test", "test");
            Console.WriteLine();
            Console.WriteLine("Repeating the test but this time setting the PreserveState property to true");

            manager.PreserveState = true;
            manager.Dispose();
            try
            {
                manager.Dispose();
                manager.Dispose();
                Console.WriteLine("Subsequent Dispose() calls did not throw errors as expected since PreserveState is set to true");
            }
            catch (RdfStorageException)
            {
                Assert.Fail("Subsequent Dispose() calls should not error if PreserveState is set to true");
            }

            manager.PreserveState = false;
            manager.Dispose();
        }

        //[TestMethod]
        //public void StorageSqlIOAssertRetract()
        //{

        //}

    }
}
