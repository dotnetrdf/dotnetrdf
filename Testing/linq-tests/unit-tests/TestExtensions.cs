using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using VDS.RDF.Linq;

namespace UnitTests
{
    /// <summary>
    /// Summary description for TestExtensions
    /// </summary>
    [TestFixture]
    public class TestExtensions
    {
        public TestExtensions()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        [Test]
        public void TestGetPrefix()
        {
            Assert.AreEqual("xsd", AttributeExtensions.GetOntologyPrefix("Data Types"));
        }
    }
}
