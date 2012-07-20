/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

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
using VDS.RDF.Parsing;

namespace VDS.RDF.Utilities.Editor.Syntax
{
    public static class SyntaxExtensions
    {
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
