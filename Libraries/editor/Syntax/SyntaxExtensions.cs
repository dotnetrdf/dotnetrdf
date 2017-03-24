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
using VDS.RDF.Parsing;

namespace VDS.RDF.Utilities.Editor.Syntax
{
    /// <summary>
    /// Static Helper class with useful syntax extensions
    /// </summary>
    public static class SyntaxExtensions
    {
        /// <summary>
        /// Convert friendly display names for syntax into recognized internal syntax name
        /// </summary>
        /// <param name="name">Friendly Name</param>
        /// <returns>Internal Syntax Name</returns>
        public static String GetSyntaxName(this String name)
        {
            switch (name)
            {
                case "RDF/JSON":
                    return "RdfJson";

                case "RDF/XML":
                    return "RdfXml";

                case "HTML":
                    return "XHtmlRdfA";

                case "Notation 3":
                    return "Notation3";

                case "SPARQL Results XML":
                    return "SparqlResultsXml";

                case "SPARQL Results JSON":
                    return "SparqlResultsJson";

                case "SPARQL Query":
                    return "SparqlQuery11";

                case "SPARQL Update":
                    return "SparqlUpdate11";

                case "NTriples":
                case "NQuads":
                case "Turtle":
                case "TriG":
                case "TriX":
                default:
                    return name;
            }
        }

        /// <summary>
        /// Convert SPARQL results parser into internal syntax name
        /// </summary>
        /// <param name="parser">SPARQL Results Parser</param>
        /// <returns>Internal Syntax Name</returns>
        public static String GetSyntaxName(this ISparqlResultsReader parser)
        {
            if (parser is SparqlJsonParser)
            {
                return "SparqlResultsJson";
            }
            else if (parser is SparqlXmlParser)
            {
                return "SparqlResultsXml";
            }
            else
            {
                return parser.ToString();
            }
        }

        /// <summary>
        /// Converts RDF parser into internal syntax name
        /// </summary>
        /// <param name="parser">RDF Parser</param>
        /// <returns>Internal Syntax Name</returns>
        public static String GetSyntaxName(this IRdfReader parser)
        {
            /*if (parser is HtmlPlusRdfAParser)
            {
                return "XHtmlRdfA";
            }
            else*/ if (parser is Notation3Parser)
            {
                return "Notation3";
            }
            else if (parser is NTriplesParser)
            {
                return "NTriples";
            }
            else if (parser is RdfJsonParser)
            {
                return "RdfJson";
            }
            else if (parser is RdfXmlParser)
            {
                return "RdfXml";
            }
            else if (parser is TurtleParser)
            {
                return "Turtle";
            }
            /*else if (parser is XmlPlusRdfAParser)
            {
                return "XmlRdfA";
            }*/
            else
            {
                return parser.ToString();
            }
        }

        /// <summary>
        /// Converts RDF dataset parsers into internal syntax name
        /// </summary>
        /// <param name="parser">Dataset Parser</param>
        /// <returns>Internal Syntax Name</returns>
        public static String GetSyntaxName(this IStoreReader parser)
        {
            if (parser is NQuadsParser)
            {
                return "NQuads";
            }
            else if (parser is TriGParser)
            {
                return "TriG";
            }
            else if (parser is TriXParser)
            {
                return "TriX";
            }
            else
            {
                return parser.ToString();
            }
        }
    }
}
