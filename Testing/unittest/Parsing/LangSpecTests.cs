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
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Writing;

namespace VDS.RDF.Parsing
{

    public class LangSpecTests
    {
        private const string NTriplesLangSpec = "resources\\langspec.nt";
        private const string TurtleLangSpec = "resources\\langspec.ttl";
        private const string N3LangSpec = "resources\\langspec.n3";
        private const string TrigLangSpec = "resources\\langspec.trig";
        private const string NQuadsLangSpec = "resources\\langspec.nq";

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

            Assert.True(File.Exists(file), "Unable to ensure test data");
            if (!File.Exists(file))
            {
                MimeTypeDefinition def = MimeTypesHelper.GetDefinitions(MimeTypesHelper.GetMimeTypes(Path.GetExtension(file))).FirstOrDefault();
                Assert.NotNull(def);
                if (def != null)
                {
                    Assert.True(def.CanWriteRdf || def.CanWriteRdfDatasets, "Unable to ensure test data");
                    if (def.CanWriteRdf)
                    {
                        this._original.SaveToFile(file);
                    }
                    else if (def.CanWriteRdfDatasets)
                    {
                        this._store.SaveToFile(file);
                    }
                }
            }
        }

        [Theory]
        [InlineData(NTriplesLangSpec)]
        [InlineData(TurtleLangSpec)]
        [InlineData(N3LangSpec)]
        [InlineData(TrigLangSpec)]
        [InlineData(NQuadsLangSpec)]
        public void TestLangSpecParsing(String file)
        {
            this.EnsureTestData(file);

            MimeTypeDefinition def = MimeTypesHelper.GetDefinitions(MimeTypesHelper.GetMimeTypes(Path.GetExtension(file))).FirstOrDefault();
            Assert.NotNull(def);
            if (def != null)
            {
                if (def.CanParseRdf)
                {
                    Graph g = new Graph();
                    g.LoadFromFile(file);

                    Assert.Equal(this._original, g);
                }
                else if (def.CanParseRdfDatasets)
                {
                    TripleStore store = new TripleStore();
                    store.LoadFromFile(file);

                    Assert.Equal(this._original, store.Graphs.First());
                }
            }
        }

        public LangSpecTests()
        {
            DeleteLangSpec(NTriplesLangSpec);
            DeleteLangSpec(TurtleLangSpec);
            DeleteLangSpec(N3LangSpec);
            DeleteLangSpec(TrigLangSpec);
            DeleteLangSpec(NQuadsLangSpec);
        }

        private static void DeleteLangSpec(string langSpecFile)
        {
            if (File.Exists(langSpecFile))
            {
                File.Delete(langSpecFile);
            }
        }
    }
}
