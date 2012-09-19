using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Configuration;

namespace VDS.RDF.Test.Configuration
{
    [TestClass]
    public class DefaultTypeTests
    {
        private void TestDefaultType(String typeUri, String expectedType)
        {
            String actualType = ConfigurationLoader.GetDefaultType(typeUri);
            Assert.AreEqual(expectedType, actualType);
        }

        [TestMethod]
        public void ConfigurationDefaultTypeGraph()
        {
            this.TestDefaultType(ConfigurationLoader.ClassGraph, ConfigurationLoader.DefaultTypeGraph);
        }

        [TestMethod]
        public void ConfigurationDefaultTypeGraphCollection()
        {
            this.TestDefaultType(ConfigurationLoader.ClassGraphCollection, ConfigurationLoader.DefaultTypeGraphCollection);
        }

        [TestMethod]
        public void ConfigurationDefaultTypeSparqlHttpProtocolProcessor()
        {
            this.TestDefaultType(ConfigurationLoader.ClassSparqlHttpProtocolProcessor, ConfigurationLoader.DefaultTypeSparqlHttpProtocolProcessor);
        }

        [TestMethod]
        public void ConfigurationDefaultTypeSparqlQueryProcessor()
        {
            this.TestDefaultType(ConfigurationLoader.ClassSparqlQueryProcessor, ConfigurationLoader.DefaultTypeSparqlQueryProcessor);
        }

        [TestMethod]
        public void ConfigurationDefaultTypeSparqlUpdateProcessor()
        {
            this.TestDefaultType(ConfigurationLoader.ClassSparqlUpdateProcessor, ConfigurationLoader.DefaultTypeSparqlUpdateProcessor);
        }

        [TestMethod]
        public void ConfigurationDefaultTypeTripleCollection()
        {
            this.TestDefaultType(ConfigurationLoader.ClassTripleCollection, ConfigurationLoader.DefaultTypeTripleCollection);
        }

        [TestMethod]
        public void ConfigurationDefaultTypeTripleStore()
        {
            this.TestDefaultType(ConfigurationLoader.ClassTripleStore, ConfigurationLoader.DefaultTypeTripleStore);
        }

        [TestMethod]
        public void ConfigurationDefaultTypeUser()
        {
            this.TestDefaultType(ConfigurationLoader.ClassUser, typeof(System.Net.NetworkCredential).AssemblyQualifiedName);
        }

        [TestMethod]
        public void ConfigurationDefaultTypeUserGroup()
        {
            this.TestDefaultType(ConfigurationLoader.ClassUserGroup, ConfigurationLoader.DefaultTypeUserGroup);
        }

        [TestMethod]
        public void ConfigurationDefaultTypeProxy()
        {
            this.TestDefaultType(ConfigurationLoader.ClassProxy, typeof(System.Net.WebProxy).AssemblyQualifiedName);
        }
    }
}
