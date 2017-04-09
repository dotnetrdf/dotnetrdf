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
        [Fact]
        public void ParsingGZipByFilenameManual1()
        {
            foreach (String filename in this._manualTestFiles)
            {
                Graph g = new Graph();

                String ext = MimeTypesHelper.GetTrueFileExtension(filename);
                ext = ext.Substring(1);
                MimeTypeDefinition def = MimeTypesHelper.Definitions.Where(d => d.CanParseRdf && d.SupportsFileExtension(ext)).FirstOrDefault();
                Assert.NotNull(def);

                IRdfReader reader = def.GetRdfParser();
                reader.Load(g, filename);

                Assert.Equal(this._g, g);
            }
        }

        [Fact]
        public void ParsingGZipByFilenameManual2()
        {
            foreach (String filename in this._manualTestFiles)
            {
                Graph g = new Graph();

                IRdfReader reader = MimeTypesHelper.GetParserByFileExtension(MimeTypesHelper.GetTrueFileExtension(filename));
                reader.Load(g, filename);

                Assert.Equal(this._g, g);
            }
        }

        [Fact]
        public void ParsingGZipByFilenameManual3()
        {
            foreach (String filename in this._manualTestFiles)
            {
                Graph g = new Graph();
                g.LoadFromFile(filename);

                Assert.Equal(this._g, g);
            }
        }

        [Fact]
        public void ParsingGZipByStreamManual()
        {
            foreach (String filename in this._manualTestFiles)
            {
                Graph g = new Graph();

                String ext = MimeTypesHelper.GetTrueFileExtension(filename);
                ext = ext.Substring(1);
                MimeTypeDefinition def = MimeTypesHelper.Definitions.Where(d => d.CanParseRdf && d.SupportsFileExtension(ext)).FirstOrDefault();
                Assert.NotNull(def);

                IRdfReader reader = def.GetRdfParser();
                reader.Load(g, File.OpenText(filename));

                Assert.Equal(this._g, g);
            }
        }

        [Fact]
        public void ParsingGZipByGZipStreamManual()
        {
            foreach (String filename in this._manualTestFiles)
            {
                Graph g = new Graph();

                String ext = MimeTypesHelper.GetTrueFileExtension(filename);
                ext = ext.Substring(1);
                MimeTypeDefinition def = MimeTypesHelper.Definitions.Where(d => d.CanParseRdf && d.SupportsFileExtension(ext)).FirstOrDefault();
                Assert.NotNull(def);

                IRdfReader reader = def.GetRdfParser();
                reader.Load(g, new StreamReader(new GZipStream(new FileStream(filename, FileMode.Open, FileAccess.Read), CompressionMode.Decompress)));

                Assert.Equal(this._g, g);
            }
        }

        [Fact]
        public void ParsingGZipByFilenameAuto1()
        {
            foreach (String filename in this._autoTestFiles)
            {
                Graph g = new Graph();

                String ext = MimeTypesHelper.GetTrueFileExtension(filename);
                ext = ext.Substring(1);
                MimeTypeDefinition def = MimeTypesHelper.Definitions.Where(d => d.CanParseRdf && d.SupportsFileExtension(ext)).FirstOrDefault();
                Assert.NotNull(def);

                IRdfReader reader = def.GetRdfParser();
                reader.Load(g, filename);

                Assert.Equal(this._g, g);
            }
        }

        [Fact]
        public void ParsingGZipByFilenameAuto2()
        {
            foreach (String filename in this._autoTestFiles)
            {
                Graph g = new Graph();

                IRdfReader reader = MimeTypesHelper.GetParserByFileExtension(MimeTypesHelper.GetTrueFileExtension(filename));
                reader.Load(g, filename);

                Assert.Equal(this._g, g);
            }
        }

        [Fact]
        public void ParsingGZipByFilenameAuto3()
        {
            foreach (String filename in this._autoTestFiles)
            {
                Graph g = new Graph();
                g.LoadFromFile(filename);

                Assert.Equal(this._g, g);
            }
        }

        [Fact]
        public void ParsingGZipByStreamAuto()
        {
            foreach (String filename in this._autoTestFiles)
            {
                Graph g = new Graph();

                String ext = MimeTypesHelper.GetTrueFileExtension(filename);
                ext = ext.Substring(1);
                MimeTypeDefinition def = MimeTypesHelper.Definitions.Where(d => d.CanParseRdf && d.SupportsFileExtension(ext)).FirstOrDefault();
                Assert.NotNull(def);

                IRdfReader reader = def.GetRdfParser();
                reader.Load(g, File.OpenText(filename));

                Assert.Equal(this._g, g);
            }
        }

        [Fact]
        public void ParsingGZipByGZipStreamAuto()
        {
            foreach (String filename in this._autoTestFiles)
            {
                Graph g = new Graph();

                String ext = MimeTypesHelper.GetTrueFileExtension(filename);
                ext = ext.Substring(1);
                MimeTypeDefinition def = MimeTypesHelper.Definitions.Where(d => d.CanParseRdf && d.SupportsFileExtension(ext)).FirstOrDefault();
                Assert.NotNull(def);

                IRdfReader reader = def.GetRdfParser();
                reader.Load(g, new StreamReader(new GZipStream(new FileStream(filename, FileMode.Open, FileAccess.Read), CompressionMode.Decompress)));

                Assert.Equal(this._g, g);
            }
        }
    }
}
