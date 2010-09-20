using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using VDS.RDF.Linq;

namespace UnitTests
{
    /// <summary>
    /// Summary description for NamspaceManagetTests
    /// </summary>
    [TestFixture]
    public class NamspaceManagerTests
    {
        public NamspaceManagerTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }
        public NamespaceManager nsm = null;
        [SetUp]
        public void MySetUp()
        {
            nsm = new NamespaceManager();
        }
       [TearDown]
        public void MyTearDown()
        {
            nsm = null;
        }
        private static Dictionary<string, OntologyAttribute> CreateNamespaceDictionary()
        {
            Dictionary<string, OntologyAttribute> dict = new Dictionary<string, OntologyAttribute>();
            dict["a"] = new OntologyAttribute { Name = "Namea"};
            dict["b"] = new OntologyAttribute { Name = "Nameb" };
            dict["c"] = new OntologyAttribute { Name = "Namec" };
            dict["d"] = new OntologyAttribute { Name = "Named" };
            return dict;
        }
        private Dictionary<string, OntologyAttribute> CreateRealisticNamespaceDictionary()
        {
            Dictionary<string, OntologyAttribute> dict = new Dictionary<string, OntologyAttribute>();
            dict["Name1"] = new OntologyAttribute
            {
                BaseUri = "BaseUri1",
                GraphName = "GraphName1",
                Name = "Name1",
                Prefix = "Prefix1",
                UrlOfOntology = "UrlOfOntology1"
            };
            dict["Name2"] = new OntologyAttribute
            {
                BaseUri = "BaseUri2",
                GraphName = "GraphName2",
                Name = "Name2",
                Prefix = "Prefix2",
                UrlOfOntology = "UrlOfOntology2"
            };
            dict["Name3"] = new OntologyAttribute
            {
                BaseUri = "BaseUri3",
                GraphName = "GraphName3",
                Name = "Name3",
                Prefix = "Prefix3",
                UrlOfOntology = "UrlOfOntology3"
            };
            dict["Name4"] = new OntologyAttribute
            {
                BaseUri = "BaseUri4",
                GraphName = "GraphName4",
                Name = "Name4",
                Prefix = "Prefix4",
                UrlOfOntology = "UrlOfOntology4"
            };
            return dict;
        }

        [Test]
        public void TestCreateWithNoParameters()
        {
            Assert.IsNotNull(nsm);
            Assert.IsTrue(nsm.Count == 0);
            Assert.IsTrue(nsm.Default == null);
        }
        [Test]
        public void TestCreateWithDictionary()
        {
            OntologyAttribute expectedOntology = new OntologyAttribute();
            Dictionary<string, OntologyAttribute> dict = new Dictionary<string, OntologyAttribute>();
            dict["ontology"] = expectedOntology;
            dict["b"] = new OntologyAttribute();
            dict["c"] = new OntologyAttribute();
            dict["d"] = new OntologyAttribute();
            nsm = new NamespaceManager(dict);
            Assert.IsTrue(nsm.Count == 4);
            Assert.IsTrue(nsm["ontology"] == expectedOntology);
        }

        [Test]
        public void TestCreateWithEnumerable()
        {
            Dictionary<string, OntologyAttribute> dict = CreateNamespaceDictionary();
            nsm = new NamespaceManager(dict.Values.AsEnumerable());
            Assert.IsTrue(nsm.Count == 4);
        }

        [Test]
        public void TestCreateWithAnotherNamespaceManager()
        {
            Dictionary<string, OntologyAttribute> dict = CreateNamespaceDictionary();
            nsm = new NamespaceManager(dict);
            NamespaceManager nsm2 = new NamespaceManager(nsm);
            Assert.IsTrue(nsm2.Count == 4);
        }

        [Test]
        public void TestEnumerateNamespaces()
        {
            Dictionary<string, OntologyAttribute> dict = CreateRealisticNamespaceDictionary();
            nsm = new NamespaceManager(dict);
            int counter = 0;
            foreach (string ns in nsm.Namespaces)
            {
                counter++;
                Assert.IsTrue(ns == ("BaseUri" + counter));
            }
            Assert.IsTrue(counter == 4);
        }

        [Test]
        public void TestEnumerateOntologies()
        {
            Dictionary<string, OntologyAttribute> dict = CreateRealisticNamespaceDictionary();
            nsm = new NamespaceManager(dict);
            int counter = 0;
            foreach (OntologyAttribute ont in nsm.Ontologies)
            {
                counter++;
                Assert.IsNotNull(ont);
                Assert.IsTrue(ont.BaseUri == "BaseUri" + counter.ToString());
                Assert.IsTrue(ont.GraphName == "GraphName" + counter.ToString());
                Assert.IsTrue(ont.Name  == "Name" + counter.ToString());
                Assert.IsTrue(ont.Prefix == "Prefix" + counter.ToString());
                Assert.IsTrue(ont.UrlOfOntology == "UrlOfOntology" + counter.ToString());
            }
            Assert.IsTrue(counter == 4);
        }
        [Test]
        public void TestSelectOntologyByName()
        {
            Dictionary<string, OntologyAttribute> dict = CreateRealisticNamespaceDictionary();
            nsm = new NamespaceManager(dict);
            Assert.IsNotNull(nsm["Name3"]);
        }
        [Test]
        public void TestAddDefaultOntology()
        {
            Dictionary<string, OntologyAttribute> dict = CreateRealisticNamespaceDictionary();
            nsm = new NamespaceManager(dict);
            nsm[""] = new OntologyAttribute
            {
                BaseUri = "BaseUri4",
                GraphName = "GraphName4",
                Name = "Name4",
                Prefix = "Prefix4",
                UrlOfOntology = "UrlOfOntology4"
            };
            Assert.IsNotNull(nsm[""]);
            Assert.IsTrue(nsm[""] == nsm.Default);
        }
        [Test]
        public void TestAddDefaultOntologyViaDefaultProperty()
        {
            Dictionary<string, OntologyAttribute> dict = CreateRealisticNamespaceDictionary();
            nsm = new NamespaceManager(dict);
            nsm.Default = new OntologyAttribute
            {
                BaseUri = "BaseUri4",
                GraphName = "GraphName4",
                Name = "Name4",
                Prefix = "Prefix4",
                UrlOfOntology = "UrlOfOntology4"
            };
            Assert.IsNotNull(nsm[""]);
            Assert.IsTrue(nsm[""] == nsm.Default);
        }
        [Test]
        public void TestRemoveOntology()
        {
            Dictionary<string, OntologyAttribute> dict = CreateRealisticNamespaceDictionary();
            nsm = new NamespaceManager(dict);
            Assert.IsTrue(nsm.Count == 4);
            nsm.Remove("Name3");
            Assert.IsTrue(nsm.Count == 3);
            Assert.IsNull(nsm["Name3"]);
        }
        [Test]
        public void TestAddNonClashingOntology()
        {
            Dictionary<string, OntologyAttribute> dict = CreateRealisticNamespaceDictionary();
            nsm = new NamespaceManager(dict);
            nsm["mytest"] = new OntologyAttribute
            {
                BaseUri = "mytest",
                GraphName = "mytest",
                Name = "mytest",
                Prefix = "mytest",
                UrlOfOntology = "mytest"
            };
            Assert.IsNotNull(nsm["mytest"]);
            Assert.IsTrue(nsm.Count == 5);
        }
        [Test]
        public void TestAddClashingOntology()
        {
            Dictionary<string, OntologyAttribute> dict = CreateRealisticNamespaceDictionary();
            nsm = new NamespaceManager(dict);
            try
            {
                Assert.IsNotNull(nsm["Name1"]);
                Assert.IsTrue(nsm.Count == 4);
                nsm["Name1"] = new OntologyAttribute
                {
                    BaseUri = "BaseUri1",
                    GraphName = "GraphName1",
                    Name = "Name1",
                    Prefix = "Prefix1",
                    UrlOfOntology = "UrlOfOntology1"
                };
                Assert.Fail("Should have thrown an exception");
            }
            catch { }
            Assert.IsNotNull(nsm["Name1"]);
            Assert.IsTrue(nsm.Count == 4);
        }
        [Test]
        public void TestRenamePrefix()
        {
            Dictionary<string, OntologyAttribute> dict = CreateRealisticNamespaceDictionary();
            nsm = new NamespaceManager(dict);
            Assert.IsNotNull(nsm["Name1"]);
            Assert.IsTrue(nsm.Count == 4);
            Assert.IsTrue(nsm.Count == 4);
            nsm.Rename("Name1", "Name5");
            Assert.IsNull(nsm["Name1"]);
            Assert.IsNotNull(nsm["Name5"]);
            Assert.IsTrue(nsm.Count == 4);
        }
        [Test]
        public void TestQueryOntologyPrefix()
        {
            Dictionary<string, OntologyAttribute> dict = CreateRealisticNamespaceDictionary();
            nsm = new NamespaceManager(dict);
            Assert.IsTrue(nsm["Name1"].Prefix == "Prefix1");
        }
        [Test]
        public void TestQueryOntologyUri()
        {
            Dictionary<string, OntologyAttribute> dict = CreateRealisticNamespaceDictionary();
            nsm = new NamespaceManager(dict);
            Assert.IsTrue(nsm["Name1"].BaseUri == "BaseUri1");
        }
        [Test]
        public void TestDefaultNamespaceExists()
        {
            Dictionary<string, OntologyAttribute> dict = CreateRealisticNamespaceDictionary();
            nsm = new NamespaceManager(dict);
            Assert.IsTrue(nsm.HasDefault == false);
            nsm.Default = new OntologyAttribute
            {
                BaseUri = "BaseUri5",
                GraphName = "GraphName5",
                Name = "Name5",
                Prefix = "Prefix5",
                UrlOfOntology = "UrlOfOntology5"
            };
            Assert.IsTrue(nsm.HasDefault == true);
        }
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TestAddASecondDefaultNamespace()
        {
            Dictionary<string, OntologyAttribute> dict = CreateRealisticNamespaceDictionary();
            nsm = new NamespaceManager(dict);
            Assert.IsTrue(nsm.HasDefault == false);
            nsm.Default = new OntologyAttribute
            {
                BaseUri = "BaseUri5",
                GraphName = "GraphName5",
                Name = "Name5",
                Prefix = "Prefix5",
                UrlOfOntology = "UrlOfOntology5"
            };
            nsm.Default = new OntologyAttribute
            {
                BaseUri = "BaseUri6",
                GraphName = "GraphName6",
                Name = "Name6",
                Prefix = "Prefix6",
                UrlOfOntology = "UrlOfOntology6"
            };
        }
        [Test]
        public void TestRenameADefaultNamespace()
        {
            Dictionary<string, OntologyAttribute> dict = CreateRealisticNamespaceDictionary();
            nsm = new NamespaceManager(dict);
            Assert.IsTrue(nsm.HasDefault == false);
            nsm.Default = new OntologyAttribute
            {
                BaseUri = "BaseUri5",
                GraphName = "GraphName5",
                Name = "Name5",
                Prefix = "Prefix5",
                UrlOfOntology = "UrlOfOntology5"
            };
            Assert.IsTrue(nsm.HasDefault == true);
            nsm.Rename("", "Name5");
            Assert.IsTrue(nsm.HasDefault == false);
        }
        [Test]
        public void TestDeclareADefaultNamespace()
        {
            Dictionary<string, OntologyAttribute> dict = CreateRealisticNamespaceDictionary();
            nsm = new NamespaceManager(dict);
            Assert.IsTrue(nsm.HasDefault == false);
            nsm.MakeDefaultNamespace("Name1");
            Assert.IsTrue(nsm.HasDefault == true);
            Assert.IsTrue(nsm.Default.BaseUri == "BaseUri1");
        }
        [Test]
        public void TestDeclareADefaultNamespaceWithAnotherAlreadyExistsOverridingException()
        {
            Dictionary<string, OntologyAttribute> dict = CreateRealisticNamespaceDictionary();
            nsm = new NamespaceManager(dict);
            Assert.IsTrue(nsm.HasDefault == false);
            Assert.IsTrue(nsm.Count == 4);
            nsm.Default = new OntologyAttribute
            {
                BaseUri = "BaseUri5",
                GraphName = "GraphName5",
                Name = "Name5",
                Prefix = "Prefix5",
                UrlOfOntology = "UrlOfOntology5"
            };
            Assert.IsTrue(nsm.HasDefault == true);
            Assert.IsTrue(nsm.Count == 5);
            nsm.AddNewDefaultNamespace(new OntologyAttribute
            {
                BaseUri = "BaseUri5",
                GraphName = "GraphName5",
                Name = "Name5",
                Prefix = "Prefix5",
                UrlOfOntology = "UrlOfOntology5"
            }, "NewName");
            Assert.IsTrue(nsm.HasDefault == true);
            Assert.IsTrue(nsm.Count == 6);
            Assert.IsNotNull(nsm["NewName"]);
        }
    }
}
