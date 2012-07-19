using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VDS.RDF.Test
{
    [TestClass]
    public class NamespaceMapperTest : BaseTest
    {
        [TestMethod]
        public void NamespaceMapperEvent()
        {
            bool eventRaised = false;

            NamespaceChanged added = delegate(String prefix, Uri u) { eventRaised = true; };
            NamespaceChanged changed = delegate(String prefix, Uri u) { eventRaised = true; };
            NamespaceChanged removed = delegate(String prefix, Uri u) { eventRaised = true; };

            NamespaceMapper nsmap = new NamespaceMapper();
            nsmap.NamespaceAdded += added;
            nsmap.NamespaceModified += changed;
            nsmap.NamespaceRemoved += removed;

            Console.WriteLine("Trying to add the RDF Namespace, this should already be defined");
            nsmap.AddNamespace("rdf", new Uri(NamespaceMapper.RDF));
            Assert.AreEqual(false, eventRaised, "Trying to add a Namespace that already exists should have no effect");
            eventRaised = false;
            Console.WriteLine();

            Console.WriteLine("Trying to add an example Namespace which isn't defined");
            nsmap.AddNamespace("ex", new Uri("http://example.org/"));
            Assert.AreEqual(true, eventRaised, "Adding a new Namespace should raise the NamespaceAdded event");
            eventRaised = false;
            Console.WriteLine(nsmap.GetNamespaceUri("ex").AbsoluteUri);
            Console.WriteLine();

            Console.WriteLine("Trying to modify the example Namespace");
            nsmap.AddNamespace("ex", new Uri("http://example.org/test/"));
            Assert.AreEqual(true, eventRaised, "Modifying a Namespace should raise the NamespaceModified event");
            eventRaised = false;
            Console.WriteLine(nsmap.GetNamespaceUri("ex").AbsoluteUri);
            Console.WriteLine();

            Console.WriteLine("Trying to remove the example Namespace");
            nsmap.RemoveNamespace("ex");
            Assert.AreEqual(true, eventRaised, "Removing an existing Namespace should raise the NamespaceRemoved event");
            eventRaised = false;
            Console.WriteLine();

            Console.WriteLine("Trying to remove a non-existent Namespace");
            nsmap.RemoveNamespace("ex");
            Assert.AreEqual(false, eventRaised, "Removing a non-existent Namespace should not raise the NamespaceRemoved event");
            eventRaised = false;
            Console.WriteLine();

            Console.WriteLine("Adding some example Namespace back in again for an import test");
            nsmap.AddNamespace("ex", new Uri("http://example.org/"));
            nsmap.AddNamespace("ns0", new Uri("http://example.org/clashes/"));

            Console.WriteLine("Creating another Namespace Mapper with the ex prefix mapped to a different URI");
            NamespaceMapper nsmap2 = new NamespaceMapper();
            nsmap2.AddNamespace("ex", new Uri("http://example.org/test/"));

            Console.WriteLine("Importing the new NamespaceMapper into the original");
            nsmap.Import(nsmap2);
            Console.WriteLine("NamespaceMapper now contains the following Namespaces:");
            foreach (String prefix in nsmap.Prefixes)
            {
                Console.WriteLine("\t" + prefix + " <" + nsmap.GetNamespaceUri(prefix).AbsoluteUri + ">");
            }
            Assert.AreEqual(nsmap.GetNamespaceUri("ex"), new Uri("http://example.org/"), "ex prefix should be mapped to the original URI");
            Assert.AreEqual(nsmap.GetNamespaceUri("ns1"), new Uri("http://example.org/test/"), "ex prefix from other NamespaceMapper should get remapped to ns1 since ns0 is already in use");
        }
    }
}
