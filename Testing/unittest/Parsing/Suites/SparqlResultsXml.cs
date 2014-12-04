/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using VDS.RDF.Query;

namespace VDS.RDF.Parsing.Suites
{
   
    [TestFixture]
    public class SparqlResultsXml
        : BaseResultsParserSuite
    {
        public SparqlResultsXml()
            : base(new SparqlXmlParser(), new SparqlXmlParser(), "srx\\")
        {
            this.CheckResults = false;
        }

        [Test]
        public void ParsingSuiteSparqlResultsXml()
        {
            //Run manifests
            this.RunDirectory(f => Path.GetExtension(f).Equals(".srx") && !f.Contains("bad"), true);
            this.RunDirectory(f => Path.GetExtension(f).Equals(".srx") && f.Contains("bad"), false);

            if (this.Count == 0) Assert.Fail("No tests found");

            Console.WriteLine(this.Count + " Tests - " + this.Passed + " Passed - " + this.Failed + " Failed");
            Console.WriteLine((((double)this.Passed / (double)this.Count) * 100) + "% Passed");

            if (this.Failed > 0) Assert.Fail(this.Failed + " Tests failed");
            if (this.Indeterminate > 0) Assert.Inconclusive(this.Indeterminate + " Tests are indeterminate");
        }

        [Test]
        public void ParsingSparqlResultsXmlCustomAttributes()
        {
            // Test case based off of CORE-410
            SparqlResultSet results = new SparqlResultSet();
            this.ResultsParser.Load(results, @"resources\sparql\core-410.srx");

            TestTools.ShowResults(results);

            INode first = results[0]["test"];
            INode second = results[1]["test"];
            INode third = results[2]["test"];

            Assert.AreEqual(NodeType.Literal, first.NodeType);
            ILiteralNode firstLit = (ILiteralNode) first;
            Assert.IsNotNull(firstLit.DataType);
            Assert.AreEqual(XmlSpecsHelper.XmlSchemaDataTypeInteger, firstLit.DataType.AbsoluteUri);
            Assert.AreEqual("1993", firstLit.Value);

            Assert.AreEqual(NodeType.Literal, second.NodeType);
            ILiteralNode secondLit = (ILiteralNode) second;
            Assert.AreNotEqual(String.Empty, secondLit.Language);
            Assert.AreEqual("en", secondLit.Language);
            Assert.AreEqual("test", secondLit.Value);

            Assert.AreEqual(NodeType.Literal, third.NodeType);
            ILiteralNode thirdLit = (ILiteralNode) third;
            Assert.AreEqual(String.Empty, thirdLit.Language);
            Assert.IsNull(thirdLit.DataType);
            Assert.AreEqual("test plain literal", thirdLit.Value);
        }

        [Test, ExpectedException(typeof(RdfParseException))]
        public void ParsingSparqlResultsXmlConflictingAttributes()
        {
            // Test case based off of CORE-410
            SparqlResultSet results = new SparqlResultSet();
            this.ResultsParser.Load(results, @"resources\sparql\bad-core-410.srx");
        }
    }
}
