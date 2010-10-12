using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;

namespace branch_tests
{
    [TestClass]
    public class VariableNodeTests
    {
        [TestMethod]
        public void N3VariableParsing()
        {
            String TestFragment = "@prefix rdfs: <" + NamespaceMapper.RDFS + ">. { ?s a ?type } => { ?s rdfs:label \"This has a type\" } .";
            Notation3Parser parser = new Notation3Parser();
            Graph g = new Graph();
            StringParser.Parse(g, TestFragment, parser);

            foreach (Triple t in g.Triples)
            {
                Console.WriteLine(t.ToString());
            }

            StringWriter.Write(g, new Notation3Writer());
        }
    }
}
