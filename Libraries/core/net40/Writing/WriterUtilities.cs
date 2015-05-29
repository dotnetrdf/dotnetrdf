/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-develop@lists.sf.net)

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
using System.Linq;
using System.Text.RegularExpressions;
using VDS.RDF.Graphs;
using VDS.RDF.Namespaces;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Writing.Contexts;

namespace VDS.RDF.Writing
{
    /// <summary>
    /// Class containing constants for possible Compression Levels
    /// </summary>
    /// <remarks>These are intended as guidance only, Writer implementations are free to interpret these levels as they desire or to ignore them entirely and use their own levels</remarks>
    public static class WriterCompressionLevel
    {
        /// <summary>
        /// No Compression should be used (-1)
        /// </summary>
        public const int None = -1;
        /// <summary>
        /// Minimal Compression should be used (0)
        /// </summary>
        public const int Minimal = 0;
        /// <summary>
        /// Default Compression should be used (1)
        /// </summary>
        public const int Default = 1;
        /// <summary>
        /// Medium Compression should be used (3)
        /// </summary>
        public const int Medium = 3;
        /// <summary>
        /// More Compression should be used (5)
        /// </summary>
        public const int More = 5;
        /// <summary>
        /// High Compression should be used (10)
        /// </summary>
        public const int High = 10;
    }

    /// <summary>
    /// Helper methods for writers
    /// </summary>
    public static class WriterHelper
    {
        private const String UriEncodeForXmlPattern = @"&([^;&\s]*)(?=\s|$|&)";

        /// <summary>
        /// Determines whether a Blank Node ID is valid as-is when serialised in NTriple like syntaxes (Turtle/N3/SPARQL)
        /// </summary>
        /// <param name="id">ID to test</param>
        /// <returns></returns>
        /// <remarks>If false is returned then the writer will alter the ID in some way</remarks>
        public static bool IsValidBlankNodeId(String id)
        {
            if (id == null) 
            {
                //Can't be null
                return false;
            }
            else if (id.Equals(String.Empty))
            {
                //Can't be empty
                return false;
            }
            else
            {
                char[] cs = id.ToCharArray();
                if (Char.IsDigit(cs[0]) || cs[0] == '-' || cs[0] == '_')
                {
                    //Can't start with a Digit, Hyphen or Underscore
                    return false;
                }
                else
                {
                    //Otherwise OK
                    return true;
                }
            }
            
        }

        /// <summary>
        /// Determines whether a Blank Node ID is valid as-is when serialised as NTriples
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool IsValidStrictBlankNodeId(String id)
        {
            if (id == null)
            {
                //Can't be null
                return false;
            }
            else if (id.Equals(String.Empty))
            {
                //Can't be empty
                return false;
            }
            else
            {
                //All characters must be alphanumeric and not start with a digit in NTriples
                char[] cs = id.ToCharArray();
                return Char.IsLetter(cs[0]) && cs.All(c => Char.IsLetterOrDigit(c) && c <= 127);
            }
        }

        /// <summary>
        /// Encodes values for use in XML
        /// </summary>
        /// <param name="value">Value to encode</param>
        /// <returns>
        /// The value with any ampersands escaped to &amp;
        /// </returns>
        public static String EncodeForXml(String value)
        {
            while (Regex.IsMatch(value, UriEncodeForXmlPattern))
            {
                value = Regex.Replace(value, UriEncodeForXmlPattern, "&amp;$1");
            }
            if (value.EndsWith("&")) value += "amp;";
            return value.Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("'", "&apos;")
                .Replace("\"", "&quot;");
        }

        /// <summary>
        /// Extracts all namespaces used by graphs within the store
        /// </summary>
        /// <param name="store">Graph store</param>
        /// <returns>Namespaces</returns>
        public static INamespaceMapper ExtractNamespaces(IGraphStore store)
        {
            INamespaceMapper namespaces = new NamespaceMapper(true);
            foreach (IGraph g in store.Graphs)
            {
                namespaces.Import(g.Namespaces);
            }
            return namespaces;
        }
    }
}