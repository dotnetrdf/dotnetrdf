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
using System.Security.Cryptography;
using VDS.RDF.Graphs;
using VDS.RDF.Nodes;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF
{
    /// <summary>
    /// Provides useful Extension Methods for use elsewhere in the Library
    /// </summary>
    public static class Extensions
    {
        private static SHA256Managed _sha256;

        #region Enumerable Extensions

        /// <summary>
        /// Takes a single item and generates an IEnumerable containing only it
        /// </summary>
        /// <typeparam name="T">Type of the enumerable</typeparam>
        /// <param name="item">Item to wrap in an IEnumerable</param>
        /// <returns></returns>
        /// <remarks>
        /// This method taken from Stack Overflow - see <a href="http://stackoverflow.com/questions/1577822/passing-a-single-item-as-ienumerablet">here</a>
        /// </remarks>
        public static IEnumerable<T> AsEnumerable<T>(this T item)
        {
            yield return item;
        }

        /// <summary>
        /// Converts an enumerable of triples into an enumerable of quads with the given graph name
        /// </summary>
        /// <param name="ts">Triples</param>
        /// <param name="graphName">Graph name</param>
        /// <returns></returns>
        public static IEnumerable<Quad> AsQuads(this IEnumerable<Triple> ts, INode graphName)
        {
            return ReferenceEquals(ts, null) ? Enumerable.Empty<Quad>() : ts.Select(t => t.AsQuad(graphName));
        }

        /// <summary>
        /// Determines whether the contents of two enumerables are disjoint
        /// </summary>
        /// <typeparam name="T">Type Parameter</typeparam>
        /// <param name="x">An Enumerable</param>
        /// <param name="y">Another Enumerable</param>
        /// <returns></returns>
        public static bool IsDisjoint<T>(this IEnumerable<T> x, IEnumerable<T> y)
        {
            return x.All(item => !y.Contains(item));
        }

        /// <summary>
        /// Gets the Subset of Triples from an existing Enumerable that have a given Subject
        /// </summary>
        /// <param name="ts">Enumerable of Triples</param>
        /// <param name="subject">Subject to match</param>
        /// <returns></returns>
        public static IEnumerable<Triple> WithSubject(this IEnumerable<Triple> ts, INode subject)
        {
            return (from t in ts
                    where t.Subject.Equals(subject)
                    select t);
        }

        /// <summary>
        /// Gets the Subset of Triples from an existing Enumerable that have a given Predicate
        /// </summary>
        /// <param name="ts">Enumerable of Triples</param>
        /// <param name="predicate">Predicate to match</param>
        /// <returns></returns>
        public static IEnumerable<Triple> WithPredicate(this IEnumerable<Triple> ts, INode predicate)
        {
            return (from t in ts
                    where t.Predicate.Equals(predicate)
                    select t);
        }

        /// <summary>
        /// Gets the Subset of Triples from an existing Enumerable that have a given Object
        /// </summary>
        /// <param name="ts">Enumerable of Triples</param>
        /// <param name="obj">Object to match</param>
        /// <returns></returns>
        public static IEnumerable<Triple> WithObject(this IEnumerable<Triple> ts, INode obj)
        {
            return (from t in ts
                    where t.Object.Equals(obj)
                    select t);
        }

        #endregion

        #region Node Collection replacement extensions

        /// <summary>
        /// Gets the Blank Nodes
        /// </summary>
        /// <param name="ns">Nodes</param>
        /// <returns></returns>
        public static IEnumerable<INode> BlankNodes(this IEnumerable<INode> ns)
        {
            return ns.Where(n => n.NodeType == NodeType.Blank);
        }

        /// <summary>
        /// Gets the Graph Literal Nodes
        /// </summary>
        /// <param name="ns">Nodes</param>
        /// <returns></returns>
        public static IEnumerable<INode> GraphLiteralNodes(this IEnumerable<INode> ns)
        {
            return ns.Where(n => n.NodeType == NodeType.GraphLiteral);
        }

        /// <summary>
        /// Gets the Literal Nodes
        /// </summary>
        /// <param name="ns">Nodes</param>
        /// <returns></returns>
        public static IEnumerable<INode> LiteralNodes(this IEnumerable<INode> ns)
        {
            return ns.Where(n => n.NodeType == NodeType.Literal);
        }

        /// <summary>
        /// Gets the URI Nodes
        /// </summary>
        /// <param name="ns">Nodes</param>
        /// <returns></returns>
        public static IEnumerable<INode> UriNodes(this IEnumerable<INode> ns)
        {
            return ns.Where(n => n.NodeType == NodeType.Uri);
        }

        /// <summary>
        /// Gets the Variable Nodes
        /// </summary>
        /// <param name="ns">Nodes</param>
        /// <returns></returns>
        public static IEnumerable<INode> VariableNodes(this IEnumerable<INode> ns)
        {
            return ns.Where(n => n.NodeType == NodeType.Variable);
        }

        #endregion

        #region Hash Code Extensions

        /// <summary>
        /// Gets an Enhanced Hash Code for a Uri
        /// </summary>
        /// <param name="u">Uri to get Hash Code for</param>
        /// <returns></returns>
        /// <remarks>
        /// The .Net <see cref="Uri">Uri</see> class Hash Code ignores the Fragment ID when computing the Hash Code which means that URIs with the same basic URI but different Fragment IDs have identical Hash Codes.  This is perfectly acceptable and sensible behaviour for normal URI usage since Fragment IDs are only relevant to the Client and not the Server.  <strong>But</strong> in the case of URIs in RDF the Fragment ID is significant and so we need in some circumstances to compute a Hash Code which includes this information.  However in the case on relative URIs we just use the normal hash code of the URI.
        /// </remarks>
        public static int GetEnhancedHashCode(this Uri u)
        {
            if (u == null) throw new ArgumentNullException("u", "Cannot calculate an Enhanced Hash Code for a null URI");
            return u.IsAbsoluteUri ? u.AbsoluteUri.GetHashCode() : u.GetHashCode();
        }

        /// <summary>
        /// Gets an SHA256 Hash for a URI
        /// </summary>
        /// <param name="u">URI to get Hash Code for</param>
        /// <returns></returns>
        public static String GetSha256Hash(this Uri u)
        {
            if (u == null) throw new ArgumentNullException("u");

            //Only instantiate the SHA256 class when we first use it
            if (_sha256 == null) _sha256 = new SHA256Managed();

            Byte[] input = Encoding.UTF8.GetBytes(u.AbsoluteUri);
            Byte[] output = _sha256.ComputeHash(input);

            StringBuilder hash = new StringBuilder();
            foreach (Byte b in output)
            {
                hash.Append(b.ToString("x2"));
            }

            return hash.ToString();
        }

        /// <summary>
        /// Gets a SHA256 Hash for a String
        /// </summary>
        /// <param name="s">String to hash</param>
        /// <returns></returns>
        internal static String GetSha256Hash(this String s)
        {
            if (s == null) throw new ArgumentNullException("s");

            //Only instantiate the SHA256 class when we first use it
            if (_sha256 == null) _sha256 = new SHA256Managed();

            Byte[] input = Encoding.UTF8.GetBytes(s);
            Byte[] output = _sha256.ComputeHash(input);

            StringBuilder hash = new StringBuilder();
            foreach (Byte b in output)
            {
                hash.Append(b.ToString("x2"));
            }

            return hash.ToString();
        }

        #endregion

        #region Node and Triple Copying Extensions

        /// <summary>
        /// Copies a Node to the target Graph
        /// </summary>
        /// <param name="n">Node to copy</param>
        /// <param name="target">Target Graph</param>
        /// <returns></returns>
        /// <remarks>Shorthand for the <see cref="Tools.CopyNode(INode, IGraph)">Tools.CopyNode()</see> method</remarks>
        [Obsolete("Copying Nodes is no longer required", true)]
        public static INode CopyNode(this INode n, IGraph target)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Copies a Node to the target Graph
        /// </summary>
        /// <param name="n">Node to copy</param>
        /// <param name="target">Target Graph</param>
        /// <param name="keepOriginalGraphUri">Indicates whether Nodes should preserve the Graph Uri of the Graph they originated from</param>
        /// <returns></returns>
        /// <remarks>Shorthand for the <see cref="Tools.CopyNode(INode, IGraph, bool)">Tools.CopyNode()</see> method</remarks>
        [Obsolete("Copying Nodes is no longer required", true)]
        public static INode CopyNode(this INode n, IGraph target, bool keepOriginalGraphUri)
        {
            throw new NotSupportedException();
        }


        /// <summary>
        /// Copies a Triple to the target Graph
        /// </summary>
        /// <param name="t">Triple to copy</param>
        /// <param name="target">Target Graph</param>
        /// <returns></returns>
        /// <remarks>Shorthand for the <see cref="Tools.CopyTriple(Triple, IGraph)">Tools.CopyTriple()</see> method</remarks>
        [Obsolete("Copying Triples is no longer required", true)]
        public static Triple CopyTriple(this Triple t, IGraph target)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Copies a Triple to the target Graph
        /// </summary>
        /// <param name="t">Triple to copy</param>
        /// <param name="target">Target Graph</param>
        /// <param name="keepOriginalGraphUri">Indicates whether Nodes should preserve the Graph Uri of the Graph they originated from</param>
        /// <returns></returns>
        /// <remarks>Shorthand for the <see cref="Tools.CopyTriple(Triple, IGraph, bool)">Tools.CopyTriple()</see> method</remarks>
        [Obsolete("Copying Triples is no longer required", true)]
        public static Triple CopyTriple(this Triple t, IGraph target, bool keepOriginalGraphUri)
        {
            return Tools.CopyTriple(t, target, keepOriginalGraphUri);
        }

        /// <summary>
        /// Copies a Triple from one Graph mapping Nodes as appropriate
        /// </summary>
        /// <param name="t">Triple to copy</param>
        /// <param name="target">TargetGraph</param>
        /// <param name="mapping">Mapping of Nodes</param>
        /// <returns></returns>
        public static Triple MapTriple(this Triple t, IGraph target, Dictionary<INode,INode> mapping)
        {
            INode s, p, o;
            if (mapping.ContainsKey(t.Subject))
            {
                s = mapping[t.Subject];
            }
            else
            {
                s = t.Subject;
            }
            if (mapping.ContainsKey(t.Predicate))
            {
                p = mapping[t.Predicate];
            }
            else
            {
                p = t.Predicate;
            }
            if (mapping.ContainsKey(t.Object))
            {
                o = mapping[t.Object];
            }
            else
            {
                o = t.Object;
            }
            return new Triple(s, p, o);
        }

        #endregion

        #region String related Extensions

        /// <summary>
        /// Gets either the String representation of the Object or the Empty String if the object is null
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        public static String ToSafeString(this Object obj)
        {
            return (obj == null) ? String.Empty : obj.ToString();
        }

        /// <summary>
        /// Gets either the String representation of the URI or the Empty String if the URI is null
        /// </summary>
        /// <param name="u">URI</param>
        /// <returns></returns>
        internal static String ToSafeString(this Uri u)
        {
            return (u == null) ? String.Empty : u.AbsoluteUri;
        }

        /// <summary>
        /// Turns a string into a safe URI
        /// </summary>
        /// <param name="str">String</param>
        /// <returns>Either null if the string is null/empty or a URI otherwise</returns>
        internal static Uri ToSafeUri(this String str)
        {
            return (String.IsNullOrEmpty(str) ? null : UriFactory.Create(str));
        }

        /// <summary>
        /// Gets the String representation of the URI formatted using the given Formatter
        /// </summary>
        /// <param name="u">URI</param>
        /// <param name="formatter">URI Formatter</param>
        /// <returns></returns>
        public static String ToString(this Uri u, IUriFormatter formatter)
        {
            return formatter.FormatUri(u);
        }

        /// <summary>
        /// Appends a String to the StringBuilder with an indent of <paramref name="indent"/> spaces
        /// </summary>
        /// <param name="builder">String Builder</param>
        /// <param name="line">String to append</param>
        /// <param name="indent">Indent</param>
        internal static void AppendIndented(this StringBuilder builder, String line, int indent)
        {
            builder.Append(new String(' ', indent) + line);
        }

        /// <summary>
        /// Appends a String to the StringBuilder with an indent of <paramref name="indent"/> spaces
        /// </summary>
        /// <param name="builder">String Builder</param>
        /// <param name="line">String to append</param>
        /// <param name="indent">Indent</param>
        /// <remarks>
        /// Strings containing new lines are split over multiple lines
        /// </remarks>
        internal static void AppendLineIndented(this StringBuilder builder, String line, int indent)
        {
            if (line.Contains('\n'))
            {
                String[] lines = line.Split('\n');
                foreach (String l in lines)
                {
                    if (String.IsNullOrEmpty(l) || l.ToCharArray().All(c => Char.IsWhiteSpace(c))) continue;
                    builder.AppendLine(new String(' ', indent) + l);
                }
            }
            else
            {
                builder.AppendLine(new String(' ', indent) + line);
            }
        }

        /// <summary>
        /// Takes a String and escapes any backslashes in it which are not followed by a valid escape character
        /// </summary>
        /// <param name="value">String value</param>
        /// <param name="cs">Valid Escape Characters i.e. characters which may follow a backslash</param>
        /// <returns></returns>
        public static String EscapeBackslashes(this String value, char[] cs)
        {
            if (value.Length == 0) return value;
            if (value.Length == 1)
            {
                if (value.Equals(@"\"))
                {
                    return @"\\";
                }
                else
                {
                    return value;
                }
            }
            else
            {
                StringBuilder output = new StringBuilder();
                for (int i = 0; i < value.Length; i++)
                {
                    if (value[i] == '\\')
                    {
                        if (i < value.Length - 1)
                        {
                            //Not at end of the input so check whether the next character is a valid escape
                            char next = value[i + 1];
                            if (cs.Contains(next))
                            {
                                //Valid Escape
                                output.Append(value[i]);
                                output.Append(next);
                                i++;
                            }
                            else
                            {
                                //Not a Valid Escape so escape the backslash
                                output.Append(@"\\");
                            }
                        }
                        else
                        {
                            //At the end of the input and found a trailing backslash
                            output.Append(@"\\");
                            break;
                        }
                    }
                    else
                    {
                        output.Append(value[i]);
                    }
                }
                return output.ToString();
            }
        }

        /// <summary>
        /// Determines whether a string is ASCII
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsAscii(this String value)
        {
            if (value.Length == 0) return true;
            return value.ToCharArray().All(c => c <= 127);
        }

        /// <summary>
        /// Determines whether a String is fully escaped
        /// </summary>
        /// <param name="value">String value</param>
        /// <param name="cs">Valid Escape Characters i.e. characters which may follow a backslash</param>
        /// <param name="ds">Characters which must be escaped i.e. must be preceded by a backslash</param>
        /// <returns></returns>
        public static bool IsFullyEscaped(this String value, char[] cs, char[] ds)
        {
            if (value.Length == 0) return true;
            if (value.Length == 1)
            {
                if (value[0] == '\\') return false;
                if (cs.Contains(value[0])) return false;
            }
            else
            {
                //Work through the characters in pairs
                for (int i = 0; i < value.Length; i += 2)
                {
                    char c = value[i];
                    if (i < value.Length - 1)
                    {
                        char d = value[i + 1];
                        if (c == '\\')
                        {
                            //Only fully escaped if followed by an escape character
                            if (!cs.Contains(d)) return false;
                        }
                        else if (ds.Contains(c))
                        {
                            //If c is a character that must be escaped then not fully escaped
                            return false;
                        }
                        else if (d == '\\')
                        {
                            //If d is a backslash shift the index back by 1 so that this will be the first
                            //character of the next character pair we assess
                            i--;
                        }
                        else if (ds.Contains(d))
                        {
                            //If d is a character that must be escaped we know that the preceding character
                            //was not a backslash so the string is not fully escaped
                            return false;
                        }
                    }
                    else
                    {
                        //If trailing character is a backslash or a character that must be escaped then not fully escaped
                        if (c == '\\' || ds.Contains(c)) return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Escapes all occurrences of a given character in a String
        /// </summary>
        /// <param name="value">String</param>
        /// <param name="toEscape">Character to escape</param>
        /// <returns></returns>
        /// <remarks>
        /// Ignores all existing escapes (indicated by a \) and so avoids double escaping as far as possible
        /// </remarks>
        public static String Escape(this String value, char toEscape)
        {
            return value.Escape(toEscape, toEscape);
        }

        /// <summary>
        /// Escapes all occurrences of a given character in a String using the given escape character
        /// </summary>
        /// <param name="value">String</param>
        /// <param name="toEscape">Character to escape</param>
        /// <param name="escapeAs">Character to escape as</param>
        /// <returns></returns>
        /// <remarks>
        /// Ignores all existing escapes (indicated by a \) and so avoids double escaping as far as possible
        /// </remarks>
        public static String Escape(this String value, char toEscape, char escapeAs)
        {
            if (value.Length == 0) return value;
            if (value.Length == 1)
            {
                if (value[0] == toEscape) return new String(new char[] { '\\', toEscape });
                return value;
            }
            else
            {
                //Work through the characters in pairs
                StringBuilder output = new StringBuilder();
                for (int i = 0; i < value.Length; i += 2)
                {
                    char c = value[i];
                    if (i < value.Length - 1)
                    {
                        char d = value[i + 1];
                        if (c == toEscape)
                        {
                            //Must escape this
                            output.Append('\\');
                            output.Append(escapeAs);
                            //Reduce index by 1 as next character is now start of next pair
                            i--;
                        }
                        else if (c == '\\')
                        {
                            //Regardless of the next character we append this to the output since it is an escape
                            //of some kind - whether it relates to the character we want to escape or not is
                            //irrelevant in this case
                            output.Append(c);
                            output.Append(d);
                        }
                        else if (d == toEscape)
                        {
                            //If d is the character to be escaped and we get to this case then it isn't escaped
                            //currently so we must escape it
                            output.Append(c);
                            output.Append('\\');
                            output.Append(escapeAs);
                        }
                        else if (d == '\\')
                        {
                            //If d is a backslash shift the index back by 1 so that this will be the first
                            //character of the next character pair we assess
                            output.Append(c);
                            i--;
                        }
                        else
                        {
                            output.Append(c);
                            output.Append(d);
                        }
                    }
                    else
                    {
                        //If trailing character is character to escape then do so
                        if (c == toEscape)
                        {
                            output.Append('\\');
                        }
                        output.Append(c);
                    }
                }
                return output.ToString();
            }
        }

        #endregion

    }
}
