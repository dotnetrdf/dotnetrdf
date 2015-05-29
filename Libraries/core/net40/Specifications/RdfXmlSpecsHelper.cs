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
using System.Text;
using System.Text.RegularExpressions;

namespace VDS.RDF.Specifications
{
    /// <summary>
    /// Static Helper class for providing Constants and Helper functions for use by RDF/XML parsers
    /// </summary>
    public static class RdfXmlSpecsHelper
    {
        private static readonly Regex _isAbsoluteUri = new Regex("^([a-zA-Z]+:(//)?|mailto:)", RegexOptions.IgnoreCase);

        /// <summary>
        /// Checks whether a Uri Reference is an absolute Uri
        /// </summary>
        /// <param name="uriref">Uri Reference to Test</param>
        /// <returns></returns>
        /// <remarks>Implemented by seeing if the Uri Reference starts with a Uri scheme specifier</remarks>
        public static bool IsAbsoluteUri(String uriref)
        {
            return _isAbsoluteUri.IsMatch(uriref);
        }

        //The following set of Grammar Productions encode Tests as to the validity of Node URIs
        #region Uri Validity Test Grammar Productions

        /// <summary>
        /// Array containing the Core Syntax Terms
        /// </summary>
        private static readonly String[] _coreSyntaxTerms = { "rdf:RDF", "rdf:ID", "rdf:about", "rdf:parseType", "rdf:resource", "rdf:nodeID", "rdf:datatype" };
        /// <summary>
        /// Array containing the other Syntax Terms
        /// </summary>
        private static readonly String[] _syntaxTerms = { "rdf:Description", "rdf:li" };
        /// <summary>
        /// Array containing the Old Syntax Terms
        /// </summary>
        private static readonly String[] _oldTerms = { "rdf:aboutEach", "rdf:aboutEachPrefix", "rdf:bagID" };
        /// <summary>
        /// Array containing Syntax Terms where the rdf: Prefix is mandated
        /// </summary>
        private static readonly String[] _requiresRdfPrefix = { "about", "aboutEach", "ID", "bagID", "type", "resource", "parseType" };

        /// <summary>
        /// Checks whether a given QName is a Core Syntax Term
        /// </summary>
        /// <param name="qname">QName to Test</param>
        /// <returns>True if the QName is a Core Syntax Term</returns>
        public static bool IsCoreSyntaxTerm(String qname)
        {
            //Does the QName occur in the array of Core Syntax Terms?
            return _coreSyntaxTerms.Contains(qname);
        }

        /// <summary>
        /// Checks whether a given QName is a Syntax Term
        /// </summary>
        /// <param name="qname">QName to Test</param>
        /// <returns>True if the QName is a Syntax Term</returns>
        public static bool IsSyntaxTerm(String qname)
        {
            //Does the QName occur as a Core Syntax Term or in the Array of Syntax Terms?
            return (IsCoreSyntaxTerm(qname) || _syntaxTerms.Contains(qname));
        }

        /// <summary>
        /// Checks whether a given QName is a Old Syntax Term
        /// </summary>
        /// <param name="qname">QName to Test</param>
        /// <returns>True if the QName is a Old Syntax Term</returns>
        public static bool IsOldTerm(String qname)
        {
            //Does the QName occur in the array of Old Syntax Terms?
            return _oldTerms.Contains(qname);
        }

        /// <summary>
        /// Checks whether a given QName is valid as a Node Element Uri
        /// </summary>
        /// <param name="qname">QName to Test</param>
        /// <returns>True if the QName is valid</returns>
        public static bool IsNodeElementUri(String qname)
        {
            //Not allowed to be a Core Syntax Term, rdf:li or an Old Syntax Term
            if (IsCoreSyntaxTerm(qname) || qname.Equals("rdf:li") || IsOldTerm(qname))
            {
                return false;
            }
            else
            {
                //Any other URIs are allowed
                return true;
            }
        }

        /// <summary>
        /// Checks whether a given QName is valid as a Property Element Uri
        /// </summary>
        /// <param name="qname">QName to Test</param>
        /// <returns>True if the QName is valid</returns>
        public static bool IsPropertyElementUri(String qname)
        {
            //Not allowed to be a Core Syntax Term, rdf:Description or an Old Syntax Term
            if (IsCoreSyntaxTerm(qname) || qname.Equals("rdf:Description") || IsOldTerm(qname))
            {
                return false;
            }
            else
            {
                //Any other URIs are allowed
                return true;
            }
        }

        /// <summary>
        /// Checks whether a given QName is valid as a Property Attribute Uri
        /// </summary>
        /// <param name="qname">QName to Test</param>
        /// <returns>True if the QName is valid</returns>
        public static bool IsPropertyAttributeUri(String qname)
        {
            //Not allowed to be a Core Syntax Term, rdf:li, rdf:Description or an Old Syntax Term
            if (IsCoreSyntaxTerm(qname) || qname.Equals("rdf:li") || qname.Equals("rdf:Description") || IsOldTerm(qname))
            {
                return false;
            }
            else
            {
                //Any other URIs are allowed
                return true;
            }
        }

        /// <summary>
        /// Checks whether a given Local Name is potentially ambigious
        /// </summary>
        /// <param name="name">Local Name to Test</param>
        /// <returns>True if the Local Name is ambigious</returns>
        /// <remarks>This embodies Local Names which must have an rdf prefix</remarks>
        public static bool IsAmbigiousAttributeName(String name)
        {
            return _requiresRdfPrefix.Contains(name);
        }

        /// <summary>
        /// Checks whether a given URIRef is encoded in Unicode Normal Form C
        /// </summary>
        /// <param name="uriref">URIRef to Test</param>
        /// <returns>True if the URIRef is encoded correctly</returns>
        public static bool IsValidUriRefEncoding(String uriref)
        {
#if NO_NORM
            return true;
#else
            return uriref.IsNormalized();
#endif
        }

        /// <summary>
        /// Checks whether a given Base Uri can be used for relative Uri resolution
        /// </summary>
        /// <param name="baseUri">Base Uri to Test</param>
        /// <returns>True if the Base Uri can be used for relative Uri resolution</returns>
        public static bool IsValidBaseUri(String baseUri)
        {
            if (baseUri.StartsWith("mailto:"))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Determines whether a QName is valid for use in RDF/XML
        /// </summary>
        /// <param name="qname">QName</param>
        /// <returns></returns>
        public static bool IsValidQName(String qname)
        {
            if (qname.Contains(":"))
            {
                String[] parts = qname.Split(':');
                if (parts[0].Length == 0)
                {
                    //Empty Prefix is permitted
                    return XmlSpecsHelper.IsNCName(parts[1]);
                }
                else
                {
                    return XmlSpecsHelper.IsNCName(parts[0]) && XmlSpecsHelper.IsNCName(parts[1]);
                }
            }
            else
            {
                return XmlSpecsHelper.IsNCName(qname);
            }
        }

        #endregion
    }
}
