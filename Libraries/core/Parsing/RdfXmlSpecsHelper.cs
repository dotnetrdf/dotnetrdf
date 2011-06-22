using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using VDS.RDF.Parsing.Events;
using VDS.RDF.Parsing.Events.RdfXml;

namespace VDS.RDF.Parsing
{
    /// <summary>
    /// Static Helper class for providing Constants and Helper functions for use by RDF/XML parsers
    /// </summary>
    public static class RdfXmlSpecsHelper
    {
        private static Regex _isabsoluteuri = new Regex("^([a-zA-Z]+:(//)?|mailto:)", RegexOptions.IgnoreCase);

        /// <summary>
        /// Checks whether a Uri Reference is an absolute Uri
        /// </summary>
        /// <param name="uriref">Uri Reference to Test</param>
        /// <returns></returns>
        /// <remarks>Implemented by seeing if the Uri Reference starts with a Uri scheme specifier</remarks>
        public static bool IsAbsoluteURI(String uriref)
        {
            return _isabsoluteuri.IsMatch(uriref);
        }

        //The following set of Grammar Productions encode Tests as to the validity of Node URIs
        #region Uri Validity Test Grammar Productions

        /// <summary>
        /// Array containing the Core Syntax Terms
        /// </summary>
        private static String[] coreSyntaxTerms = { "rdf:RDF", "rdf:ID", "rdf:about", "rdf:parseType", "rdf:resource", "rdf:nodeID", "rdf:datatype" };
        /// <summary>
        /// Array containing the other Syntax Terms
        /// </summary>
        private static String[] syntaxTerms = { "rdf:Description", "rdf:li" };
        /// <summary>
        /// Array containing the Old Syntax Terms
        /// </summary>
        private static String[] oldTerms = { "rdf:aboutEach", "rdf:aboutEachPrefix", "rdf:bagID" };
        /// <summary>
        /// Array containing Syntax Terms where the rdf: Prefix is mandated
        /// </summary>
        private static String[] requiresRdfPrefix = { "about", "aboutEach", "ID", "bagID", "type", "resource", "parseType" };

        /// <summary>
        /// Checks whether a given QName is a Core Syntax Term
        /// </summary>
        /// <param name="qname">QName to Test</param>
        /// <returns>True if the QName is a Core Syntax Term</returns>
        public static bool IsCoreSyntaxTerm(String qname)
        {
            //Does the QName occur in the array of Core Syntax Terms?
            return coreSyntaxTerms.Contains(qname);
        }

        /// <summary>
        /// Checks whether a given QName is a Syntax Term
        /// </summary>
        /// <param name="qname">QName to Test</param>
        /// <returns>True if the QName is a Syntax Term</returns>
        public static bool IsSyntaxTerm(String qname)
        {
            //Does the QName occur as a Core Syntax Term or in the Array of Syntax Terms?
            return (IsCoreSyntaxTerm(qname) || syntaxTerms.Contains(qname));
        }

        /// <summary>
        /// Checks whether a given QName is a Old Syntax Term
        /// </summary>
        /// <param name="qname">QName to Test</param>
        /// <returns>True if the QName is a Old Syntax Term</returns>
        public static bool IsOldTerm(String qname)
        {
            //Does the QName occur in the array of Old Syntax Terms?
            return oldTerms.Contains(qname);
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
        public static bool IsPropertyElementURI(String qname)
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
        public static bool IsPropertyAttributeURI(String qname)
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
            return requiresRdfPrefix.Contains(name);
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

        #endregion

        //The following set of Grammar Productions encode Tests pertaining to the Types of Attributes
        #region Attribute Type Test Grammar Productions

        /// <summary>
        /// Checks whether an attribute is an rdf:ID attribute
        /// </summary>
        /// <param name="attr">Attribute to Test</param>
        /// <returns>True if is an rdf:ID attribute</returns>
        /// <remarks>Does some validation on ID value but other validation occurs in the <see cref="ValidateID()">ValidateID</see> method which is called at other points in the Parsing</remarks>
        public static bool IsIDAttribute(AttributeEvent attr)
        {
            //QName must be rdf:id
            if (attr.QName.Equals("rdf:ID"))
            {
                //Must be a valid RDF ID
                if (IsRdfID(attr.Value))
                {
                    //OK
                    return true;
                }
                else
                {
                    //Invalid RDF ID so Error
                    throw ParserHelper.Error("The value '" + attr.Value + "' for rdf:ID is not valid, RDF IDs can only be valid NCNames as defined by the W3C XML Namespaces specification", attr);
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Checks whether an attribute is an rdf:nodeID attribute
        /// </summary>
        /// <param name="attr">Attribute to Test</param>
        /// <returns>True if is an rdf:nodeID attribute</returns>
        /// <remarks>Does some validation on ID value but other validation occurs in the <see cref="ValidateID()">ValidateID</see> method which is called at other points in the Parsing</remarks>
        public static bool IsNodeIDAttribute(AttributeEvent attr)
        {
            //QName must be rdf:nodeID
            if (attr.QName.Equals("rdf:nodeID"))
            {
                //Must be a valid RDF ID
                if (IsRdfID(attr.Value))
                {
                    //OK
                    return true;
                }
                else
                {
                    //Invalid RDF ID so Error
                    throw ParserHelper.Error("The value '" + attr.Value + "' for rdf:id is not valid, RDF IDs can only be valid NCNames as defined by the W3C XML Namespaces specification", attr);
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Checks whether an attribute is an rdf:about attribute
        /// </summary>
        /// <param name="attr">Attribute to Test</param>
        /// <returns>True if is an rdf:about attribute</returns>
        public static bool IsAboutAttribute(AttributeEvent attr)
        {
            //QName must be rdf:id
            if (attr.QName.Equals("rdf:about"))
            {
                //Must be a valid RDF Uri Reference
                return IsRdfUriReference(attr.Value);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Checks whether an attribute is an property attribute
        /// </summary>
        /// <param name="attr">Attribute to Test</param>
        /// <returns>True if is an property attribute</returns>
        public static bool IsPropertyAttribute(AttributeEvent attr)
        {
            //QName must be a valid Property Attribute Uri
            //Any string value allowed so if Uri test is true then we're a property Attribute
            return IsPropertyAttributeURI(attr.QName);
        }

        /// <summary>
        /// Checks whether an attribute is an rdf:resource attribute
        /// </summary>
        /// <param name="attr">Attribute to Test</param>
        /// <returns>True if is an rdf:resource attribute</returns>
        public static bool IsResourceAttribute(AttributeEvent attr)
        {
            //QName must be rdf:resource
            if (attr.QName.Equals("rdf:resource"))
            {
                //Must be a valid RDF Uri Reference
                return IsRdfUriReference(attr.Value);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Checks whether an attribute is an rdf:datatype attribute
        /// </summary>
        /// <param name="attr">Attribute to Test</param>
        /// <returns>True if is an rdf:datatype attribute</returns>
        public static bool IsDataTypeAttribute(AttributeEvent attr)
        {
            //QName must be rdf:datatype
            if (attr.QName.Equals("rdf:datatype"))
            {
                //Must be a valid RDF Uri Reference
                return IsRdfUriReference(attr.Value);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Validates that an ID is a valid NCName
        /// </summary>
        /// <param name="value">ID Value to Test</param>
        /// <returns>True if the ID is valid</returns>
        /// <remarks>Actual Validation conditions on IDs are stricter than this and this is handled separately by the <see cref="ValidateID()">ValidateID</see> method</remarks>
        public static bool IsRdfID(String value)
        {
            //Must be a valid NCName as defined in the XML Namespace Specification
            //Which is itself defined in terms of the XML Specification
            return XmlSpecsHelper.IsNCName(value);
        }

        /// <summary>
        /// Validates that a URIReference is valid
        /// </summary>
        /// <param name="value">URIReference to Test</param>
        /// <returns>True</returns>
        /// <remarks>
        /// Currently partially implemented, some invalid Uri References may be considered valid
        /// </remarks>
        public static bool IsRdfUriReference(String value)
        {
            //OPT: Add Some more validation of Uri References here?
            char[] cs = value.ToCharArray();
            foreach (char c in cs)
            {
                if ((c >= 0x00 && c <= 0x1f) || (c >= 0x7f && c <= 0x9f))
                {
                    //Throw an error if we find a Control Character which are not permitted
                    throw new RdfParseException("An Invalid RDF URI Reference was encountered, the URI Reference '" + value + "' is not valid since it contains Unicode Control Characters!");
                }
            }
            return true;
        }

        #endregion
    }
}
