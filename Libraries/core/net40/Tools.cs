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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Net;
using System.Diagnostics;
using VDS.RDF.Graphs;
using VDS.RDF.Namespaces;
using VDS.RDF.Nodes;
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
        /// Constants used to add salt to the hashes of different Literal Nodes
        /// </summary>
        private const String PlainLiteralHashCodeSalt = "plain";

        /// <summary>
        /// Checks whether a Uri is valid as a Base Uri for resolving Relative URIs against
        /// </summary>
        /// <param name="baseUri">Base Uri to test</param>
        /// <returns>True if the Base Uri can be used to resolve Relative URIs against</returns>
        /// <remarks>A Base Uri is valid if it is an absolute Uri and not using the mailto: scheme</remarks>
        [Obsolete("No longer used", true)]
        public static bool IsValidBaseUri(Uri baseUri)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Checks whether a URI Reference appears malformed and if so fixes it
        /// </summary>
        /// <param name="uriref">URI Reference</param>
        /// <returns></returns>
        [Obsolete("No longer used", true)]
        static String FixMalformedUriStrings(String uriref)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Returns a URI with any Fragment ID removed from it
        /// </summary>
        /// <param name="u">URI</param>
        /// <returns></returns>
        [Obsolete("Replaced by the UriFactory.StripUriFragment() method", true)]
        public static Uri StripUriFragment(Uri u)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Generic Helper Function which Resolves Uri References against a Base Uri
        /// </summary>
        /// <param name="uriref">Uri Reference to resolve</param>
        /// <param name="baseUri">Base Uri to resolve against</param>
        /// <returns>Resolved Uri as a String</returns>
        /// <exception cref="UriFormatException">Uri Format Exception if one/both of the URIs is malformed</exception>
        [Obsolete("Replaced by the UriFactory.ResolveUri() method", true)]
        public static String ResolveUri(String uriref, String baseUri)
        {
           throw new NotSupportedException();
        }

        /// <summary>
        /// Generic Helper Function which Resolves Uri References against a Base Uri
        /// </summary>
        /// <param name="uriref">Uri Reference to resolve</param>
        /// <param name="baseUri">Base Uri to resolve against</param>
        /// <returns>Resolved Uri as a String</returns>
        /// <exception cref="UriFormatException">Uri Format Exception if one/both of the URIs is malformed</exception>
        [Obsolete("Replaced by the UriFactory.ResolveUri() method", true)]
        public static Uri ResolveUri(Uri uriref, Uri baseUri)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Resolves a QName into a Uri using the Namespace Mapper and Base Uri provided
        /// </summary>
        /// <param name="qname">QName to resolve</param>
        /// <param name="nsmap">Namespace Map to resolve against</param>
        /// <param name="baseUri">Base Uri to resolve against</param>
        /// <returns></returns>
        [Obsolete("Replaced by the UriFactory.ResolvePrefixedName() method", true)]
        public static String ResolveQName(String qname, INamespaceMapper nsmap, Uri baseUri)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Resolves a QName into a Uri using the Namespace Mapper and Base Uri provided
        /// </summary>
        /// <param name="qname">QName to resolve</param>
        /// <param name="nsmap">Namespace Map to resolve against</param>
        /// <param name="baseUri">Base Uri to resolve against</param>
        /// <param name="allowDefaultPrefixFallback">Whether when the default prefix is used but not defined it can fallback to Base URI</param>
        /// <returns></returns>
        [Obsolete("Replaced by the UriFactory.ResolvePrefixedName() method", true)]
        public static String ResolveQName(String qname, INamespaceMapper nsmap, Uri baseUri, bool allowDefaultPrefixFallback)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Copies a Node so it can be used in another Graph since by default Triples cannot contain Nodes from more than one Graph
        /// </summary>
        /// <param name="original">Node to Copy</param>
        /// <param name="target">Graph to Copy into</param>
        /// <param name="keepOriginalGraphUri">Indicates whether the Copy should preserve the Graph Uri of the Node being copied</param>
        /// <returns></returns>
        [Obsolete("Copying Nodes is no longer required", true)]
        public static INode CopyNode(INode original, IGraph target, bool keepOriginalGraphUri)
        {
            throw new NotSupportedException();
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
        [Obsolete("Copying Nodes is no longer required", true)]
        public static INode CopyNode(INode original, IGraph target)
        {
            throw new NotSupportedException();
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
        [Obsolete("Copying Nodes is no longer required", true)]
        public static INode CopyNode(INode original, INodeFactory target)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Copies a Triple from one Graph to another
        /// </summary>
        /// <param name="t">Triple to copy</param>
        /// <param name="target">Graph to copy to</param>
        /// <returns></returns>
        [Obsolete("Copying Triples is no longer required", true)]
        public static Triple CopyTriple(Triple t, IGraph target)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Copies a Triple from one Graph to another
        /// </summary>
        /// <param name="t">Triple to copy</param>
        /// <param name="target">Graph to copy to</param>
        /// <param name="keepOriginalGraphUri">Indicates whether the Copy should preserve the Graph Uri of the Nodes being copied</param>
        /// <returns></returns>
        [Obsolete("Copying Triples is no longer required", true)]
        public static Triple CopyTriple(Triple t, IGraph target, bool keepOriginalGraphUri)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Helper method which ensures that nodes return a consistent hash code based on their value
        /// </summary>
        /// <param name="n">Node</param>
        /// <returns>Hash code for the node</returns>
        /// <remarks>
        /// This method is provided so that implementors of custom node implementations can ensure that they always return a consistent hash code and thus correctly respect the contract for <em>GetHashCode()</em>
        /// </remarks>
        /// <exception cref="NullReferenceException">Thrown if a null node is passed</exception>
        /// <exception cref="NodeValueException">Thrown if an unknown node type is passed</exception>
        public static int CreateHashCode(INode n)
        {
            if (ReferenceEquals(n, null)) throw new NullReferenceException("Cannot create a hash code for a null node");
            switch (n.NodeType)
            {
                case NodeType.Blank:
                    return Tools.CombineHashCodes(NodeType.Blank, n.AnonID);
                case NodeType.GraphLiteral:
                    return Tools.CombineHashCodes(NodeType.GraphLiteral, n.SubGraph);
                    case NodeType.Literal:
                    if (n.HasLanguage)
                    {
                        return Tools.CombineHashCodes(NodeType.Literal, Tools.CombineHashCodes(n.Value, n.Language));
                    } 
                    else if (n.HasDataType)
                    {
                        return Tools.CombineHashCodes(NodeType.Literal, Tools.CombineHashCodes(n.Value, n.DataType));
                    }
                    else
                    {
                        return Tools.CombineHashCodes(NodeType.Literal, Tools.CombineHashCodes(n.Value, PlainLiteralHashCodeSalt));
                    }
                case NodeType.Uri:
                    return Tools.CombineHashCodes(NodeType.Uri, n.Uri);
                case NodeType.Variable:
                    return Tools.CombineHashCodes(NodeType.Variable, n.VariableName);
                default:
                    throw new NodeValueException("Cannot create a hash code for an unknown node type");
            }
        }

        /// <summary>
        /// Helper method which ensures that triples return a consistent hash code based on their value
        /// </summary>
        /// <param name="t">Triple</param>
        /// <returns>Hash code for the triple</returns>
        internal static int CreateHashCode(Triple t)
        {
            return Tools.CombineHashCodes(t.Subject, Tools.CombineHashCodes(t.Predicate, t.Object));
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
        /// Does a quick and simple combination of Hash Codes for an object and a URI
        /// </summary>
        /// <param name="x">First Object</param>
        /// <param name="y">URI</param>
        /// <returns></returns>
        /// <remarks>This overload is needed because the .Net hash code implementation for URIs is deficient for use for RDF since it treats URIs with the same fragment as being equivalent</remarks>
        public static int CombineHashCodes(Object x, Uri y)
        {
            int hash = 17;
            hash = hash*31 + x.GetHashCode();
            hash = hash*31 + y.GetEnhancedHashCode();
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

            //Output the Request Headers
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

            //Output the Response Uri and Headers
            Console.Error.WriteLine();
            Console.Error.WriteLine("HTTP Response from " + httpResponse.ResponseUri.AbsoluteUri);
#if SILVERLIGHT
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
                //Output the actual Response
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
