/*

Copyright Robert Vesse 2009-10
rvesse@vdesign-studios.com

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

#if !NO_XMLDOM && !NO_XSL

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
            : this(stylesheetUri.ToString()) { }

        /// <summary>
        /// Creates a new SPARQL XSLT Writer
        /// </summary>
        /// <param name="stylesheetUri">Stylesheet URI</param>
        public SparqlXsltWriter(String stylesheetUri)
        {
            //Load the Transform
            this._transform = new XslCompiledTransform();
            XsltSettings settings = new XsltSettings();
            this._transform.Load(stylesheetUri, settings, null);
        }

        /// <summary>
        /// Saves a SPARQL Result Set to the given File
        /// </summary>
        /// <param name="results">Result Set</param>
        /// <param name="filename">File to save to</param>
        public override void Save(SparqlResultSet results, string filename)
        {
            this.Save(results, new StreamWriter(filename, false, new UTF8Encoding(Options.UseBomForUtf8)));
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
                //XmlDocument doc = this.GenerateOutput(results);
                StringBuilder temp = new StringBuilder();
                System.IO.StringWriter writer = new System.IO.StringWriter(temp);
                base.Save(results, writer);
                //this._transform.Transform(doc, null, XmlWriter.Create(output));
                this._transform.Transform(XmlReader.Create(new StringReader(temp.ToString())), null, XmlWriter.Create(output));

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
                    //No catch - just trying to clean up
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

#endif