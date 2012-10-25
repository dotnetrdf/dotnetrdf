/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

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
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.Common;
using VDS.Common.Trees;
using VDS.RDF.Parsing;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Test
{
    /// <summary>
    /// Summary description for IndexingTests
    /// </summary>
    [TestClass]
    public class IndexingTests
    {
        [TestMethod]
        public void IndexingNodesInMultiDictionary1()
        {
            Graph g = new Graph();
            ILiteralNode canonical = (1).ToLiteral(g);
            ILiteralNode alternate = g.CreateLiteralNode("01", UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeInteger));

            //Use a dud hash function to put everything into a single bucket
            MultiDictionary<INode, int> dictionary = new MultiDictionary<INode, int>(n => 1);
            dictionary.Add(canonical, 1);
            Assert.AreEqual(1, dictionary[canonical]);
            dictionary.Add(alternate, 2);

            //With everything in a single bucket the keys should be considered
            //equal by the default comparer hence the key count will only be one
            //and retrieving with either 2 gives the value from the second Add()
            Assert.AreEqual(1, dictionary.Count);
            Assert.AreEqual(2, dictionary[alternate]);
            Assert.AreEqual(2, dictionary[canonical]);
        }

        [TestMethod]
        public void IndexingNodesInMultiDictionary2()
        {
            Graph g = new Graph();
            ILiteralNode canonical = (1).ToLiteral(g);
            ILiteralNode alternate = g.CreateLiteralNode("01", UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeInteger));

            MultiDictionary<INode, int> dictionary = new MultiDictionary<INode, int>();
            dictionary.Add(canonical, 1);
            Assert.AreEqual(1, dictionary[canonical]);
            dictionary.Add(alternate, 2);
            Assert.AreEqual(2, dictionary.Count);
            Assert.AreEqual(2, dictionary[alternate]);
        }

        [TestMethod]
        public void IndexingNodesInMultiDictionary3()
        {
            Graph g = new Graph();
            ILiteralNode canonical = (1).ToLiteral(g);
            ILiteralNode alternate = g.CreateLiteralNode("01", UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeInteger));

            //Use a dud hash function to put everything into a single bucket and use
            //the FastNodeComparer
            MultiDictionary<INode, int> dictionary = new MultiDictionary<INode, int>(n => 1, new FastNodeComparer(), MultiDictionaryMode.AVL);
            dictionary.Add(canonical, 1);
            Assert.AreEqual(1, dictionary[canonical]);
            dictionary.Add(alternate, 2);

            //With everything in a single bucket the keys should be considered
            //non-equal by FastNodeComparer so should see key count of 2 and be able
            //to retrieve the specific values by their keys
            Assert.AreEqual(2, dictionary.Count);
            Assert.AreEqual(2, dictionary[alternate]);
            Assert.AreEqual(1, dictionary[canonical]);
            Assert.AreNotEqual(2, dictionary[canonical]);
        }

        [TestMethod]
        public void IndexingNodesInBinaryTree1()
        {
            Graph g = new Graph();
            ILiteralNode canonical = (1).ToLiteral(g);
            ILiteralNode alternate = g.CreateLiteralNode("01", UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeInteger));

            AVLTree<INode, int> tree = new AVLTree<INode, int>();
            
            tree.Add(canonical, 1);
            Assert.AreEqual(1, tree[canonical]);
            tree.Add(alternate, 2);

            //Since the default comparer considers the keys to be equal
            //lookup via either key should now give the value 2 rather than the originally
            //set value since the 2nd Add() just changes the existing value for the key
            //rather than adding a new key value pair
            Assert.AreEqual(2, tree[alternate]);
            Assert.AreEqual(2, tree[canonical]);

            //With the default comparer we expect to see 1 here rather than 2 because
            //the keys are considered equal
            Assert.AreEqual(1, tree.Keys.Count());
        }

        [TestMethod]
        public void IndexingNodesInBinaryTree2()
        {
            Graph g = new Graph();
            ILiteralNode canonical = (1).ToLiteral(g);
            ILiteralNode alternate = g.CreateLiteralNode("01", UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeInteger));

            AVLTree<INode, int> tree = new AVLTree<INode, int>(new FastNodeComparer());

            tree.Add(canonical, 1);
            Assert.AreEqual(1, tree[canonical]);
            tree.Add(alternate, 2);

            //With the FastNodeComparer the keys are non-equal so should
            //create separate key value pairs in the tree
            Assert.AreEqual(2, tree[alternate]);
            Assert.AreEqual(1, tree[canonical]);
            Assert.AreNotEqual(2, tree[canonical]);

            //With the FastNodeComparer there should be 2 keys in the tree
            //because the keys are not considered equal
            Assert.AreEqual(2, tree.Keys.Count());
        }

        [TestMethod]
        public void IndexingTriplesInBinaryTree1()
        {
            Graph g = new Graph();
            ILiteralNode canonical = (1).ToLiteral(g);
            ILiteralNode alternate = g.CreateLiteralNode("01", UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeInteger));
            Triple a = new Triple(g.CreateVariableNode("s"), g.CreateVariableNode("p"), canonical);
            Triple b = new Triple(g.CreateVariableNode("s"), g.CreateVariableNode("p"), alternate);

            AVLTree<Triple, int> tree = new AVLTree<Triple, int>();

            tree.Add(a, 1);
            Assert.AreEqual(1, tree[a]);
            tree.Add(b, 2);

            //Since the default comparer considers the keys to be equal
            //lookup via either key should now give the value 2 rather than the originally
            //set value since the 2nd Add() just changes the existing value for the key
            //rather than adding a new key value pair
            Assert.AreEqual(2, tree[a]);
            Assert.AreEqual(2, tree[b]);

            //With the default comparer we expect to see 1 here rather than 2 because
            //the keys are considered equal
            Assert.AreEqual(1, tree.Keys.Count());
        }

        [TestMethod]
        public void IndexingTriplesInBinaryTree2()
        {
            Graph g = new Graph();
            ILiteralNode canonical = (1).ToLiteral(g);
            ILiteralNode alternate = g.CreateLiteralNode("01", UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeInteger));
            Triple a = new Triple(g.CreateVariableNode("s"), g.CreateVariableNode("p"), canonical);
            Triple b = new Triple(g.CreateVariableNode("s"), g.CreateVariableNode("p"), alternate);

            AVLTree<Triple, int> tree = new AVLTree<Triple, int>(new FullTripleComparer(new FastNodeComparer()));

            tree.Add(a, 1);
            Assert.AreEqual(1, tree[a]);
            tree.Add(b, 2);

            //With the FastNodeComparer the keys are non-equal so should
            //create separate key value pairs in the tree
            Assert.AreEqual(2, tree[b]);
            Assert.AreEqual(1, tree[a]);
            Assert.AreNotEqual(2, tree[a]);

            //With the FastNodeComparer there should be 2 keys in the tree
            //because the keys are not considered equal
            Assert.AreEqual(2, tree.Keys.Count());
        }
    }
}
