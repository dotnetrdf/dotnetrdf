/*

Copyright Robert Vesse 2009-10
rvesse@vdesign-studios.com

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
using System.Security.Cryptography;
using System.Xml;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
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

        #endregion

        #region Triple Selection Extensions

        /// <summary>
        /// Gets the Subset of Triples from an existing Enumerable that are accepted by a given Selector
        /// </summary>
        /// <param name="ts">Enumerable of Triples</param>
        /// <param name="selector">Selector to apply</param>
        /// <returns></returns>
        public static IEnumerable<Triple> Subset(this IEnumerable<Triple> ts, ISelector<Triple> selector) {
            return (from t in ts
                    where selector.Accepts(t)
                    select t);
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

        #region Hash Code Extensions

        /// <summary>
        /// Gets an Enhanced Hash Code for a Uri
        /// </summary>
        /// <param name="u">Uri to get Hash Code for</param>
        /// <returns></returns>
        /// <remarks>
        /// The .Net <see cref="Uri">Uri</see> class Hash Code ignores the Fragment ID when computing the Hash Code which means that URIs with the same basic URI but different Fragment IDs have identical Hash Codes.  This is perfectly acceptable and sensible behaviour for normal URI usage since Fragment IDs are only relevant to the Client and not the Server.  <strong>But</strong> in the case of URIs in RDF the Fragment ID is significant and so we need in some circumstances to compute a Hash Code which includes this information.
        /// </remarks>
        public static int GetEnhancedHashCode(this Uri u)
        {
            if (u == null) throw new ArgumentNullException("Cannot calculate an Enhanced Hash Code for a null URI");
            return u.ToString().GetHashCode();
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

            Byte[] input = Encoding.UTF8.GetBytes(u.ToString());
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

        #region Triple Assertion and Retraction Extensions

        /// <summary>
        /// Asserts a new Triple in the Graph
        /// </summary>
        /// <param name="g">Graph to assert in</param>
        /// <param name="subj">Subject</param>
        /// <param name="pred">Predicate</param>
        /// <param name="obj">Object</param>
        /// <remarks>Handy method which means you can assert a Triple by specifying the Subject, Predicate and Object without having to explicity declare a new Triple</remarks>
        public static void Assert(this IGraph g, INode subj, INode pred, INode obj)
        {
            g.Assert(new Triple(subj, pred, obj));
        }

        /// <summary>
        /// Retracts a Triple from the Graph
        /// </summary>
        /// <param name="g">Graph to retract from</param>
        /// <param name="subj">Subject</param>
        /// <param name="pred">Predicate</param>
        /// <param name="obj">Object</param>
        /// <remarks>Handy method which means you can retract a Triple by specifying the Subject, Predicate and Object without having to explicity declare a new Triple</remarks>
        public static void Retract(this IGraph g, INode subj, INode pred, INode obj)
        {
            g.Retract(new Triple(subj, pred, obj));
        }

        #endregion

        #region Node and Triple Copying Extensions

        /// <summary>
        /// Copies a Node to the target Graph
        /// </summary>
        /// <param name="n">Node to copy</param>
        /// <param name="target">Target Graph</param>
        /// <returns></returns>
        /// <remarks>Shorthand for the <see cref="Tools.CopyNode">Tools.CopyNode()</see> method</remarks>
        public static INode CopyNode(this INode n, IGraph target)
        {
            return Tools.CopyNode(n, target);
        }

        /// <summary>
        /// Copies a Node to the target Graph
        /// </summary>
        /// <param name="n">Node to copy</param>
        /// <param name="target">Target Graph</param>
        /// <param name="keepOriginalGraphUri">Indicates whether Nodes should preserve the Graph Uri of the Graph they originated from</param>
        /// <returns></returns>
        /// <remarks>Shorthand for the <see cref="Tools.CopyNode">Tools.CopyNode()</see> method</remarks>
        public static INode CopyNode(this INode n, IGraph target, bool keepOriginalGraphUri)
        {
            return Tools.CopyNode(n, target, keepOriginalGraphUri);
        }


        /// <summary>
        /// Copies a Triple to the target Graph
        /// </summary>
        /// <param name="t">Triple to copy</param>
        /// <param name="target">Target Graph</param>
        /// <returns></returns>
        /// <remarks>Shorthand for the <see cref="Tools.CopyTriple">Tools.CopyTriple()</see> method</remarks>
        public static Triple CopyTriple(this Triple t, IGraph target)
        {
            return Tools.CopyTriple(t, target);
        }

        /// <summary>
        /// Copies a Triple to the target Graph
        /// </summary>
        /// <param name="t">Triple to copy</param>
        /// <param name="target">Target Graph</param>
        /// <param name="keepOriginalGraphUri">Indicates whether Nodes should preserve the Graph Uri of the Graph they originated from</param>
        /// <returns></returns>
        /// <remarks>Shorthand for the <see cref="Tools.CopyTriple">Tools.CopyTriple()</see> method</remarks>
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
                s = mapping[t.Subject].CopyNode(target);
            }
            else
            {
                s = t.Subject.CopyNode(target);
            }
            if (mapping.ContainsKey(t.Predicate))
            {
                p = mapping[t.Predicate].CopyNode(target);
            }
            else
            {
                p = t.Predicate.CopyNode(target);
            }
            if (mapping.ContainsKey(t.Object))
            {
                o = mapping[t.Object].CopyNode(target);
            }
            else
            {
                o = t.Object.CopyNode(target);
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
        internal static String ToSafeString(this Object obj)
        {
            return (obj == null) ? String.Empty : obj.ToString();
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
                    if (!l.Equals(String.Empty)) builder.AppendLine(new String(' ', indent) + l);
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

    /// <summary>
    /// Provides useful Extension Methods for working with Graphs
    /// </summary>
    public static class GraphExtensions
    {
        /// <summary>
        /// Executes a SPARQL Query on a Graph
        /// </summary>
        /// <param name="g">Graph to query</param>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <returns></returns>
        public static Object ExecuteQuery(this IGraph g, String sparqlQuery)
        {
            TripleStore store = new TripleStore();
            store.Add(g);
            return store.ExecuteQuery(sparqlQuery);
        }

        /// <summary>
        /// Executes a SPARQL Query on a Graph handling the results using the handlers provided
        /// </summary>
        /// <param name="g">Graph to query</param>
        /// <param name="rdfHandler">RDF Handler</param>
        /// <param name="resultsHandler">SPARQL Results Handler</param>
        /// <param name="sparqlQuery">SPARQL Query</param>
        public static void ExecuteQuery(this IGraph g, IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, String sparqlQuery)
        {
            TripleStore store = new TripleStore();
            store.Add(g);
            store.ExecuteQuery(rdfHandler, resultsHandler, sparqlQuery);
        }

        /// <summary>
        /// Executes a SPARQL Query on a Graph
        /// </summary>
        /// <param name="g">Graph to query</param>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <returns></returns>
        public static Object ExecuteQuery(this IGraph g, SparqlParameterizedString sparqlQuery)
        {
            return g.ExecuteQuery(sparqlQuery.ToString());
        }

        /// <summary>
        /// Executes a SPARQL Query on a Graph handling the results using the handlers provided
        /// </summary>
        /// <param name="g">Graph to query</param>
        /// <param name="rdfHandler">RDF Handler</param>
        /// <param name="resultsHandler">SPARQL Results Handler</param>
        /// <param name="sparqlQuery">SPARQL Query</param>
        public static void ExecuteQuery(this IGraph g, IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, SparqlParameterizedString sparqlQuery)
        {
            g.ExecuteQuery(rdfHandler, resultsHandler, sparqlQuery.ToString());
        }

        /// <summary>
        /// Executes a SPARQL Query on a Graph
        /// </summary>
        /// <param name="g">Graph to query</param>
        /// <param name="query">SPARQL Query</param>
        /// <returns></returns>
        public static Object ExecuteQuery(this IGraph g, SparqlQuery query)
        {
            TripleStore store = new TripleStore();
            store.Add(g);
            return store.ExecuteQuery(query);
        }

        /// <summary>
        /// Executes a SPARQL Query on a Graph handling the results using the handlers provided
        /// </summary>
        /// <param name="g">Graph to query</param>
        /// <param name="rdfHandler">RDF Handler</param>
        /// <param name="resultsHandler">SPARQL Results Handler</param>
        /// <param name="query">SPARQL Query</param>
        public static void ExecuteQuery(this IGraph g, IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, SparqlQuery query)
        {
            TripleStore store = new TripleStore();
            store.Add(g);
            store.ExecuteQuery(rdfHandler, resultsHandler, query);
        }

        /// <summary>
        /// Loads RDF data from a file into a Graph
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="file">File to load from</param>
        /// <param name="parser">Parser to use</param>
        /// <remarks>
        /// This is just a shortcut to using the static <strong>Load()</strong> methods from the <see cref="FileLoader">FileLoader</see> class located in the <see cref="VDS.RDF.Parsing">Parsing</see> namespace
        /// </remarks>
        public static void LoadFromFile(this IGraph g, String file, IRdfReader parser)
        {
            FileLoader.Load(g, file, parser);
        }

        /// <summary>
        /// Loads RDF data from a file into a Graph
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="file">File to load from</param>
        /// <remarks>
        /// This is just a shortcut to using the static <strong>Load()</strong> methods from the <see cref="FileLoader">FileLoader</see> class located in the <see cref="VDS.RDF.Parsing">Parsing</see> namespace
        /// </remarks>
        public static void LoadFromFile(this IGraph g, String file)
        {
            FileLoader.Load(g, file);
        }

#if !SILVERLIGHT

        /// <summary>
        /// Loads RDF data from a URI into a Graph
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="u">URI to load from</param>
        /// <param name="parser">Parser to use</param>
        /// <remarks>
        /// This is just a shortcut to using the static <strong>Load()</strong> methods from the <see cref="UriLoader">UriLoader</see> class located in the <see cref="VDS.RDF.Parsing">Parsing</see> namespace
        /// </remarks>
        public static void LoadFromUri(this IGraph g, Uri u, IRdfReader parser)
        {
            UriLoader.Load(g, u, parser);
        }

        /// <summary>
        /// Loads RDF data from a URI into a Graph
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="u">URI to load from</param>
        /// <remarks>
        /// This is just a shortcut to using the static <strong>Load()</strong> methods from the <see cref="UriLoader">UriLoader</see> class located in the <see cref="VDS.RDF.Parsing">Parsing</see> namespace
        /// </remarks>
        public static void LoadFromUri(this IGraph g, Uri u)
        {
            UriLoader.Load(g, u);
        }

#endif

        //REQ: Add LoadFromUri extensions that do the loading asychronously for use on Silverlight/Windows Phone 7

        /// <summary>
        /// Loads RDF data from a String into a Graph
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="data">Data to load</param>
        /// <param name="parser">Parser to use</param>
        /// <remarks>
        /// This is just a shortcut to using the static <strong>Parse()</strong> methods from the <see cref="StringParser">StringParser</see> class located in the <see cref="VDS.RDF.Parsing">Parsing</see> namespace
        /// </remarks>
        public static void LoadFromString(this IGraph g, String data, IRdfReader parser)
        {
            StringParser.Parse(g, data, parser);
        }

        /// <summary>
        /// Loads RDF data from a String into a Graph
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="data">Data to load</param>
        /// <remarks>
        /// This is just a shortcut to using the static <strong>Parse()</strong> methods from the <see cref="StringParser">StringParser</see> class located in the <see cref="VDS.RDF.Parsing">Parsing</see> namespace
        /// </remarks>
        public static void LoadFromString(this IGraph g, String data)
        {
            StringParser.Parse(g, data);
        }

        /// <summary>
        /// Loads RDF data from an Embedded Resource into a Graph
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="resource">Assembly qualified name of the resource to load from</param>
        /// <remarks>
        /// This is just a shortcut to using the static <strong>Load()</strong> methods from the <see cref="EmbeddedResourceLoader">EmbeddedResourceLoader</see> class located in the <see cref="VDS.RDF.Parsing">Parsing</see> namespace
        /// </remarks>
        public static void LoadFromEmbeddedResource(this IGraph g, String resource)
        {
            EmbeddedResourceLoader.Load(g, resource);
        }

        /// <summary>
        /// Loads RDF data from an Embedded Resource into a Graph
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="resource">Assembly qualified name of the resource to load from</param>
        /// <param name="parser">Parser to use</param>
        /// <remarks>
        /// This is just a shortcut to using the static <strong>Load()</strong> methods from the <see cref="EmbeddedResourceLoader">EmbeddedResourceLoader</see> class located in the <see cref="VDS.RDF.Parsing">Parsing</see> namespace
        /// </remarks>
        public static void LoadFromEmbeddedResource(this IGraph g, String resource, IRdfReader parser)
        {
            EmbeddedResourceLoader.Load(g, resource, parser);
        }

        /// <summary>
        /// Saves a Graph to a File
        /// </summary>
        /// <param name="g">Graph to save</param>
        /// <param name="file">File to save to</param>
        /// <param name="writer">Writer to use</param>
        public static void SaveToFile(this IGraph g, String file, IRdfWriter writer)
        {
            if (writer == null)
            {
                g.SaveToFile(file);
            } 
            else 
            {
                writer.Save(g, file);
            }
        }

        /// <summary>
        /// Saves a Graph to a File
        /// </summary>
        /// <param name="g">Graph to save</param>
        /// <param name="file">File to save to</param>
        public static void SaveToFile(this IGraph g, String file)
        {
            IRdfWriter writer = MimeTypesHelper.GetWriter(MimeTypesHelper.GetMimeTypes(System.IO.Path.GetExtension(file)));
            writer.Save(g, file);
        }
    }

    /// <summary>
    /// Provides useful Extension Methods for working with Triple Stores
    /// </summary>
    public static class TripleStoreExtensions
    {
        /// <summary>
        /// Loads an RDF dataset from a file into a Triple Store
        /// </summary>
        /// <param name="store">Triple Store to load into</param>
        /// <param name="file">File to load from</param>
        /// <param name="parser">Parser to use</param>
        /// <remarks>
        /// This is just a shortcut to using the static <strong>Load()</strong> methods from the <see cref="FileLoader">FileLoader</see> class located in the <see cref="VDS.RDF.Parsing">Parsing</see> namespace
        /// </remarks>
        public static void LoadFromFile(this ITripleStore store, String file, IStoreReader parser)
        {
            FileLoader.Load(store, file, parser);
        }

        /// <summary>
        /// Loads an RDF dataset from a file into a Triple Store
        /// </summary>
        /// <param name="store">Triple Store to load into</param>
        /// <param name="file">File to load from</param>
        /// <remarks>
        /// This is just a shortcut to using the static <strong>Load()</strong> methods from the <see cref="FileLoader">FileLoader</see> class located in the <see cref="VDS.RDF.Parsing">Parsing</see> namespace
        /// </remarks>
        public static void LoadFromFile(this ITripleStore store, String file)
        {
            FileLoader.Load(store, file);
        }

#if !SILVERLIGHT

        /// <summary>
        /// Loads an RDF dataset from a URI into a Triple Store
        /// </summary>
        /// <param name="store">Triple Store to load into</param>
        /// <param name="u">URI to load from</param>
        /// <param name="parser">Parser to use</param>
        /// <remarks>
        /// This is just a shortcut to using the static <strong>Load()</strong> methods from the <see cref="UriLoader">UriLoader</see> class located in the <see cref="VDS.RDF.Parsing">Parsing</see> namespace
        /// </remarks>
        public static void LoadFromUri(this ITripleStore store, Uri u, IStoreReader parser)
        {
            UriLoader.Load(store, u, parser);
        }

        /// <summary>
        /// Loads an RDF dataset from a URI into a Triple Store
        /// </summary>
        /// <param name="store">Triple Store to load into</param>
        /// <param name="u">URI to load from</param>
        /// <remarks>
        /// This is just a shortcut to using the static <strong>Load()</strong> methods from the <see cref="UriLoader">UriLoader</see> class located in the <see cref="VDS.RDF.Parsing">Parsing</see> namespace
        /// </remarks>
        public static void LoadFromUri(this ITripleStore store, Uri u)
        {
            UriLoader.Load(store, u);
        }

#endif

        /// <summary>
        /// Loads an RDF dataset from a String into a Triple Store
        /// </summary>
        /// <param name="store">Triple Store to load into</param>
        /// <param name="data">Data to load</param>
        /// <param name="parser">Parser to use</param>
        /// <remarks>
        /// This is just a shortcut to using the static <strong>ParseDataset()</strong> methods from the <see cref="StringParser">StringParser</see> class located in the <see cref="VDS.RDF.Parsing">Parsing</see> namespace
        /// </remarks>
        public static void LoadFromString(this ITripleStore store, String data, IStoreReader parser)
        {
            StringParser.ParseDataset(store, data, parser);
        }

        /// <summary>
        /// Loads an RDF dataset from a String into a Triple Store
        /// </summary>
        /// <param name="store">Triple Store to load into</param>
        /// <param name="data">Data to load</param>
        /// <remarks>
        /// This is just a shortcut to using the static <strong>ParseDataset()</strong> methods from the <see cref="StringParser">StringParser</see> class located in the <see cref="VDS.RDF.Parsing">Parsing</see> namespace
        /// </remarks>
        public static void LoadFromString(this ITripleStore store, String data)
        {
            StringParser.ParseDataset(store, data);
        }

        /// <summary>
        /// Loads an RDF dataset from an Embedded Resource into a Triple Store
        /// </summary>
        /// <param name="store">Triple Store to load into</param>
        /// <param name="resource">Assembly Qualified Name of the Embedded Resource to load from</param>
        /// <param name="parser">Parser to use</param>
        /// <remarks>
        /// This is just a shortcut to using the static <strong>Load()</strong> methods from the <see cref="EmbeddedResourceLoader">EmbeddedResourceLoader</see> class located in the <see cref="VDS.RDF.Parsing">Parsing</see> namespace
        /// </remarks>
        public static void LoadFromEmbeddedResource(this ITripleStore store, String resource, IStoreReader parser)
        {
            EmbeddedResourceLoader.Load(store, resource, parser);
        }

        /// <summary>
        /// Loads an RDF dataset from an Embedded Resource into a Triple Store
        /// </summary>
        /// <param name="store">Triple Store to load into</param>
        /// <param name="resource">Assembly Qualified Name of the Embedded Resource to load from</param>
        /// <remarks>
        /// This is just a shortcut to using the static <strong>Load()</strong> methods from the <see cref="EmbeddedResourceLoader">EmbeddedResourceLoader</see> class located in the <see cref="VDS.RDF.Parsing">Parsing</see> namespace
        /// </remarks>
        public static void LoadFromEmbeddedResource(this ITripleStore store, String resource)
        {
            EmbeddedResourceLoader.Load(store, resource);
        }

        /// <summary>
        /// Saves a Triple Store to a file
        /// </summary>
        /// <param name="store">Triple Store to save</param>
        /// <param name="file">File to save to</param>
        /// <param name="writer">Writer to use</param>
        public static void SaveToFile(this ITripleStore store, String file, IStoreWriter writer)
        {
            if (writer == null)
            {
                store.SaveToFile(file);
            }
            else
            {
                writer.Save(store, new VDS.RDF.Storage.Params.StreamParams(file));
            }
        }

        /// <summary>
        /// Saves a Triple Store to a file
        /// </summary>
        /// <param name="store">Triple Store to save</param>
        /// <param name="file">File to save to</param>
        public static void SaveToFile(this ITripleStore store, String file)
        {
            IStoreWriter writer = MimeTypesHelper.GetStoreWriter(MimeTypesHelper.GetMimeType(System.IO.Path.GetExtension(file)));
            writer.Save(store, new VDS.RDF.Storage.Params.StreamParams(file));
        }
    }

    /// <summary>
    /// Provides extension methods for converting primitive types into appropriately typed Literal Nodes
    /// </summary>
    public static class LiteralExtensions
    {
        /// <summary>
        /// Creates a new Boolean typed literal
        /// </summary>
        /// <param name="b">Boolean</param>
        /// <param name="factory">Node Factory create in</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if the Factory argument is null</exception>
        public static ILiteralNode ToLiteral(this bool b, INodeFactory factory)
        {
            if (factory == null) throw new ArgumentNullException("factory", "Cannot create a Literal Node in a null Node Factory");

            return factory.CreateLiteralNode(b.ToString().ToLower(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeBoolean));
        }

        /// <summary>
        /// Creates a new Byte typed literal
        /// </summary>
        /// <param name="b">Byte</param>
        /// <param name="factory">Node Factory create in</param>
        /// <returns></returns>
        /// <remarks>
        /// Byte in .Net is actually equivalent to Unsigned Byte in XML Schema so depending on the value of the Byte the type will either be xsd:byte if it fits or xsd:usignedByte
        /// </remarks>
        public static ILiteralNode ToLiteral(this byte b, INodeFactory factory)
        {
            if (factory == null) throw new ArgumentNullException("factory", "Cannot create a Literal Node in a null Node Factory");

            if (b > 128)
            {
                //If value is > 128 must use unsigned byte as the type as xsd:byte has range -127 to 128 
                //while .Net byte has range 0-255
                return factory.CreateLiteralNode(XmlConvert.ToString(b), new Uri(XmlSpecsHelper.XmlSchemaDataTypeUnsignedByte));
            }
            else
            {
                return factory.CreateLiteralNode(XmlConvert.ToString(b), new Uri(XmlSpecsHelper.XmlSchemaDataTypeByte));
            }
        }

        /// <summary>
        /// Creates a new Byte typed literal
        /// </summary>
        /// <param name="b">Byte</param>
        /// <param name="factory">Node Factory create in</param>
        /// <returns></returns>
        /// <remarks>
        /// SByte in .Net is directly equivalent to Byte in XML Schema so the type will always be xsd:byte
        /// </remarks>
        public static ILiteralNode ToLiteral(this sbyte b, INodeFactory factory)
        {
            if (factory == null) throw new ArgumentNullException("factory", "Cannot create a Literal Node in a null Node Factory");

            return factory.CreateLiteralNode(XmlConvert.ToString(b), new Uri(XmlSpecsHelper.XmlSchemaDataTypeByte));
        }

        /// <summary>
        /// Creates a new Date Time typed literal
        /// </summary>
        /// <param name="dt">Date Time</param>
        /// <param name="factory">Node Factory create in</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if the Factory argument is null</exception>
        public static ILiteralNode ToLiteral(this DateTime dt, INodeFactory factory)
        {
            if (factory == null) throw new ArgumentNullException("factory", "Cannot create a Literal Node in a null Node Factory");

            return factory.CreateLiteralNode(dt.ToString(XmlSpecsHelper.XmlSchemaDateTimeFormat), new Uri(XmlSpecsHelper.XmlSchemaDataTypeDateTime));
        }

        /// <summary>
        /// Creates a new Date typed literal
        /// </summary>
        /// <param name="dt">Date Time</param>
        /// <param name="factory">Node Factory create in</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if the Factory argument is null</exception>
        public static ILiteralNode ToLiteralDate(this DateTime dt, INodeFactory factory)
        {
            if (factory == null) throw new ArgumentNullException("factory", "Cannot create a Literal Node in a null Node Factory");

            return factory.CreateLiteralNode(dt.ToString(XmlSpecsHelper.XmlSchemaDateFormat), new Uri(XmlSpecsHelper.XmlSchemaDataTypeDate));
        }

        /// <summary>
        /// Creates a new Time typed literal
        /// </summary>
        /// <param name="dt">Date Time</param>
        /// <param name="factory">Node Factory create in</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if the Factory argument is null</exception>
        public static ILiteralNode ToLiteralTime(this DateTime dt, INodeFactory factory)
        {
            if (factory == null) throw new ArgumentNullException("factory", "Cannot create a Literal Node in a null Node Factory");

            return factory.CreateLiteralNode(dt.ToString(XmlSpecsHelper.XmlSchemaTimeFormat), new Uri(XmlSpecsHelper.XmlSchemaDataTypeTime));
        }
        
        /// <summary>
        /// Creates a new duration typed literal
        /// </summary>
        /// <param name="t">Time Span</param>
        /// <param name="factory">Node Factory create in</param>
        /// <returns></returns>
        public static ILiteralNode ToLiteral(this TimeSpan t, INodeFactory factory)
        {
            if (factory == null) throw new ArgumentNullException("factory", "Cannot create a Literal Node in a null Node Factory");

            return factory.CreateLiteralNode(XmlConvert.ToString(t), new Uri(XmlSpecsHelper.XmlSchemaDataTypeDayTimeDuration));
        }

        /// <summary>
        /// Creates a new Decimal typed literal
        /// </summary>
        /// <param name="d">Decimal</param>
        /// <param name="factory">Node Factory create in</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if the Factory argument is null</exception>
        public static ILiteralNode ToLiteral(this decimal d, INodeFactory factory)
        {
            if (factory == null) throw new ArgumentNullException("factory", "Cannot create a Literal Node in a null Node Factory");

            return factory.CreateLiteralNode(d.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeDecimal));
        }

        /// <summary>
        /// Creates a new Double typed literal
        /// </summary>
        /// <param name="d">Double</param>
        /// <param name="factory">Node Factory create in</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if the Factory argument is null</exception>
        public static ILiteralNode ToLiteral(this double d, INodeFactory factory)
        {
            if (factory == null) throw new ArgumentNullException("factory", "Cannot create a Literal Node in a null Node Factory");

            return factory.CreateLiteralNode(d.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeDouble));
        }

        /// <summary>
        /// Creates a new Float typed literal
        /// </summary>
        /// <param name="f">Float</param>
        /// <param name="factory">Node Factory create in</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if the Factory argument is null</exception>
        public static ILiteralNode ToLiteral(this float f, INodeFactory factory)
        {
            if (factory == null) throw new ArgumentNullException("factory", "Cannot create a Literal Node in a null Node Factory");

            return factory.CreateLiteralNode(f.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeFloat));
        }

        /// <summary>
        /// Creates a new Integer typed literal
        /// </summary>
        /// <param name="i">Integer</param>
        /// <param name="factory">Node Factory create in</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if the Factory argument is null</exception>
        public static ILiteralNode ToLiteral(this short i, INodeFactory factory)
        {
            if (factory == null) throw new ArgumentNullException("factory", "Cannot create a Literal Node in a null Node Factory");

            return factory.CreateLiteralNode(i.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger));
        }

        /// <summary>
        /// Creates a new Integer typed literal
        /// </summary>
        /// <param name="i">Integer</param>
        /// <param name="factory">Node Factory create in</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if the Factory argument is null</exception>
        public static ILiteralNode ToLiteral(this int i, INodeFactory factory)
        {
            if (factory == null) throw new ArgumentNullException("factory", "Cannot create a Literal Node in a null Node Factory");

            return factory.CreateLiteralNode(i.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger));
        }

        /// <summary>
        /// Creates a new Integer typed literal
        /// </summary>
        /// <param name="l">Integer</param>
        /// <param name="factory">Node Factory create in</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if the Factory argument is null</exception>
        public static ILiteralNode ToLiteral(this long l, INodeFactory factory)
        {
            if (factory == null) throw new ArgumentNullException("factory", "Cannot create a Literal Node in a null Node Factory");

            return factory.CreateLiteralNode(l.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger));
        }

        /// <summary>
        /// Creates a new String typed literal
        /// </summary>
        /// <param name="s">String</param>
        /// <param name="factory">Node Factory create in</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if the Graph/String argument is null</exception>
        public static ILiteralNode ToLiteral(this String s, INodeFactory factory)
        {
            if (factory == null) throw new ArgumentNullException("factory", "Cannot create a Literal Node in a null Node Factory");
            if (s == null) throw new ArgumentNullException("s", "Cannot create a Literal Node for a null String");

            return factory.CreateLiteralNode(s, new Uri(XmlSpecsHelper.XmlSchemaDataTypeString));
        }
    }
}
