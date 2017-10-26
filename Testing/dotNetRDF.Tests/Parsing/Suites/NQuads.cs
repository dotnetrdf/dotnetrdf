using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.XunitExtensions;
using Xunit;

namespace VDS.RDF.Parsing.Suites
{

    public class NQuads
        : BaseDatasetParserSuite
    {
        public NQuads()
            : base(new NQuadsParser(), new NQuadsParser(), @"nquads11\")
        {
            this.CheckResults = false;
            this.Parser.Warning += TestTools.WarningPrinter;
        }

        [SkippableFact]
        public void ParsingSuitesNQuads11()
        {
            //Nodes for positive and negative tests
            Graph g = new Graph();
            g.NamespaceMap.AddNamespace("rdft", UriFactory.Create("http://www.w3.org/ns/rdftest#"));
            INode posSyntaxTest = g.CreateUriNode("rdft:TestNQuadsPositiveSyntax");
            INode negSyntaxTest = g.CreateUriNode("rdft:TestNQuadsNegativeSyntax");

            //Run manifests
            this.RunManifest(@"..\\resources\nquads11\manifest.ttl", posSyntaxTest, negSyntaxTest);

            if (this.Count == 0) Assert.True(false, "No tests found");

            Console.WriteLine(this.Count + " Tests - " + this.Passed + " Passed - " + this.Failed + " Failed");
            Console.WriteLine((((double)this.Passed / (double)this.Count) * 100) + "% Passed");

            if (this.Failed > 0) Assert.True(false, this.Failed + " Tests failed");
            if (this.Indeterminate > 0) throw new SkipTestException(Indeterminate + " Tests are indeterminate");
        }
    }
}
