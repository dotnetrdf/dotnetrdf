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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace VDS.RDF.Parsing
{
    /// <summary>
    /// Static Helper Class which allows raw strings of RDF to be parsed directly
    /// </summary>
    /// <remarks>
    /// The API structure for dotNetRDF means that our <see cref="IRdfReader">IRdfReader</see> classes which are our Parsers only have to support parsing from a file or a stream.  For most applications this is fine but there may be occassions when you wish to parse a small fragment of RDF and you don't want to have to put it into a file before you can parse it.
    /// </remarks>
    public static class StringParser
    {
        /// <summary>
        /// Parses a raw RDF String using the given <see cref="IRdfReader">IRdfReader</see>
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="data">Raw RDF String</param>
        /// <param name="reader">Parser to use to read the data</param>
        /// <remarks>Use this when you have a raw RDF string and you know the syntax the RDF is in</remarks>
        public static void Parse(IGraph g, String data, IRdfReader reader)
        {
            try
            {
                MemoryStream mem = new MemoryStream();
                StreamWriter writer = new StreamWriter(mem);
                writer.Write(data);
                writer.Flush();
                mem.Seek(0, SeekOrigin.Begin);

                reader.Load(g, new StreamReader(mem));
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Parses a raw RDF String (attempts to auto-detect the format)
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="data">Raw RDF String</param>
        /// <remarks>
        /// <p>
        /// Auto-detection is based on testing the string to see if it contains certain keyword constructs which might indicate a particular syntax has been used.  This detection may not always be accurate and it may choose a parser which is less expressive than the actual syntax e.g. <see cref="TurtleParser">TurtleParser</see> instead of <see cref="Notation3Parser">Notation3Parser</see> as it tends to guess downwards.  
        /// </p>
        /// <p>
        /// For example if you parsed a Notation 3 string that contained Graph Literals but didn't use any of the Notation 3 specific directives like @keywords it would be assumed to be Turtle but then would fail to parse
        /// </p>
        /// <p>
        /// The auto-detection rules used are as follows:
        /// </p>
        /// <ol>
        /// <li>If it contains &lt;?xml and &lt;rdf:RDF then it's most likely RDF/XML</li>
        /// <li>If it contains &lt;html then it's most likely HTML with possibly RDFa embedded</li>
        /// <li>
        /// If it contains @prefix or @base then its Turtle/Notation 3
        ///     <ol>
        ///     <li>If it contains @keywords, @forall or @forsome then it's Notation 3</li>
        ///     <li>Otherwise it's Turtle</li>
        ///     </ol>
        /// </li>
        /// <li>If it contains all of a set of terms and symbols that occur in RDF/Json then it's most likely RDF/Json.  These terms are "value","type",{,},[ and ]</li>
        /// <li>Otherwise try it as NTriples, NTriples has no real distinctive syntax so hard to test if it's NTriples</li>
        /// </ol>
        /// </remarks>
        public static void Parse(IGraph g, String data)
        {
            //Try to guess the format
            String format = "Unknown";
            try
            {
                if (data.Contains("<?xml") && data.Contains("<rdf:RDF"))
                {
                    //Probably RDF/XML
                    format = "RDF/XML";
                    Parse(g, data, new RdfXmlParser());
                }
                else if (data.Contains("<html"))
                {
                    //HTML (possibly containing RDFa)
                    format = "HTML+RDFa";
                    Parse(g, data, new RdfAParser());
                }
                else if (data.Contains("@prefix") || data.Contains("@base"))
                {
                    //Turtle/Notation 3
                    if (data.Contains("@keywords") || data.Contains("@forall") || data.Contains("@forsome"))
                    {
                        //Notation 3
                        format = "Notation 3";
                        Parse(g, data, new Notation3Parser());
                    }
                    else
                    {
                        //Probably Turtle
                        format = "Turtle";
                        Parse(g, data, new TurtleParser());
                    }
                }
                else if (data.Contains("\"value\"") &&
                           data.Contains("\"type\"") &&
                           data.Contains("{") &&
                           data.Contains("}") &&
                           data.Contains("[") &&
                           data.Contains("]"))
                {
                    //If we have all those things then it's very likely RDF/Json
                    format = "RDF/JSON";
                    Parse(g, data, new RdfJsonParser());
                }
                else
                {
                    //Take a stab at it being NTriples
                    //No real way to test as there's nothing particularly distinctive in NTriples
                    format = "NTriples";
                    Parse(g, data, new NTriplesParser());
                }
            }
            catch (RdfParseException parseEx)
            {
                //Wrap the exception in an informational exception about what we guessed
                throw new RdfParseException("StringParser failed to parse the RDF string correctly, StringParser auto-detection guessed '" + format + "' but this failed to parse.  RDF string may be malformed or StringParser may have guessed incorrectly", parseEx);
            }
        }
    }
}
