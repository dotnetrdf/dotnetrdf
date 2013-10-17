using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Graphs;
using VDS.RDF.Specifications;
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
            this._explicit = explicitCollection;
        }

        /// <summary>
        /// Gets whether this is an Explicit collection (specified using square bracket notation)
        /// </summary>
        public bool IsExplicit
        {
            get
            {
                return this._explicit;
            }
        }

        /// <summary>
        /// Gets/Sets whether the Collection has been written
        /// </summary>
        public bool HasBeenWritten
        {
            get
            {
                return this._written;
            }
            set
            {
                this._written = value;
            }
        }

        /// <summary>
        /// Gets the Triples that make up the Collection
        /// </summary>
        public List<Triple> Triples
        {
            get
            {
                return this._triples;
            }
        }
    }

    public static class CompressionHelper
    {
        /// <summary>
        /// Helper method which finds Collections expressed in the Graph which can be compressed into concise collection syntax constructs in some RDF syntaxes
        /// </summary>
        /// <param name="context">Writer Context</param>
        /// <param name="mode">Collection Search Mode</param>
        public static void FindCollections(ICollectionCompressingWriterContext context, CollectionSearchMode mode)
        {
            //Prepare the RDF Nodes we need
            INode first, rest, nil;
            first = context.Graph.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfListFirst));
            rest = context.Graph.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfListRest));
            nil = context.Graph.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfListNil));

            //First we're going to look for implicit collections we can represent using the 
            //brackets syntax of (a b c)

            if (mode == CollectionSearchMode.All || mode == CollectionSearchMode.ImplicitOnly)
            {
                //Find all rdf:rest rdf:nil Triples
                foreach (Triple t in context.Graph.GetTriplesWithPredicateObject(rest, nil))
                {
                    //If has named list node cannot compress
                    if (t.Subject.NodeType != NodeType.Blank) break;

                    //Build the collection recursively
                    OutputRdfCollection c = new OutputRdfCollection(false);

                    //Get the thing that is the rdf:first related to this rdf:rest
                    IEnumerable<Triple> firsts = context.Graph.GetTriplesWithSubjectPredicate(t.Subject, first);//context.Graph.GetTriples(relfirstsel).Distinct();
                    Triple temp;
                    if (firsts.Count() > 1)
                    {
                        //Strange error
                        throw new RdfOutputException(WriterErrorMessages.MalformedCollectionWithMultipleFirsts);
                    }
                    else
                    {
                        //Stick this item onto the Stack
                        temp = firsts.First();
                        c.Triples.Add(temp);
                    }

                    //See if this thing is the rdf:rest of anything else
                    do
                    {
                        IEnumerable<Triple> ts = context.Graph.GetTriplesWithPredicateObject(rest, firsts.First().Subject);

                        //Stop when there isn't a rdf:rest
                        if (ts.Count() == 0)
                        {
                            break;
                        }

                        foreach (Triple t2 in ts)
                        {
                            firsts = context.Graph.GetTriplesWithSubjectPredicate(t2.Subject, first).Distinct();

                            if (firsts.Count() > 1)
                            {
                                //Strange error
                                throw new RdfOutputException(WriterErrorMessages.MalformedCollectionWithMultipleFirsts);
                            }
                            else
                            {
                                //Stick this item onto the Stack
                                temp = firsts.First();
                                //If Item is a named list node cannot compress
                                if (temp.Subject.NodeType != NodeType.Blank) break;
                                c.Triples.Add(temp);
                            }
                        }
                    } while (true);

                    //Can only compress if every List Node has a Blank Node Subject
                    if (c.Triples.All(x => x.Subject.NodeType == NodeType.Blank))
                    {
                        context.Collections.Add(firsts.First().Subject, c);
                    }
                }
            }

            //Now we want to look for explicit collections which are representable
            //using Blank Node syntax [p1 o1; p2 o2; p3 o3]
            if (mode == CollectionSearchMode.All || mode == CollectionSearchMode.ExplicitOnly)
            {
                List<IBlankNode> bnodes = context.Graph.Nodes.BlankNodes().ToList();
                foreach (IBlankNode b in bnodes)
                {
                    if (context.Collections.ContainsKey(b)) continue;

                    List<Triple> ts = context.Graph.GetTriples(b).ToList();
                    ts.RemoveAll(t => t.Predicate.Equals(first));
                    ts.RemoveAll(t => t.Predicate.Equals(rest));

                    if (ts.Count <= 1)
                    {
                        //This Blank Node is only used once
                        //Add an empty explicit collection - we'll interpret this as [] later
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

            //If no collections found then no further processing
            if (context.Collections.Count == 0) return;

            //Once we've found all the Collections we need to check which are actually eligible for compression
            List<KeyValuePair<INode, OutputRdfCollection>> cs = context.Collections.ToList();

            //1 - If all the Triples pertaining to a particular Node are in the Collection then a collection is not eligible
            foreach (KeyValuePair<INode, OutputRdfCollection> kvp in cs)
            {
                OutputRdfCollection c = kvp.Value;
                if (c.IsExplicit)
                {
                    //For explicit collections if all Triples mentioning the Target Blank Node are in the Collection then can't compress
                    //If there are no Triples in the Collection then this is a single use Blank Node so can always compress
                    if (c.Triples.Count > 0 && c.Triples.Count == context.Graph.GetTriples(kvp.Key).Count())
                    {
                        //TODO: This doesn't work because it can conflict
                        //In this case we can remove a single Triple from the Collection and hope this allows us to compress
                        //context.Collections[kvp.Key].Triples.RemoveAt(0);

                        context.Collections.Remove(kvp.Key);
                    }
                }
                else
                {
                    //For implicit collections if the number of Triples in the Collection is exactly ((t*3) - 1) those in the Graph then
                    //can't compress i.e. the collection is not linked to anything else
                    //Or if the number of mentions compared to the expected mentions differs by more than 1 then
                    //can't compress i.e. the collection is linked to more than one thing
                    int mentions = context.Graph.GetTriples(kvp.Key).Count();
                    int expectedMentions = ((c.Triples.Count * 3) - 1);
                    if (expectedMentions == mentions || mentions - expectedMentions != 1)
                    {
                        context.Collections.Remove(kvp.Key);
                    }
                }
            }
            if (context.Collections.Count == 0) return;

            //2 - Look for cyclic collection dependencies
            cs = context.Collections.OrderByDescending(kvp => kvp.Value.Triples.Count).ToList();

            //First build up a dependencies table
            Dictionary<INode, List<INode>> dependencies = new Dictionary<INode, List<INode>>();
            foreach (KeyValuePair<INode, OutputRdfCollection> kvp in cs)
            {
                OutputRdfCollection c = kvp.Value;

                //Empty Blank Node Collections cannot be cyclic i.e. []
                if (c.Triples.Count == 0) continue;

                //Otherwise check each Object of the Triples for other Blank Nodes
                List<INode> ds = new List<INode>();
                foreach (Triple t in c.Triples)
                {
                    //Only care about Blank Nodes which aren't the collection root but are the root for another collection
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

            //Now go back through that table looking for cycles
            foreach (INode n in dependencies.Keys)
            {
                List<INode> ds = dependencies[n];
                if (ds.Count == 0) continue;

                foreach (INode d in ds.ToList())
                {
                    if (dependencies.ContainsKey(d))
                    {
                        ds.AddRange(dependencies[d]);
                    }
                }

                //We can tell if there is a cycle since ds will now contain n
                if (ds.Contains(n))
                {
                    context.Collections.Remove(n);
                }
            }
            if (context.Collections.Count == 0) return;

            //Finally fill out the TriplesDone for each Collection
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

            //As a final sanity check look for any Explicit Collection Key which is used more than once
            cs = context.Collections.ToList();
            foreach (KeyValuePair<INode, OutputRdfCollection> kvp in cs)
            {
                OutputRdfCollection c = kvp.Value;
                if (c.IsExplicit)
                {
                    int mentions = context.Graph.GetTriples(kvp.Key).Where(t => !context.TriplesDone.Contains(t)).Count();
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
    }
}
