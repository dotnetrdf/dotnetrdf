using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Writing.Contexts;

namespace VDS.RDF.Writing
{
    /// <summary>
    /// Possible URI Reference Types
    /// </summary>
    enum UriRefType : int
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
        private const String GraphLiteralsUnserializableError = "Graph Literal Nodes are not serializable in {0}";
        /// <summary>
        /// Error message produced when a User attempts to serialize a Graph containing Unknown Node Types
        /// </summary>
        private const String UnknownNodeTypeUnserializableError = "Unknown Node Types cannot be serialized as {0}";
        /// <summary>
        /// Error message produced when a User attempts to serialize a Graph containing Triples with Literal Subjects
        /// </summary>
        private const String LiteralSubjectsUnserializableError = "Triples with a Literal Subject are not serializable in {0}";
        /// <summary>
        /// Error message produced when a User attempts to serialize a Graph containing Triples with Literal Predicates
        /// </summary>
        private const String LiteralPredicatesUnserializableError = "Triples with a Literal Predicate are not serializable in {0}";
        /// <summary>
        /// Error message produced when a User attempts to serialize a Graph containing Triples with Blank Node Predicates
        /// </summary>
        private const String BlankPredicatesUnserializableError = "Triples with a Blank Node Predicate are not serializable in {0}";
        /// <summary>
        /// Error message produced when a User attempts to serialize a Graph containing URIs which cannot be reduced to a URIRef or QName as required by the serialization
        /// </summary>
        public const String UnreducablePropertyURIUnserializable = "Unable to serialize this Graph since a Property has an unreducable URI";
        /// <summary>
        /// Error message produced when a User attempts to serialize a Graph containing collections where a collection item has more than one rdf:first triple
        /// </summary>
        public const String MalformedCollectionWithMultipleFirsts = "This RDF Graph contains more than one rdf:first Triple for an Item in a Collection which means the Graph is not serializable";
        /// <summary>
        /// Error messages produced when errors occur in a multi-threaded writing process
        /// </summary>
        public const String ThreadedOutputError = "One/more errors occurred while outputting RDF in {0} using a multi-threaded writing process";

        /// <summary>
        /// Gets an Error message indicating that Graph Literals are not serializable with the appropriate RDF format name inserted in the error
        /// </summary>
        /// <param name="format">RDF format (syntax)</param>
        /// <returns></returns>
        public static String GraphLiteralsUnserializable(String format)
        {
            return String.Format(GraphLiteralsUnserializableError, format);
        }

        /// <summary>
        /// Gets an Error message indicating that Unknown Node Types are not serializable with the appropriate RDF format name inserted in the error
        /// </summary>
        /// <param name="format">RDF format (syntax)</param>
        /// <returns></returns>
        public static String UnknownNodeTypeUnserializable(String format)
        {
            return String.Format(UnknownNodeTypeUnserializableError, format);
        }

        /// <summary>
        /// Gets an Error message indicating that Literal Subjects are not serializable with the appropriate RDF format name inserted in the error
        /// </summary>
        /// <param name="format">RDF format (syntax)</param>
        /// <returns></returns>
        public static String LiteralSubjectsUnserializable(String format)
        {
            return String.Format(LiteralSubjectsUnserializableError, format);
        }

        /// <summary>
        /// Gets an Error message indicating that Literal Predicates are not serializable with the appropriate RDF format name inserted in the error
        /// </summary>
        /// <param name="format">RDF format (syntax)</param>
        /// <returns></returns>
        public static String LiteralPredicatesUnserializable(String format)
        {
            return String.Format(LiteralPredicatesUnserializableError, format);
        }

        /// <summary>
        /// Gets an Error message indicating that Blank Node Predicates are not serializable with the appropriate RDF format name inserted in the error
        /// </summary>
        /// <param name="format">RDF format (syntax)</param>
        /// <returns></returns>
        public static String BlankPredicatesUnserializable(String format)
        {
            return String.Format(BlankPredicatesUnserializableError, format);
        }

        /// <summary>
        /// Gets an Error message indicating that a multi-threading writer process failed
        /// </summary>
        /// <param name="format">RDF format (syntax)</param>
        /// <returns></returns>
        public static String ThreadedOutputFailure(String format)
        {
            return String.Format(ThreadedOutputError, format);
        }
    }

    /// <summary>
    /// Indicates which Segment of a Triple Node Output is being generated for
    /// </summary>
    /// <remarks>Used by Writers to ensure restrictions on which Nodes can appear where in the syntax are enforced</remarks>
    enum TripleSegment
    {
        Subject,
        Predicate,
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
    public class OutputRDFCollection : Stack<INode>
    {
        private bool _explicit;

        /// <summary>
        /// Creates a new Instance of a Collection
        /// </summary>
        /// <param name="explicitCollection">Whether the collection is explicit (specified using square bracket notation) or implicit (specified using normal parentheses)</param>
        public OutputRDFCollection(bool explicitCollection)
        {
            this._explicit = explicitCollection;
        }

        /// <summary>
        /// Gets whether this is an Explicit collection
        /// </summary>
        public bool IsExplicit
        {
            get
            {
                return this._explicit;
            }
        }
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
        /// <summary>
        /// Array of Default Graph URIs used by parsers which parse RDF dataset formats
        /// </summary>
        public static String[] StoreDefaultGraphURIs = new String[] 
        { 
            TriGParser.DefaultGraphURI, 
            NQuadsParser.DefaultGraphURI,
#if !NO_XMLDOM
            TriXParser.DefaultGraphURI,
#endif
            GraphCollection.DefaultGraphUri };

        private static String _uriEncodeForXmlPattern = "&([^;]+)$";

        /// <summary>
        /// Determines whether a Blank Node ID is valid as-is when serialised in NTriple like syntaxes
        /// </summary>
        /// <param name="id">ID to test</param>
        /// <returns></returns>
        /// <remarks>If false is returned then the writer will alter the ID in some way</remarks>
        public static bool IsValidBlankNodeID(String id)
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
        /// Determines whether a given Uri refers to one of the Default Graph URIs assigned to the default Graph when parsing from some RDF dataset syntax
        /// </summary>
        /// <param name="u">Uri to test</param>
        /// <returns></returns>
        public static bool IsDefaultGraph(Uri u)
        {
            if (u == null)
            {
                return true;
            }
            else
            {
                return StoreDefaultGraphURIs.Contains(u.ToString());
            }
        }

        /// <summary>
        /// Helper method which finds Collections expressed in the Graph which can be compressed into concise collection syntax constructs in some RDF syntaxes
        /// </summary>
        /// <param name="g">Graph to find collections in</param>
        /// <param name="triplesDone">Triple Collection in which Triples that have been output are to be listed</param>
        public static Dictionary<INode, OutputRDFCollection> FindCollections(IGraph g, BaseTripleCollection triplesDone)
        {
            //Clear existing Collections Dictionary as we may well get reused
            Dictionary<INode, OutputRDFCollection> collections = new Dictionary<INode, OutputRDFCollection>();

            //First we're going to look for implicit collections we can represent using the 
            //brackets syntax of (a b c)

            //Prepare the RDF Nodes we need
            INode first, rest, nil;
            first = g.CreateUriNode(new Uri(RdfSpecsHelper.RdfListFirst));
            rest = g.CreateUriNode(new Uri(RdfSpecsHelper.RdfListRest));
            nil = g.CreateUriNode(new Uri(RdfSpecsHelper.RdfListNil));

            //Find all rdf:rest rdf:nil Triples
            //HasPropertyValueSelector cendsel = new HasPropertyValueSelector(rest, nil);
            foreach (Triple t in g.GetTriplesWithPredicateObject(rest, nil))
            {
                //Build the collection recursively
                OutputRDFCollection c = new OutputRDFCollection(false);
                triplesDone.Add(t);

                //Get the thing that is the rdf:first related to this rdf:rest
                //SubjectHasPropertySelector relfirstsel = new SubjectHasPropertySelector(t.Subject, first);
                IEnumerable<Triple> firsts = g.GetTriplesWithSubjectPredicate(t.Subject, first);//g.GetTriples(relfirstsel).Distinct();
                INode temp;
                if (firsts.Count() > 1)
                {
                    //Strange error
                    throw new RdfOutputException(WriterErrorMessages.MalformedCollectionWithMultipleFirsts);
                }
                else
                {
                    //Stick this item onto the Stack
                    temp = firsts.First().Object;
                    c.Push(temp);
                    triplesDone.Add(firsts.First());
                }

                //See if this thing is the rdf:rest of anything else
                do
                {
                    //cendsel = new HasPropertyValueSelector(rest, firsts.First().Subject);
                    IEnumerable<Triple> ts = g.GetTriplesWithPredicateObject(rest, firsts.First().Subject);//g.GetTriples(cendsel);

                    //Stop when there isn't a rdf:rest
                    if (ts.Count() == 0)
                    {
                        break;
                    }

                    foreach (Triple t2 in ts)
                    {
                        //relfirstsel = new SubjectHasPropertySelector(t2.Subject, first);
                        firsts = g.GetTriplesWithSubjectPredicate(t2.Subject, first).Distinct();//g.GetTriples(relfirstsel).Distinct();
                        triplesDone.Add(t2);

                        if (firsts.Count() > 1)
                        {
                            //Strange error
                            throw new RdfOutputException(WriterErrorMessages.MalformedCollectionWithMultipleFirsts);
                        }
                        else
                        {
                            //Stick this item onto the Stack
                            temp = firsts.First().Object;
                            c.Push(temp);
                            triplesDone.Add(firsts.First());
                        }
                    }
                } while (true);

                collections.Add(firsts.First().Subject, c);
            }

            //Now we want to look for explicit collections which are representable
            //using Blank Node syntax [p1 o1; p2 o2; p3 o3]

            foreach (BlankNode b in g.Nodes.BlankNodes)
            {
                if (collections.ContainsKey(b)) continue;

                List<Triple> ts = g.GetTriplesWithSubject(b).Concat(g.GetTriplesWithObject(b)).Distinct().ToList();
                ts.RemoveAll(t => t.Predicate.Equals(first));
                ts.RemoveAll(t => t.Predicate.Equals(rest));

                if (ts.Count == 0)
                {
                    //This Blank Node is only used once
                    //Add an empty explicit collection - we'll interpret this as [] later
                    collections.Add(b, new OutputRDFCollection(true));
                }
                else
                {
                    //Sort into Triples with the Blank Node as the Subject, Predicate or Object
                    List<Triple> subjTriples = new List<Triple>();
                    List<Triple> predTriples = new List<Triple>();
                    List<Triple> objTriples = new List<Triple>();

                    foreach (Triple t2 in ts)
                    {
                        if (t2.Subject.Equals(b))
                        {
                            subjTriples.Add(t2);
                        }
                        else if (t2.Predicate.Equals(b))
                        {
                            predTriples.Add(t2);
                        }
                        else if (t2.Object.Equals(b))
                        {
                            objTriples.Add(t2);
                        }
                    }

                    if (subjTriples.Count == 0) continue;

                    OutputRDFCollection c = new OutputRDFCollection(true);
                    if (predTriples.Count > 0 || objTriples.Count > 0)
                    {
                        //The collection is the Object/Predicate of some other Triples
                        //All the Subject Triples represent the Predicate Object list of the Collection
                        foreach (Triple t2 in subjTriples)
                        {
                            c.Push(t2.Object);
                            c.Push(t2.Predicate);
                            triplesDone.Add(t2);
                        }
                        collections.Add(subjTriples[0].Subject, c);
                    }
                    else
                    {
                        //The Collection is the Subject of some Triple
                        //Assume the first Triple is the Triple with it as the subject
                        //The other Triples are it's Predicate Object List
                        for (int i = 1; i < subjTriples.Count; i++)
                        {
                            c.Push(subjTriples[i].Object);
                            c.Push(subjTriples[i].Predicate);
                            triplesDone.Add(subjTriples[i]);
                        }
                        collections.Add(subjTriples[0].Subject, c);
                    }
                }
            }

            return collections;
        }

        /// <summary>
        /// Helper method which finds Collections expressed in the Graph which can be compressed into concise collection syntax constructs in some RDF syntaxes
        /// </summary>
        /// <param name="context">Writer Context</param>
        /// <param name="mode">Collection Search Mode</param>
        public static void FindCollections(ICollectionCompressingWriterContext context, CollectionSearchMode mode)
        {
            //Prepare the RDF Nodes we need
            INode first, rest, nil;
            first = context.Graph.CreateUriNode(new Uri(RdfSpecsHelper.RdfListFirst));
            rest = context.Graph.CreateUriNode(new Uri(RdfSpecsHelper.RdfListRest));
            nil = context.Graph.CreateUriNode(new Uri(RdfSpecsHelper.RdfListNil));

            //First we're going to look for implicit collections we can represent using the 
            //brackets syntax of (a b c)

            if (mode == CollectionSearchMode.All || mode == CollectionSearchMode.ImplicitOnly)
            {
                //Find all rdf:rest rdf:nil Triples
                //HasPropertyValueSelector cendsel = new HasPropertyValueSelector(rest, nil);
                foreach (Triple t in context.Graph.GetTriplesWithPredicateObject(rest, nil))
                {
                    //Build the collection recursively
                    OutputRDFCollection c = new OutputRDFCollection(false);
                    context.TriplesDone.Add(t);

                    //Get the thing that is the rdf:first related to this rdf:rest
                    //SubjectHasPropertySelector relfirstsel = new SubjectHasPropertySelector(t.Subject, first);
                    IEnumerable<Triple> firsts = context.Graph.GetTriplesWithSubjectPredicate(t.Subject, first);//context.Graph.GetTriples(relfirstsel).Distinct();
                    INode temp;
                    if (firsts.Count() > 1)
                    {
                        //Strange error
                        throw new RdfOutputException(WriterErrorMessages.MalformedCollectionWithMultipleFirsts);
                    }
                    else
                    {
                        //Stick this item onto the Stack
                        temp = firsts.First().Object;
                        c.Push(temp);
                        context.TriplesDone.Add(firsts.First());
                    }

                    //See if this thing is the rdf:rest of anything else
                    do
                    {
                        //cendsel = new HasPropertyValueSelector(rest, firsts.First().Subject);
                        IEnumerable<Triple> ts = context.Graph.GetTriplesWithPredicateObject(rest, firsts.First().Subject);//context.Graph.GetTriples(cendsel);

                        //Stop when there isn't a rdf:rest
                        if (ts.Count() == 0)
                        {
                            break;
                        }

                        foreach (Triple t2 in ts)
                        {
                            //relfirstsel = new SubjectHasPropertySelector(t2.Subject, first);
                            firsts = context.Graph.GetTriplesWithSubjectPredicate(t2.Subject, first).Distinct();//context.Graph.GetTriples(relfirstsel).Distinct();
                            context.TriplesDone.Add(t2);

                            if (firsts.Count() > 1)
                            {
                                //Strange error
                                throw new RdfOutputException(WriterErrorMessages.MalformedCollectionWithMultipleFirsts);
                            }
                            else
                            {
                                //Stick this item onto the Stack
                                temp = firsts.First().Object;
                                c.Push(temp);
                                context.TriplesDone.Add(firsts.First());
                            }
                        }
                    } while (true);

                    context.Collections.Add(firsts.First().Subject, c);
                }
            }

            //Now we want to look for explicit collections which are representable
            //using Blank Node syntax [p1 o1; p2 o2; p3 o3]

            if (mode == CollectionSearchMode.All || mode == CollectionSearchMode.ExplicitOnly)
            {
                foreach (BlankNode b in context.Graph.Nodes.BlankNodes)
                {
                    if (context.Collections.ContainsKey(b)) continue;

                    List<Triple> ts = context.Graph.GetTriplesWithSubject(b).Concat(context.Graph.GetTriplesWithObject(b)).Distinct().ToList();
                    ts.RemoveAll(t => t.Predicate.Equals(first));
                    ts.RemoveAll(t => t.Predicate.Equals(rest));

                    if (ts.Count == 0)
                    {
                        //This Blank Node is only used once
                        //Add an empty explicit collection - we'll interpret this as [] later
                        context.Collections.Add(b, new OutputRDFCollection(true));
                    }
                    else
                    {
                        //Sort into Triples with the Blank Node as the Subject, Predicate or Object
                        List<Triple> subjTriples = new List<Triple>();
                        List<Triple> predTriples = new List<Triple>();
                        List<Triple> objTriples = new List<Triple>();

                        foreach (Triple t2 in ts)
                        {
                            if (t2.Subject.Equals(b))
                            {
                                subjTriples.Add(t2);
                            }
                            else if (t2.Predicate.Equals(b))
                            {
                                predTriples.Add(t2);
                            }
                            else if (t2.Object.Equals(b))
                            {
                                objTriples.Add(t2);
                            }
                        }

                        if (subjTriples.Count == 0) continue;

                        OutputRDFCollection c = new OutputRDFCollection(true);
                        if (predTriples.Count > 0 || objTriples.Count > 0)
                        {
                            //The collection is the Object/Predicate of some other Triples
                            //All the Subject Triples represent the Predicate Object list of the Collection
                            foreach (Triple t2 in subjTriples)
                            {
                                c.Push(t2.Object);
                                c.Push(t2.Predicate);
                                context.TriplesDone.Add(t2);
                            }
                            context.Collections.Add(subjTriples[0].Subject, c);
                        }
                        else
                        {
                            //The Collection is the Subject of some Triple
                            //Assume the first Triple is the Triple with it as the subject
                            //The other Triples are it's Predicate Object List
                            for (int i = 1; i < subjTriples.Count; i++)
                            {
                                c.Push(subjTriples[i].Object);
                                c.Push(subjTriples[i].Predicate);
                                context.TriplesDone.Add(subjTriples[i]);
                            }
                            context.Collections.Add(subjTriples[0].Subject, c);
                        }
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
        public static String EncodeForXml(String value)
        {
            value = Regex.Replace(value, _uriEncodeForXmlPattern, "&amp;$1");
            if (value.EndsWith("&")) value += "amp;";
            //value = value.Replace("\"", "&quot;");
            value = value.Replace("<", "&lt;");
            value = value.Replace(">", "&gt;");
            return value;
        }
    }
}