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
using System.Security.Cryptography;
using System.Xml;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Specifications;
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

        #region Selection Extensions

        /// <summary>
        /// Selects all Triples which contain the given Node
        /// </summary>
        /// <param name="n">Node</param>
        /// <returns></returns>
        public static IEnumerable<Triple> GetTriples(this IGraph g, INode n)
        {
            return g.Triples.Where(t => t.Involves(n));
        }

        /// <summary>
        /// Selects all Triples which have a URI Node with the given URI
        /// </summary>
        /// <param name="uri">URI</param>
        /// <returns></returns>
        public static IEnumerable<Triple> GetTriples(this IGraph g, Uri uri)
        {
            return ReferenceEquals(uri, null) ? Enumerable.Empty<Triple>() : g.GetTriples(g.CreateUriNode(uri));
        }

        /// <summary>
        /// Selects all Triples where the Object is a URI Node with the given Uri
        /// </summary>
        /// <param name="u">Uri</param>
        /// <returns></returns>
        public static IEnumerable<Triple> GetTriplesWithObject(this IGraph g, Uri u)
        {
            return ReferenceEquals(u, null) ? Enumerable.Empty<Triple>() : g.GetTriplesWithObject(g.CreateUriNode(u));
        }

        /// <summary>
        /// Selects all Triples where the Object is a given Node
        /// </summary>
        /// <param name="n">Node</param>
        /// <returns></returns>
        public static IEnumerable<Triple> GetTriplesWithObject(this IGraph g, INode n)
        {
            return ReferenceEquals(n, null) ? g.Triples : g.Find(null, null, n);
        }

        /// <summary>
        /// Selects all Triples where the Predicate is a given Node
        /// </summary>
        /// <param name="n">Node</param>
        /// <returns></returns>
        public static IEnumerable<Triple> GetTriplesWithPredicate(this IGraph g, INode n)
        {
            return ReferenceEquals(n, null) ? g.Triples : g.Find(null, n, null);
        }

        /// <summary>
        /// Selects all Triples where the Predicate is a Uri Node with the given Uri
        /// </summary>
        /// <param name="u">Uri</param>
        /// <returns></returns>
        public static IEnumerable<Triple> GetTriplesWithPredicate(this IGraph g, Uri u)
        {
            return ReferenceEquals(u, null) ? Enumerable.Empty<Triple>() : g.GetTriplesWithPredicate(g.CreateUriNode(u));
        }

        /// <summary>
        /// Selects all Triples where the Subject is a given Node
        /// </summary>
        /// <param name="n">Node</param>
        /// <returns></returns>
        public static IEnumerable<Triple> GetTriplesWithSubject(this IGraph g, INode n)
        {
            return ReferenceEquals(n, null) ? g.Triples : g.Find(n, null, null);
        }

        /// <summary>
        /// Selects all Triples where the Subject is a Uri Node with the given Uri
        /// </summary>
        /// <param name="u">Uri</param>
        /// <returns></returns>
        public static IEnumerable<Triple> GetTriplesWithSubject(this IGraph g, Uri u)
        {
            return ReferenceEquals(u, null) ? Enumerable.Empty<Triple>() : g.GetTriplesWithSubject(g.CreateUriNode(u));
        }

        /// <summary>
        /// Selects all Triples with the given Subject and Predicate
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <param name="pred">Predicate</param>
        /// <returns></returns>
        public static IEnumerable<Triple> GetTriplesWithSubjectPredicate(this IGraph g, INode subj, INode pred)
        {
            return g.Find(subj, pred, null);
        }

        /// <summary>
        /// Selects all Triples with the given Subject and Object
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        public static IEnumerable<Triple> GetTriplesWithSubjectObject(this IGraph g, INode subj, INode obj)
        {
            return g.Find(subj, null, obj);
        }

        /// <summary>
        /// Selects all Triples with the given Predicate and Object
        /// </summary>
        /// <param name="pred">Predicate</param>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        public static IEnumerable<Triple> GetTriplesWithPredicateObject(this IGraph g, INode pred, INode obj)
        {
            return g.Find(null, pred, obj);
        }

        #endregion

        #region Node Collection replacement extensions

        /// <summary>
        /// Gets the Blank Nodes
        /// </summary>
        /// <param name="ns">Nodes</param>
        /// <returns></returns>
        public static IEnumerable<IBlankNode> BlankNodes(this IEnumerable<INode> ns)
        {
            return ns.OfType<IBlankNode>();
        }

        /// <summary>
        /// Gets the Graph Literal Nodes
        /// </summary>
        /// <param name="ns">Nodes</param>
        /// <returns></returns>
        public static IEnumerable<IGraphLiteralNode> GraphLiteralNodes(this IEnumerable<INode> ns)
        {
            return ns.OfType<IGraphLiteralNode>();
        }

        /// <summary>
        /// Gets the Literal Nodes
        /// </summary>
        /// <param name="ns">Nodes</param>
        /// <returns></returns>
        public static IEnumerable<ILiteralNode> LiteralNodes(this IEnumerable<INode> ns)
        {
            return ns.OfType<ILiteralNode>();
        }

        /// <summary>
        /// Gets the URI Nodes
        /// </summary>
        /// <param name="ns">Nodes</param>
        /// <returns></returns>
        public static IEnumerable<IUriNode> UriNodes(this IEnumerable<INode> ns)
        {
            return ns.OfType<IUriNode>();
        }

        /// <summary>
        /// Gets the Variable Nodes
        /// </summary>
        /// <param name="ns">Nodes</param>
        /// <returns></returns>
        public static IEnumerable<IVariableNode> VariableNodes(this IEnumerable<INode> ns)
        {
            return ns.OfType<IVariableNode>();
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
            return u.AbsoluteUri.GetHashCode();
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

        #region List Helpers

        /// <summary>
        /// Asserts a list as a RDF collection and returns the node that represents the root of the RDF collection
        /// </summary>
        /// <typeparam name="T">Type of Objects</typeparam>
        /// <param name="g">Graph to assert in</param>
        /// <param name="objects">Objects to place in the collection</param>
        /// <param name="mapFunc">Mapping from Object Type to <see cref="INode">INode</see></param>
        /// <returns>
        /// Either the blank node which is the root of the collection or <strong>rdf:nil</strong> for empty collections
        /// </returns>
        public static INode AssertList<T>(this IGraph g, IEnumerable<T> objects, Func<T, INode> mapFunc)
        {
            if (!objects.Any())
            {
                return g.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfListNil));
            }
            else
            {
                INode listRoot = g.CreateBlankNode();
                AssertList<T>(g, listRoot, objects, mapFunc);
                return listRoot;
            }
        }

        /// <summary>
        /// Asserts a list as a RDF collection using an existing node as the list root
        /// </summary>
        /// <typeparam name="T">Type of Objects</typeparam>
        /// <param name="g">Graph to assert in</param>
        /// <param name="listRoot">Root Node for List</param>
        /// <param name="objects">Objects to place in the collection</param>
        /// <param name="mapFunc">Mapping from Object Type to <see cref="INode">INode</see></param>
        public static void AssertList<T>(this IGraph g, INode listRoot, IEnumerable<T> objects, Func<T, INode> mapFunc)
        {
            INode rdfNil = g.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfListNil));
            INode rdfFirst = g.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfListFirst));
            INode rdfRest = g.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfListRest));
            INode listCurrent = listRoot;

            //Then we can assert the collection
            List<INode> nodes = objects.Select(x => mapFunc(x)).ToList();
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i] == null) throw new RdfException("Unable to assert list because one of the items was null");
                g.Assert(listCurrent, rdfFirst, nodes[i]);

                if (i < nodes.Count - 1)
                {
                    INode listNext = g.CreateBlankNode();
                    g.Assert(listCurrent, rdfRest, listNext);
                    listCurrent = listNext;
                }
                else
                {
                    g.Assert(listCurrent, rdfRest, rdfNil);
                }
            }
        }

        /// <summary>
        /// Asserts a list as a RDF collection and returns the node that represents the root of the RDF collection
        /// </summary>
        /// <param name="g">Graph to assert in</param>
        /// <param name="objects">Objects to place in the collection</param>
        /// <returns>
        /// Either the blank node which is the root of the collection or <strong>rdf:nil</strong> for empty collections
        /// </returns>
        public static INode AssertList(this IGraph g, IEnumerable<INode> objects)
        {
            return AssertList(g, objects, n => n);
        }

        /// <summary>
        /// Asserts a list as a RDF collection using an existing node as the list root
        /// </summary>
        /// <param name="g">Graph to assert in</param>
        /// <param name="listRoot">Root Node for List</param>
        /// <param name="objects">Objects to place in the collection</param>
        public static void AssertList(this IGraph g, INode listRoot, IEnumerable<INode> objects)
        {
            AssertList(g, listRoot, objects, n => n);
        }

        /// <summary>
        /// Gets all the Triples that make up a list (aka a RDF collection)
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="listRoot">Root Node for List</param>
        /// <returns>Triples that make up the List</returns>
        public static IEnumerable<Triple> GetListAsTriples(this IGraph g, INode listRoot)
        {
            INode rdfFirst = g.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfListFirst));
            INode rdfRest = g.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfListRest));
            INode rdfNil = g.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfListNil));

            if (listRoot.Equals(rdfNil)) return Enumerable.Empty<Triple>();

            List<Triple> ts = new List<Triple>();
            int currCount = 0;
            int diff = 0;
            INode listCurrent = listRoot;
            do
            {
                ts.AddRange(g.GetTriplesWithSubjectPredicate(listCurrent, rdfFirst));
                diff = ts.Count - currCount;
                if (diff == 0) throw new RdfException("Unable to get list as there was no rdf:first associated with the list item " + listCurrent.ToString());
                if (diff > 1) throw new RdfException("Unable to get list as there was more than one rdf:first associated with the list item " + listCurrent.ToString());
                currCount = ts.Count;

                ts.AddRange(g.GetTriplesWithSubjectPredicate(listCurrent, rdfRest));
                diff = ts.Count - currCount;
                if (diff == 0) throw new RdfException("Unable to get list as there was no rdf:rest associated with the list item " + listCurrent.ToString());
                if (diff > 1) throw new RdfException("Unable to get list as there was more than one rdf:rest associated with the list item " + listCurrent.ToString());
                currCount = ts.Count;

                listCurrent = ts[ts.Count - 1].Object;
            } while (!listCurrent.Equals(rdfNil));

            return ts;
        }

        /// <summary>
        /// Gets all the Nodes which are the items of the list (aka the RDF collection)
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="listRoot">Root Node for List</param>
        /// <returns>Nodes that are the items in the list</returns>
        public static IEnumerable<INode> GetListItems(this IGraph g, INode listRoot)
        {
            INode rdfFirst = g.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfListFirst));
            return GetListAsTriples(g, listRoot).Where(t => t.Predicate.Equals(rdfFirst)).Select(t => t.Object);
        }

        /// <summary>
        /// Gets all the Nodes which are the intermediate nodes in the list (aka the RDF collection).  These represents the nodes used to link the actual items of the list together rather than the actual items of the list
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="listRoot">Root Node for List</param>
        /// <returns>Nodes that are the intermediate nodes of the list</returns>
        public static IEnumerable<INode> GetListNodes(this IGraph g, INode listRoot)
        {
            INode rdfFirst = g.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfListFirst));
            return GetListAsTriples(g, listRoot).Where(t => t.Predicate.Equals(rdfFirst)).Select(t => t.Subject);
        }

        /// <summary>
        /// Gets whether a given Node is valid as a List Root, this does not guarantee that the list itself is valid simply that the Node appears to be the root of a list
        /// </summary>
        /// <param name="n">Node to check</param>
        /// <param name="g">Graph</param>
        /// <returns></returns>
        /// <remarks>
        /// We consider a node to be a list root if there are no incoming rdf:rest triples and only a single outgoing rdf:first triple
        /// </remarks>
        public static bool IsListRoot(this INode n, IGraph g)
        {
            INode rdfRest = g.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfListRest));
            INode rdfFirst = g.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfListFirst));
            return !g.GetTriplesWithPredicateObject(rdfRest, n).Any() && (g.GetTriplesWithSubjectPredicate(n, rdfFirst).Count() == 1);
        }

        /// <summary>
        /// Gets the Node that represents the last item in the list
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="listRoot">Root Node for List</param>
        /// <returns></returns>
        private static INode GetListTail(this IGraph g, INode listRoot)
        {
            return GetListAsTriples(g, listRoot).Select(t => t.Subject).Last();
        }

        /// <summary>
        /// Retracts a List (aka a RDF collection)
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="listRoot">Root Node for List</param>
        public static void RetractList(this IGraph g, INode listRoot)
        {
            g.Retract(GetListAsTriples(g, listRoot));
        }

        /// <summary>
        /// Adds new items to the end of a list (aka a RDF collection)
        /// </summary>
        /// <typeparam name="T">Type of Objects</typeparam>
        /// <param name="g">Graph to assert in</param>
        /// <param name="listRoot">Root Node for the list</param>
        /// <param name="objects">Objects to add to the collection</param>
        /// <param name="mapFunc">Mapping from Object Type to <see cref="INode">INode</see></param>
        public static void AddToList<T>(this IGraph g, INode listRoot, IEnumerable<T> objects, Func<T, INode> mapFunc)
        {
            //If no objects to add then nothing to do
            if (!objects.Any()) return;

            //Get the List Tail
            INode listTail = GetListTail(g, listRoot);

            //Remove the rdf:rest rdf:nil triple
            INode rdfRest = g.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfListRest));
            INode rdfNil = g.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfListNil));
            g.Retract(new Triple(listTail, rdfRest, rdfNil));

            //Create a new tail for the list that will act as the root of the extended list
            INode newRoot = g.CreateBlankNode();
            INode rdfFirst = g.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfListFirst));
            g.Assert(new Triple(listTail, rdfRest, newRoot));

            //Then assert the new list
            AssertList<T>(g, newRoot, objects, mapFunc);
        }

        /// <summary>
        /// Adds new items to the end of a list (aka a RDF collection)
        /// </summary>
        /// <param name="g">Graph to assert in</param>
        /// <param name="listRoot">Root Node for the list</param>
        /// <param name="objects">Objects to add to the collection</param>
        public static void AddToList(this IGraph g, INode listRoot, IEnumerable<INode> objects)
        {
            AddToList(g, listRoot, objects, n => n);
        }

        /// <summary>
        /// Removes the given items from a list (aka a RDF collection), if an item occurs multiple times in the list all occurrences will be removed
        /// </summary>
        /// <typeparam name="T">Type of Objects</typeparam>
        /// <param name="g">Graph to retract from</param>
        /// <param name="listRoot">Root Node for the list</param>
        /// <param name="objects">Objects to remove from the collection</param>
        /// <param name="mapFunc">Mapping from Object Type to <see cref="INode">INode</see></param>
        public static void RemoveFromList<T>(this IGraph g, INode listRoot, IEnumerable<T> objects, Func<T, INode> mapFunc)
        {
            //If no objects to remove then nothing to do
            if (!objects.Any()) return;

            //Figure out which items need removing
            List<INode> currObjects = GetListItems(g, listRoot).ToList();
            int initialCount = currObjects.Count;
            foreach (INode obj in objects.Select(x => mapFunc(x)))
            {
                currObjects.Remove(obj);
            }
            if (initialCount != currObjects.Count)
            {
                //We are removing one/more items
                //Easiest way to do this is simply to retract the entire list and then add the new list
                RetractList(g, listRoot);
                AssertList(g, listRoot, currObjects);
            }
        }

        /// <summary>
        /// Removes the given items from a list (aka a RDF collection), if an item occurs multiple times in the list all occurrences will be removed
        /// </summary>
        /// <param name="g">Graph to retract from</param>
        /// <param name="listRoot">Root Node for the list</param>
        /// <param name="objects">Objects to remove from the collection</param>
        public static void RemoveFromList(this IGraph g, INode listRoot, IEnumerable<INode> objects)
        {
            RemoveFromList(g, listRoot, objects, n => n);
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
        internal static String ToSafeString(this Object obj)
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

    /// <summary>
    /// Provides extension methods for converting primitive types into appropriately typed Literal Nodes
    /// </summary>
    public static class LiteralExtensions
    {
        /// <summary>
        /// Creates a new Boolean typed literal
        /// </summary>
        /// <param name="b">Boolean</param>
        /// <param name="factory">Node Factory to use for Node creation</param>
        /// <returns>Literal representing the boolean</returns>
        /// <exception cref="ArgumentNullException">Thrown if the Factory argument is null</exception>
        public static ILiteralNode ToLiteral(this bool b, INodeFactory factory)
        {
            if (factory == null) throw new ArgumentNullException("factory", "Cannot create a Literal Node in a null Node Factory");

            return factory.CreateLiteralNode(b.ToString().ToLower(), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeBoolean));
        }

        /// <summary>
        /// Creates a new Byte typed literal
        /// </summary>
        /// <param name="b">Byte</param>
        /// <param name="factory">Node Factory to use for Node creation</param>
        /// <returns>Literal representing the byte</returns>
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
                return factory.CreateLiteralNode(XmlConvert.ToString(b), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeUnsignedByte));
            }
            else
            {
                return factory.CreateLiteralNode(XmlConvert.ToString(b), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeByte));
            }
        }

        /// <summary>
        /// Creates a new Byte typed literal
        /// </summary>
        /// <param name="b">Byte</param>
        /// <param name="factory">Node Factory to use for Node creation</param>
        /// <returns>Literal representing the signed bytes</returns>
        /// <remarks>
        /// SByte in .Net is directly equivalent to Byte in XML Schema so the type will always be xsd:byte
        /// </remarks>
        public static ILiteralNode ToLiteral(this sbyte b, INodeFactory factory)
        {
            if (factory == null) throw new ArgumentNullException("factory", "Cannot create a Literal Node in a null Node Factory");

            return factory.CreateLiteralNode(XmlConvert.ToString(b), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeByte));
        }

        /// <summary>
        /// Creates a new Date Time typed literal
        /// </summary>
        /// <param name="dt">Date Time</param>
        /// <param name="factory">Node Factory to use for Node creation</param>
        /// <returns>Literal representing the date time</returns>
        /// <exception cref="ArgumentNullException">Thrown if the Factory argument is null</exception>
        public static ILiteralNode ToLiteral(this DateTime dt, INodeFactory factory)
        {
            return ToLiteral(dt, factory, true);
        }

        /// <summary>
        /// Creates a new Date Time typed literal
        /// </summary>
        /// <param name="dt">Date Time</param>
        /// <param name="factory">Node Factory to use for Node creation</param>
        /// <param name="precise">Whether to preserve precisely i.e. include fractional seconds</param>
        /// <returns>Literal representing the date time</returns>
        /// <exception cref="ArgumentNullException">Thrown if the Factory argument is null</exception>
        public static ILiteralNode ToLiteral(this DateTime dt, INodeFactory factory, bool precise)
        {
            if (factory == null) throw new ArgumentNullException("factory", "Cannot create a Literal Node in a null Node Factory");

            return factory.CreateLiteralNode(dt.ToString(precise ? XmlSpecsHelper.XmlSchemaDateTimeFormat : XmlSpecsHelper.XmlSchemaDateTimeFormatImprecise), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDateTime));
        }

        /// <summary>
        /// Creates a new Date Time typed literal
        /// </summary>
        /// <param name="dt">Date Time</param>
        /// <param name="factory">Node Factory to use for Node creation</param>
        /// <returns>Literal representing the date time</returns>
        /// <exception cref="ArgumentNullException">Thrown if the Factory argument is null</exception>
        public static ILiteralNode ToLiteral(this DateTimeOffset dt, INodeFactory factory)
        {
            return ToLiteral(dt, factory, true);
        }

        /// <summary>
        /// Creates a new Date Time typed literal
        /// </summary>
        /// <param name="dt">Date Time</param>
        /// <param name="factory">Node Factory to use for Node creation</param>
        /// <param name="precise">Whether to preserve precisely i.e. include fractional seconds</param>
        /// <returns>Literal representing the date time</returns>
        /// <exception cref="ArgumentNullException">Thrown if the Factory argument is null</exception>
        public static ILiteralNode ToLiteral(this DateTimeOffset dt, INodeFactory factory, bool precise)
        {
            if (factory == null) throw new ArgumentNullException("factory", "Cannot create a Literal Node in a null Node Factory");

            return factory.CreateLiteralNode(dt.ToString(precise ? XmlSpecsHelper.XmlSchemaDateTimeFormat : XmlSpecsHelper.XmlSchemaDateTimeFormatImprecise), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDateTime));
        }

        /// <summary>
        /// Creates a new Date typed literal
        /// </summary>
        /// <param name="dt">Date Time</param>
        /// <param name="factory">Node Factory to use for Node creation</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if the Factory argument is null</exception>
        public static ILiteralNode ToLiteralDate(this DateTime dt, INodeFactory factory)
        {
            if (factory == null) throw new ArgumentNullException("factory", "Cannot create a Literal Node in a null Node Factory");

            return factory.CreateLiteralNode(dt.ToString(XmlSpecsHelper.XmlSchemaDateFormat), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDate));
        }

        /// <summary>
        /// Creates a new Date typed literal
        /// </summary>
        /// <param name="dt">Date Time</param>
        /// <param name="factory">Node Factory to use for Node creation</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if the Factory argument is null</exception>
        public static ILiteralNode ToLiteralDate(this DateTimeOffset dt, INodeFactory factory)
        {
            if (factory == null) throw new ArgumentNullException("factory", "Cannot create a Literal Node in a null Node Factory");

            return factory.CreateLiteralNode(dt.ToString(XmlSpecsHelper.XmlSchemaDateFormat), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDate));
        }

        /// <summary>
        /// Creates a new Time typed literal
        /// </summary>
        /// <param name="dt">Date Time</param>
        /// <param name="factory">Node Factory to use for Node creation</param>
        /// <returns>Literal representing the time</returns>
        /// <exception cref="ArgumentNullException">Thrown if the Factory argument is null</exception>
        public static ILiteralNode ToLiteralTime(this DateTime dt, INodeFactory factory)
        {
            return ToLiteralTime(dt, factory, true);
        }

        /// <summary>
        /// Creates a new Time typed literal
        /// </summary>
        /// <param name="dt">Date Time</param>
        /// <param name="factory">Node Factory to use for Node creation</param>
        /// <param name="precise">Whether to preserve precisely i.e. include fractional seconds</param>
        /// <returns>Literal representing the time</returns>
        /// <exception cref="ArgumentNullException">Thrown if the Factory argument is null</exception>
        public static ILiteralNode ToLiteralTime(this DateTime dt, INodeFactory factory, bool precise)
        {
            if (factory == null) throw new ArgumentNullException("factory", "Cannot create a Literal Node in a null Node Factory");

            return factory.CreateLiteralNode(dt.ToString(precise ? XmlSpecsHelper.XmlSchemaTimeFormat : XmlSpecsHelper.XmlSchemaTimeFormatImprecise), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeTime));
        }

        
        /// <summary>
        /// Creates a new duration typed literal
        /// </summary>
        /// <param name="t">Time Span</param>
        /// <param name="factory">Node Factory to use for Node creation</param>
        /// <returns>Literal representing the time span</returns>
        public static ILiteralNode ToLiteral(this TimeSpan t, INodeFactory factory)
        {
            if (factory == null) throw new ArgumentNullException("factory", "Cannot create a Literal Node in a null Node Factory");

            return factory.CreateLiteralNode(XmlConvert.ToString(t), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDuration));
        }

        /// <summary>
        /// Creates a new Time typed literal
        /// </summary>
        /// <param name="dt">Date Time</param>
        /// <param name="factory">Node Factory to use for Node creation</param>
        /// <returns>Literal representing the time</returns>
        /// <exception cref="ArgumentNullException">Thrown if the Factory argument is null</exception>
        public static ILiteralNode ToLiteralTime(this DateTimeOffset dt, INodeFactory factory)
        {
            return ToLiteralTime(dt, factory, true);
        }

        /// <summary>
        /// Creates a new Time typed literal
        /// </summary>
        /// <param name="dt">Date Time</param>
        /// <param name="factory">Node Factory to use for Node creation</param>
        /// <param name="precise">Whether to preserve precisely i.e. include fractional seconds</param>
        /// <returns>Literal representing the time</returns>
        /// <exception cref="ArgumentNullException">Thrown if the Factory argument is null</exception>
        public static ILiteralNode ToLiteralTime(this DateTimeOffset dt, INodeFactory factory, bool precise)
        {
            if (factory == null) throw new ArgumentNullException("factory", "Cannot create a Literal Node in a null Node Factory");

            return factory.CreateLiteralNode(dt.ToString(precise ? XmlSpecsHelper.XmlSchemaTimeFormat : XmlSpecsHelper.XmlSchemaTimeFormatImprecise), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeTime));
        }

        /// <summary>
        /// Creates a new Decimal typed literal
        /// </summary>
        /// <param name="d">Decimal</param>
        /// <param name="factory">Node Factory to use for Node creation</param>
        /// <returns>Literal representing the decimal</returns>
        /// <exception cref="ArgumentNullException">Thrown if the Factory argument is null</exception>
        public static ILiteralNode ToLiteral(this decimal d, INodeFactory factory)
        {
            if (factory == null) throw new ArgumentNullException("factory", "Cannot create a Literal Node in a null Node Factory");

            return factory.CreateLiteralNode(d.ToString(CultureInfo.InvariantCulture), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDecimal));
        }

        /// <summary>
        /// Creates a new Double typed literal
        /// </summary>
        /// <param name="d">Double</param>
        /// <param name="factory">Node Factory to use for Node creation</param>
        /// <returns>Literal representing the double</returns>
        /// <exception cref="ArgumentNullException">Thrown if the Factory argument is null</exception>
        public static ILiteralNode ToLiteral(this double d, INodeFactory factory)
        {
            if (factory == null) throw new ArgumentNullException("factory", "Cannot create a Literal Node in a null Node Factory");

            return factory.CreateLiteralNode(d.ToString(CultureInfo.InvariantCulture), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDouble));
        }

        /// <summary>
        /// Creates a new Float typed literal
        /// </summary>
        /// <param name="f">Float</param>
        /// <param name="factory">Node Factory to use for Node creation</param>
        /// <returns>Literal representing the float</returns>
        /// <exception cref="ArgumentNullException">Thrown if the Factory argument is null</exception>
        public static ILiteralNode ToLiteral(this float f, INodeFactory factory)
        {
            if (factory == null) throw new ArgumentNullException("factory", "Cannot create a Literal Node in a null Node Factory");

            return factory.CreateLiteralNode(f.ToString(CultureInfo.InvariantCulture), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeFloat));
        }

        /// <summary>
        /// Creates a new Integer typed literal
        /// </summary>
        /// <param name="i">Integer</param>
        /// <param name="factory">Node Factory to use for Node creation</param>
        /// <returns>Literal representing the short</returns>
        /// <exception cref="ArgumentNullException">Thrown if the Factory argument is null</exception>
        public static ILiteralNode ToLiteral(this short i, INodeFactory factory)
        {
            if (factory == null) throw new ArgumentNullException("factory", "Cannot create a Literal Node in a null Node Factory");

            return factory.CreateLiteralNode(i.ToString(CultureInfo.InvariantCulture), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeInteger));
        }

        /// <summary>
        /// Creates a new Integer typed literal
        /// </summary>
        /// <param name="i">Integer</param>
        /// <param name="factory">Node Factory to use for Node creation</param>
        /// <returns>Literal representing the integer</returns>
        /// <exception cref="ArgumentNullException">Thrown if the Factory argument is null</exception>
        public static ILiteralNode ToLiteral(this int i, INodeFactory factory)
        {
            if (factory == null) throw new ArgumentNullException("factory", "Cannot create a Literal Node in a null Node Factory");

            return factory.CreateLiteralNode(i.ToString(CultureInfo.InvariantCulture), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeInteger));
        }

        /// <summary>
        /// Creates a new Integer typed literal
        /// </summary>
        /// <param name="l">Integer</param>
        /// <param name="factory">Node Factory to use for Node creation</param>
        /// <returns>Literal representing the integer</returns>
        /// <exception cref="ArgumentNullException">Thrown if the Factory argument is null</exception>
        public static ILiteralNode ToLiteral(this long l, INodeFactory factory)
        {
            if (factory == null) throw new ArgumentNullException("factory", "Cannot create a Literal Node in a null Node Factory");

            return factory.CreateLiteralNode(l.ToString(CultureInfo.InvariantCulture), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeInteger));
        }

        /// <summary>
        /// Creates a new String typed literal
        /// </summary>
        /// <param name="s">String</param>
        /// <param name="factory">Node Factory to use for Node creation</param>
        /// <returns>Literal representing the string</returns>
        /// <exception cref="ArgumentNullException">Thrown if the Graph/String argument is null</exception>
        public static ILiteralNode ToLiteral(this String s, INodeFactory factory)
        {
            if (factory == null) throw new ArgumentNullException("factory", "Cannot create a Literal Node in a null Node Factory");
            if (s == null) throw new ArgumentNullException("s", "Cannot create a Literal Node for a null String");

            return factory.CreateLiteralNode(s, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
        }
    }
}
