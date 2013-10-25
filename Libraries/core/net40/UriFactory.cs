/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

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
using System.Globalization;
using System.Linq;
using System.Text;
using VDS.Common.Tries;
using VDS.RDF.Namespaces;

namespace VDS.RDF
{
    /// <summary>
    /// A static helper class for interning URIs to reduce memory usage
    /// </summary>
    public static class UriFactory
    {
        private static readonly ITrie<Uri, String, Uri> _uris = new Trie<Uri, string, Uri>(UriFactory.UriToSegments);

        /// <summary>
        /// Decomposes a URI into a series of segments for use with the interning
        /// </summary>
        /// <param name="u">URI</param>
        /// <returns>URI segments</returns>
        private static IEnumerable<String> UriToSegments(Uri u)
        {
            if (ReferenceEquals(u, null) || !u.IsAbsoluteUri) throw new RdfException("Cannot split a relative URI into segments");

            // Scheme, Host, Port, UserInfo, Path Segment(s), Querystring, Fragment
            List<String> segments = new List<String>
                {
                    u.Scheme,
                    u.Host,
                    u.Port.ToString(CultureInfo.InvariantCulture),
                    ReferenceEquals(u.UserInfo, null) ? String.Empty : u.UserInfo
                };
#if !SILVERLIGHT
            segments.AddRange(u.Segments);
#else
            segments.AddRange(u.Segments());
#endif
            if (!String.IsNullOrEmpty(u.Query)) segments.Add(u.Query);
            if (!String.IsNullOrEmpty(u.Fragment)) segments.Add(u.Fragment);

            return segments;
        }

        /// <summary>
        /// Creates a URI interning it if it is an absolute URI and interning is enabled via the <see cref="Options.InternUris">Options.InternUris</see> option
        /// </summary>
        /// <param name="uri">URI string</param>
        /// <returns></returns>
        /// <remarks>
        /// When URI interning is disabled this is equivalent to just invoking the constructor of the <see cref="Uri">Uri</see> class with <see cref="UriKind.RelativeOrAbsolute"/> as the second argument
        /// </remarks>
        /// <exception cref="RdfException">Thrown if the given string is null or invalid</exception>
        public static Uri Create(String uri)
        {
            if (ReferenceEquals(uri, null)) throw new RdfException("Cannot create a URI from a null string");
            if (!Options.InternUris)
            {
                return new Uri(uri);
            }
            else
            {
                try
                {
                    Uri u = new Uri(uri, UriKind.RelativeOrAbsolute);
                    return u.IsAbsoluteUri ? Intern(u) : u;
                }
#if PORTABLE
                catch (FormatException fEx)
#else
                catch (UriFormatException fEx)
#endif
                {
                    throw new RdfException("Given URI string was invalid, see inner exception for details", fEx);
                }
            }
        }

        /// <summary>
        /// Interns a URI provided it is an absolute URI and interning has been enabled via the <see cref="Options.InternUris"/> option
        /// </summary>
        /// <param name="u">URI</param>
        /// <returns>Interned URI</returns>
        /// <exception cref="RdfException">Thrown if the given URI is null</exception>
        public static Uri Intern(Uri u)
        {
            if (ReferenceEquals(u, null)) throw new RdfException("Cannot intern a null URI");
            if (!Options.InternUris || !u.IsAbsoluteUri) return u;
            ITrieNode<String, Uri> node = _uris.MoveToNode(u);
            if (!node.HasValue) node.Value = u;
            return node.Value;
        }

        /// <summary>
        /// Gets whether a given URI is currently interned
        /// </summary>
        /// <param name="u">URI</param>
        /// <returns>True if the URI is interned, false otherwise</returns>
        public static bool IsInterned(Uri u)
        {
            if (ReferenceEquals(u, null) || !u.IsAbsoluteUri) return false;
            ITrieNode<String, Uri> node = _uris.Find(u);
            return !ReferenceEquals(node, null) && node.HasValue;
        }

        /// <summary>
        /// Clears all interned URIs
        /// </summary>
        public static void Clear()
        {
            _uris.Clear();
        }

        /// <summary>
        /// Returns a URI with any Fragment ID removed from it
        /// </summary>
        /// <param name="u">URI</param>
        /// <returns>URI with Fragment ID removed</returns>
        public static Uri StripUriFragment(Uri u)
        {
            if (String.IsNullOrEmpty(u.Fragment))
            {
                return u;
            }
            else
            {
                if (u.IsAbsoluteUri)
                {
                    // TODO: Rewrite to just use Uri constructor to copy relevant portions
                    String temp = u.AbsoluteUri;
                    temp = temp.Substring(0, temp.Length - u.Fragment.Length);
                    return UriFactory.Create(temp);
                }
                else
                {
                    // TODO: Support stripping fragments from relative URIs
                    throw new RdfException("Stripping Fragments from relative URIs is not currently supported");
                }
            }
        }

        /// <summary>
        /// Attempts to resolve a possibly relative URI to an absolute URI against a given base
        /// </summary>
        /// <param name="uri">Relative or Absolute URI</param>
        /// <param name="baseUri">Base URI</param>
        /// <returns>Absolute URI if possible to resolve, relative URI otherwise</returns>
        /// <exception cref="RdfException">Thrown if the string given is null or an invalid URI</exception>
        public static Uri ResolveUri(String uri, Uri baseUri)
        {
            if (ReferenceEquals(uri, null)) throw new RdfException("Cannot resolve a null URI");
            try
            {
                Uri u = new Uri(uri, UriKind.RelativeOrAbsolute);
                return Intern(u.IsAbsoluteUri ? u : ResolveUri(u, baseUri));
            }
#if PORTABLE
            catch (FormatException fEx)
#else
            catch (UriFormatException fEx)
#endif
            {
                throw new RdfException("Cannot resolve an invalid URI, see inner exception for details", fEx);
            }
        }

        /// <summary>
        /// Attempts to resolve a possibly relative URI to an absolute URI against a given base
        /// </summary>
        /// <param name="uri">Relative or Absolute URI</param>
        /// <param name="baseUri">Base URI</param>
        /// <returns>Absolute URI if possible to resolve, relative URI otherwise</returns>
        public static Uri ResolveUri(Uri uri, Uri baseUri)
        {
            if (ReferenceEquals(uri, null)) throw new RdfException("Cannot resolve a null URI");
            // Can't resolve against a non-absolute Base
            // Plus no need to resolve if already an absolute URI
            if (ReferenceEquals(baseUri, null) || uri.IsAbsoluteUri || !baseUri.IsAbsoluteUri) return uri.IsAbsoluteUri ? Intern(uri) : uri;

            return Intern(new Uri(baseUri, uri));
        }

        /// <summary>
        /// Resolves a prefixed name into a URI using the Namespace Mapper and Base URI provided
        /// </summary>
        /// <param name="prefixedName">Prefixed name to resolve</param>
        /// <param name="nsMap">Namespace Map to resolve against</param>
        /// <param name="baseUri">Base URI to resolve against</param>
        /// <returns>Resolved prefix name</returns>
        public static Uri ResolvePrefixedName(String prefixedName, INamespaceMapper nsMap, Uri baseUri)
        {
            return ResolvePrefixedName(prefixedName, nsMap, baseUri, false);
        }

        /// <summary>
        /// Resolves a prefixed name into a URI using the Namespace Mapper and Base URI provided
        /// </summary>
        /// <param name="prefixedName">Prefixed name to resolve</param>
        /// <param name="nsmap">Namespace Map to resolve against</param>
        /// <param name="baseUri">Base URI to resolve against</param>
        /// <param name="allowDefaultPrefixFallback">Whether when the default prefix is used but not defined it can fallback to Base URI</param>
        /// <returns>Resolved prefix name</returns>
        /// <exception cref="RdfException">Thrown if the prefixed name cannot be resolved for any reason e.g. undefined namespace, results in an invalid URI etc</exception>
        public static Uri ResolvePrefixedName(String prefixedName, INamespaceMapper nsmap, Uri baseUri, bool allowDefaultPrefixFallback)
        {
            String output;

            if (prefixedName.StartsWith(":"))
            {
                //Prefixed name in Default Namespace
                if (nsmap.HasNamespace(String.Empty))
                {
                    //Default Namespace Defined
                    output = nsmap.GetNamespaceUri(String.Empty).AbsoluteUri + prefixedName.Substring(1);
                }
                else if (allowDefaultPrefixFallback)
                {
                    //No Default Namespace so use Base Uri
                    //These type of prefixed names are scoped to the local Uri regardless of the type of the Base Uri
                    //i.e. these always result in Hash URIs
                    if (!ReferenceEquals(baseUri, null) && baseUri.IsAbsoluteUri)
                    {
                        output = baseUri.AbsoluteUri;
                        if (output.EndsWith("#"))
                        {
                            output += prefixedName.Substring(1);
                        }
                        else
                        {
                            output += "#" + prefixedName.Substring(1);
                        }
                    }
                    else
                    {
                        throw new RdfException("Cannot resolve the prefixed name '" + prefixedName + "' in the Default Namespace when there is no in-scope Base URI and no Default Namespace defined.  Did you forget to define a namespace for the : prefix?");
                    }
                }
                else
                {
                    throw new RdfException("Cannot resolve the prefixed name '" + prefixedName + "' in the Default Namespace since the namespace is not defined.  Did you to forget to define a namespace for the : prefix?");
                }
            }
            else
            {
                //Prefixed name in some other Namespace
                String[] parts = prefixedName.Split(new char[] { ':' }, 2);
                if (parts.Length == 1)
                {
                    output = nsmap.GetNamespaceUri(String.Empty).AbsoluteUri + parts[0];
                }
                else
                {
                    output = nsmap.GetNamespaceUri(parts[0]).AbsoluteUri + parts[1];
                }
            }

            try
            {
                return Intern(new Uri(output));
            }
#if PORTABLE
            catch (FormatException fEx)
#else
            catch (UriFormatException fEx)
#endif
            {
                throw new RdfException("Prefixed name resolution resulted in an invalid URI, see inner exception for details", fEx);
            }
        }
    }
}
