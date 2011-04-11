using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;

namespace VDS.RDF.Test.Parsing
{
    [TestClass]
    public class BaseUriAssignmentTests
    {
        private String ShowBaseUri(Uri baseUri)
        {
            if (baseUri == null)
            {
                return "<NULL>";
            }
            else
            {
                return "<" + baseUri.ToString() + ">";
            }
        }

        [TestMethod]
        public void ParsingBaseUriAssignmentFileLoader()
        {
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            Console.WriteLine("Base URI: " + ShowBaseUri(g.BaseUri));
            Assert.IsNotNull(g.BaseUri, "Base URI should not be null");
        }

        [TestMethod]
        public void ParsingBaseUriAssignmentUriLoader()
        {
            Graph g = new Graph();
            UriLoader.Load(g, new Uri("http://dbpedia.org/resource/Ilkeston"));
            Console.WriteLine("Base URI: " + ShowBaseUri(g.BaseUri));
            Assert.IsNotNull(g.BaseUri, "Base URI should not be null");

        }


    }
}
