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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF;

/// <summary>
/// Provides useful Extension Methods for use elsewhere in the Library.
/// </summary>
public static class Extensions
{

    #region Enumerable Extensions

    /// <summary>
    /// Takes a single item and generates an IEnumerable containing only it.
    /// </summary>
    /// <typeparam name="T">Type of the enumerable.</typeparam>
    /// <param name="item">Item to wrap in an IEnumerable.</param>
    /// <returns></returns>
    /// <remarks>
    /// This method taken from Stack Overflow - see. <a href="http://stackoverflow.com/questions/1577822/passing-a-single-item-as-ienumerablet">here</a>
    /// </remarks>
    public static IEnumerable<T> AsEnumerable<T>(this T item)
    {
        yield return item;
    }

    /// <summary>
    /// Determines whether the contents of two enumerables are disjoint.
    /// </summary>
    /// <typeparam name="T">Type Parameter.</typeparam>
    /// <param name="x">An Enumerable.</param>
    /// <param name="y">Another Enumerable.</param>
    /// <returns></returns>
    public static bool IsDisjoint<T>(this IEnumerable<T> x, IEnumerable<T> y)
    {
        return x.All(item => !y.Contains(item));
    }

    /// <summary>
    /// Splits a sequence into bounded chunks.
    /// </summary>
    /// <typeparam name="T">Type Parameter.</typeparam>
    /// <param name="source">An Enumerable.</param>
    /// <param name="size">Max Chunk Size.</param>
    /// <returns></returns>
    public static IEnumerable<T[]> ChunkBy<T>(this IEnumerable<T> source, int size)
    {
        var buffer = new List<T>();
        foreach (var item in source)
        {
            buffer.Add(item);
            if (buffer.Count == size)
            {
                yield return buffer.ToArray();
                buffer.Clear();
            }
        }
        if (buffer.Count > 0)
        {
            yield return buffer.ToArray();
            buffer.Clear();
        }
    }

    #endregion

    #region Triple Selection Extensions

    /// <summary>
    /// Gets the Subset of Triples from an existing Enumerable that have a given Subject.
    /// </summary>
    /// <param name="ts">Enumerable of Triples.</param>
    /// <param name="subject">Subject to match.</param>
    /// <returns></returns>
    public static IEnumerable<Triple> WithSubject(this IEnumerable<Triple> ts, INode subject)
    {
        return (from t in ts
                where t.Subject.Equals(subject)
                select t);
    }

    /// <summary>
    /// Gets the Subset of Triples from an existing Enumerable that have a given Predicate.
    /// </summary>
    /// <param name="ts">Enumerable of Triples.</param>
    /// <param name="predicate">Predicate to match.</param>
    /// <returns></returns>
    public static IEnumerable<Triple> WithPredicate(this IEnumerable<Triple> ts, INode predicate)
    {
        return (from t in ts
                where t.Predicate.Equals(predicate)
                select t);
    }

    /// <summary>
    /// Gets the Subset of Triples from an existing Enumerable that have a given Object.
    /// </summary>
    /// <param name="ts">Enumerable of Triples.</param>
    /// <param name="obj">Object to match.</param>
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
    /// Gets the Blank Nodes.
    /// </summary>
    /// <param name="ns">Nodes.</param>
    /// <returns></returns>
    public static IEnumerable<IBlankNode> BlankNodes(this IEnumerable<INode> ns)
    {
        return ns.OfType<IBlankNode>().Where(n => n.NodeType == NodeType.Blank);
    }

    /// <summary>
    /// Gets the Graph Literal Nodes.
    /// </summary>
    /// <param name="ns">Nodes.</param>
    /// <returns></returns>
    public static IEnumerable<IGraphLiteralNode> GraphLiteralNodes(this IEnumerable<INode> ns)
    {
        return ns.OfType<IGraphLiteralNode>();
    }

    /// <summary>
    /// Gets the Literal Nodes.
    /// </summary>
    /// <param name="ns">Nodes.</param>
    /// <returns></returns>
    public static IEnumerable<ILiteralNode> LiteralNodes(this IEnumerable<INode> ns)
    {
        return ns.OfType<ILiteralNode>().Where(n => n.NodeType == NodeType.Literal);
    }

    /// <summary>
    /// Gets the URI Nodes.
    /// </summary>
    /// <param name="ns">Nodes.</param>
    /// <returns></returns>
    public static IEnumerable<IUriNode> UriNodes(this IEnumerable<INode> ns)
    {
        return ns.OfType<IUriNode>().Where(n => n.NodeType == NodeType.Uri);
    }

    /// <summary>
    /// Gets the Variable Nodes.
    /// </summary>
    /// <param name="ns">Nodes.</param>
    /// <returns></returns>
    public static IEnumerable<IVariableNode> VariableNodes(this IEnumerable<INode> ns)
    {
        return ns.OfType<IVariableNode>();
    }

    #endregion

    #region Hash Code Extensions

    /// <summary>
    /// Gets an Enhanced Hash Code for a Uri.
    /// </summary>
    /// <param name="u">Uri to get Hash Code for.</param>
    /// <returns></returns>
    /// <remarks>
    /// The .Net <see cref="Uri">Uri</see> class Hash Code ignores the Fragment ID when computing the Hash Code which means that URIs with the same basic URI but different Fragment IDs have identical Hash Codes.  This is perfectly acceptable and sensible behaviour for normal URI usage since Fragment IDs are only relevant to the Client and not the Server.  <strong>But</strong> in the case of URIs in RDF the Fragment ID is significant and so we need in some circumstances to compute a Hash Code which includes this information.
    /// </remarks>
    public static int GetEnhancedHashCode(this Uri u)
    {
        if (u == null) throw new ArgumentNullException("Cannot calculate an Enhanced Hash Code for a null URI");
        return u.AbsoluteUri.GetHashCode();
    }

#if NETCORE
    private static SHA256 _sha256 = null;
    /// <summary>
    /// Gets an SHA256 Hash for a URI
    /// </summary>
    /// <param name="u">URI to get Hash Code for</param>
    /// <returns></returns>
    public static String GetSha256Hash(this Uri u)
    {
        if (u == null) throw new ArgumentNullException("u");

        // Only instantiate the SHA256 class when we first use it
        if (_sha256 == null) _sha256 = SHA256.Create();

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

        // Only instantiate the SHA256 class when we first use it
        if (_sha256 == null) _sha256 = SHA256.Create();

        Byte[] input = Encoding.UTF8.GetBytes(s);
        Byte[] output = _sha256.ComputeHash(input);

        StringBuilder hash = new StringBuilder();
        foreach (Byte b in output)
        {
            hash.Append(b.ToString("x2"));
        }

        return hash.ToString();
    }
#else
    private static SHA256Managed _sha256;

    /// <summary>
    /// Gets an SHA256 Hash for a URI.
    /// </summary>
    /// <param name="u">URI to get Hash Code for.</param>
    /// <returns></returns>
    public static string GetSha256Hash(this Uri u)
    {
        if (u == null) throw new ArgumentNullException("u");

        // Only instantiate the SHA256 class when we first use it
        if (_sha256 == null) _sha256 = new SHA256Managed();

        var input = Encoding.UTF8.GetBytes(u.AbsoluteUri);
        var output = _sha256.ComputeHash(input);

        var hash = new StringBuilder();
        foreach (var b in output)
        {
            hash.Append(b.ToString("x2"));
        }

        return hash.ToString();
    }


    /// <summary>
    /// Gets a SHA256 Hash for a String.
    /// </summary>
    /// <param name="s">String to hash.</param>
    /// <returns></returns>
    public static string GetSha256Hash(this string s)
    {
        if (s == null) throw new ArgumentNullException("s");

        // Only instantiate the SHA256 class when we first use it
        if (_sha256 == null) _sha256 = new SHA256Managed();

        var input = Encoding.UTF8.GetBytes(s);
        var output = _sha256.ComputeHash(input);

        var hash = new StringBuilder();
        foreach (var b in output)
        {
            hash.Append(b.ToString("x2"));
        }

        return hash.ToString();
    }
#endif

    #endregion

    #region Triple Assertion and Retraction Extensions

    /// <summary>
    /// Asserts a new Triple in the Graph.
    /// </summary>
    /// <param name="g">Graph to assert in.</param>
    /// <param name="subj">Subject.</param>
    /// <param name="pred">Predicate.</param>
    /// <param name="obj">Object.</param>
    /// <remarks>Handy method which means you can assert a Triple by specifying the Subject, Predicate and Object without having to explicity declare a new Triple.</remarks>
    public static void Assert(this IGraph g, INode subj, INode pred, INode obj)
    {
        g.Assert(new Triple(subj, pred, obj));
    }

    /// <summary>
    /// Retracts a Triple from the Graph.
    /// </summary>
    /// <param name="g">Graph to retract from.</param>
    /// <param name="subj">Subject.</param>
    /// <param name="pred">Predicate.</param>
    /// <param name="obj">Object.</param>
    /// <remarks>Handy method which means you can retract a Triple by specifying the Subject, Predicate and Object without having to explicity declare a new Triple.</remarks>
    public static void Retract(this IGraph g, INode subj, INode pred, INode obj)
    {
        g.Retract(new Triple(subj, pred, obj));
    }

#endregion

#region List Helpers

    /// <summary>
    /// Asserts a list as a RDF collection and returns the node that represents the root of the RDF collection.
    /// </summary>
    /// <typeparam name="T">Type of Objects.</typeparam>
    /// <param name="g">Graph to assert in.</param>
    /// <param name="objects">Objects to place in the collection.</param>
    /// <param name="mapFunc">Mapping from Object Type to <see cref="INode">INode</see>.</param>
    /// <returns>
    /// Either the blank node which is the root of the collection or <strong>rdf:nil</strong> for empty collections.
    /// </returns>
    public static INode AssertList<T>(this IGraph g, IEnumerable<T> objects, Func<T, INode> mapFunc)
    {
        if (!objects.Any())
        {
            return g.CreateUriNode(g.UriFactory.Create(RdfSpecsHelper.RdfListNil));
        }
        else
        {
            INode listRoot = g.CreateBlankNode();
            AssertList<T>(g, listRoot, objects, mapFunc);
            return listRoot;
        }
    }

    /// <summary>
    /// Asserts a list as a RDF collection using an existing node as the list root.
    /// </summary>
    /// <typeparam name="T">Type of Objects.</typeparam>
    /// <param name="g">Graph to assert in.</param>
    /// <param name="listRoot">Root Node for List.</param>
    /// <param name="objects">Objects to place in the collection.</param>
    /// <param name="mapFunc">Mapping from Object Type to <see cref="INode">INode</see>.</param>
    public static void AssertList<T>(this IGraph g, INode listRoot, IEnumerable<T> objects, Func<T, INode> mapFunc)
    {
        INode rdfNil = g.CreateUriNode(g.UriFactory.Create(RdfSpecsHelper.RdfListNil));
        INode rdfFirst = g.CreateUriNode(g.UriFactory.Create(RdfSpecsHelper.RdfListFirst));
        INode rdfRest = g.CreateUriNode(g.UriFactory.Create(RdfSpecsHelper.RdfListRest));
        INode listCurrent = listRoot;

        // Then we can assert the collection
        var nodes = objects.Select(x => mapFunc(x)).ToList();
        for (var i = 0; i < nodes.Count; i++)
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
    /// Asserts a list as a RDF collection and returns the node that represents the root of the RDF collection.
    /// </summary>
    /// <param name="g">Graph to assert in.</param>
    /// <param name="objects">Objects to place in the collection.</param>
    /// <returns>
    /// Either the blank node which is the root of the collection or <strong>rdf:nil</strong> for empty collections.
    /// </returns>
    public static INode AssertList(this IGraph g, IEnumerable<INode> objects)
    {
        return AssertList(g, objects, n => n);
    }

    /// <summary>
    /// Asserts a list as a RDF collection using an existing node as the list root.
    /// </summary>
    /// <param name="g">Graph to assert in.</param>
    /// <param name="listRoot">Root Node for List.</param>
    /// <param name="objects">Objects to place in the collection.</param>
    public static void AssertList(this IGraph g, INode listRoot, IEnumerable<INode> objects)
    {
        AssertList(g, listRoot, objects, n => n);
    }

    /// <summary>
    /// Gets all the Triples that make up a list (aka a RDF collection).
    /// </summary>
    /// <param name="g">Graph.</param>
    /// <param name="listRoot">Root Node for List.</param>
    /// <returns>Triples that make up the List.</returns>
    public static IEnumerable<Triple> GetListAsTriples(this IGraph g, INode listRoot)
    {
        INode rdfFirst = g.CreateUriNode(g.UriFactory.Create(RdfSpecsHelper.RdfListFirst));
        INode rdfRest = g.CreateUriNode(g.UriFactory.Create(RdfSpecsHelper.RdfListRest));
        INode rdfNil = g.CreateUriNode(g.UriFactory.Create(RdfSpecsHelper.RdfListNil));

        if (listRoot.Equals(rdfNil)) return Enumerable.Empty<Triple>();

        var ts = new List<Triple>();
        var currCount = 0;
        var diff = 0;
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
    /// Gets all the Nodes which are the items of the list (aka the RDF collection).
    /// </summary>
    /// <param name="g">Graph.</param>
    /// <param name="listRoot">Root Node for List.</param>
    /// <returns>Nodes that are the items in the list.</returns>
    public static IEnumerable<INode> GetListItems(this IGraph g, INode listRoot)
    {
        INode rdfFirst = g.CreateUriNode(g.UriFactory.Create(RdfSpecsHelper.RdfListFirst));
        return GetListAsTriples(g, listRoot).Where(t => t.Predicate.Equals(rdfFirst)).Select(t => t.Object);
    }

    /// <summary>
    /// Gets all the Nodes which are the intermediate nodes in the list (aka the RDF collection).  These represents the nodes used to link the actual items of the list together rather than the actual items of the list.
    /// </summary>
    /// <param name="g">Graph.</param>
    /// <param name="listRoot">Root Node for List.</param>
    /// <returns>Nodes that are the intermediate nodes of the list.</returns>
    public static IEnumerable<INode> GetListNodes(this IGraph g, INode listRoot)
    {
        INode rdfFirst = g.CreateUriNode(g.UriFactory.Create(RdfSpecsHelper.RdfListFirst));
        return GetListAsTriples(g, listRoot).Where(t => t.Predicate.Equals(rdfFirst)).Select(t => t.Subject);
    }

    /// <summary>
    /// Gets whether a given Node is valid as a List Root, this does not guarantee that the list itself is valid simply that the Node appears to be the root of a list.
    /// </summary>
    /// <param name="n">Node to check.</param>
    /// <param name="g">Graph.</param>
    /// <returns></returns>
    /// <remarks>
    /// We consider a node to be a list root if there are no incoming rdf:rest triples and only a single outgoing rdf:first triple.
    /// </remarks>
    public static bool IsListRoot(this INode n, IGraph g)
    {
        INode rdfRest = g.CreateUriNode(g.UriFactory.Create(RdfSpecsHelper.RdfListRest));
        INode rdfFirst = g.CreateUriNode(g.UriFactory.Create(RdfSpecsHelper.RdfListFirst));
        return !g.GetTriplesWithPredicateObject(rdfRest, n).Any() && (g.GetTriplesWithSubjectPredicate(n, rdfFirst).Count() == 1);
    }

    /// <summary>
    /// Gets the Node that represents the last item in the list.
    /// </summary>
    /// <param name="g">Graph.</param>
    /// <param name="listRoot">Root Node for List.</param>
    /// <returns></returns>
    private static INode GetListTail(this IGraph g, INode listRoot)
    {
        return GetListAsTriples(g, listRoot).Select(t => t.Subject).Last();
    }

    /// <summary>
    /// Retracts a List (aka a RDF collection).
    /// </summary>
    /// <param name="g">Graph.</param>
    /// <param name="listRoot">Root Node for List.</param>
    public static void RetractList(this IGraph g, INode listRoot)
    {
        g.Retract(GetListAsTriples(g, listRoot));
    }

    /// <summary>
    /// Adds new items to the end of a list (aka a RDF collection).
    /// </summary>
    /// <typeparam name="T">Type of Objects.</typeparam>
    /// <param name="g">Graph to assert in.</param>
    /// <param name="listRoot">Root Node for the list.</param>
    /// <param name="objects">Objects to add to the collection.</param>
    /// <param name="mapFunc">Mapping from Object Type to <see cref="INode">INode</see>.</param>
    public static void AddToList<T>(this IGraph g, INode listRoot, IEnumerable<T> objects, Func<T, INode> mapFunc)
    {
        // If no objects to add then nothing to do
        if (!objects.Any()) return;

        // Get the List Tail
        INode listTail = GetListTail(g, listRoot);

        // Remove the rdf:rest rdf:nil triple
        INode rdfRest = g.CreateUriNode(g.UriFactory.Create(RdfSpecsHelper.RdfListRest));
        INode rdfNil = g.CreateUriNode(g.UriFactory.Create(RdfSpecsHelper.RdfListNil));
        g.Retract(new Triple(listTail, rdfRest, rdfNil));

        // Create a new tail for the list that will act as the root of the extended list
        INode newRoot = g.CreateBlankNode();
        INode rdfFirst = g.CreateUriNode(g.UriFactory.Create(RdfSpecsHelper.RdfListFirst));
        g.Assert(new Triple(listTail, rdfRest, newRoot));

        // Then assert the new list
        AssertList<T>(g, newRoot, objects, mapFunc);
    }

    /// <summary>
    /// Adds new items to the end of a list (aka a RDF collection).
    /// </summary>
    /// <param name="g">Graph to assert in.</param>
    /// <param name="listRoot">Root Node for the list.</param>
    /// <param name="objects">Objects to add to the collection.</param>
    public static void AddToList(this IGraph g, INode listRoot, IEnumerable<INode> objects)
    {
        AddToList(g, listRoot, objects, n => n);
    }

    /// <summary>
    /// Removes the given items from a list (aka a RDF collection), if an item occurs multiple times in the list, only the first occurrence will be removed.
    /// </summary>
    /// <typeparam name="T">Type of Objects.</typeparam>
    /// <param name="g">Graph to retract from.</param>
    /// <param name="listRoot">Root Node for the list.</param>
    /// <param name="objects">Objects to remove from the collection.</param>
    /// <param name="mapFunc">Mapping from Object Type to <see cref="INode">INode</see>.</param>
    public static void RemoveFromList<T>(this IGraph g, INode listRoot, IEnumerable<T> objects, Func<T, INode> mapFunc)
    {
        // If no objects to remove, then nothing to do
        if (!objects.Any()) return;

        // Figure out which items need removing
        var currObjects = GetListItems(g, listRoot).ToList();
        var initialCount = currObjects.Count;
        foreach (INode obj in objects.Select(x => mapFunc(x)))
        {
            currObjects.Remove(obj);
        }
        if (initialCount != currObjects.Count)
        {
            // We are removing one/more items
            // Easiest way to do this is simply to retract the entire list and then add the new list
            RetractList(g, listRoot);
            AssertList(g, listRoot, currObjects);
        }
    }

    /// <summary>
    /// Removes the given items from a list (aka a RDF collection), if an item occurs multiple times in the list, only the first occurrence will be removed.
    /// </summary>
    /// <param name="g">Graph to retract from.</param>
    /// <param name="listRoot">Root Node for the list.</param>
    /// <param name="objects">Objects to remove from the collection.</param>
    public static void RemoveFromList(this IGraph g, INode listRoot, IEnumerable<INode> objects)
    {
        RemoveFromList(g, listRoot, objects, n => n);
    }

    /// <summary>
    /// Removes the given items from a list (aka an RDF collection).
    /// </summary>
    /// <remarks>If an item occurs multiple times in the list, all instances of that item will be removed.</remarks>
    /// <param name="g">Graph to retract from.</param>
    /// <param name="listRoot">Root node of the list to be modified.</param>
    /// <param name="objects">Objects to remove from the collection.</param>
    /// <param name="mapFunc">Mapping from object type <typeparamref name="T"/> to <see cref="INode"/>.</param>
    /// <typeparam name="T">Type of objects to be removed.</typeparam>
    public static void RemoveAllFromList<T>(this IGraph g, INode listRoot, IEnumerable<T> objects,
        Func<T, INode> mapFunc)
    {
        var removeNodes = objects.Select(mapFunc).ToList();
        if (!removeNodes.Any())
        {
            // Nothing to remove
            return;
        }
        var currObjects = GetListItems(g, listRoot).ToList();
        var initialCount = currObjects.Count;
        currObjects.RemoveAll(x=>removeNodes.Contains(x));
        if (initialCount != currObjects.Count)
        {
            RetractList(g, listRoot);
            AssertList(g, listRoot, currObjects);
        }
    }

    /// <summary>
    /// Removes the given items from a list (aka an RDF collection).
    /// </summary>
    /// <remarks>If an item occurs multiple times in the list, all instances of that item will be removed.</remarks>
    /// <param name="g">Graph to retract from.</param>
    /// <param name="listRoot">Root node of the list to be modified.</param>
    /// <param name="objects">Nodes to remove from the collection.</param>
    public static void RemoveAllFromList(this IGraph g, INode listRoot, IEnumerable<INode> objects)
    {
        RemoveAllFromList(g, listRoot, objects, n => n);
    }
#endregion


    /// <summary>
    /// Copies a Triple from one Graph mapping Nodes as appropriate.
    /// </summary>
    /// <param name="t">Triple to copy.</param>
    /// <param name="target">TargetGraph.</param>
    /// <param name="mapping">Mapping of Nodes.</param>
    /// <returns></returns>
    public static Triple MapTriple(this Triple t, IGraph target, Dictionary<INode,INode> mapping)
    {
        INode s, p, o;
        if (mapping.ContainsKey(t.Subject))
        {
            s = mapping[t.Subject];
        }
        else if (t.Subject is ITripleNode stn)
        {
            s = new TripleNode(stn.Triple.MapTriple(target, mapping));
        }
        else
        {
            s = t.Subject;
        }

        p = mapping.ContainsKey(t.Predicate) ? mapping[t.Predicate] : t.Predicate;

        if (mapping.ContainsKey(t.Object))
        {
            o = mapping[t.Object];
        }
        else if (t.Object is ITripleNode otn)
        {
            o = new TripleNode(otn.Triple.MapTriple(target, mapping));
        }
        else
        {
            o = t.Object;
        }
        return new Triple(s, p, o);
    }

#region String related Extensions

    /// <summary>
    /// Gets either the String representation of the Object or the Empty String if the object is null.
    /// </summary>
    /// <param name="obj">Object.</param>
    /// <returns></returns>
    public static string ToSafeString(this object obj)
    {
        return (obj == null) ? string.Empty : obj.ToString();
    }

    /// <summary>
    /// Gets either the String representation of the URI or the Empty String if the URI is null.
    /// </summary>
    /// <param name="u">URI.</param>
    /// <returns></returns>
    public static string ToSafeString(this Uri u)
    {
        return (u == null) ? string.Empty : u.AbsoluteUri;
    }

    /// <summary>
    /// Turns a string into a safe URI.
    /// </summary>
    /// <param name="str">String.</param>
    /// <param name="uriFactory">Factor to use when creating the URI. If not specified, the <see cref="UriFactory.Root"/> instance will be used.</param>
    /// <returns>Either null if the string is null/empty or a URI otherwise.</returns>
    public static Uri ToSafeUri(this string str, IUriFactory uriFactory = null)
    {
        return string.IsNullOrEmpty(str) ? null : (uriFactory ?? UriFactory.Root).Create(str);
    }

    internal static string ToSafeString(IRefNode refNode)
    {
        if (refNode == null) return string.Empty;
        if (refNode.NodeType == NodeType.Uri) return ((IUriNode)refNode).Uri.AbsoluteUri;
        if (refNode.NodeType == NodeType.Blank) return $"_:{((IBlankNode)refNode).InternalID}";
        throw new RdfException("Unexpected node type in ToSafeString(IRefNode). Expected Either Uri or Blank but got " + refNode.NodeType);
    }

    /// <summary>
    /// Gets the String representation of the URI formatted using the given Formatter.
    /// </summary>
    /// <param name="u">URI.</param>
    /// <param name="formatter">URI Formatter.</param>
    /// <returns></returns>
    public static string ToString(this Uri u, IUriFormatter formatter)
    {
        return formatter.FormatUri(u);
    }

    /// <summary>
    /// Appends a String to the StringBuilder with an indent of <paramref name="indent"/> spaces.
    /// </summary>
    /// <param name="builder">String Builder.</param>
    /// <param name="line">String to append.</param>
    /// <param name="indent">Indent.</param>
    public static void AppendIndented(this StringBuilder builder, string line, int indent)
    {
        builder.Append(new string(' ', indent) + line);
    }

    /// <summary>
    /// Appends a String to the StringBuilder with an indent of <paramref name="indent"/> spaces.
    /// </summary>
    /// <param name="builder">String Builder.</param>
    /// <param name="line">String to append.</param>
    /// <param name="indent">Indent.</param>
    /// <remarks>
    /// Strings containing new lines are split over multiple lines.
    /// </remarks>
    public static void AppendLineIndented(this StringBuilder builder, string line, int indent)
    {
        var indentString = new string(' ', indent);
        if (line.Contains('\n'))
        {
            var lines = line.Split('\n');
            foreach (var l in lines)
            {
                if (string.IsNullOrEmpty(l) || l.ToCharArray().All(c => char.IsWhiteSpace(c))) continue;
                builder.Append(indentString);
                builder.AppendLine(l.TrimEnd('\r'));
            }
        }
        else
        {
            builder.Append(indentString);
            builder.AppendLine(line);
        }
    }

    /// <summary>
    /// Append all of the lines in <paramref name="block"/> to this StringBuilder with the specified indent
    /// as space characters.
    /// </summary>
    /// <param name="builder">The StringBuilder to append the block to.</param>
    /// <param name="block">The block to be appended.</param>
    /// <param name="indent">The string to prepend to each indented line.</param>
    /// <param name="indentFirst">Whether to indent the first line of the block or not.</param>
    public static void AppendBlockIndented(this StringBuilder builder, string block, string indent,
        bool indentFirst = false)
    {
        if (!block.Contains('\n'))
        {
            if (indentFirst)
            {
                builder.Append(indent);
            }
            builder.Append(block);
            return;
        }

        var lines = block.Split('\n');
        for (var ix = 0; ix < lines.Length; ix++)
        {
            if (ix > 0 || indentFirst)
            {
                builder.Append(indent);
            }

            var line = lines[ix].TrimEnd('\r');
            if (ix < lines.Length - 1)
            {
                builder.AppendLine(line);
            }
            else
            {
                builder.Append(line);
            }
        }
    }

    /// <summary>
    /// Determines whether a string is ASCII.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool IsAscii(this string value)
    {
        if (value.Length == 0) return true;
        return value.ToCharArray().All(c => c <= 127);
    }

    #endregion

    #region Node related Extensions

    /// <summary>
    /// Calculates the Effective Boolean Value of a given Node according to the Sparql specification.
    /// </summary>
    /// <param name="n">Node to computer EBV for.</param>
    /// <returns></returns>
    public static bool EffectiveBooleanValue(this INode n)
    {
        if (n == null)
        {
            // Nulls give Type Error
            throw new RdfQueryException("Cannot calculate the Effective Boolean Value of a null value");
        }
        else
        {
            if (n.NodeType == NodeType.Literal)
            {
                var lit = (ILiteralNode)n;

                if (lit.DataType == null)
                {
                    if (lit.Value == string.Empty)
                    {
                        // Empty String Literals have EBV of False
                        return false;
                    }
                    else
                    {
                        // Non-Empty String Literals have EBV of True
                        return true;
                    }
                }
                else
                {
                    // EBV is dependent on the Data Type for Typed Literals
                    var dt = lit.DataType.ToString();

                    if (dt.Equals(XmlSpecsHelper.XmlSchemaDataTypeBoolean))
                    {
                        // Boolean Typed Literal
                        if (bool.TryParse(lit.Value, out var b))
                        {
                            // Valid Booleans have EBV of their value
                            return b;
                        }
                        // Invalid Booleans have EBV of false
                        return false;
                    }
                    else if (dt.Equals(XmlSpecsHelper.XmlSchemaDataTypeString))
                    {
                        // String Typed Literal
                        if (lit.Value == string.Empty)
                        {
                            // Empty String Literals have EBV of False
                            return false;
                        }
                        else
                        {
                            // Non-Empty String Literals have EBV of True
                            return true;
                        }
                    }
                    else
                    {
                        // Is it a Number?
                        SparqlNumericType numType = NumericTypesHelper.GetNumericTypeFromDataTypeUri(dt);
                        switch (numType)
                        {
                            case SparqlNumericType.Decimal:
                                // Should be a decimal
                                decimal dec;
                                if (decimal.TryParse(lit.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out dec))
                                {
                                    if (dec == decimal.Zero)
                                    {
                                        // Zero gives EBV of false
                                        return false;
                                    }
                                    else
                                    {
                                        // Non-Zero gives EBV of true
                                        return true;
                                    }
                                }
                                else
                                {
                                    // Invalid Numerics have EBV of false
                                    return false;
                                }

                            case SparqlNumericType.Float:
                            case SparqlNumericType.Double:
                                // Should be a double
                                double dbl;
                                if (double.TryParse(lit.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out dbl))
                                {
                                    if (dbl == 0.0d || double.IsNaN(dbl))
                                    {
                                        // Zero/NaN gives EBV of false
                                        return false;
                                    }
                                    else
                                    {
                                        // Non-Zero gives EBV of true
                                        return true;
                                    }
                                }
                                else
                                {
                                    // Invalid Numerics have EBV of false
                                    return false;
                                }

                            case SparqlNumericType.Integer:
                                // Should be an Integer
                                long l;
                                if (long.TryParse(lit.Value, out l))
                                {
                                    if (l == 0)
                                    {
                                        // Zero gives EBV of false
                                        return false;
                                    }
                                    else
                                    {
                                        // Non-Zero gives EBV of true
                                        return true;
                                    }
                                }
                                else
                                {
                                    // Invalid Numerics have EBV of false
                                    return false;
                                }

                            case SparqlNumericType.NaN:
                                // If not a Numeric Type then Type error
                                throw new RdfQueryException("Unable to compute an Effective Boolean Value for a Literal Typed <" + dt + ">");

                            default:
                                // Shouldn't hit this case but included to keep compiler happy
                                throw new RdfQueryException("Unable to compute an Effective Boolean Value for a Literal Typed <" + dt + ">");
                        }
                    }
                }
            }
            else
            {
                // Non-Literal Nodes give type error
                throw new RdfQueryException("Cannot calculate the Effective Boolean Value of a non-literal RDF Term");
            }
        }
    }
    #endregion


}

/// <summary>
/// Provides useful Extension Methods for working with Graphs.
/// </summary>
public static class GraphExtensions
{
    /// <summary>
    /// Loads RDF data from a file into a Graph.
    /// </summary>
    /// <param name="g">Graph to load into.</param>
    /// <param name="file">File to load from.</param>
    /// <param name="parser">Parser to use.</param>
    /// <remarks>
    /// <para>
    /// This is just a shortcut to using the static <strong>Load()</strong> methods from the <see cref="FileLoader">FileLoader</see> class located in the <see cref="VDS.RDF.Parsing">Parsing</see> namespace.
    /// </para>
    /// <para>
    /// <strong>Note:</strong> FileLoader will assign the Graph a file URI as it's Base URI unless the Graph already has a Base URI or is non-empty prior to attempting parsing.  Note that any Base URI specified in the RDF contained in the file will override this initial Base URI.  In some cases this may lead to invalid RDF being accepted and generating strange relative URIs, if you encounter this either set a Base URI prior to calling this method or create an instance of the relevant parser and invoke it directly.
    /// </para>
    /// <para>
    /// If a File URI is assigned it will always be an absolute URI for the file.
    /// </para>
    /// </remarks>
    public static void LoadFromFile(this IGraph g, string file, IRdfReader parser)
    {
        FileLoader.Load(g, file, parser);
    }

    /// <summary>
    /// Loads RDF data from a file into a Graph.
    /// </summary>
    /// <param name="g">Graph to load into.</param>
    /// <param name="file">File to load from.</param>
    /// <remarks>
    /// <para>
    /// This is just a shortcut to using the static <strong>Load()</strong> methods from the <see cref="FileLoader">FileLoader</see> class located in the <see cref="VDS.RDF.Parsing">Parsing</see> namespace.
    /// </para>
    /// <para>
    /// <strong>Note:</strong> FileLoader will assign the Graph a file URI as it's Base URI unless the Graph already has a Base URI or is non-empty prior to attempting parsing.  Note that any Base URI specified in the RDF contained in the file will override this initial Base URI.  In some cases this may lead to invalid RDF being accepted and generating strange relative URIs, if you encounter this either set a Base URI prior to calling this method or create an instance of the relevant parser and invoke it directly.
    /// </para>
    /// <para>
    /// If a File URI is assigned it will always be an absolute URI for the file.
    /// </para>
    /// </remarks>
    public static void LoadFromFile(this IGraph g, string file)
    {
        FileLoader.Load(g, file);
    }

    /// <summary>
    /// Loads RDF data from a URI into a Graph.
    /// </summary>
    /// <param name="g">Graph to load into.</param>
    /// <param name="u">URI to load from.</param>
    /// <param name="parser">Parser to use.</param>
    /// <param name="loader">Loader to use.</param>
    /// <remarks>
    /// <para>
    /// This is just a shortcut to using the static <strong>LoadGraph()</strong> methods from the <see cref="Loader">UriLoader</see> class located in the <see cref="VDS.RDF.Parsing">Parsing</see> namespace.
    /// </para>
    /// <para>
    /// <strong>Note:</strong> Loader will assign the Graph the source URI as it's Base URI unless the Graph already has a Base URI or is non-empty prior to attempting parsing.  Note that any Base URI specified in the RDF contained in the file will override this initial Base URI.  In some cases this may lead to invalid RDF being accepted and generating strange relative URIs, if you encounter this either set a Base URI prior to calling this method or create an instance of the relevant parser and invoke it directly.
    /// </para>
    /// </remarks>
    public static void LoadFromUri(this IGraph g, Uri u, IRdfReader parser, Loader loader = null)
    {
        loader ??= new Loader();
        loader.LoadGraph(g, u, parser);
    }

    /// <summary>
    /// Loads RDF data from a URI into a Graph.
    /// </summary>
    /// <param name="g">Graph to load into.</param>
    /// <param name="u">URI to load from.</param>
    /// <param name="loader">Loader to use.</param>
    /// <remarks>
    /// <para>
    /// This is just a shortcut to using the static <strong>LoadGraph()</strong> methods from the <see cref="Loader">UriLoader</see> class located in the <see cref="VDS.RDF.Parsing">Parsing</see> namespace.
    /// </para>
    /// <para>
    /// <strong>Note:</strong> UriLoader will assign the Graph the source URI as it's Base URI unless the Graph already has a Base URI or is non-empty prior to attempting parsing.  Note that any Base URI specified in the RDF contained in the file will override this initial Base URI.  In some cases this may lead to invalid RDF being accepted and generating strange relative URIs, if you encounter this either set a Base URI prior to calling this method or create an instance of the relevant parser and invoke it directly.
    /// </para>
    /// </remarks>
    public static void LoadFromUri(this IGraph g, Uri u, Loader loader = null)
    {
        loader ??= new Loader();
        loader.LoadGraph(g, u);
    }

    // REQ: Add LoadFromUri extensions that do the loading asychronously for use on Silverlight/Windows Phone 7

    /// <summary>
    /// Loads RDF data from a String into a Graph.
    /// </summary>
    /// <param name="g">Graph to load into.</param>
    /// <param name="data">Data to load.</param>
    /// <param name="parser">Parser to use.</param>
    /// <remarks>
    /// This is just a shortcut to using the static <strong>Parse()</strong> methods from the <see cref="StringParser">StringParser</see> class located in the <see cref="VDS.RDF.Parsing">Parsing</see> namespace.
    /// </remarks>
    public static void LoadFromString(this IGraph g, string data, IRdfReader parser)
    {
        StringParser.Parse(g, data, parser);
    }

    /// <summary>
    /// Loads RDF data from a String into a Graph.
    /// </summary>
    /// <param name="g">Graph to load into.</param>
    /// <param name="data">Data to load.</param>
    /// <remarks>
    /// This is just a shortcut to using the static <strong>Parse()</strong> methods from the <see cref="StringParser">StringParser</see> class located in the <see cref="VDS.RDF.Parsing">Parsing</see> namespace.
    /// </remarks>
    public static void LoadFromString(this IGraph g, string data)
    {
        StringParser.Parse(g, data);
    }

    /// <summary>
    /// Loads RDF data from an Embedded Resource into a Graph.
    /// </summary>
    /// <param name="g">Graph to load into.</param>
    /// <param name="resource">Assembly qualified name of the resource to load from.</param>
    /// <remarks>
    /// This is just a shortcut to using the static <strong>Load()</strong> methods from the <see cref="EmbeddedResourceLoader">EmbeddedResourceLoader</see> class located in the <see cref="VDS.RDF.Parsing">Parsing</see> namespace.
    /// </remarks>
    public static void LoadFromEmbeddedResource(this IGraph g, string resource)
    {
        EmbeddedResourceLoader.Load(g, resource);
    }

    /// <summary>
    /// Loads RDF data from an Embedded Resource into a Graph.
    /// </summary>
    /// <param name="g">Graph to load into.</param>
    /// <param name="resource">Assembly qualified name of the resource to load from.</param>
    /// <param name="parser">Parser to use.</param>
    /// <remarks>
    /// This is just a shortcut to using the static <strong>Load()</strong> methods from the <see cref="EmbeddedResourceLoader">EmbeddedResourceLoader</see> class located in the <see cref="VDS.RDF.Parsing">Parsing</see> namespace.
    /// </remarks>
    public static void LoadFromEmbeddedResource(this IGraph g, string resource, IRdfReader parser)
    {
        EmbeddedResourceLoader.Load(g, resource, parser);
    }

    /// <summary>
    /// Saves a Graph to a File.
    /// </summary>
    /// <param name="g">Graph to save.</param>
    /// <param name="file">File to save to.</param>
    /// <param name="writer">Writer to use.</param>
    public static void SaveToFile(this IGraph g, string file, IRdfWriter writer)
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
    /// Saves a Graph to a File.
    /// </summary>
    /// <param name="g">Graph to save.</param>
    /// <param name="file">File to save to.</param>
    /// <param name="writer">Writer to use.</param>
    public static void SaveToFile(this IGraph g, string file, IStoreWriter writer)
    {
        if (writer == null)
        {
            throw new ArgumentNullException(nameof(writer));
        }
        else
        {
            var graphWriter = new SingleGraphWriter(writer);
            graphWriter.Save(g, file);
        }
    }

    /// <summary>
    /// Saves a Graph to a File.
    /// </summary>
    /// <param name="g">Graph to save.</param>
    /// <param name="file">File to save to.</param>
    public static void SaveToFile(this IGraph g, string file)
    {
        IRdfWriter writer = MimeTypesHelper.GetWriterByFileExtension(MimeTypesHelper.GetTrueFileExtension(file));
        writer.Save(g, file);
    }

    /// <summary>
    /// Saves a Graph to a stream.
    /// </summary>
    /// <param name="g">Graph to save.</param>
    /// <param name="streamWriter">Stream to save to.</param>
    /// <param name="writer">Writer to use.</param>
    public static void SaveToStream(this IGraph g, TextWriter streamWriter, IRdfWriter writer)
    {
        if (writer == null)
        {
            throw new ArgumentNullException(nameof(writer));
        }
        writer.Save(g, streamWriter);
    }

    /// <summary>
    /// Saves a Graph to a stream.
    /// </summary>
    /// <param name="g">Graph to save.</param>
    /// <param name="streamWriter">Stream to save to.</param>
    /// <param name="writer">Writer to use.</param>
    public static void SaveToStream(this IGraph g, TextWriter streamWriter, IStoreWriter writer)
    {
        if (writer == null)
        {
            throw new ArgumentNullException(nameof(writer));
        }
        var sgWriter = new SingleGraphWriter(writer);
        sgWriter.Save(g, streamWriter);
    }

    /// <summary>
    /// Save a graph to a stream, determining the type of writer to use by the output file name.
    /// </summary>
    /// <param name="g">The graph to write.</param>
    /// <param name="filename">The output file name to use to determine the output format to write.</param>
    /// <param name="streamWriter">The stream to write to.</param>
    public static void SaveToStream(this IGraph g, string filename, TextWriter streamWriter)
    {
        IRdfWriter writer = MimeTypesHelper.GetWriterByFileExtension(MimeTypesHelper.GetTrueFileExtension(filename));
        g.SaveToStream(streamWriter, writer);
    }

    /// <summary>
    /// Computes the ETag for a Graph.
    /// </summary>
    /// <param name="g">Graph.</param>
    /// <returns></returns>
    public static string GetETag(this IGraph g)
    {
        var ts = g.Triples.ToList();
        ts.Sort();

        var hash = new StringBuilder();
        foreach (Triple t in ts)
        {
            hash.AppendLine(t.GetHashCode().ToString());
        }
        var h = hash.ToString().GetHashCode().ToString();

        var sha1 = SHA1.Create();
        var hashBytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(h));
        hash = new StringBuilder();
        foreach (var b in hashBytes)
        {
            hash.Append(b.ToString("x2"));
        }
        return hash.ToString();
    }

}

/// <summary>
/// Provides useful Extension Methods for working with Triple Stores.
/// </summary>
public static class TripleStoreExtensions
{
    /// <summary>
    /// Loads an RDF dataset from a file into a Triple Store.
    /// </summary>
    /// <param name="store">Triple Store to load into.</param>
    /// <param name="file">File to load from.</param>
    /// <param name="parser">Parser to use.</param>
    /// <remarks>
    /// This is just a shortcut to using the static <strong>Load()</strong> methods from the <see cref="FileLoader">FileLoader</see> class located in the <see cref="VDS.RDF.Parsing">Parsing</see> namespace.
    /// </remarks>
    public static void LoadFromFile(this ITripleStore store, string file, IStoreReader parser)
    {
        FileLoader.Load(store, file, parser);
    }

    /// <summary>
    /// Loads an RDF dataset from a file into a Triple Store.
    /// </summary>
    /// <param name="store">Triple Store to load into.</param>
    /// <param name="file">File to load from.</param>
    /// <remarks>
    /// This is just a shortcut to using the static <strong>Load()</strong> methods from the <see cref="FileLoader">FileLoader</see> class located in the <see cref="VDS.RDF.Parsing">Parsing</see> namespace.
    /// </remarks>
    public static void LoadFromFile(this ITripleStore store, string file)
    {
        FileLoader.Load(store, file);
    }

    /// <summary>
    /// Loads an RDF dataset from a URI into a Triple Store.
    /// </summary>
    /// <param name="store">Triple Store to load into.</param>
    /// <param name="u">URI to load from.</param>
    /// <param name="parser">Parser to use.</param>
    /// <remarks>
    /// This is just a shortcut to using the static <strong>LoadDataset()</strong> methods from the <see cref="Loader">UriLoader</see> class located in the <see cref="VDS.RDF.Parsing">Parsing</see> namespace.
    /// </remarks>
    public static void LoadFromUri(this ITripleStore store, Uri u, IStoreReader parser)
    {
        LoadFromUri(store, u, parser, new Loader());
    }

    /// <summary>
    /// Loads an RDF dataset from a URI into a Triple Store.
    /// </summary>
    /// <param name="store">Triple Store to load into.</param>
    /// <param name="u">URI to load from.</param>
    /// <remarks>
    /// This is just a shortcut to using the static <strong>LoadDataset()</strong> methods from the <see cref="Loader">UriLoader</see> class located in the <see cref="VDS.RDF.Parsing">Parsing</see> namespace.
    /// </remarks>
    public static void LoadFromUri(this ITripleStore store, Uri u)
    {
        LoadFromUri(store, u, null, new Loader());
    }

    /// <summary>
    /// Loads an RDF dataset from a URI into a Triple Store.
    /// </summary>
    /// <param name="store">Triple Store to load into.</param>
    /// <param name="u">URI to load from.</param>
    /// <param name="parser">Parser to use.</param>
    /// <param name="loader">Loader to use.</param>
    /// <remarks>
    /// This is just a shortcut to using the static <strong>LoadDataset()</strong> methods from the <see cref="Loader">UriLoader</see> class located in the <see cref="VDS.RDF.Parsing">Parsing</see> namespace.
    /// </remarks>

    public static void LoadFromUri(this ITripleStore store, Uri u, IStoreReader parser, Loader loader)
    {
        loader.LoadDataset(store, u, parser);
    }

    /// <summary>
    /// Loads an RDF dataset from a String into a Triple Store.
    /// </summary>
    /// <param name="store">Triple Store to load into.</param>
    /// <param name="data">Data to load.</param>
    /// <param name="parser">Parser to use.</param>
    /// <remarks>
    /// This is just a shortcut to using the static <strong>ParseDataset()</strong> methods from the <see cref="StringParser">StringParser</see> class located in the <see cref="VDS.RDF.Parsing">Parsing</see> namespace.
    /// </remarks>
    public static void LoadFromString(this ITripleStore store, string data, IStoreReader parser)
    {
        StringParser.ParseDataset(store, data, parser);
    }

    /// <summary>
    /// Loads an RDF dataset from a String into a Triple Store.
    /// </summary>
    /// <param name="store">Triple Store to load into.</param>
    /// <param name="data">Data to load.</param>
    /// <remarks>
    /// This is just a shortcut to using the static <strong>ParseDataset()</strong> methods from the <see cref="StringParser">StringParser</see> class located in the <see cref="VDS.RDF.Parsing">Parsing</see> namespace.
    /// </remarks>
    public static void LoadFromString(this ITripleStore store, string data)
    {
        StringParser.ParseDataset(store, data);
    }

    /// <summary>
    /// Loads an RDF dataset from an Embedded Resource into a Triple Store.
    /// </summary>
    /// <param name="store">Triple Store to load into.</param>
    /// <param name="resource">Assembly Qualified Name of the Embedded Resource to load from.</param>
    /// <param name="parser">Parser to use.</param>
    /// <remarks>
    /// This is just a shortcut to using the static <strong>Load()</strong> methods from the <see cref="EmbeddedResourceLoader">EmbeddedResourceLoader</see> class located in the <see cref="VDS.RDF.Parsing">Parsing</see> namespace.
    /// </remarks>
    public static void LoadFromEmbeddedResource(this ITripleStore store, string resource, IStoreReader parser)
    {
        EmbeddedResourceLoader.Load(store, resource, parser);
    }

    /// <summary>
    /// Loads an RDF dataset from an Embedded Resource into a Triple Store.
    /// </summary>
    /// <param name="store">Triple Store to load into.</param>
    /// <param name="resource">Assembly Qualified Name of the Embedded Resource to load from.</param>
    /// <remarks>
    /// This is just a shortcut to using the static <strong>Load()</strong> methods from the <see cref="EmbeddedResourceLoader">EmbeddedResourceLoader</see> class located in the <see cref="VDS.RDF.Parsing">Parsing</see> namespace.
    /// </remarks>
    public static void LoadFromEmbeddedResource(this ITripleStore store, string resource)
    {
        EmbeddedResourceLoader.Load(store, resource);
    }

    /// <summary>
    /// Saves a Triple Store to a file.
    /// </summary>
    /// <param name="store">Triple Store to save.</param>
    /// <param name="file">File to save to.</param>
    /// <param name="writer">Writer to use.</param>
    public static void SaveToFile(this ITripleStore store, string file, IStoreWriter writer)
    {
        if (writer == null)
        {
            store.SaveToFile(file);
        }
        else
        {
            writer.Save(store, file);
        }
    }

    /// <summary>
    /// Saves a Triple Store to a file.
    /// </summary>
    /// <param name="store">Triple Store to save.</param>
    /// <param name="file">File to save to.</param>
    public static void SaveToFile(this ITripleStore store, string file)
    {
        IStoreWriter writer = MimeTypesHelper.GetStoreWriterByFileExtension(MimeTypesHelper.GetTrueFileExtension(file));
        writer.Save(store, file);
    }
}

/// <summary>
/// Provides extension methods for converting primitive types into appropriately typed Literal Nodes.
/// </summary>
public static class LiteralExtensions
{
    /// <summary>
    /// Creates a new Boolean typed literal.
    /// </summary>
    /// <param name="b">Boolean.</param>
    /// <param name="factory">Node Factory to use for Node creation.</param>
    /// <returns>Literal representing the boolean.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the Factory argument is null.</exception>
    public static ILiteralNode ToLiteral(this bool b, INodeFactory factory)
    {
        if (factory == null) throw new ArgumentNullException(nameof(factory), "Cannot create a Literal Node in a null Node Factory");

        return factory.CreateLiteralNode(b.ToString().ToLower(), factory.UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeBoolean));
    }

    /// <summary>
    /// Creates a new Byte typed literal.
    /// </summary>
    /// <param name="b">Byte.</param>
    /// <param name="factory">Node Factory to use for Node creation.</param>
    /// <returns>Literal representing the byte.</returns>
    /// <remarks>
    /// Byte in .Net is actually equivalent to Unsigned Byte in XML Schema so depending on the value of the Byte the type will either be xsd:byte if it fits or xsd:usignedByte.
    /// </remarks>
    public static ILiteralNode ToLiteral(this byte b, INodeFactory factory)
    {
        if (factory == null) throw new ArgumentNullException(nameof(factory), "Cannot create a Literal Node in a null Node Factory");

        if (b > 128)
        {
            // If value is > 128 must use unsigned byte as the type as xsd:byte has range -127 to 128 
            // while .Net byte has range 0-255
            return factory.CreateLiteralNode(XmlConvert.ToString(b), factory.UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeUnsignedByte));
        }
        else
        {
            return factory.CreateLiteralNode(XmlConvert.ToString(b), factory.UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeByte));
        }
    }

    /// <summary>
    /// Creates a new Byte typed literal.
    /// </summary>
    /// <param name="b">Byte.</param>
    /// <param name="factory">Node Factory to use for Node creation.</param>
    /// <returns>Literal representing the signed bytes.</returns>
    /// <remarks>
    /// SByte in .Net is directly equivalent to Byte in XML Schema so the type will always be xsd:byte.
    /// </remarks>
    public static ILiteralNode ToLiteral(this sbyte b, INodeFactory factory)
    {
        if (factory == null) throw new ArgumentNullException("factory", "Cannot create a Literal Node in a null Node Factory");

        return factory.CreateLiteralNode(XmlConvert.ToString(b), factory.UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeByte));
    }

    /// <summary>
    /// Creates a new Date Time typed literal.
    /// </summary>
    /// <param name="dt">Date Time.</param>
    /// <param name="factory">Node Factory to use for Node creation.</param>
    /// <returns>Literal representing the date time.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the Factory argument is null.</exception>
    public static ILiteralNode ToLiteral(this DateTime dt, INodeFactory factory)
    {
        return ToLiteral(dt, factory, true);
    }

    /// <summary>
    /// Creates a new Date Time typed literal.
    /// </summary>
    /// <param name="dt">Date Time.</param>
    /// <param name="factory">Node Factory to use for Node creation.</param>
    /// <param name="precise">Whether to preserve precisely i.e. include fractional seconds.</param>
    /// <returns>Literal representing the date time.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the Factory argument is null.</exception>
    public static ILiteralNode ToLiteral(this DateTime dt, INodeFactory factory, bool precise)
    {
        if (factory == null) throw new ArgumentNullException("factory", "Cannot create a Literal Node in a null Node Factory");

        return factory.CreateLiteralNode(dt.ToString(precise ? XmlSpecsHelper.XmlSchemaDateTimeFormat : XmlSpecsHelper.XmlSchemaDateTimeFormatImprecise), factory.UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDateTime));
    }

    /// <summary>
    /// Creates a new Date Time typed literal.
    /// </summary>
    /// <param name="dt">Date Time.</param>
    /// <param name="factory">Node Factory to use for Node creation.</param>
    /// <returns>Literal representing the date time.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the Factory argument is null.</exception>
    public static ILiteralNode ToLiteral(this DateTimeOffset dt, INodeFactory factory)
    {
        return ToLiteral(dt, factory, true);
    }

    /// <summary>
    /// Creates a new Date Time typed literal.
    /// </summary>
    /// <param name="dt">Date Time.</param>
    /// <param name="factory">Node Factory to use for Node creation.</param>
    /// <param name="precise">Whether to preserve precisely i.e. include fractional seconds.</param>
    /// <returns>Literal representing the date time.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the Factory argument is null.</exception>
    public static ILiteralNode ToLiteral(this DateTimeOffset dt, INodeFactory factory, bool precise)
    {
        if (factory == null) throw new ArgumentNullException("factory", "Cannot create a Literal Node in a null Node Factory");

        return factory.CreateLiteralNode(dt.ToString(precise ? XmlSpecsHelper.XmlSchemaDateTimeFormat : XmlSpecsHelper.XmlSchemaDateTimeFormatImprecise), factory.UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDateTime));
    }

    /// <summary>
    /// Creates a new Date typed literal.
    /// </summary>
    /// <param name="dt">Date Time.</param>
    /// <param name="factory">Node Factory to use for Node creation.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">Thrown if the Factory argument is null.</exception>
    public static ILiteralNode ToLiteralDate(this DateTime dt, INodeFactory factory)
    {
        if (factory == null) throw new ArgumentNullException("factory", "Cannot create a Literal Node in a null Node Factory");

        return factory.CreateLiteralNode(dt.ToString(XmlSpecsHelper.XmlSchemaDateFormat), factory.UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDate));
    }

    /// <summary>
    /// Creates a new Date typed literal.
    /// </summary>
    /// <param name="dt">Date Time.</param>
    /// <param name="factory">Node Factory to use for Node creation.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">Thrown if the Factory argument is null.</exception>
    public static ILiteralNode ToLiteralDate(this DateTimeOffset dt, INodeFactory factory)
    {
        if (factory == null) throw new ArgumentNullException("factory", "Cannot create a Literal Node in a null Node Factory");

        return factory.CreateLiteralNode(dt.ToString(XmlSpecsHelper.XmlSchemaDateFormat), factory.UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDate));
    }

    /// <summary>
    /// Creates a new Time typed literal.
    /// </summary>
    /// <param name="dt">Date Time.</param>
    /// <param name="factory">Node Factory to use for Node creation.</param>
    /// <returns>Literal representing the time.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the Factory argument is null.</exception>
    public static ILiteralNode ToLiteralTime(this DateTime dt, INodeFactory factory)
    {
        return ToLiteralTime(dt, factory, true);
    }

    /// <summary>
    /// Creates a new Time typed literal.
    /// </summary>
    /// <param name="dt">Date Time.</param>
    /// <param name="factory">Node Factory to use for Node creation.</param>
    /// <param name="precise">Whether to preserve precisely i.e. include fractional seconds.</param>
    /// <returns>Literal representing the time.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the Factory argument is null.</exception>
    public static ILiteralNode ToLiteralTime(this DateTime dt, INodeFactory factory, bool precise)
    {
        if (factory == null) throw new ArgumentNullException("factory", "Cannot create a Literal Node in a null Node Factory");

        return factory.CreateLiteralNode(dt.ToString(precise ? XmlSpecsHelper.XmlSchemaTimeFormat : XmlSpecsHelper.XmlSchemaTimeFormatImprecise), factory.UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeTime));
    }

    
    /// <summary>
    /// Creates a new duration typed literal.
    /// </summary>
    /// <param name="t">Time Span.</param>
    /// <param name="factory">Node Factory to use for Node creation.</param>
    /// <returns>Literal representing the time span.</returns>
    public static ILiteralNode ToLiteral(this TimeSpan t, INodeFactory factory)
    {
        if (factory == null) throw new ArgumentNullException("factory", "Cannot create a Literal Node in a null Node Factory");

        return factory.CreateLiteralNode(XmlConvert.ToString(t), factory.UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDuration));
    }

    /// <summary>
    /// Creates a new Time typed literal.
    /// </summary>
    /// <param name="dt">Date Time.</param>
    /// <param name="factory">Node Factory to use for Node creation.</param>
    /// <returns>Literal representing the time.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the Factory argument is null.</exception>
    public static ILiteralNode ToLiteralTime(this DateTimeOffset dt, INodeFactory factory)
    {
        return ToLiteralTime(dt, factory, true);
    }

    /// <summary>
    /// Creates a new Time typed literal.
    /// </summary>
    /// <param name="dt">Date Time.</param>
    /// <param name="factory">Node Factory to use for Node creation.</param>
    /// <param name="precise">Whether to preserve precisely i.e. include fractional seconds.</param>
    /// <returns>Literal representing the time.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the Factory argument is null.</exception>
    public static ILiteralNode ToLiteralTime(this DateTimeOffset dt, INodeFactory factory, bool precise)
    {
        if (factory == null) throw new ArgumentNullException("factory", "Cannot create a Literal Node in a null Node Factory");

        return factory.CreateLiteralNode(dt.ToString(precise ? XmlSpecsHelper.XmlSchemaTimeFormat : XmlSpecsHelper.XmlSchemaTimeFormatImprecise), factory.UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeTime));
    }

    /// <summary>
    /// Creates a new Decimal typed literal.
    /// </summary>
    /// <param name="d">Decimal.</param>
    /// <param name="factory">Node Factory to use for Node creation.</param>
    /// <returns>Literal representing the decimal.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the Factory argument is null.</exception>
    public static ILiteralNode ToLiteral(this decimal d, INodeFactory factory)
    {
        if (factory == null) throw new ArgumentNullException("factory", "Cannot create a Literal Node in a null Node Factory");

        return factory.CreateLiteralNode(d.ToString(CultureInfo.InvariantCulture), factory.UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDecimal));
    }

    /// <summary>
    /// Creates a new Double typed literal.
    /// </summary>
    /// <param name="d">Double.</param>
    /// <param name="factory">Node Factory to use for Node creation.</param>
    /// <returns>Literal representing the double.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the Factory argument is null.</exception>
    public static ILiteralNode ToLiteral(this double d, INodeFactory factory)
    {
        if (factory == null) throw new ArgumentNullException("factory", "Cannot create a Literal Node in a null Node Factory");

        return factory.CreateLiteralNode(d.ToString(CultureInfo.InvariantCulture), factory.UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDouble));
    }

    /// <summary>
    /// Creates a new Float typed literal.
    /// </summary>
    /// <param name="f">Float.</param>
    /// <param name="factory">Node Factory to use for Node creation.</param>
    /// <returns>Literal representing the float.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the Factory argument is null.</exception>
    public static ILiteralNode ToLiteral(this float f, INodeFactory factory)
    {
        if (factory == null) throw new ArgumentNullException("factory", "Cannot create a Literal Node in a null Node Factory");

        return factory.CreateLiteralNode(f.ToString(CultureInfo.InvariantCulture), factory.UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeFloat));
    }

    /// <summary>
    /// Creates a new Integer typed literal.
    /// </summary>
    /// <param name="i">Integer.</param>
    /// <param name="factory">Node Factory to use for Node creation.</param>
    /// <returns>Literal representing the short.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the Factory argument is null.</exception>
    public static ILiteralNode ToLiteral(this short i, INodeFactory factory)
    {
        if (factory == null) throw new ArgumentNullException("factory", "Cannot create a Literal Node in a null Node Factory");

        return factory.CreateLiteralNode(i.ToString(CultureInfo.InvariantCulture), factory.UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeInteger));
    }

    /// <summary>
    /// Creates a new Integer typed literal.
    /// </summary>
    /// <param name="i">Integer.</param>
    /// <param name="factory">Node Factory to use for Node creation.</param>
    /// <returns>Literal representing the integer.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the Factory argument is null.</exception>
    public static ILiteralNode ToLiteral(this int i, INodeFactory factory)
    {
        if (factory == null) throw new ArgumentNullException("factory", "Cannot create a Literal Node in a null Node Factory");

        return factory.CreateLiteralNode(i.ToString(CultureInfo.InvariantCulture), factory.UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeInteger));
    }

    /// <summary>
    /// Creates a new Integer typed literal.
    /// </summary>
    /// <param name="l">Integer.</param>
    /// <param name="factory">Node Factory to use for Node creation.</param>
    /// <returns>Literal representing the integer.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the Factory argument is null.</exception>
    public static ILiteralNode ToLiteral(this long l, INodeFactory factory)
    {
        if (factory == null) throw new ArgumentNullException("factory", "Cannot create a Literal Node in a null Node Factory");

        return factory.CreateLiteralNode(l.ToString(CultureInfo.InvariantCulture), factory.UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeInteger));
    }

    /// <summary>
    /// Creates a new String typed literal.
    /// </summary>
    /// <param name="s">String.</param>
    /// <param name="factory">Node Factory to use for Node creation.</param>
    /// <returns>Literal representing the string.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the Graph/String argument is null.</exception>
    public static ILiteralNode ToLiteral(this string s, INodeFactory factory)
    {
        if (factory == null) throw new ArgumentNullException("factory", "Cannot create a Literal Node in a null Node Factory");
        if (s == null) throw new ArgumentNullException("s", "Cannot create a Literal Node for a null String");

        return factory.CreateLiteralNode(s, factory.UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
    }

    /// <summary>
    /// Creates <see cref="BaseTripleCollection"/> from an enumeration of triples.
    /// </summary>
    /// <param name="triples">The triples to add to the collection.</param>
    /// <param name="indexed">True to create an indexed collection, false to create an un-indexed collection.</param>
    /// <returns></returns>
    public static BaseTripleCollection ToTripleCollection(this IEnumerable<Triple> triples, bool indexed=true)
    {
        BaseTripleCollection collection = indexed ? new TreeIndexedTripleCollection() : new TripleCollection();
        foreach (Triple t in triples) collection.Add(t);
        return collection;
    }
}
