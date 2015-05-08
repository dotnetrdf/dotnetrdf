/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2013 dotNetRDF Project (dotnetrdf-develop@lists.sf.net)

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
using VDS.RDF.Graphs;
using VDS.RDF.Nodes;
using VDS.RDF.Specifications;

namespace VDS.RDF
{
    /// <summary>
    /// Extension methods to make working with RDF lists easier
    /// </summary>
    public static class ListExtensions 
    {
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
            INode listRoot = g.CreateBlankNode();
            AssertList<T>(g, listRoot, objects, mapFunc);
            return listRoot;
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
    }
}