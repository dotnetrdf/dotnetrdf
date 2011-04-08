using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;

namespace VDS.RDF.Test.Parsing
{
    [TestClass]
    public class RdfXmlNamespaces
    {
        [TestMethod]
        public void ParsingRdfXmlNamespaces()
        {
            Graph g = new Graph();
            try
            {
                g.LoadFromFile("rdfxml-namespaces.rdf");
                Assert.Fail("Parsing should fail as namespaces are not properly defined in the RDF/XML");
            }
            catch (RdfParseException parseEx)
            {
                Console.WriteLine("Parser Error thrown as expected");
                Assert.IsTrue(parseEx.HasPositionInformation, "Should have position information");
                Console.WriteLine("Line " + parseEx.StartLine + " Column " + parseEx.StartPosition);

                TestTools.ReportError("Parser Error", parseEx, false);
            }
        }

    }
}
