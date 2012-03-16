using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;

namespace VDS.RDF.Test.Parsing
{
    [TestClass]
    public class GZipTests
    {
        private IGraph _g;
        private List<String> _testFiles = new List<string>();

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
                    String filename = "gzip-tests." + def.CanonicalFileExtension + ".gz";

                    using (StreamWriter output = new StreamWriter(new GZipStream(new FileStream(filename, FileMode.Create, FileAccess.Write), CompressionMode.Compress)))
                    {
                        writer.Save(this._g, output);
                        output.Close();
                    }

                    this._testFiles.Add(filename);
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
        public void ParsingGZipByFilename()
        {
            foreach (String filename in this._testFiles)
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
    }
}
