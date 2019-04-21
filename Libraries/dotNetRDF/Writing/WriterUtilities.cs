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
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using VDS.RDF.Parsing;
using VDS.RDF.Writing.Contexts;

namespace VDS.RDF.Writing
{
    /// <summary>
    /// Possible URI Reference Types
    /// </summary>
    internal enum UriRefType : int
    {
        /// <summary>
        /// Must be a QName
        /// </summary>
        QName = 1,
        /// <summary>
        /// May be a QName or a URI
        /// </summary>
        QNameOrUri = 2,
        /// <summary>
        /// URI Reference
        /// </summary>
        UriRef = 3,
        /// <summary>
        /// URI
        /// </summary>
        Uri = 4
    }

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
    /// Class containing constants for standardised Writer Error Messages
    /// </summary>
    public static class WriterErrorMessages
    {
        /// <summary>
        /// Error message produced when a User attempts to serialize a Graph containing Graph Literals
        /// </summary>
        private const string GraphLiteralsUnserializableError = "Graph Literal Nodes are not serializable in {0}";
        /// <summary>
        /// Error message produced when a User attempts to serialize a Graph containing Unknown Node Types
        /// </summary>
        private const string UnknownNodeTypeUnserializableError = "Unknown Node Types cannot be serialized as {0}";
        /// <summary>
        /// Error message produced when a User attempts to serialize a Graph containing Triples with Literal Subjects
        /// </summary>
        private const string LiteralSubjectsUnserializableError = "Triples with a Literal Subject are not serializable in {0}";
        /// <summary>
        /// Error message produced when a User attempts to serialize a Graph containing Triples with Literal Predicates
        /// </summary>
        private const string LiteralPredicatesUnserializableError = "Triples with a Literal Predicate are not serializable in {0}";
        /// <summary>
        /// Error message produced when a User attempts to serialized a Graph containing Triples with Graph Literal Predicates
        /// </summary>
        private const string GraphLiteralPredicatesUnserializableError = "Triples with a Graph Literal Predicate are not serializable in {0}";
        /// <summary>
        /// Error message produced when a User attempts to serialize a Graph containing Triples with Blank Node Predicates
        /// </summary>
        private const string BlankPredicatesUnserializableError = "Triples with a Blank Node Predicate are not serializable in {0}";
        /// <summary>
        /// Error message produced when a User attempts to serialize a Graph containing URIs which cannot be reduced to a URIRef or QName as required by the serialization
        /// </summary>
        public const string UnreducablePropertyURIUnserializable = "Unable to serialize this Graph since a Property has an unreducable URI";
        /// <summary>
        /// Error message produced when a User attempts to serialize a Graph containing collections where a collection item has more than one rdf:first triple
        /// </summary>
        public const string MalformedCollectionWithMultipleFirsts = "This RDF Graph contains more than one rdf:first Triple for an Item in a Collection which means the Graph is not serializable";
        /// <summary>
        /// Error messages produced when errors occur in a multi-threaded writing process
        /// </summary>
        public const string ThreadedOutputError = "One/more errors occurred while outputting RDF in {0} using a multi-threaded writing process";
        /// <summary>
        /// Error message produced when a User attempts to serialize a Variable Node in a format which does not support it
        /// </summary>
        public const string VariableNodesUnserializableError = "Variable Nodes cannot be serialized as {0}";

        /// <summary>
        /// Gets an Error message indicating that Graph Literals are not serializable with the appropriate RDF format name inserted in the error
        /// </summary>
        /// <param name="format">RDF format (syntax)</param>
        /// <returns></returns>
        public static string GraphLiteralsUnserializable(string format)
        {
            return string.Format(GraphLiteralsUnserializableError, format);
        }

        /// <summary>
        /// Gets an Error message indicating that Unknown Node Types are not serializable with the appropriate RDF format name inserted in the error
        /// </summary>
        /// <param name="format">RDF format (syntax)</param>
        /// <returns></returns>
        public static string UnknownNodeTypeUnserializable(string format)
        {
            return string.Format(UnknownNodeTypeUnserializableError, format);
        }

        /// <summary>
        /// Gets an Error message indicating that Variable Nodes are not serializable with the appropriate RDF format name inserted in the error
        /// </summary>
        /// <param name="format">RDF format (syntax)</param>
        /// <returns></returns>
        public static string VariableNodesUnserializable(string format)
        {
            return string.Format(VariableNodesUnserializableError, format);
        }

        /// <summary>
        /// Gets an Error message indicating that Literal Subjects are not serializable with the appropriate RDF format name inserted in the error
        /// </summary>
        /// <param name="format">RDF format (syntax)</param>
        /// <returns></returns>
        public static string LiteralSubjectsUnserializable(string format)
        {
            return string.Format(LiteralSubjectsUnserializableError, format);
        }

        /// <summary>
        /// Gets an Error message indicating that Literal Predicates are not serializable with the appropriate RDF format name inserted in the error
        /// </summary>
        /// <param name="format">RDF format (syntax)</param>
        /// <returns></returns>
        public static string LiteralPredicatesUnserializable(string format)
        {
            return string.Format(LiteralPredicatesUnserializableError, format);
        }

        /// <summary>
        /// Gets an Error message indicating that Graph Literal Predicates are not serializable with the appropriate RDF format name inserted in the error
        /// </summary>
        /// <param name="format">RDF format (syntax)</param>
        /// <returns></returns>
        public static string GraphLiteralPredicatesUnserializable(string format)
        {
            return string.Format(GraphLiteralPredicatesUnserializableError, format);
        }

        /// <summary>
        /// Gets an Error message indicating that Blank Node Predicates are not serializable with the appropriate RDF format name inserted in the error
        /// </summary>
        /// <param name="format">RDF format (syntax)</param>
        /// <returns></returns>
        public static string BlankPredicatesUnserializable(string format)
        {
            return string.Format(BlankPredicatesUnserializableError, format);
        }

        /// <summary>
        /// Gets an Error message indicating that a multi-threading writer process failed
        /// </summary>
        /// <param name="format">RDF format (syntax)</param>
        /// <returns></returns>
        public static string ThreadedOutputFailure(string format)
        {
            return string.Format(ThreadedOutputError, format);
        }
    }

    /// <summary>
    /// Indicates which Segment of a Triple Node Output is being generated for
    /// </summary>
    /// <remarks>
    /// Used by Writers and Formatters to ensure restrictions on which Nodes can appear where in the syntax are enforced
    /// </remarks>
    public enum TripleSegment
    {
        /// <summary>
        /// Subject of the Triple
        /// </summary>
        Subject,
        /// <summary>
        /// Predicate of the Triple
        /// </summary>
        Predicate,
        /// <summary>
        /// Object of the Triple
        /// </summary>
        Object
    }

    /// <summary>
    /// Controls what type of collections
    /// </summary>
    public enum CollectionSearchMode
    {
        /// <summary>
        /// Find all collections
        /// </summary>
        All,
        /// <summary>
        /// Find explicit collections only (those specified with Blank Node syntax)
        /// </summary>
        ExplicitOnly,
        /// <summary>
        /// Find implicit collections only (those using rdf:first and rdf:rest)
        /// </summary>
        ImplicitOnly
    }

    /// <summary>
    /// Class used to store Collections as part of the writing process for Compressing Writers
    /// </summary>
    public class OutputRdfCollection
    {
        private bool _explicit;
        private bool _written = false;
        private List<Triple> _triples = new List<Triple>();

        /// <summary>
        /// Creates a new Instance of a Collection
        /// </summary>
        /// <param name="explicitCollection">Whether the collection is explicit (specified using square bracket notation) or implicit (specified using normal parentheses)</param>
        public OutputRdfCollection(bool explicitCollection)
        {
            _explicit = explicitCollection;
        }

        /// <summary>
        /// Gets whether this is an Explicit collection (specified using square bracket notation)
        /// </summary>
        public bool IsExplicit => _explicit;

        /// <summary>
        /// Gets/Sets whether the Collection has been written
        /// </summary>
        public bool HasBeenWritten
        {
            get => _written;
            set => _written = value;
        }

        /// <summary>
        /// Gets the Triples that make up the Collection
        /// </summary>
        public List<Triple> Triples => _triples;
    }

    /// <summary>
    /// Possible Output Formats for Nodes
    /// </summary>
    public enum NodeFormat
    {
        /// <summary>
        /// Format for NTriples
        /// </summary>
        NTriples,
        /// <summary>
        /// Format for Turtle
        /// </summary>
        Turtle,
        /// <summary>
        /// Format for Notation 3
        /// </summary>
        Notation3,
        /// <summary>
        /// Format for Uncompressed Turtle
        /// </summary>
        UncompressedTurtle,
        /// <summary>
        /// Format for Uncompressed Notation 3
        /// </summary>
        UncompressedNotation3
    }

    /// <summary>
    /// Helper methods for writers
    /// </summary>
    public static class WriterHelper
    {
        private static string _uriEncodeForXmlPattern = @"&([^;&\s]*)(?=\s|$|&)";

        /// <summary>
        /// Determines whether a Blank Node ID is valid as-is when serialised in NTriple like syntaxes (Turtle/N3/SPARQL)
        /// </summary>
        /// <param name="id">ID to test</param>
        /// <returns></returns>
        /// <remarks>If false is returned then the writer will alter the ID in some way</remarks>
        public static bool IsValidBlankNodeID(string id)
        {
            if (id == null)
            {
                // Can't be null
                return false;
            }
            else if (id.Equals(string.Empty))
            {
                // Can't be empty
                return false;
            }
            else
            {
                char[] cs = id.ToCharArray();
                if (char.IsDigit(cs[0]) || cs[0] == '-' || cs[0] == '_')
                {
                    // Can't start with a Digit, Hyphen or Underscore
                    return false;
                }
                else
                {
                    // Otherwise OK
                    return true;
                }
            }

        }

        /// <summary>
        /// Determines whether a Blank Node ID is valid as-is when serialised as NTriples
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool IsValidStrictBlankNodeID(string id)
        {
            if (id == null)
            {
                // Can't be null
                return false;
            }
            else if (id.Equals(string.Empty))
            {
                // Can't be empty
                return false;
            }
            else
            {
                // All characters must be alphanumeric and not start with a digit in NTriples
                char[] cs = id.ToCharArray();
                return char.IsLetter(cs[0]) && cs.All(c => char.IsLetterOrDigit(c) && c <= 127);
            }
        }

        /// <summary>
        /// Determines whether a given Uri refers to one of the Default Graph URIs assigned to the default Graph when parsing from some RDF dataset syntax
        /// </summary>
        /// <param name="u">Uri to test</param>
        /// <returns></returns>
        [Obsolete("No longer required since all code that uses 'magic' URIs to refer to the default graph has been removed from the API", true)]
        public static bool IsDefaultGraph(Uri u)
        {
            return u == null;
        }

        /// <summary>
        /// Helper method which finds Collections expressed in the Graph which can be compressed into concise collection syntax constructs in some RDF syntaxes
        /// </summary>
        /// <param name="g">Graph to find collections in</param>
        /// <param name="triplesDone">Triple Collection in which Triples that have been output are to be listed</param>
        [Obsolete("Use the alternative overloads of this method which take an ICollectionCompressingWriterContext instead", true)]
        public static Dictionary<INode, OutputRdfCollection> FindCollections(IGraph g, BaseTripleCollection triplesDone)
        {
            throw new NotSupportedException("No longer supported");
        }

        /// <summary>
        /// Helper method which finds Collections expressed in the Graph which can be compressed into concise collection syntax constructs in some RDF syntaxes
        /// </summary>
        /// <param name="context">Writer Context</param>
        /// <param name="mode">Collection Search Mode</param>
        public static void FindCollections(ICollectionCompressingWriterContext context, CollectionSearchMode mode)
        {
            // Prepare the RDF Nodes we need
            INode first, rest, nil;
            first = context.Graph.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfListFirst));
            rest = context.Graph.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfListRest));
            nil = context.Graph.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfListNil));

            // First we're going to look for implicit collections we can represent using the
            // brackets syntax of (a b c)

            if (mode == CollectionSearchMode.All || mode == CollectionSearchMode.ImplicitOnly)
            {
                // Find all rdf:rest rdf:nil Triples
                foreach (Triple t in context.Graph.GetTriplesWithPredicateObject(rest, nil))
                {
                    // If has named list node cannot compress
                    if (t.Subject.NodeType != NodeType.Blank)
                    {
                        break;
                    }

                    // Build the collection recursively
                    OutputRdfCollection c = new OutputRdfCollection(false);

                    // Get the thing that is the rdf:first related to this rdf:rest
                    Triple[] firsts = context.Graph.GetTriplesWithSubjectPredicate(t.Subject, first).ToArray();//context.Graph.GetTriples(relfirstsel).Distinct();
                    Triple temp = null;
                    if (firsts.Length > 1)
                    {
                        // Strange error
                        throw new RdfOutputException(WriterErrorMessages.MalformedCollectionWithMultipleFirsts);
                    }
                    else if (firsts.Length == 1)
                    {
                        // Stick this item onto the Stack
                        temp = firsts[0];
                        c.Triples.Add(temp);
                    }

                    // See if this thing is the rdf:rest of anything else
                    do
                    {
                        Triple[] ts = context.Graph.GetTriplesWithPredicateObject(rest, firsts.First().Subject).ToArray();

                        // Stop when there isn't a rdf:rest
                        if (ts.Length == 0)
                        {
                            break;
                        }

                        foreach (Triple t2 in ts)
                        {
                            firsts = context.Graph.GetTriplesWithSubjectPredicate(t2.Subject, first).Distinct(new FullTripleComparer(new FastNodeComparer())).ToArray();

                            if (firsts.Length > 1)
                            {
                                // Strange error
                                throw new RdfOutputException(WriterErrorMessages.MalformedCollectionWithMultipleFirsts);
                            }
                            else if (firsts.Length == 1)
                            {
                                // Stick this item onto the Stack
                                temp = firsts[0];
                                // If Item is a named list node cannot compress
                                if (temp.Subject.NodeType != NodeType.Blank)
                                {
                                    break;
                                }

                                c.Triples.Add(temp);
                            }
                        }
                    } while (true);

                    // Can only compress if every List Node has a Blank Node Subject
                    if (c.Triples.All(x => x.Subject.NodeType == NodeType.Blank))
                    {
                        context.Collections.Add(firsts[0].Subject, c);
                    }
                }
            }

            // Now we want to look for explicit collections which are representable
            // using Blank Node syntax [p1 o1; p2 o2; p3 o3]
            if (mode == CollectionSearchMode.All || mode == CollectionSearchMode.ExplicitOnly)
            {
                List<IBlankNode> bnodes = context.Graph.Nodes.BlankNodes().ToList();
                foreach (IBlankNode b in bnodes)
                {
                    if (context.Collections.ContainsKey(b))
                    {
                        continue;
                    }

                    List<Triple> ts = context.Graph.GetTriples(b).ToList();
                    ts.RemoveAll(t => t.Predicate.Equals(first));
                    ts.RemoveAll(t => t.Predicate.Equals(rest));

                    if (ts.Count <= 1)
                    {
                        // This Blank Node is only used once
                        // Add an empty explicit collection - we'll interpret this as [] later
                        context.Collections.Add(b, new OutputRdfCollection(true));
                    }
                    else
                    {
                        if (context.Graph.GetTriplesWithObject(b).Count() == 1)
                        {
                            ts.RemoveAll(t => t.Object.Equals(b));
                        }
                        OutputRdfCollection c = new OutputRdfCollection(true);
                        c.Triples.AddRange(ts);
                        context.Collections.Add(b, c);
                    }
                }
            }

            // If no collections found then no further processing
            if (context.Collections.Count == 0)
            {
                return;
            }

            // Once we've found all the Collections we need to check which are actually eligible for compression
            List<KeyValuePair<INode, OutputRdfCollection>> cs = context.Collections.ToList();

            // 1 - If all the Triples pertaining to a particular Node are in the Collection then a collection is not eligible
            foreach (KeyValuePair<INode, OutputRdfCollection> kvp in cs)
            {
                OutputRdfCollection c = kvp.Value;
                if (c.IsExplicit)
                {
                    // For explicit collections if all Triples mentioning the Target Blank Node are in the Collection then can't compress
                    // If there are no Triples in the Collection then this is a single use Blank Node so can always compress
                    if (c.Triples.Count > 0 && c.Triples.Count == context.Graph.GetTriples(kvp.Key).Count())
                    {
                        // TODO: This doesn't work because it can conflict
                        // In this case we can remove a single Triple from the Collection and hope this allows us to compress
                        // context.Collections[kvp.Key].Triples.RemoveAt(0);

                        context.Collections.Remove(kvp.Key);
                    }
                }
                else
                {
                    // For implicit collections if the number of Triples in the Collection is exactly ((t*3) - 1) those in the Graph then
                    // can't compress i.e. the collection is not linked to anything else
                    // Or if the number of mentions compared to the expected mentions differs by more than 1 then
                    // can't compress i.e. the collection is linked to more than one thing
                    int mentions = context.Graph.GetTriples(kvp.Key).Count();
                    int expectedMentions = ((c.Triples.Count * 3) - 1);
                    if (expectedMentions == mentions || mentions - expectedMentions != 1)
                    {
                        context.Collections.Remove(kvp.Key);
                    }
                }
            }
            if (context.Collections.Count == 0)
            {
                return;
            }

            // 2 - Look for cyclic collection dependencies
            cs = context.Collections.OrderByDescending(kvp => kvp.Value.Triples.Count).ToList();

            // First build up a dependencies table
            Dictionary<INode, HashSet<INode>> dependencies = new Dictionary<INode, HashSet<INode>>();
            foreach (KeyValuePair<INode, OutputRdfCollection> kvp in cs)
            {
                OutputRdfCollection c = kvp.Value;

                // Empty Blank Node Collections cannot be cyclic i.e. []
                if (c.Triples.Count == 0)
                {
                    continue;
                }

                // Otherwise check each Object of the Triples for other Blank Nodes
                HashSet<INode> ds = new HashSet<INode>(new FastNodeComparer());
                foreach (Triple t in c.Triples)
                {
                    // Only care about Blank Nodes which aren't the collection root but are the root for another collection
                    if (t.Object.NodeType == NodeType.Blank && !t.Object.Equals(kvp.Key) && context.Collections.ContainsKey(t.Object))
                    {
                        ds.Add(t.Object);
                    }
                }
                if (ds.Count > 0)
                {
                    dependencies.Add(kvp.Key, ds);
                }
            }

            // Now go back through that table looking for cycles
            foreach (INode n in dependencies.Keys)
            {
                HashSet<INode> ds = dependencies[n];

                if (ds.Count == 0)
                {
                    continue;
                }

                foreach (INode d in ds.ToList())
                {
                    if (dependencies.ContainsKey(d))
                    {
                        foreach (INode dd in dependencies[d])
                        {
                            ds.Add(dd);
                        }
                    }
                }

                //We can tell if there is a cycle since ds will now contain n
                if (ds.Contains(n))
                {
                    context.Collections.Remove(n);
                }
            }
            if (context.Collections.Count == 0)
            {
                return;
            }

            // Finally fill out the TriplesDone for each Collection
            foreach (KeyValuePair<INode, OutputRdfCollection> kvp in context.Collections)
            {
                OutputRdfCollection c = kvp.Value;
                if (c.IsExplicit)
                {
                    foreach (Triple t in c.Triples)
                    {
                        context.TriplesDone.Add(t);
                    }
                }
                else
                {
                    INode temp = kvp.Key;
                    for (int i = 0; i < c.Triples.Count; i++)
                    {
                        context.TriplesDone.Add(c.Triples[i]);
                        if (i < c.Triples.Count - 1)
                        {
                            context.TriplesDone.Add(new Triple(c.Triples[i].Subject, rest, c.Triples[i + 1].Subject));
                        }
                        else
                        {
                            context.TriplesDone.Add(new Triple(c.Triples[i].Subject, rest, nil));
                        }
                    }
                }
            }

            // As a final sanity check look for any Explicit Collection Key which is used more than once
            cs = context.Collections.ToList();
            foreach (KeyValuePair<INode, OutputRdfCollection> kvp in cs)
            {
                OutputRdfCollection c = kvp.Value;
                if (c.IsExplicit)
                {
                    int mentions = context.Graph.GetTriples(kvp.Key).Count(t => !context.TriplesDone.Contains(t));
                    if (mentions - 1 > c.Triples.Count)
                    {
                        context.Collections.Remove(kvp.Key);
                    }
                }
            }
        }

        /// <summary>
        /// Helper method which finds Collections expressed in the Graph which can be compressed into concise collection syntax constructs in some RDF syntaxes
        /// </summary>
        /// <param name="context">Writer Context</param>
        public static void FindCollections(ICollectionCompressingWriterContext context)
        {
            FindCollections(context, CollectionSearchMode.All);
        }

        /// <summary>
        /// Encodes values for use in XML
        /// </summary>
        /// <param name="value">Value to encode</param>
        /// <returns>
        /// The value with any ampersands escaped to &amp;
        /// </returns>
        public static string EncodeForXml(string value)
        {
            while (Regex.IsMatch(value, _uriEncodeForXmlPattern))
            {
                value = Regex.Replace(value, _uriEncodeForXmlPattern, "&amp;$1");
            }
            if (value.EndsWith("&"))
            {
                value += "amp;";
            }

            return value.Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("'", "&apos;")
                .Replace("\"", "&quot;");
        }

        /// <summary>
        /// Get a list of all triples in the specified graph, sorted by subject and then predicate.
        /// </summary>
        /// <param name="graph">The graph whose triples are to be returned</param>
        /// <returns>A list of the triples in <paramref name="graph"/> sorted by their subject and then predicate.</returns>
        public static List<Triple> GetTriplesSortedBySubjectPredicate(IGraph graph)
        {
            var ts = graph.Triples.ToList();
            SortTriplesBySubjectPredicate(ts);
            return ts;
        }

        /// <summary>
        /// Sort the provided list of triples by subject and then predicate. The list is modified in-place
        /// </summary>
        /// <param name="ts">The list of triples to be sorted</param>
        public static void SortTriplesBySubjectPredicate(List<Triple> ts)
        {
            var capacity = ts.Count;
            var sortHelperDictionary = new Dictionary<INode, Dictionary<INode, List<Triple>>>(capacity);
            // Fill dictionary
            foreach (var triple in ts)
            {
                if (!sortHelperDictionary.ContainsKey(triple.Subject))
                {
                    sortHelperDictionary.Add(triple.Subject, new Dictionary<INode, List<Triple>>());
                }

                if (!sortHelperDictionary[triple.Subject].ContainsKey(triple.Predicate))
                {
                    sortHelperDictionary[triple.Subject].Add(triple.Predicate, new List<Triple>());
                }

                sortHelperDictionary[triple.Subject][triple.Predicate].Add(triple);
            }

            ts.Clear();
            var keys = sortHelperDictionary.Keys.ToArray();
            Array.Sort(keys, new FastNodeComparer());
            foreach (var subjectKey in keys)
            {
                var predicateKeys = sortHelperDictionary[subjectKey].Keys.ToArray();
                Array.Sort(predicateKeys, new FastNodeComparer());
                foreach (var predicateKey in predicateKeys)
                {
                    ts.AddRange(sortHelperDictionary[subjectKey][predicateKey]);
                }
            }

        }
    }
}