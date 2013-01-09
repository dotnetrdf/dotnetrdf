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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Writing;

namespace VDS.RDF.Parsing
{
    [TestClass]
    public class LangSpecTests
    {
        private IGraph _original;
        private TripleStore _store;

        private void EnsureTestData(String file)
        {
            if (this._original == null)
            {
                Graph g = new Graph();
                g.Assert(g.CreateBlankNode(), g.CreateUriNode(UriFactory.Create("http://example.org/predicate")), g.CreateLiteralNode("literal", "en-123"));
                g.Assert(g.CreateBlankNode(), g.CreateUriNode(UriFactory.Create("http://example.org/predicate")), g.CreateLiteralNode("literal", "en-gb-us"));
                g.Assert(g.CreateBlankNode(), g.CreateUriNode(UriFactory.Create("http://example.org/predicate")), g.CreateLiteralNode("literal", "en-123-abc"));
                this._original = g;

                this._store = new TripleStore();
                this._store.Add(this._original);
            }

            if (!File.Exists(file))
            {
                MimeTypeDefinition def = MimeTypesHelper.GetDefinitions(MimeTypesHelper.GetMimeTypes(Path.GetExtension(file))).FirstOrDefault();
                if (def != null)
                {
                    if (def.CanWriteRdf)
                    {
                        this._original.SaveToFile(file);
                    }
                    else if (def.CanWriteRdfDatasets)
                    {
                        this._store.SaveToFile(file);
                    }
                    else
                    {
                        Assert.Fail("Unable to ensure test data");
                    }
                }
                else
                {
                    Assert.Fail("Unsupported file type");
                }
            }
            else
            {
                Assert.Fail("Unable to ensure test data");
            }
        }

        private void TestLangSpecParsing(String file)
        {
            this.EnsureTestData(file);

            MimeTypeDefinition def = MimeTypesHelper.GetDefinitions(MimeTypesHelper.GetMimeTypes(Path.GetExtension(file))).FirstOrDefault();
            if (def != null)
            {
                if (def.CanParseRdf)
                {
                    Graph g = new Graph();
                    g.LoadFromFile(file);

                    Assert.AreEqual(this._original, g);
                }
                else if (def.CanParseRdfDatasets)
                {
                    TripleStore store = new TripleStore();
                    store.LoadFromFile(file);

                    Assert.AreEqual(this._original, store.Graphs.First());
                }
            }
            else
            {
                Assert.Fail("Unsupported file type");
            }
        }

        [TestMethod]
        public void ParsingLangSpecNTriples()
        {
            this.TestLangSpecParsing("langspec.nt");            
        }

        [TestMethod]
        public void ParsingLangSpecTurtle()
        {
            this.TestLangSpecParsing("langspec.ttl");
        }

        [TestMethod]
        public void ParsingLangSpecN3()
        {
            this.TestLangSpecParsing("langspec.n3");
        }

        [TestMethod]
        public void ParsingLangSpecTriG()
        {
            this.TestLangSpecParsing("langspec.trig");
        }

        [TestMethod]
        public void ParsingLangSpecNQuads()
        {
            this.TestLangSpecParsing("langspec.nq");
        }
    }
}
