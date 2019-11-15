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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Xsl;
using VDS.RDF.Query;

namespace VDS.RDF.Writing
{
    /// <summary>
    /// Class for generating arbitrary XML Output from SPARQL Result Sets by transforming the XML Results Format via an XSLT stylesheet
    /// </summary>
    public class SparqlXsltWriter : SparqlXmlWriter
    {
        private XslCompiledTransform _transform;

        /// <summary>
        /// Creates a new SPARQL XSLT Writer
        /// </summary>
        /// <param name="stylesheetUri">Stylesheet URI</param>
        public SparqlXsltWriter(Uri stylesheetUri)
            : this(stylesheetUri.AbsoluteUri) { }

        /// <summary>
        /// Creates a new SPARQL XSLT Writer
        /// </summary>
        /// <param name="stylesheetUri">Stylesheet URI</param>
        public SparqlXsltWriter(String stylesheetUri)
        {
            // Load the Transform
            _transform = new XslCompiledTransform();
            XsltSettings settings = new XsltSettings();
            _transform.Load(stylesheetUri, settings, null);
        }

        /// <summary>
        /// Saves a SPARQL Result Set to the given File
        /// </summary>
        /// <param name="results">Result Set</param>
        /// <param name="filename">File to save to</param>
        public override void Save(SparqlResultSet results, string filename)
        {
            Save(results, new StreamWriter(filename, false, new UTF8Encoding(Options.UseBomForUtf8)));
        }

        /// <summary>
        /// Saves a SPARQL Result Set to the given Text Writer
        /// </summary>
        /// <param name="results">Result Set</param>
        /// <param name="output">Text Writer to write to</param>
        public override void Save(SparqlResultSet results, TextWriter output)
        {
            try
            {
                // XmlDocument doc = this.GenerateOutput(results);
                StringBuilder temp = new StringBuilder();
                System.IO.StringWriter writer = new System.IO.StringWriter(temp);
                base.Save(results, writer);
                // this._transform.Transform(doc, null, XmlWriter.Create(output));
                _transform.Transform(XmlReader.Create(new StringReader(temp.ToString())), null, XmlWriter.Create(output));

                output.Close();
            }
            catch
            {
                try
                {
                    output.Close();
                }
                catch
                {
                    // No catch - just trying to clean up
                }
                throw;
            }
        }

        /// <summary>
        /// Gets the String representation of the writer which is a description of the syntax it produces
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "SPARQL Results XML transformed using XSLT";
        }
    }
}
