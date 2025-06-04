/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2021 dotNetRDF Project (http://dotnetrdf.org/)
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

using Xunit;

namespace VDS.RDF;


public class TreeIndexedTripleCollectionTests : AbstractTripleCollectionTests
{
    protected override BaseTripleCollection GetInstance()
    {
        return new TreeIndexedTripleCollection();
    }

    [Fact]
    public void TripleCollectionInstantiation1()
    {
        var collection = new TreeIndexedTripleCollection();
    }

    [Fact]
    public void TestClearTreeIndexedCollectionUpdatesKeys()
    {
        // Added as a repro for issue #188
        var indexed = new Graph();
        indexed.LoadFromString("_:s <urn:p> _:o.");

        var nonIndexed = new NonIndexedGraph();
        nonIndexed.LoadFromString("_:s <urn:p> _:o.");

        Assert.Equal(nonIndexed, indexed); // Yes

        indexed.Clear();
        nonIndexed.Clear();

        Assert.Equal(nonIndexed, indexed); // Yes

        Assert.Empty(nonIndexed.Triples); // Yes
        Assert.Empty(indexed.Triples); // Yes

        Assert.Empty(nonIndexed.Triples.SubjectNodes); // Yes
        Assert.Empty(nonIndexed.Triples.PredicateNodes); // Yes
        Assert.Empty(nonIndexed.Triples.ObjectNodes); // Yes

        Assert.Empty(indexed.Triples.SubjectNodes); // No
        Assert.Empty(indexed.Triples.PredicateNodes); // No
        Assert.Empty(indexed.Triples.ObjectNodes); // No

    }

}
