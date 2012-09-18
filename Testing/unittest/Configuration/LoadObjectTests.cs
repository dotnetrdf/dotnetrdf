using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Configuration;
using VDS.RDF.Query.PropertyFunctions;

namespace VDS.RDF.Test.Configuration
{
    [TestClass]
    public class LoadObjectTests
    {
        [TestMethod]
        public void ConfigurationLoadObjectPropertyFunctionFactory()
        {
            String graph = ConfigLookupTests.Prefixes + @"
_:a a dnr:SparqlPropertyFunctionFactory ;
  dnr:type """ + typeof(MockPropertyFunctionFactory).AssemblyQualifiedName + @""" .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            IPropertyFunctionFactory factory = ConfigurationLoader.LoadObject(g, g.GetBlankNode("a")) as IPropertyFunctionFactory;
            Assert.IsNotNull(factory);
            Assert.AreEqual(typeof(MockPropertyFunctionFactory), factory.GetType());
        }
    }

    class MockPropertyFunctionFactory
        : IPropertyFunctionFactory
    {
        public bool IsPropertyFunction(Uri u)
        {
            throw new NotImplementedException();
        }

        public bool TryCreatePropertyFunction(PropertyFunctionInfo info, out RDF.Query.Patterns.IPropertyFunctionPattern function)
        {
            throw new NotImplementedException();
        }
    }
}
