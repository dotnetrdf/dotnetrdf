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
using System.IO;
using System.Net;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Tokens;

namespace VDS.RDF
{
    /// <summary>
    /// Tools class which contains a number of utility methods which are declared as static methods
    /// </summary>
    public static class Tools
    {
        /// <summary>
        /// Checks whether a Uri is valid as a Base Uri for resolving Relative URIs against
        /// </summary>
        /// <param name="baseUri">Base Uri to test</param>
        /// <returns>True if the Base Uri can be used to resolve Relative URIs against</returns>
        /// <remarks>A Base Uri is valid if it is an absolute Uri and not using the mailto: scheme</remarks>
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
        /// Checks whether a URI Reference appears malformed and if so fixes it
        /// </summary>
        /// <param name="uriref">URI Reference</param>
        /// <returns></returns>
        static String FixMalformedUriStrings(String uriref)
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
        /// Returns a URI with any Fragment ID removed from it
        /// </summary>
        /// <param name="u">URI</param>
        /// <returns></returns>
        public static Uri StripUriFragment(Uri u)
        {
            if (u.Fragment.Equals(String.Empty))
            {
                return u;
            }
            else
            {
                String temp = u.AbsoluteUri;
                temp = temp.Substring(0, temp.Length - u.Fragment.Length);
                return UriFactory.Create(temp);
            }
        }

        /// <summary>
        /// Generic Helper Function which Resolves Uri References against a Base Uri
        /// </summary>
        /// <param name="uriref">Uri Reference to resolve</param>
        /// <param name="baseUri">Base Uri to resolve against</param>
        /// <returns>Resolved Uri as a String</returns>
        /// <exception cref="RdfParseException">RDF Parse Exception if the Uri cannot be resolved for a know reason</exception>
        /// <exception cref="UriFormatException">Uri Format Exception if one/both of the URIs is malformed</exception>
        public static String ResolveUri(String uriref, String baseUri)
        {
            if (!baseUri.Equals(String.Empty))
            {
                if (uriref.Equals(String.Empty))
                {
                    // Empty Uri reference refers to the Base Uri
                    return UriFactory.Create(FixMalformedUriStrings(baseUri)).AbsoluteUri;
                }
                else
                {
                    // Resolve the Uri by combining the Absolute/Relative Uri with the in-scope Base Uri
                    Uri u = new Uri(FixMalformedUriStrings(uriref), UriKind.RelativeOrAbsolute);
                    if (u.IsAbsoluteUri) 
                    {
                        // Uri Reference is an Absolute Uri so no need to resolve against Base Uri
                        return u.AbsoluteUri;
                    } 
                    else 
                    {
                        Uri b = UriFactory.Create(FixMalformedUriStrings(baseUri));

                        // Check that the Base Uri is valid for resolving Relative URIs
                        // If the Uri Reference is a Fragment ID then Base Uri validity is irrelevant
                        // We have to use ToString() here because this is a Relative URI so AbsoluteUri would be invalid here
                        if (u.ToString().StartsWith("#"))
                        {
                            return ResolveUri(u, b).AbsoluteUri;
                        }
                        else if (IsValidBaseUri(b))
                        {
                            return ResolveUri(u, b).AbsoluteUri;
                        }
                        else
                        {
                            throw new RdfParseException("Cannot resolve a URI since the Base URI is not a valid for resolving Relative URIs against");
                        }
                    }
                }
            }
            else
            {
                if (uriref.Equals(String.Empty))
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
        }

        /// <summary>
        /// Generic Helper Function which Resolves Uri References against a Base Uri
        /// </summary>
        /// <param name="uriref">Uri Reference to resolve</param>
        /// <param name="baseUri">Base Uri to resolve against</param>
        /// <returns>Resolved Uri as a String</returns>
        /// <exception cref="UriFormatException">Uri Format Exception if one/both of the URIs is malformed</exception>
        public static Uri ResolveUri(Uri uriref, Uri baseUri)
        {
            Uri result = new Uri(baseUri, uriref);
            return result;
        }

                /// <summary>
        /// Resolves a QName into a Uri using the Namespace Mapper and Base Uri provided
        /// </summary>
        /// <param name="qname">QName to resolve</param>
        /// <param name="nsmap">Namespace Map to resolve against</param>
        /// <param name="baseUri">Base Uri to resolve against</param>
        /// <returns></returns>
        public static String ResolveQName(String qname, INamespaceMapper nsmap, Uri baseUri)
        {
            return ResolveQName(qname, nsmap, baseUri, false);
        }

        /// <summary>
        /// Resolves a QName into a Uri using the Namespace Mapper and Base Uri provided
        /// </summary>
        /// <param name="qname">QName to resolve</param>
        /// <param name="nsmap">Namespace Map to resolve against</param>
        /// <param name="baseUri">Base Uri to resolve against</param>
        /// <param name="allowDefaultPrefixFallback">Whether when the default prefix is used but not defined it can fallback to Base URI</param>
        /// <returns></returns>
        public static String ResolveQName(String qname, INamespaceMapper nsmap, Uri baseUri, bool allowDefaultPrefixFallback)
        {
            String output;

            if (qname.StartsWith(":"))
            {
                // QName in Default Namespace
                if (nsmap.HasNamespace(String.Empty))
                {
                    // Default Namespace Defined
                    output = nsmap.GetNamespaceUri(String.Empty).AbsoluteUri + qname.Substring(1);
                }
                else if (allowDefaultPrefixFallback)
                {
                    // No Default Namespace so use Base Uri
                    // These type of QNames are scoped to the local Uri regardless of the type of the Base Uri
                    // i.e. these always result in Hash URIs
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
                String[] parts = qname.Split(new char[] { ':' }, 2);
                if (parts.Length == 1)
                {
                    output = nsmap.GetNamespaceUri(String.Empty).AbsoluteUri + parts[0];
                }
                else
                {
                    output = nsmap.GetNamespaceUri(parts[0]).AbsoluteUri + parts[1];
                }
            }

            return output;
        }

        /// <summary>
        /// Resolves a QName/Uri into a Uri using the Namespace Mapper and Base Uri provided
        /// </summary>
        /// <param name="t">QName/Uri to resolve</param>
        /// <param name="nsmap">Namespace Map to resolve against</param>
        /// <param name="baseUri">Base Uri to resolve against</param>
        /// <returns></returns>
        public static String ResolveUriOrQName(IToken t, INamespaceMapper nsmap, Uri baseUri)
        {
            if (t.TokenType == Token.QNAME)
            {
                return ResolveQName(t.Value, nsmap, baseUri);
            }
            else if (t.TokenType == Token.URI)
            {
                String uriBase = (baseUri == null) ? String.Empty : baseUri.AbsoluteUri;
                return ResolveUri(t.Value, uriBase);
            }
            else
            {
                throw new RdfParseException("Unable to resolve a '" + t.GetType().ToString() + "' Token into a URI", t);
            }
        }

        /// <summary>
        /// Copies a Node so it can be used in another Graph since by default Triples cannot contain Nodes from more than one Graph
        /// </summary>
        /// <param name="original">Node to Copy</param>
        /// <param name="target">Graph to Copy into</param>
        /// <param name="keepOriginalGraphUri">Indicates whether the Copy should preserve the Graph Uri of the Node being copied</param>
        /// <returns></returns>
        public static INode CopyNode(INode original, IGraph target, bool keepOriginalGraphUri)
        {
            if (!keepOriginalGraphUri)
            {
                return CopyNode(original, target);
            }
            else
            {
                INode temp = CopyNode(original, target);
                temp.GraphUri = original.GraphUri;
                return temp;
            }
        }

        /// <summary>
        /// Copies a Node so it can be used in another Graph since by default Triples cannot contain Nodes from more than one Graph
        /// </summary>
        /// <param name="original">Node to Copy</param>
        /// <param name="target">Graph to Copy into</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// <strong>Warning:</strong> Copying Blank Nodes may lead to unforseen circumstances since no remapping of IDs between Graphs is done
        /// </para>
        /// </remarks>
        public static INode CopyNode(INode original, IGraph target)
        {
            // No need to copy if it's already in the relevant Graph
            if (ReferenceEquals(original.Graph, target)) return original;

            // if a node can copy itself then let it do it
            var selfcopyable_original = original as Storage.Virtualisation.ICanCopy;
            if (selfcopyable_original != null) return selfcopyable_original.CopyNode(target);
            
            // if it doesn't, copy it's values:

            if (original.NodeType == NodeType.Uri)
            {
                IUriNode u = (IUriNode)original;
                IUriNode u2 = new UriNode(target, u.Uri);

                return u2;
            }
            else if (original.NodeType == NodeType.Literal)
            {
                ILiteralNode l = (ILiteralNode)original;
                ILiteralNode l2;
                if (l.Language.Equals(String.Empty))
                {
                    if (!(l.DataType == null))
                    {
                        l2 = new LiteralNode(target, l.Value, l.DataType);
                    }
                    else
                    {
                        l2 = new LiteralNode(target, l.Value);
                    }
                }
                else
                {
                    l2 = new LiteralNode(target, l.Value, l.Language);
                }

                return l2;
            }
            else if (original.NodeType == NodeType.Blank)
            {
                IBlankNode b = (IBlankNode)original;
                IBlankNode b2;

                b2 = new BlankNode(target, b.InternalID);
                return b2;
            }
            else if (original.NodeType == NodeType.Variable)
            {
                IVariableNode v = (IVariableNode)original;
                return new VariableNode(target, v.VariableName);
            }
            else
            {
                throw new RdfException("Unable to Copy '" + original.GetType().ToString() + "' Nodes between Graphs");
            }
        }

        /// <summary>
        /// Copies a Node using another Node Factory
        /// </summary>
        /// <param name="original">Node to copy</param>
        /// <param name="target">Factory to copy into</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// <strong>Warning:</strong> Copying Blank Nodes may lead to unforseen circumstances since no remapping of IDs between Factories is done
        /// </para>
        /// </remarks>
        public static INode CopyNode(INode original, INodeFactory target)
        {
            if (ReferenceEquals(original.Graph, target)) return original;

            switch (original.NodeType)
            {
                case NodeType.Blank:
                    return target.CreateBlankNode(((IBlankNode)original).InternalID);
                case NodeType.GraphLiteral:
                    return target.CreateGraphLiteralNode(((IGraphLiteralNode)original).SubGraph);
                case NodeType.Literal:
                    ILiteralNode lit = (ILiteralNode)original;
                    if (lit.DataType != null)
                    {
                        return target.CreateLiteralNode(lit.Value, lit.DataType);
                    }
                    else if (!lit.Language.Equals(String.Empty))
                    {
                        return target.CreateLiteralNode(lit.Value, lit.Language);
                    }
                    else
                    {
                        return target.CreateLiteralNode(lit.Value);
                    }
                case NodeType.Uri:
                    return target.CreateUriNode(((IUriNode)original).Uri);
                case NodeType.Variable:
                    return target.CreateVariableNode(((IVariableNode)original).VariableName);
                default:
                    throw new RdfException("Unable to Copy '" + original.GetType().ToString() + "' Nodes between Node Factories");
            }
        }

        /// <summary>
        /// Copies a Triple from one Graph to another
        /// </summary>
        /// <param name="t">Triple to copy</param>
        /// <param name="target">Graph to copy to</param>
        /// <returns></returns>
        public static Triple CopyTriple(Triple t, IGraph target)
        {
            return CopyTriple(t, target, false);
        }

        /// <summary>
        /// Copies a Triple from one Graph to another
        /// </summary>
        /// <param name="t">Triple to copy</param>
        /// <param name="target">Graph to copy to</param>
        /// <param name="keepOriginalGraphUri">Indicates whether the Copy should preserve the Graph Uri of the Nodes being copied</param>
        /// <returns></returns>
        public static Triple CopyTriple(Triple t, IGraph target, bool keepOriginalGraphUri)
        {
            // No need to copy if Triple already comes from the Target Graph
            if (ReferenceEquals(t.Graph, target)) return t;

            // Copy the Nodes
            INode subj, pred, obj;
            subj = CopyNode(t.Subject, target, keepOriginalGraphUri);
            pred = CopyNode(t.Predicate, target, keepOriginalGraphUri);
            obj = CopyNode(t.Object, target, keepOriginalGraphUri);

            // Return a new Triple
            return new Triple(subj, pred, obj, t.Context);
        }

        /// <summary>
        /// Does a quick and simple combination of the Hash Codes of two Objects
        /// </summary>
        /// <param name="x">First Object</param>
        /// <param name="y">Second Object</param>
        /// <returns></returns>
        public static int CombineHashCodes(Object x, Object y)
        {
            int hash = 17;
            hash = hash * 31 + x.GetHashCode();
            hash = hash * 31 + y.GetHashCode();
            return hash;
        }

        /// <summary>
        /// Prints Debugging Output to the Console Standard Out for a HTTP Web Request
        /// </summary>
        /// <param name="httpRequest">HTTP Web Request</param>
        /// <remarks><strong>Only available in Debug builds</strong></remarks>
        public static void HttpDebugRequest(HttpWebRequest httpRequest)
        {
            if (!Options.HttpDebugging && !Options.HttpFullDebugging)
                return;

            // Output the Request Headers
            Console.Error.WriteLine("# HTTP DEBUGGING #");
            Console.Error.WriteLine("HTTP Request to " + httpRequest.RequestUri.AbsoluteUri);
            Console.Error.WriteLine();
            Console.Error.WriteLine(httpRequest.Method);
            foreach (String header in httpRequest.Headers.AllKeys)
            {
                Console.Error.WriteLine(header + ":" + httpRequest.Headers[header]);
            }
            Console.Error.WriteLine();
        }

        /// <summary>
        /// Prints Debugging Output to the Console Standard Out for a HTTP Web Response
        /// </summary>
        /// <param name="httpResponse">HTTP Web Response</param>
        /// <remarks><strong>Only available in Debug builds</strong></remarks>
        public static void HttpDebugResponse(HttpWebResponse httpResponse)
        {
            if (!Options.HttpDebugging && !Options.HttpFullDebugging)
                return;

            // Output the Response Uri and Headers
            Console.Error.WriteLine();
            Console.Error.WriteLine("HTTP Response from " + httpResponse.ResponseUri.AbsoluteUri);
#if NETCORE
            Console.Error.WriteLine("HTTP " + (int)httpResponse.StatusCode + " " + httpResponse.StatusDescription);
#else
            Console.Error.WriteLine("HTTP/" + httpResponse.ProtocolVersion + " " + (int)httpResponse.StatusCode + " " + httpResponse.StatusDescription);
#endif
            Console.Error.WriteLine();
            foreach (String header in httpResponse.Headers.AllKeys)
            {
                Console.Error.WriteLine(header + ":" + httpResponse.Headers[header]);
            }
            Console.Error.WriteLine();

            if (Options.HttpFullDebugging)
            {
                // Output the actual Response
                Stream data = httpResponse.GetResponseStream();
                if (data != null)
                {
                    StreamReader reader = new StreamReader(data);
                    while (!reader.EndOfStream)
                    {
                        Console.Error.WriteLine(reader.ReadLine());
                    }
                    Console.Error.WriteLine();

                    if (data.CanSeek)
                    {
                        data.Seek(0, SeekOrigin.Begin);
                    }
                    else
                    {
                        throw new RdfException("Full HTTP Debugging is enabled and the HTTP response stream has been consumed and written to the standard error stream, the stream is no longer available for calling code to consume");
                    }
                }
            }

            Console.Error.WriteLine("# END HTTP DEBUGGING #");
        }
    }
}
