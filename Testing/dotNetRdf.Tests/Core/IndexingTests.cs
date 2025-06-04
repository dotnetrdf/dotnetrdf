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

using System.Linq;
using Xunit;
using VDS.Common.Collections;
using VDS.Common.Trees;
using VDS.RDF.Parsing;

namespace VDS.RDF;

/// <summary>
/// Summary description for IndexingTests
/// </summary>

public class IndexingTests
{
    [Fact]
    public void IndexingNodesInMultiDictionary1()
    {
        var g = new Graph();
        ILiteralNode canonical = (1).ToLiteral(g);
        ILiteralNode alternate = g.CreateLiteralNode("01", UriFactory.Root.Create(XmlSpecsHelper.XmlSchemaDataTypeInteger));

        //Use a dud hash function to put everything into a single bucket
        var dictionary = new MultiDictionary<INode, int>(n => 1, false);
        dictionary.Add(canonical, 1);
        Assert.Equal(1, dictionary[canonical]);
        dictionary[alternate] = 2;

        //With everything in a single bucket the keys should be considered
        //equal by the default comparer hence the key count will only be one
        //and retrieving with either 2 gives the value from the second Add()
        Assert.Single(dictionary);
        Assert.Equal(2, dictionary[alternate]);
        Assert.Equal(2, dictionary[canonical]);
    }

    [Fact]
    public void IndexingNodesInMultiDictionary2()
    {
        var g = new Graph();
        ILiteralNode canonical = (1).ToLiteral(g);
        ILiteralNode alternate = g.CreateLiteralNode("01", UriFactory.Root.Create(XmlSpecsHelper.XmlSchemaDataTypeInteger));

        var dictionary = new MultiDictionary<INode, int>();
        dictionary.Add(canonical, 1);
        Assert.Equal(1, dictionary[canonical]);
        dictionary.Add(alternate, 2);
        Assert.Equal(2, dictionary.Count);
        Assert.Equal(2, dictionary[alternate]);
    }

    [Fact]
    public void IndexingNodesInMultiDictionary3()
    {
        var g = new Graph();
        ILiteralNode canonical = (1).ToLiteral(g);
        ILiteralNode alternate = g.CreateLiteralNode("01", UriFactory.Root.Create(XmlSpecsHelper.XmlSchemaDataTypeInteger));

        //Use a dud hash function to put everything into a single bucket and use
        //the FastNodeComparer
        var dictionary = new MultiDictionary<INode, int>(n => 1, false, new FastNodeComparer(), MultiDictionaryMode.Avl);
        dictionary.Add(canonical, 1);
        Assert.Equal(1, dictionary[canonical]);
        dictionary.Add(alternate, 2);

        //With everything in a single bucket the keys should be considered
        //non-equal by FastNodeComparer so should see key count of 2 and be able
        //to retrieve the specific values by their keys
        Assert.Equal(2, dictionary.Count);
        Assert.Equal(2, dictionary[alternate]);
        Assert.Equal(1, dictionary[canonical]);
        Assert.NotEqual(2, dictionary[canonical]);
    }

    [Fact]
    public void IndexingNodesInBinaryTree1()
    {
        var g = new Graph();
        ILiteralNode canonical = (1).ToLiteral(g);
        ILiteralNode alternate = g.CreateLiteralNode("01", UriFactory.Root.Create(XmlSpecsHelper.XmlSchemaDataTypeInteger));

        var tree = new AvlTree<INode, int>();
        
        tree.Add(canonical, 1);
        Assert.Equal(1, tree[canonical]);
        tree[alternate] = 2;

        //Since the default comparer considers the keys to be equal
        //lookup via either key should now give the value 2 rather than the originally
        //set value since the 2nd Add() just changes the existing value for the key
        //rather than adding a new key value pair
        Assert.Equal(2, tree[alternate]);
        Assert.Equal(2, tree[canonical]);

        //With the default comparer we expect to see 1 here rather than 2 because
        //the keys are considered equal
        Assert.Single(tree.Keys);
    }

    [Fact]
    public void IndexingNodesInBinaryTree2()
    {
        var g = new Graph();
        ILiteralNode canonical = (1).ToLiteral(g);
        ILiteralNode alternate = g.CreateLiteralNode("01", UriFactory.Root.Create(XmlSpecsHelper.XmlSchemaDataTypeInteger));

        var tree = new AvlTree<INode, int>(new FastNodeComparer());

        tree.Add(canonical, 1);
        Assert.Equal(1, tree[canonical]);
        tree.Add(alternate, 2);

        //With the FastNodeComparer the keys are non-equal so should
        //create separate key value pairs in the tree
        Assert.Equal(2, tree[alternate]);
        Assert.Equal(1, tree[canonical]);
        Assert.NotEqual(2, tree[canonical]);

        //With the FastNodeComparer there should be 2 keys in the tree
        //because the keys are not considered equal
        Assert.Equal(2, tree.Keys.Count());
    }

    [Fact]
    public void IndexingTriplesInBinaryTree1()
    {
        var g = new Graph();
        ILiteralNode canonical = (1).ToLiteral(g);
        ILiteralNode alternate = g.CreateLiteralNode("01", UriFactory.Root.Create(XmlSpecsHelper.XmlSchemaDataTypeInteger));
        var a = new Triple(g.CreateVariableNode("s"), g.CreateVariableNode("p"), canonical);
        var b = new Triple(g.CreateVariableNode("s"), g.CreateVariableNode("p"), alternate);

        var tree = new AvlTree<Triple, int>();

        tree.Add(a, 1);
        Assert.Equal(1, tree[a]);
        tree[b] = 2;

        //Since the default comparer considers the keys to be equal
        //lookup via either key should now give the value 2 rather than the originally
        //set value since the 2nd Add() just changes the existing value for the key
        //rather than adding a new key value pair
        Assert.Equal(2, tree[a]);
        Assert.Equal(2, tree[b]);

        //With the default comparer we expect to see 1 here rather than 2 because
        //the keys are considered equal
        Assert.Single(tree.Keys);
    }

    [Fact]
    public void IndexingTriplesInBinaryTree2()
    {
        var g = new Graph();
        ILiteralNode canonical = 1.ToLiteral(g);
        ILiteralNode alternate = g.CreateLiteralNode("01", UriFactory.Root.Create(XmlSpecsHelper.XmlSchemaDataTypeInteger));
        var a = new Triple(g.CreateVariableNode("s"), g.CreateVariableNode("p"), canonical);
        var b = new Triple(g.CreateVariableNode("s"), g.CreateVariableNode("p"), alternate);

        var tree = new AvlTree<Triple, int>(new FullTripleComparer(new FastNodeComparer()));

        tree.Add(a, 1);
        Assert.Equal(1, tree[a]);
        tree.Add(b, 2);

        //With the FastNodeComparer the keys are non-equal so should
        //create separate key value pairs in the tree
        Assert.Equal(2, tree[b]);
        Assert.Equal(1, tree[a]);
        Assert.NotEqual(2, tree[a]);

        //With the FastNodeComparer there should be 2 keys in the tree
        //because the keys are not considered equal
        Assert.Equal(2, tree.Keys.Count());
    }
}
