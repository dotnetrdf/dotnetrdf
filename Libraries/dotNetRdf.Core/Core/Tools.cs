/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2025 dotNetRDF Project (http://dotnetrdf.org/)
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
using System.Linq;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Tokens;

namespace VDS.RDF;

/// <summary>
/// Tools class which contains a number of utility methods which are declared as static methods.
/// </summary>
public static class Tools
{
    /// <summary>
    /// Checks whether a Uri is valid as a Base Uri for resolving Relative URIs against.
    /// </summary>
    /// <param name="baseUri">Base Uri to test.</param>
    /// <returns>True if the Base Uri can be used to resolve Relative URIs against.</returns>
    /// <remarks>A Base Uri is valid if it is an absolute Uri and not using the mailto: scheme.</remarks>
    public static bool IsValidBaseUri(Uri baseUri)
    {
        if (baseUri.Scheme.Equals("mailto"))
        {
            return false;
        }
        else
        {
            return baseUri.IsAbsoluteUri;
        }
    }

    /// <summary>
    /// Checks whether a URI Reference appears malformed and if so fixes it.
    /// </summary>
    /// <param name="uriref">URI Reference.</param>
    /// <returns></returns>
    static string FixMalformedUriStrings(string uriref)
    {
        if (uriref.StartsWith("file:/"))
        {
            // HACK: This is something of a Hack as a workaround to the issue that some systems may generate RDF which have technically malformed file:// scheme URIs in it
            // This is because *nix style filesystems use paths of the form /path/to/somewhere and some serializers will serialize such
            // a file path by just prepending file: when they should be prepending file://
            if (uriref.Length > 6)
            {
                if (uriref[6] != '/')
                {
                    return "file://" + uriref.Substring(5);
                }
            }
            return uriref;
        }
        else
        {
            return uriref;
        }
    }

    /// <summary>
    /// Returns a URI with any Fragment ID removed from it.
    /// </summary>
    /// <param name="u">URI.</param>
    /// <param name="uriFactory">The factory to use to create the returned URI. If not specified, defaults to <see cref="UriFactory.Root"/>.</param>
    /// <returns></returns>
    public static Uri StripUriFragment(Uri u, IUriFactory uriFactory = null)
    {
        if (u.Fragment.Equals(string.Empty))
        {
            return u;
        }
        else
        {
            var temp = u.AbsoluteUri;
            temp = temp.Substring(0, temp.Length - u.Fragment.Length);
            return (uriFactory??UriFactory.Root).Create(temp);
        }
    }

    /// <summary>
    /// Generic Helper Function which Resolves Uri References against a Base Uri.
    /// </summary>
    /// <param name="uriref">Uri Reference to resolve.</param>
    /// <param name="baseUri">Base Uri to resolve against.</param>
    /// <param name="uriFactory">The factory to use to create the temporary URI created internally by this method. If not specified, defaults to <see cref="UriFactory.Root"/>.</param>
    /// <returns>Resolved Uri as a String.</returns>
    /// <exception cref="RdfParseException">RDF Parse Exception if the Uri cannot be resolved for a know reason.</exception>
    /// <exception cref="UriFormatException">Uri Format Exception if one/both of the URIs is malformed.</exception>
    public static string ResolveUri(string uriref, string baseUri, IUriFactory uriFactory = null)
    {
        uriFactory ??= UriFactory.Root;
        if (!baseUri.Equals(string.Empty))
        {
            if (uriref.Equals(string.Empty))
            {
                // Empty Uri reference refers to the Base Uri
                return uriFactory.Create(FixMalformedUriStrings(baseUri)).AbsoluteUri;
            }

            // Resolve the Uri by combining the Absolute/Relative Uri with the in-scope Base Uri
            var u = new Uri(FixMalformedUriStrings(uriref), UriKind.RelativeOrAbsolute);
            if (u.IsAbsoluteUri) 
            {
                // Uri reference is an absolute URI, so no need to resolve against Base Uri
                return u.AbsoluteUri;
            }

            Uri b = uriFactory.Create(FixMalformedUriStrings(baseUri));

            // Check that the Base Uri is valid for resolving Relative URIs
            // If the Uri Reference is a Fragment ID then Base Uri validity is irrelevant
            // We have to use ToString() here because this is a Relative URI so AbsoluteUri would be invalid here
            if (u.ToString().StartsWith("#"))
            {
                return ResolveUri(u, b).AbsoluteUri;
            }

            if (IsValidBaseUri(b))
            {
                return ResolveUri(u, b).AbsoluteUri;
            }

            throw new RdfParseException("Cannot resolve a URI since the Base URI is not a valid for resolving Relative URIs against");
        }

        if (uriref.Equals(string.Empty))
        {
            throw new RdfParseException("Cannot use an Empty URI to refer to the document Base URI since there is no in-scope Base URI!");
        }

        try
        {
            return new Uri(FixMalformedUriStrings(uriref), UriKind.Absolute).AbsoluteUri;
        }
        catch (UriFormatException)
        {
            throw new RdfParseException("Cannot resolve a Relative URI Reference since there is no in-scope Base URI!");
        }
    }

    /// <summary>
    /// Generic Helper Function which Resolves Uri References against a Base Uri.
    /// </summary>
    /// <param name="uriref">Uri Reference to resolve.</param>
    /// <param name="baseUri">Base Uri to resolve against.</param>
    /// <returns>Resolved Uri as a String.</returns>
    /// <exception cref="UriFormatException">Uri Format Exception if one/both of the URIs is malformed.</exception>
    public static Uri ResolveUri(Uri uriref, Uri baseUri)
    {
        var result = new Uri(baseUri, uriref);
        return result;
    }

            /// <summary>
    /// Resolves a QName into a Uri using the Namespace Mapper and Base Uri provided.
    /// </summary>
    /// <param name="qname">QName to resolve.</param>
    /// <param name="nsmap">Namespace Map to resolve against.</param>
    /// <param name="baseUri">Base Uri to resolve against.</param>
    /// <returns></returns>
    public static string ResolveQName(string qname, INamespaceMapper nsmap, Uri baseUri)
    {
        return ResolveQName(qname, nsmap, baseUri, false);
    }

    /// <summary>
    /// Resolves a QName into a Uri using the Namespace Mapper and Base Uri provided.
    /// </summary>
    /// <param name="qname">QName to resolve.</param>
    /// <param name="nsmap">Namespace Map to resolve against.</param>
    /// <param name="baseUri">Base Uri to resolve against.</param>
    /// <param name="allowDefaultPrefixFallback">Whether when the default prefix is used but not defined, it can fall back to Base URI.</param>
    /// <returns></returns>
    public static string ResolveQName(string qname, INamespaceMapper nsmap, Uri baseUri, bool allowDefaultPrefixFallback)
    {
        string output;

        if (qname.StartsWith(":"))
        {
            // QName in Default Namespace
            if (nsmap.HasNamespace(string.Empty))
            {
                // Default Namespace Defined
                output = nsmap.GetNamespaceUri(string.Empty).AbsoluteUri + qname.Substring(1);
            }
            else if (allowDefaultPrefixFallback)
            {
                // No Default Namespace so use Base Uri
                // These type of QNames are scoped to the local Uri regardless of the type of the Base Uri
                // i.e., these always result in Hash URIs
                if (baseUri != null)
                {
                    output = baseUri.AbsoluteUri;
                    if (output.EndsWith("#"))
                    {
                        output += qname.Substring(1);
                    }
                    else
                    {
                        output += "#" + qname.Substring(1);
                    }
                }
                else
                {
                    throw new RdfParseException("Cannot resolve the QName '" + qname + "' in the Default Namespace when there is no in-scope Base URI and no Default Namespace defined.  Did you forget to define a namespace for the : prefix?");
                }
            }
            else
            {
                throw new RdfParseException("Cannot resolve the QName '" + qname + "' in the Default Namespace since the namespace is not defined.  Did you to forget to define a namespace for the : prefix?");
            }
        }
        else
        {
            // QName in some other Namespace
            var parts = qname.Split([':'], 2);
            if (parts.Length == 1)
            {
                output = nsmap.GetNamespaceUri(string.Empty).AbsoluteUri + parts[0];
            }
            else
            {
                output = nsmap.GetNamespaceUri(parts[0]).AbsoluteUri + parts[1];
            }
        }

        return output;
    }

    /// <summary>
    /// Resolves a QName/Uri into a Uri using the Namespace Mapper and Base Uri provided.
    /// </summary>
    /// <param name="t">QName/Uri to resolve.</param>
    /// <param name="nsmap">Namespace Map to resolve against.</param>
    /// <param name="baseUri">Base Uri to resolve against.</param>
    /// <returns></returns>
    public static string ResolveUriOrQName(IToken t, INamespaceMapper nsmap, Uri baseUri)
    {
        switch (t.TokenType)
        {
            case Token.QNAME:
                return ResolveQName(t.Value, nsmap, baseUri);
            case Token.URI:
                {
                    var uriBase = (baseUri == null) ? string.Empty : baseUri.AbsoluteUri;
                    return ResolveUri(t.Value, uriBase);
                }
            case Token.DATATYPE when t.Value.StartsWith("<"):
                {
                    var dturi = t.Value.Substring(1, t.Value.Length - 2);
                    var uriBase = (baseUri == null) ? string.Empty : baseUri.AbsoluteUri;
                    return ResolveUri(dturi, uriBase);
                }
            case Token.DATATYPE:
                return ResolveQName(t.Value, nsmap, baseUri);
            default:
                throw new RdfParseException("Unable to resolve a '" + t.GetType() + "' Token into a URI", t);
        }
    }



    /// <summary>
    /// Does a quick and simple combination of the Hash Codes of two or more objects.
    /// </summary>
    /// <param name="objects">The objects whose hash codes are to be combined.</param>
    /// <returns></returns>
    public static int CombineHashCodes(params object[] objects)
    {
        return objects.Aggregate(17, (current, o) => (31 * current) + o.GetHashCode());
    }
}
