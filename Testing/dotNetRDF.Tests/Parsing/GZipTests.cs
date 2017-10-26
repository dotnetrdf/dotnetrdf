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
using System.IO;
using System.IO.Compression;
using System.Linq;
using Xunit;
using VDS.RDF.Query;

namespace VDS.RDF.Parsing
{
    public partial class GZipTests
    {
        private IGraph _g;
        private SparqlResultSet _results;
        private List<String> _manualTestFiles = new List<String>();
        private List<String> _autoTestFiles = new List<String>();
        private List<String> _manualDatasetTestFiles = new List<String>();
        private List<String> _autoDatasetTestFiles = new List<String>();
        private List<String> _manualResultsTestFiles = new List<String>();
        private List<String> _autoResultsTestFiles = new List<String>();

        public GZipTests()
        {
            this._g = new Graph();
            this._g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            this._results = this._g.ExecuteQuery("SELECT * WHERE { ?s ?p ?o }") as SparqlResultSet;
            NormalizeDatatypes(_g);

            foreach (MimeTypeDefinition def in MimeTypesHelper.Definitions)
            {
                // Omit CSV since that is a lossy format that does not round trip
                if (def.CanonicalMimeType.Equals("text/csv")) continue;

                if (def.CanWriteRdf && def.CanParseRdf)
                {
                    IRdfWriter writer = def.GetRdfWriter();

                    bool isManual = !def.CanonicalFileExtension.EndsWith(".gz");
                    String filename = "gzip-tests" + (isManual ? String.Empty : "-auto") + "." + def.CanonicalFileExtension + (isManual ? ".gz" : String.Empty);

                    if (isManual)
                    {
                        using (StreamWriter output = new StreamWriter(new GZipStream(new FileStream(filename, FileMode.Create, FileAccess.Write), CompressionMode.Compress)))
                        {
                            writer.Save(this._g, output);
                            output.Close();
                        }

                        this._manualTestFiles.Add(filename);
                    }
                    else
                    {
                        writer.Save(this._g, filename);

                        this._autoTestFiles.Add(filename);
                    }
                }
                else if (def.CanParseRdfDatasets && def.CanWriteRdfDatasets)
                {
                    IStoreWriter writer = def.GetRdfDatasetWriter();

                    bool isManual = !def.CanonicalFileExtension.EndsWith(".gz");
                    String filename = "gzip-tests-datasets" + (isManual ? String.Empty : "-auto") + "." + def.CanonicalFileExtension + (isManual ? ".gz" : String.Empty);

                    TripleStore store = new TripleStore();
                    store.Add(this._g);

                    if (isManual)
                    {
                        using (Stream output = new GZipStream(new FileStream(filename, FileMode.Create, FileAccess.Write), CompressionMode.Compress))
                        {
                            writer.Save(store, new StreamWriter(output));
                            output.Close();
                        }

                        this._manualDatasetTestFiles.Add(filename);
                    }
                    else
                    {
                        writer.Save(store, new StreamWriter(new FileStream(filename, FileMode.Create, FileAccess.Write)));

                        this._autoDatasetTestFiles.Add(filename);
                    }
                }
                else if (def.CanParseSparqlResults && def.CanWriteSparqlResults)
                {
                    ISparqlResultsWriter writer = def.GetSparqlResultsWriter();

                    bool isManual = !def.CanonicalFileExtension.EndsWith(".gz");
                    String filename = "gzip-tests-results" + (isManual ? String.Empty : "-auto") + "." + def.CanonicalFileExtension + (isManual ? ".gz" : String.Empty);

                    if (isManual)
                    {
                        using (StreamWriter output = new StreamWriter(new GZipStream(new FileStream(filename, FileMode.Create, FileAccess.Write), CompressionMode.Compress)))
                        {
                            writer.Save(this._results, output);
                            output.Close();
                        }

                        this._manualResultsTestFiles.Add(filename);
                    }
                    else
                    {
                        writer.Save(this._results, new StreamWriter(File.OpenWrite(filename)));

                        this._autoResultsTestFiles.Add(filename);
                    }
                }
            }
        }

        [Fact]
        public void ParsingGZipExtensionDetectionNaive()
        {
            List<String> filenames = new List<String>()
            {
                "example.nt.gz",
                "example.ttl.gz",
                "example.n3.gz",
                "example.rdf.gz",
                "example.rj.gz"
            };

            foreach (String filename in filenames)
            {
                Console.WriteLine(filename + " => " + Path.GetExtension(filename));
                Assert.Equal(".gz", Path.GetExtension(filename));
            }
        }

        [Fact]
        public void ParsingGZipExtensionDetectionNonStackable()
        {
            List<String> filenames = new List<String>()
            {
                "example.nt.abc",
                "example.ttl.def",
                "example.n3.xyx",
                "example.rdf.rdf",
                "example.rj.ttl"
            };

            foreach (String filename in filenames)
            {
                Console.WriteLine(filename + " => " + MimeTypesHelper.GetTrueFileExtension(filename));
                Assert.Equal(Path.GetExtension(filename), MimeTypesHelper.GetTrueFileExtension(filename));
            }
        }

        [Fact]
        public void ParsingGZipExtensionDetectionTrue()
        {
            List<String> filenames = new List<String>()
            {
                "example.nt.gz",
                "example.ttl.gz",
                "example.n3.gz",
                "example.rdf.gz",
                "example.rj.gz",
                "example.gz",
                "example"
            };

            foreach (String filename in filenames)
            {
                String expectedExt = (filename.Contains('.') ? filename.Substring(filename.IndexOf('.')) : String.Empty);
                String realExt = MimeTypesHelper.GetTrueFileExtension(filename);
                Console.WriteLine(filename + " => " + realExt);

                Assert.Equal(expectedExt, realExt);
            }
        }

        [Fact]
        public void ParsingGZipDatasetByStreamManual()
        {
            foreach (String filename in this._manualDatasetTestFiles)
            {
                TripleStore store = new TripleStore();

                String ext = MimeTypesHelper.GetTrueFileExtension(filename);
                ext = ext.Substring(1);
                MimeTypeDefinition def = MimeTypesHelper.Definitions.Where(d => d.CanParseRdfDatasets && d.SupportsFileExtension(ext)).FirstOrDefault();
                Assert.NotNull(def);

                IStoreReader reader = def.GetRdfDatasetParser();
                reader.Load(store, new StreamReader(new FileStream(filename, FileMode.Open, FileAccess.Read)));

                NormalizeDatatypes(store.Graphs.First());
                Assert.Equal(this._g, store.Graphs.First());
            }
        }

        [Fact]
        public void ParsingGZipDatasetByGZipStreamManual()
        {
            foreach (String filename in this._manualDatasetTestFiles)
            {
                TripleStore store = new TripleStore();

                String ext = MimeTypesHelper.GetTrueFileExtension(filename);
                ext = ext.Substring(1);
                MimeTypeDefinition def = MimeTypesHelper.Definitions.Where(d => d.CanParseRdfDatasets && d.SupportsFileExtension(ext)).FirstOrDefault();
                Assert.NotNull(def);

                IStoreReader reader = def.GetRdfDatasetParser();
                reader.Load(store, new StreamReader(new GZipStream(new FileStream(filename, FileMode.Open, FileAccess.Read), CompressionMode.Decompress)));

                Assert.Equal(this._g, store.Graphs.First());
            }
        }

        [Fact]
        public void ParsingGZipDatasetByStreamAuto()
        {
            foreach (String filename in this._autoDatasetTestFiles)
            {
                TripleStore store = new TripleStore();

                String ext = MimeTypesHelper.GetTrueFileExtension(filename);
                ext = ext.Substring(1);
                MimeTypeDefinition def = MimeTypesHelper.Definitions.Where(d => d.CanParseRdfDatasets && d.SupportsFileExtension(ext)).FirstOrDefault();
                Assert.NotNull(def);

                IStoreReader reader = def.GetRdfDatasetParser();
                reader.Load(store, new StreamReader(new FileStream(filename, FileMode.Open, FileAccess.Read)));

                NormalizeDatatypes(store.Graphs.First());
                Assert.Equal(this._g, store.Graphs.First());
            }
        }

        [Fact]
        public void ParsingGZipDatasetByGZipStreamAuto()
        {
            foreach (String filename in this._autoDatasetTestFiles)
            {
                TripleStore store = new TripleStore();

                String ext = MimeTypesHelper.GetTrueFileExtension(filename);
                ext = ext.Substring(1);
                MimeTypeDefinition def = MimeTypesHelper.Definitions.Where(d => d.CanParseRdfDatasets && d.SupportsFileExtension(ext)).FirstOrDefault();
                Assert.NotNull(def);

                IStoreReader reader = def.GetRdfDatasetParser();
                reader.Load(store, new StreamReader(new GZipStream(new FileStream(filename, FileMode.Open, FileAccess.Read), CompressionMode.Decompress)));

                NormalizeDatatypes(store.Graphs.First());
                Assert.Equal(this._g, store.Graphs.First());
            }
        }

        [Fact]
        public void ParsingGZipResultsByFilenameManual()
        {
            foreach (String filename in this._manualResultsTestFiles)
            {
                SparqlResultSet results = new SparqlResultSet();

                String ext = MimeTypesHelper.GetTrueFileExtension(filename);
                ext = ext.Substring(1);
                MimeTypeDefinition def = MimeTypesHelper.Definitions.Where(d => d.CanParseSparqlResults && d.SupportsFileExtension(ext)).FirstOrDefault();
                Assert.NotNull(def);

                ISparqlResultsReader reader = def.GetSparqlResultsParser();
                reader.Load(results, filename);

                Assert.True(this._results.Equals(results), "Result Sets for file " + filename + " were not equal");
            }
        }

        [Fact]
        public void ParsingGZipResultsByStreamManual()
        {
            foreach (String filename in this._manualResultsTestFiles)
            {
                SparqlResultSet results = new SparqlResultSet();

                String ext = MimeTypesHelper.GetTrueFileExtension(filename);
                ext = ext.Substring(1);
                MimeTypeDefinition def = MimeTypesHelper.Definitions.Where(d => d.CanParseSparqlResults && d.SupportsFileExtension(ext)).FirstOrDefault();
                Assert.NotNull(def);

                ISparqlResultsReader reader = def.GetSparqlResultsParser();
                reader.Load(results, File.OpenText(filename));

                Assert.True(this._results.Equals(results), "Result Sets for file " + filename + " were not equal");
            }
        }

        [Fact]
        public void ParsingGZipResultsByGZipStreamManual()
        {
            foreach (String filename in this._manualResultsTestFiles)
            {
                SparqlResultSet results = new SparqlResultSet();

                String ext = MimeTypesHelper.GetTrueFileExtension(filename);
                ext = ext.Substring(1);
                MimeTypeDefinition def = MimeTypesHelper.Definitions.Where(d => d.CanParseSparqlResults && d.SupportsFileExtension(ext)).FirstOrDefault();
                Assert.NotNull(def);

                ISparqlResultsReader reader = def.GetSparqlResultsParser();
                reader.Load(results, new StreamReader(new GZipStream(new FileStream(filename, FileMode.Open, FileAccess.Read), CompressionMode.Decompress)));

                Assert.True(this._results.Equals(results), "Result Sets for file " + filename + " were not equal");
            }
        }

        [Fact]
        public void ParsingGZipResultsByFilenameAuto1()
        {
            foreach (String filename in this._autoResultsTestFiles)
            {
                SparqlResultSet results = new SparqlResultSet();

                String ext = MimeTypesHelper.GetTrueFileExtension(filename);
                ext = ext.Substring(1);
                MimeTypeDefinition def = MimeTypesHelper.Definitions.Where(d => d.CanParseSparqlResults && d.SupportsFileExtension(ext)).FirstOrDefault();
                Assert.NotNull(def);

                ISparqlResultsReader reader = def.GetSparqlResultsParser();
                reader.Load(results, filename);

                Assert.True(this._results.Equals(results), "Result Sets for file " + filename + " were not equal");
            }
        }

        [Fact]
        public void ParsingGZipResultsByFilenameAuto2()
        {
            foreach (String filename in this._autoResultsTestFiles)
            {
                SparqlResultSet results = new SparqlResultSet();

                ISparqlResultsReader reader = MimeTypesHelper.GetSparqlParserByFileExtension(MimeTypesHelper.GetTrueFileExtension(filename));
                reader.Load(results, filename);

                Assert.True(this._results.Equals(results), "Result Sets for file " + filename + " were not equal");
            }
        }

        [Fact]
        public void ParsingGZipResultsByStreamAuto()
        {
            foreach (String filename in this._autoResultsTestFiles)
            {
                SparqlResultSet results = new SparqlResultSet();

                String ext = MimeTypesHelper.GetTrueFileExtension(filename);
                ext = ext.Substring(1);
                MimeTypeDefinition def = MimeTypesHelper.Definitions.Where(d => d.CanParseSparqlResults && d.SupportsFileExtension(ext)).FirstOrDefault();
                Assert.NotNull(def);

                ISparqlResultsReader reader = def.GetSparqlResultsParser();
                reader.Load(results, File.OpenText(filename));

                Assert.True(this._results.Equals(results), "Result Sets for file " + filename + " were not equal");
            }
        }

        [Fact]
        public void ParsingGZipResultsByGZipStreamAuto()
        {
            foreach (String filename in this._autoResultsTestFiles)
            {
                SparqlResultSet results = new SparqlResultSet();

                String ext = MimeTypesHelper.GetTrueFileExtension(filename);
                ext = ext.Substring(1);
                MimeTypeDefinition def = MimeTypesHelper.Definitions.Where(d => d.CanParseSparqlResults && d.SupportsFileExtension(ext)).FirstOrDefault();
                Assert.NotNull(def);

                ISparqlResultsReader reader = def.GetSparqlResultsParser();
                reader.Load(results, new StreamReader(new GZipStream(new FileStream(filename, FileMode.Open, FileAccess.Read), CompressionMode.Decompress)));

                Assert.True(this._results.Equals(results), "Result Sets for file " + filename + " were not equal");
            }
        }

        private void NormalizeDatatypes(IGraph g)
        {
            var xsdString = new Uri(XmlSpecsHelper.XmlSchemaDataTypeString);
            foreach (var t in g.Triples.Where(t => t.Object is ILiteralNode).ToList())
            {
                var lit = (ILiteralNode) t.Object;
                if (lit.DataType == null && string.IsNullOrEmpty(lit.Language))
                {
                    // literal with no datatype should be xsd:string in the RDF 1.1 data model
                    g.Retract(t);
                    g.Assert(new Triple(t.Subject, t.Predicate, g.CreateLiteralNode(lit.Value, xsdString)));
                }
            }
        }
    }
}
