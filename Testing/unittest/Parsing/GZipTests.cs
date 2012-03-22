using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Storage.Params;

namespace VDS.RDF.Test.Parsing
{
    [TestClass]
    public class GZipTests
    {
        private IGraph _g;
        private List<String> _manualTestFiles = new List<String>();
        private List<String> _autoTestFiles = new List<String>();
        private List<String> _manualDatasetTestFiles = new List<String>();
        private List<String> _autoDatasetTestFiles = new List<String>();

        [TestInitialize]
        public void Setup()
        {
            this._g = new Graph();
            this._g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");

            foreach (MimeTypeDefinition def in MimeTypesHelper.Definitions)
            {
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
                            writer.Save(store, new StreamParams(output));
                            output.Close();
                        }

                        this._manualDatasetTestFiles.Add(filename);
                    }
                    else
                    {
                        writer.Save(store, new StreamParams(new FileStream(filename, FileMode.Create, FileAccess.Write)));

                        this._autoDatasetTestFiles.Add(filename);
                    }
                }
            }
        }

        [TestMethod]
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
                Assert.AreEqual(".gz", Path.GetExtension(filename));
            }
        }

        [TestMethod]
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
                Assert.AreEqual(Path.GetExtension(filename), MimeTypesHelper.GetTrueFileExtension(filename));
            }
        }

        [TestMethod]
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

                Assert.AreEqual(expectedExt, realExt);
            }
        }

        [TestMethod]
        public void ParsingGZipByFilenameManual()
        {
            foreach (String filename in this._manualTestFiles)
            {
                Graph g = new Graph();

                String ext = MimeTypesHelper.GetTrueFileExtension(filename);
                ext = ext.Substring(1);
                MimeTypeDefinition def = MimeTypesHelper.Definitions.Where(d => d.CanParseRdf && d.FileExtensions.Contains(ext)).FirstOrDefault();
                if (def == null) Assert.Fail("Failed to find MIME Type Definition for File Extension ." + ext);

                IRdfReader reader = def.GetRdfParser();
                reader.Load(g, filename);

                Assert.AreEqual(this._g, g, "Graphs for file " + filename + " were not equal");
            }
        }

        [TestMethod]
        public void ParsingGZipByStreamManual()
        {
            foreach (String filename in this._manualTestFiles)
            {
                Graph g = new Graph();

                String ext = MimeTypesHelper.GetTrueFileExtension(filename);
                ext = ext.Substring(1);
                MimeTypeDefinition def = MimeTypesHelper.Definitions.Where(d => d.CanParseRdf && d.FileExtensions.Contains(ext)).FirstOrDefault();
                if (def == null) Assert.Fail("Failed to find MIME Type Definition for File Extension ." + ext);

                IRdfReader reader = def.GetRdfParser();
                reader.Load(g, new StreamReader(filename));

                Assert.AreEqual(this._g, g, "Graphs for file " + filename + " were not equal");
            }
        }

        [TestMethod]
        public void ParsingGZipByGZipStreamManual()
        {
            foreach (String filename in this._manualTestFiles)
            {
                Graph g = new Graph();

                String ext = MimeTypesHelper.GetTrueFileExtension(filename);
                ext = ext.Substring(1);
                MimeTypeDefinition def = MimeTypesHelper.Definitions.Where(d => d.CanParseRdf && d.FileExtensions.Contains(ext)).FirstOrDefault();
                if (def == null) Assert.Fail("Failed to find MIME Type Definition for File Extension ." + ext);

                IRdfReader reader = def.GetRdfParser();
                reader.Load(g, new StreamReader(new GZipStream(new FileStream(filename, FileMode.Open, FileAccess.Read), CompressionMode.Decompress)));

                Assert.AreEqual(this._g, g, "Graphs for file " + filename + " were not equal");
            }
        }

        [TestMethod]
        public void ParsingGZipByFilenameAuto()
        {
            foreach (String filename in this._autoTestFiles)
            {
                Graph g = new Graph();

                String ext = MimeTypesHelper.GetTrueFileExtension(filename);
                ext = ext.Substring(1);
                MimeTypeDefinition def = MimeTypesHelper.Definitions.Where(d => d.CanParseRdf && d.FileExtensions.Contains(ext)).FirstOrDefault();
                if (def == null) Assert.Fail("Failed to find MIME Type Definition for File Extension ." + ext);

                IRdfReader reader = def.GetRdfParser();
                reader.Load(g, filename);

                Assert.AreEqual(this._g, g, "Graphs for file " + filename + " were not equal");
            }
        }

        [TestMethod]
        public void ParsingGZipByStreamAuto()
        {
            foreach (String filename in this._autoTestFiles)
            {
                Graph g = new Graph();

                String ext = MimeTypesHelper.GetTrueFileExtension(filename);
                ext = ext.Substring(1);
                MimeTypeDefinition def = MimeTypesHelper.Definitions.Where(d => d.CanParseRdf && d.FileExtensions.Contains(ext)).FirstOrDefault();
                if (def == null) Assert.Fail("Failed to find MIME Type Definition for File Extension ." + ext);

                IRdfReader reader = def.GetRdfParser();
                reader.Load(g, new StreamReader(filename));

                Assert.AreEqual(this._g, g, "Graphs for file " + filename + " were not equal");
            }
        }

        [TestMethod]
        public void ParsingGZipByGZipStreamAuto()
        {
            foreach (String filename in this._autoTestFiles)
            {
                Graph g = new Graph();

                String ext = MimeTypesHelper.GetTrueFileExtension(filename);
                ext = ext.Substring(1);
                MimeTypeDefinition def = MimeTypesHelper.Definitions.Where(d => d.CanParseRdf && d.FileExtensions.Contains(ext)).FirstOrDefault();
                if (def == null) Assert.Fail("Failed to find MIME Type Definition for File Extension ." + ext);

                IRdfReader reader = def.GetRdfParser();
                reader.Load(g, new StreamReader(new GZipStream(new FileStream(filename, FileMode.Open, FileAccess.Read), CompressionMode.Decompress)));

                Assert.AreEqual(this._g, g, "Graphs for file " + filename + " were not equal");
            }
        }

        [TestMethod]
        public void ParsingGZipDatasetByStreamManual()
        {
            foreach (String filename in this._manualDatasetTestFiles)
            {
                TripleStore store = new TripleStore();

                String ext = MimeTypesHelper.GetTrueFileExtension(filename);
                ext = ext.Substring(1);
                MimeTypeDefinition def = MimeTypesHelper.Definitions.Where(d => d.CanParseRdfDatasets && d.FileExtensions.Contains(ext)).FirstOrDefault();
                if (def == null) Assert.Fail("Failed to find MIME Type Definition for File Extension ." + ext);

                IStoreReader reader = def.GetRdfDatasetParser();
                reader.Load(store, new StreamParams(new FileStream(filename, FileMode.Open, FileAccess.Read)));

                Assert.AreEqual(this._g, store.Graphs.First(), "Graphs for file " + filename + " were not equal");
            }
        }

        [TestMethod]
        public void ParsingGZipDatasetByGZipStreamManual()
        {
            foreach (String filename in this._manualDatasetTestFiles)
            {
                TripleStore store = new TripleStore();

                String ext = MimeTypesHelper.GetTrueFileExtension(filename);
                ext = ext.Substring(1);
                MimeTypeDefinition def = MimeTypesHelper.Definitions.Where(d => d.CanParseRdfDatasets && d.FileExtensions.Contains(ext)).FirstOrDefault();
                if (def == null) Assert.Fail("Failed to find MIME Type Definition for File Extension ." + ext);

                IStoreReader reader = def.GetRdfDatasetParser();
                reader.Load(store, new StreamParams(new GZipStream(new FileStream(filename, FileMode.Open, FileAccess.Read), CompressionMode.Decompress)));

                Assert.AreEqual(this._g, store.Graphs.First(), "Graphs for file " + filename + " were not equal");
            }
        }

        [TestMethod]
        public void ParsingGZipDatasetByStreamAuto()
        {
            foreach (String filename in this._autoDatasetTestFiles)
            {
                TripleStore store = new TripleStore();

                String ext = MimeTypesHelper.GetTrueFileExtension(filename);
                ext = ext.Substring(1);
                MimeTypeDefinition def = MimeTypesHelper.Definitions.Where(d => d.CanParseRdfDatasets && d.FileExtensions.Contains(ext)).FirstOrDefault();
                if (def == null) Assert.Fail("Failed to find MIME Type Definition for File Extension ." + ext);

                IStoreReader reader = def.GetRdfDatasetParser();
                reader.Load(store, new StreamParams(new FileStream(filename, FileMode.Open, FileAccess.Read)));

                Assert.AreEqual(this._g, store.Graphs.First(), "Graphs for file " + filename + " were not equal");
            }
        }

        [TestMethod]
        public void ParsingGZipDatasetByGZipStreamAuto()
        {
            foreach (String filename in this._autoDatasetTestFiles)
            {
                TripleStore store = new TripleStore();

                String ext = MimeTypesHelper.GetTrueFileExtension(filename);
                ext = ext.Substring(1);
                MimeTypeDefinition def = MimeTypesHelper.Definitions.Where(d => d.CanParseRdfDatasets && d.FileExtensions.Contains(ext)).FirstOrDefault();
                if (def == null) Assert.Fail("Failed to find MIME Type Definition for File Extension ." + ext);

                IStoreReader reader = def.GetRdfDatasetParser();
                reader.Load(store, new StreamParams(new GZipStream(new FileStream(filename, FileMode.Open, FileAccess.Read), CompressionMode.Decompress)));

                Assert.AreEqual(this._g, store.Graphs.First(), "Graphs for file " + filename + " were not equal");
            }
        }
    }
}
