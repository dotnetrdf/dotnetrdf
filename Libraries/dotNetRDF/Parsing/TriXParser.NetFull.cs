/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Storage;
using VDS.RDF.Writing;

namespace VDS.RDF.Parsing
{
    public partial class TriXParser
    {
        private static XmlReaderSettings GetSettings()
        {
            return new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Parse,
                ConformanceLevel = ConformanceLevel.Document,
                IgnoreComments = true,
                IgnoreProcessingInstructions = false,
                IgnoreWhitespace = true,
            };
        }

        /// <inheritdoc />
        public void Load(IRdfHandler handler, TextReader input)
        {
            if (handler == null)
            {
                throw new RdfParseException("Cannot parse an RDF Dataset using a null handler");
            }

            if (input == null)
            {
                throw new RdfParseException("Cannot parse an RDF Dataset from a null input");
            }

            try { 
                // Load source XML
                using (var xmlReader = XmlReader.Create(input, GetSettings()))
                {
                    var source = XDocument.Load(xmlReader);
                    foreach (var pi in source.Nodes().OfType<XProcessingInstruction>().Where(pi=>pi.Target.Equals("xml-stylesheet")))
                    {
                        source = ApplyTransform(source, pi);
                    }
                    using(var transformedXmlReader = source.CreateReader())
                    {
                        TryParseGraphset(transformedXmlReader, handler);
                    }
                }
            }
            finally
            {
        input.Close();
            }
        }

        private XDocument ApplyTransform(XDocument input, XProcessingInstruction pi)
        {
            var match = Regex.Match(pi.Data, @"href\s*=\s*""([^""]*)""");
            if (match == null) throw new RdfParseException("Expected href value in xml-stylesheet PI");
            var xslRef = match.Groups[1].Value;
            var xslt = new XslCompiledTransform();
            var xmlStringBuilder = new StringBuilder();
            xslt.Load(XmlReader.Create(new StreamReader(xslRef), GetSettings()));
            var output = new XDocument();
            using (var writer = output.CreateWriter())
            {
                xslt.Transform(input.CreateReader(), writer);
            }
            return output;
        }
    }
}