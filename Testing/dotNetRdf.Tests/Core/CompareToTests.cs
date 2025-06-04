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

using FluentAssertions;
using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF;


public class CompareToTests
{
    private readonly ITestOutputHelper _output;
    public CompareToTests(ITestOutputHelper output)
    {
        _output = output;
    }

    private void CheckCombinations(List<INode> nodes)
    {
        CheckCombinations(nodes, new SparqlOrderingComparer(CultureInfo.InvariantCulture, CompareOptions.Ordinal));
    }

    private void CheckCombinations<T>(List<T> nodes) where T : IComparable<T>
    {
        CheckCombinations<T>(nodes, Comparer<T>.Default);
    }

    private void CheckCombinations(List<INode> nodes, IComparer<INode> comparer)
    {
        if (nodes.Count == 0) Assert.Fail("No Input");

        _output.WriteLine("INode Typed Tests");
        for (var i = 0; i < nodes.Count; i++)
        {
            for (var j = 0; j < nodes.Count; j++)
            {
                INode a = nodes[i];
                INode b = nodes[j];
                if (i == j || ReferenceEquals(a, b))
                {
                    Assert.Equal(0, a.CompareTo(b));
                    Assert.Equal(0, b.CompareTo(a));
                    _output.WriteLine(i + " compareTo " + j + " => i == j");

                    Assert.Equal(0, comparer.Compare(a, b));
                    Assert.Equal(0, comparer.Compare(b, a));
                }
                else
                {
                    var c = a.CompareTo(b);
                    var d = comparer.Compare(a, b);
                    Assert.Equal(c * -1, b.CompareTo(a));
                    Assert.Equal(d * -1, comparer.Compare(b, a));

                    if (c > 0)
                    {
                        _output.WriteLine(i + " compareTo " + j + " => i > j");
                    }
                    else if (c == 0)
                    {
                        _output.WriteLine(i + " compareTo " + j + " => i == j");
                    }
                    else
                    {
                        _output.WriteLine(i + " compareTo " + j + " => i < j");
                    }
                }
            }

            _output.WriteLine(string.Empty);
        }

        _output.WriteLine(string.Empty);
    }

    private void CheckCombinations<T>(List<T> nodes, IComparer<T> comparer)
    {
        if (nodes.Count == 0) Assert.Fail("No Input");

        _output.WriteLine("Strongly Typed Tests - " + nodes.GetType().ToString());
        for (var i = 0; i < nodes.Count; i++)
        {
            for (var j = 0; j < nodes.Count; j++)
            {
                T a = nodes[i];
                T b = nodes[j];
                if (i == j || ReferenceEquals(a, b))
                {
                    Assert.Equal(0, comparer.Compare(a, b));
                    Assert.Equal(0, comparer.Compare(b, a));
                    _output.WriteLine(i + " compareTo " + j + " => i == j");
                }
                else
                {
                    var c = comparer.Compare(a, b);
                    Assert.Equal(c * -1, comparer.Compare(b, a));

                    if (c > 0)
                    {
                        _output.WriteLine(i + " compareTo " + j + " => i > j");
                    }
                    else if (c == 0)
                    {
                        _output.WriteLine(i + " compareTo " + j + " => i == j");
                    }
                    else
                    {
                        _output.WriteLine(i + " compareTo " + j + " => i < j");
                    }
                }
            }

            _output.WriteLine(string.Empty);
        }

        _output.WriteLine(string.Empty);
    }

    private void ShowOrdering(IEnumerable<INode> nodes, CompareOptions compareOptions = CompareOptions.Ordinal)
    {
        _output.WriteLine("Standard Ordering");
        var formatter = new NTriplesFormatter();
        foreach (INode n in nodes.OrderBy(x => x))
        {
            _output.WriteLine(n.ToString(formatter));
        }

        _output.WriteLine(string.Empty);

        _output.WriteLine("SPARQL Ordering");
        foreach (INode n in nodes.OrderBy(x => x, new SparqlOrderingComparer(CultureInfo.InvariantCulture, compareOptions)))
        {
            _output.WriteLine(n.ToString(formatter));
        }

        _output.WriteLine(string.Empty);
    }

    private void ShowOrdering(IEnumerable<INode> nodes, IComparer<INode> comparer)
    {
        _output.WriteLine("Standard Ordering");
        var formatter = new NTriplesFormatter();
        foreach (INode n in nodes.OrderBy(x => x))
        {
            _output.WriteLine(n.ToString(formatter));
        }

        _output.WriteLine(string.Empty);

        _output.WriteLine(comparer.GetType().Name + " Ordering");
        foreach (INode n in nodes.OrderBy(x => x, comparer))
        {
            _output.WriteLine(n.ToString(formatter));
        }

        _output.WriteLine(string.Empty);
    }

    [Fact]
    public void NodeCompareToBlankNodes()
    {
        var g = new Graph();
        var h = new Graph();

        IBlankNode b = g.CreateBlankNode();
        var nodes = new List<INode>()
        {
            b,
            g.CreateBlankNode(),
            g.CreateBlankNode("id"),
            h.CreateBlankNode(),
            h.CreateBlankNode("id"),
            b
        };

        ShowOrdering(nodes);

        CheckCombinations(nodes);
        CheckCombinations<IBlankNode>(nodes.OfType<IBlankNode>().ToList());
        CheckCombinations<BlankNode>(nodes.OfType<BlankNode>().ToList());
    }

    [Fact]
    public void NodeCompareToLiteralNodes()
    {
        var g = new Graph();

        ILiteralNode plain = g.CreateLiteralNode("plain");
        var nodes = new List<INode>()
        {
            plain,
            g.CreateLiteralNode("plain english", "en"),
            g.CreateLiteralNode("plain french", "fr"),
            g.CreateLiteralNode("typed", new Uri(XmlSpecsHelper.XmlSchemaDataTypeString)),
            (1234).ToLiteral(g),
            (12.34m).ToLiteral(g),
            (12.34d).ToLiteral(g),
            (false).ToLiteral(g),
            (true).ToLiteral(g),
            plain
        };

        ShowOrdering(nodes);

        CheckCombinations(nodes);
        CheckCombinations<ILiteralNode>(nodes.OfType<ILiteralNode>().ToList());
        CheckCombinations<LiteralNode>(nodes.OfType<LiteralNode>().ToList());
    }

    [Fact]
    public void NodeCompareToMalformedLiteralNodes()
    {
        var g = new Graph();

        var nodes = new List<INode>()
        {
            g.CreateLiteralNode("something", new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger)),
            g.CreateLiteralNode("thing", new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger)),
            g.CreateLiteralNode("1", new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger)),
            g.CreateLiteralNode("10", new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger)),
            g.CreateLiteralNode("-3", new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger)),
        };

        ShowOrdering(nodes);

        CheckCombinations(nodes);
        CheckCombinations<ILiteralNode>(nodes.OfType<ILiteralNode>().ToList());
        CheckCombinations<LiteralNode>(nodes.OfType<LiteralNode>().ToList());
    }

    [Fact]
    public void NodeCompareToWithCompareOptions()
    {
        var g = new Graph();

        var nodes = new List<INode>()
        {
            g.CreateLiteralNode("something"),
            g.CreateLiteralNode("Something"),
            g.CreateLiteralNode("thing")
        };

        // Test each comparison mode
        foreach (CompareOptions comparison in Enum.GetValues(typeof(StringComparison)))
        {
            ShowOrdering(nodes, comparison);
            CheckCombinations(nodes);
            CheckCombinations<ILiteralNode>(nodes.OfType<ILiteralNode>().ToList());
            CheckCombinations<LiteralNode>(nodes.OfType<LiteralNode>().ToList());
        }
    }

    [Fact]
    public void NodeCompareToLiteralNodesXsdBytes()
    {
        var g = new Graph();

        var nodes = new List<INode>()
        {
            g.CreateLiteralNode("thing", new Uri(XmlSpecsHelper.XmlSchemaDataTypeByte)),
            g.CreateLiteralNode("something", new Uri(XmlSpecsHelper.XmlSchemaDataTypeByte)),
            g.CreateLiteralNode("1", new Uri(XmlSpecsHelper.XmlSchemaDataTypeByte)),
            g.CreateLiteralNode("10", new Uri(XmlSpecsHelper.XmlSchemaDataTypeByte)),
            g.CreateLiteralNode("-3", new Uri(XmlSpecsHelper.XmlSchemaDataTypeByte)),
        };

        ShowOrdering(nodes);

        CheckCombinations(nodes);
        CheckCombinations<ILiteralNode>(nodes.OfType<ILiteralNode>().ToList());
        CheckCombinations<LiteralNode>(nodes.OfType<LiteralNode>().ToList());
    }

    [Fact]
    public void NodeCompareToLiteralNodesXsdUnsignedBytes()
    {
        var g = new Graph();

        var nodes = new List<INode>()
        {
            g.CreateLiteralNode("thing", new Uri(XmlSpecsHelper.XmlSchemaDataTypeUnsignedByte)),
            g.CreateLiteralNode("something", new Uri(XmlSpecsHelper.XmlSchemaDataTypeUnsignedByte)),
            g.CreateLiteralNode("1", new Uri(XmlSpecsHelper.XmlSchemaDataTypeUnsignedByte)),
            g.CreateLiteralNode("10", new Uri(XmlSpecsHelper.XmlSchemaDataTypeUnsignedByte)),
            g.CreateLiteralNode("-3", new Uri(XmlSpecsHelper.XmlSchemaDataTypeUnsignedByte)),
        };

        ShowOrdering(nodes);

        CheckCombinations(nodes);
        CheckCombinations<ILiteralNode>(nodes.OfType<ILiteralNode>().ToList());
        CheckCombinations<LiteralNode>(nodes.OfType<LiteralNode>().ToList());
    }

    [Fact]
    public void NodeCompareToLiteralNodesXsdIntegers()
    {
        var g = new Graph();

        var nodes = new List<INode>()
        {
            g.CreateLiteralNode("something", new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger)),
            g.CreateLiteralNode("thing", new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger)),
            g.CreateLiteralNode("1", new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger)),
            g.CreateLiteralNode("10", new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger)),
            g.CreateLiteralNode("-3", new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger)),
        };

        ShowOrdering(nodes);

        CheckCombinations(nodes);
        CheckCombinations<ILiteralNode>(nodes.OfType<ILiteralNode>().ToList());
        CheckCombinations<LiteralNode>(nodes.OfType<LiteralNode>().ToList());
    }

    [Fact]
    public void NodeCompareToLiteralNodesXsdShorts()
    {
        var g = new Graph();

        var nodes = new List<INode>()
        {
            g.CreateLiteralNode("something", new Uri(XmlSpecsHelper.XmlSchemaDataTypeShort)),
            g.CreateLiteralNode("thing", new Uri(XmlSpecsHelper.XmlSchemaDataTypeShort)),
            g.CreateLiteralNode("1", new Uri(XmlSpecsHelper.XmlSchemaDataTypeShort)),
            g.CreateLiteralNode("10", new Uri(XmlSpecsHelper.XmlSchemaDataTypeShort)),
            g.CreateLiteralNode("-3", new Uri(XmlSpecsHelper.XmlSchemaDataTypeShort)),
        };

        ShowOrdering(nodes);

        CheckCombinations(nodes);
        CheckCombinations<ILiteralNode>(nodes.OfType<ILiteralNode>().ToList());
        CheckCombinations<LiteralNode>(nodes.OfType<LiteralNode>().ToList());
    }

    [Fact]
    public void NodeCompareToLiteralNodesXsdLongs()
    {
        var g = new Graph();

        var nodes = new List<INode>()
        {
            g.CreateLiteralNode("something", new Uri(XmlSpecsHelper.XmlSchemaDataTypeLong)),
            g.CreateLiteralNode("thing", new Uri(XmlSpecsHelper.XmlSchemaDataTypeLong)),
            g.CreateLiteralNode("1", new Uri(XmlSpecsHelper.XmlSchemaDataTypeLong)),
            g.CreateLiteralNode("10", new Uri(XmlSpecsHelper.XmlSchemaDataTypeLong)),
            g.CreateLiteralNode("-3", new Uri(XmlSpecsHelper.XmlSchemaDataTypeLong)),
        };

        ShowOrdering(nodes);

        CheckCombinations(nodes);
        CheckCombinations<ILiteralNode>(nodes.OfType<ILiteralNode>().ToList());
        CheckCombinations<LiteralNode>(nodes.OfType<LiteralNode>().ToList());
    }

    [Fact]
    public void NodeCompareToLiteralNodesXsdUnsignedIntegers()
    {
        var g = new Graph();

        var nodes = new List<INode>()
        {
            g.CreateLiteralNode("something", new Uri(XmlSpecsHelper.XmlSchemaDataTypeUnsignedInt)),
            g.CreateLiteralNode("thing", new Uri(XmlSpecsHelper.XmlSchemaDataTypeUnsignedInt)),
            g.CreateLiteralNode("1", new Uri(XmlSpecsHelper.XmlSchemaDataTypeUnsignedInt)),
            g.CreateLiteralNode("10", new Uri(XmlSpecsHelper.XmlSchemaDataTypeUnsignedInt)),
            g.CreateLiteralNode("-3", new Uri(XmlSpecsHelper.XmlSchemaDataTypeUnsignedInt)),
        };

        ShowOrdering(nodes);

        CheckCombinations(nodes);
        CheckCombinations<ILiteralNode>(nodes.OfType<ILiteralNode>().ToList());
        CheckCombinations<LiteralNode>(nodes.OfType<LiteralNode>().ToList());
    }

    [Fact]
    public void NodeCompareToLiteralNodesXsdUnsignedShorts()
    {
        var g = new Graph();

        var nodes = new List<INode>()
        {
            g.CreateLiteralNode("something", new Uri(XmlSpecsHelper.XmlSchemaDataTypeUnsignedShort)),
            g.CreateLiteralNode("thing", new Uri(XmlSpecsHelper.XmlSchemaDataTypeUnsignedShort)),
            g.CreateLiteralNode("1", new Uri(XmlSpecsHelper.XmlSchemaDataTypeUnsignedShort)),
            g.CreateLiteralNode("10", new Uri(XmlSpecsHelper.XmlSchemaDataTypeUnsignedShort)),
            g.CreateLiteralNode("-3", new Uri(XmlSpecsHelper.XmlSchemaDataTypeUnsignedShort)),
        };

        ShowOrdering(nodes);

        CheckCombinations(nodes);
        CheckCombinations<ILiteralNode>(nodes.OfType<ILiteralNode>().ToList());
        CheckCombinations<LiteralNode>(nodes.OfType<LiteralNode>().ToList());
    }

    [Fact]
    public void NodeCompareToLiteralNodesXsdUnsignedLongs()
    {
        var g = new Graph();

        var nodes = new List<INode>()
        {
            g.CreateLiteralNode("something", new Uri(XmlSpecsHelper.XmlSchemaDataTypeUnsignedLong)),
            g.CreateLiteralNode("thing", new Uri(XmlSpecsHelper.XmlSchemaDataTypeUnsignedLong)),
            g.CreateLiteralNode("1", new Uri(XmlSpecsHelper.XmlSchemaDataTypeUnsignedLong)),
            g.CreateLiteralNode("10", new Uri(XmlSpecsHelper.XmlSchemaDataTypeUnsignedLong)),
            g.CreateLiteralNode("-3", new Uri(XmlSpecsHelper.XmlSchemaDataTypeUnsignedLong)),
        };

        ShowOrdering(nodes);

        CheckCombinations(nodes);
        CheckCombinations<ILiteralNode>(nodes.OfType<ILiteralNode>().ToList());
        CheckCombinations<LiteralNode>(nodes.OfType<LiteralNode>().ToList());
    }

    [Fact]
    public void NodeCompareToLiteralNodesXsdNegativeIntegers()
    {
        var g = new Graph();

        var nodes = new List<INode>()
        {
            g.CreateLiteralNode("something", new Uri(XmlSpecsHelper.XmlSchemaDataTypeNegativeInteger)),
            g.CreateLiteralNode("thing", new Uri(XmlSpecsHelper.XmlSchemaDataTypeNegativeInteger)),
            g.CreateLiteralNode("1", new Uri(XmlSpecsHelper.XmlSchemaDataTypeNegativeInteger)),
            g.CreateLiteralNode("10", new Uri(XmlSpecsHelper.XmlSchemaDataTypeNegativeInteger)),
            g.CreateLiteralNode("-3", new Uri(XmlSpecsHelper.XmlSchemaDataTypeNegativeInteger)),
        };

        ShowOrdering(nodes);

        CheckCombinations(nodes);
        CheckCombinations<ILiteralNode>(nodes.OfType<ILiteralNode>().ToList());
        CheckCombinations<LiteralNode>(nodes.OfType<LiteralNode>().ToList());
    }

    [Fact]
    public void NodeCompareToLiteralNodesXsdNonPositiveIntegers()
    {
        var g = new Graph();

        var nodes = new List<INode>()
        {
            g.CreateLiteralNode("something", new Uri(XmlSpecsHelper.XmlSchemaDataTypeNonPositiveInteger)),
            g.CreateLiteralNode("thing", new Uri(XmlSpecsHelper.XmlSchemaDataTypeNonPositiveInteger)),
            g.CreateLiteralNode("1", new Uri(XmlSpecsHelper.XmlSchemaDataTypeNonPositiveInteger)),
            g.CreateLiteralNode("10", new Uri(XmlSpecsHelper.XmlSchemaDataTypeNonPositiveInteger)),
            g.CreateLiteralNode("-3", new Uri(XmlSpecsHelper.XmlSchemaDataTypeNonPositiveInteger)),
        };

        ShowOrdering(nodes);

        CheckCombinations(nodes);
        CheckCombinations<ILiteralNode>(nodes.OfType<ILiteralNode>().ToList());
        CheckCombinations<LiteralNode>(nodes.OfType<LiteralNode>().ToList());
    }

    [Fact]
    public void NodeCompareToLiteralNodesXsdNonNegativeIntegers()
    {
        var g = new Graph();

        var nodes = new List<INode>()
        {
            g.CreateLiteralNode("something", new Uri(XmlSpecsHelper.XmlSchemaDataTypeNonNegativeInteger)),
            g.CreateLiteralNode("thing", new Uri(XmlSpecsHelper.XmlSchemaDataTypeNonNegativeInteger)),
            g.CreateLiteralNode("1", new Uri(XmlSpecsHelper.XmlSchemaDataTypeNonNegativeInteger)),
            g.CreateLiteralNode("10", new Uri(XmlSpecsHelper.XmlSchemaDataTypeNonNegativeInteger)),
            g.CreateLiteralNode("-3", new Uri(XmlSpecsHelper.XmlSchemaDataTypeNonNegativeInteger)),
        };

        ShowOrdering(nodes);

        CheckCombinations(nodes);
        CheckCombinations<ILiteralNode>(nodes.OfType<ILiteralNode>().ToList());
        CheckCombinations<LiteralNode>(nodes.OfType<LiteralNode>().ToList());
    }

    [Fact]
    public void NodeCompareToLiteralNodesXsdPositiveIntegers()
    {
        var g = new Graph();

        var nodes = new List<INode>()
        {
            g.CreateLiteralNode("something", new Uri(XmlSpecsHelper.XmlSchemaDataTypePositiveInteger)),
            g.CreateLiteralNode("thing", new Uri(XmlSpecsHelper.XmlSchemaDataTypePositiveInteger)),
            g.CreateLiteralNode("1", new Uri(XmlSpecsHelper.XmlSchemaDataTypePositiveInteger)),
            g.CreateLiteralNode("10", new Uri(XmlSpecsHelper.XmlSchemaDataTypePositiveInteger)),
            g.CreateLiteralNode("-3", new Uri(XmlSpecsHelper.XmlSchemaDataTypePositiveInteger)),
        };

        ShowOrdering(nodes);

        CheckCombinations(nodes);
        CheckCombinations<ILiteralNode>(nodes.OfType<ILiteralNode>().ToList());
        CheckCombinations<LiteralNode>(nodes.OfType<LiteralNode>().ToList());
    }

    [Fact]
    public void NodeCompareToLiteralNodesXsdHexBinary()
    {
        var g = new Graph();

        var nodes = new List<INode>()
        {
            g.CreateLiteralNode("something", new Uri(XmlSpecsHelper.XmlSchemaDataTypeHexBinary)),
            g.CreateLiteralNode("thing", new Uri(XmlSpecsHelper.XmlSchemaDataTypeHexBinary)),
            g.CreateLiteralNode((1).ToString("X2"), new Uri(XmlSpecsHelper.XmlSchemaDataTypeHexBinary)),
            g.CreateLiteralNode((10).ToString("X2"), new Uri(XmlSpecsHelper.XmlSchemaDataTypeHexBinary)),
            g.CreateLiteralNode("-03", new Uri(XmlSpecsHelper.XmlSchemaDataTypeHexBinary)),
        };

        ShowOrdering(nodes);

        CheckCombinations(nodes);
        CheckCombinations<ILiteralNode>(nodes.OfType<ILiteralNode>().ToList());
        CheckCombinations<LiteralNode>(nodes.OfType<LiteralNode>().ToList());
    }

    [Fact]
    public void NodeCompareToLiteralNodesXsdDoubles()
    {
        var g = new Graph();

        var nodes = new List<INode>()
        {
            g.CreateLiteralNode("something", new Uri(XmlSpecsHelper.XmlSchemaDataTypeDouble)),
            g.CreateLiteralNode("thing", new Uri(XmlSpecsHelper.XmlSchemaDataTypeDouble)),
            g.CreateLiteralNode("1.2e4", new Uri(XmlSpecsHelper.XmlSchemaDataTypeDouble)),
            g.CreateLiteralNode("1.2e3", new Uri(XmlSpecsHelper.XmlSchemaDataTypeDouble)),
            g.CreateLiteralNode("1.2", new Uri(XmlSpecsHelper.XmlSchemaDataTypeDouble)),
            g.CreateLiteralNode("1.2e1", new Uri(XmlSpecsHelper.XmlSchemaDataTypeDouble)),
            g.CreateLiteralNode("1.2E4", new Uri(XmlSpecsHelper.XmlSchemaDataTypeDouble)),
            g.CreateLiteralNode("1.2e0", new Uri(XmlSpecsHelper.XmlSchemaDataTypeDouble)),
            g.CreateLiteralNode("1.2e-1", new Uri(XmlSpecsHelper.XmlSchemaDataTypeDouble)),
            g.CreateLiteralNode("10", new Uri(XmlSpecsHelper.XmlSchemaDataTypeDouble)),
            g.CreateLiteralNode("10e14", new Uri(XmlSpecsHelper.XmlSchemaDataTypeDouble)),
            g.CreateLiteralNode("-3", new Uri(XmlSpecsHelper.XmlSchemaDataTypeDouble)),
        };

        ShowOrdering(nodes);

        CheckCombinations(nodes);
        CheckCombinations<ILiteralNode>(nodes.OfType<ILiteralNode>().ToList());
        CheckCombinations<LiteralNode>(nodes.OfType<LiteralNode>().ToList());
    }

    [Fact]
    public void NodeCompareToLiteralNodesXsdFloats()
    {
        var g = new Graph();

        var nodes = new List<INode>()
        {
            g.CreateLiteralNode("something", new Uri(XmlSpecsHelper.XmlSchemaDataTypeFloat)),
            g.CreateLiteralNode("thing", new Uri(XmlSpecsHelper.XmlSchemaDataTypeFloat)),
            g.CreateLiteralNode("1.2e4", new Uri(XmlSpecsHelper.XmlSchemaDataTypeFloat)),
            g.CreateLiteralNode("1.2e3", new Uri(XmlSpecsHelper.XmlSchemaDataTypeFloat)),
            g.CreateLiteralNode("1.2", new Uri(XmlSpecsHelper.XmlSchemaDataTypeFloat)),
            g.CreateLiteralNode("1.2e1", new Uri(XmlSpecsHelper.XmlSchemaDataTypeFloat)),
            g.CreateLiteralNode("1.2E4", new Uri(XmlSpecsHelper.XmlSchemaDataTypeFloat)),
            g.CreateLiteralNode("1.2e0", new Uri(XmlSpecsHelper.XmlSchemaDataTypeFloat)),
            g.CreateLiteralNode("1.2e-1", new Uri(XmlSpecsHelper.XmlSchemaDataTypeFloat)),
            g.CreateLiteralNode("10", new Uri(XmlSpecsHelper.XmlSchemaDataTypeFloat)),
            g.CreateLiteralNode("10e14", new Uri(XmlSpecsHelper.XmlSchemaDataTypeFloat)),
            g.CreateLiteralNode("-3", new Uri(XmlSpecsHelper.XmlSchemaDataTypeFloat)),
        };

        ShowOrdering(nodes);

        CheckCombinations(nodes);
        CheckCombinations<ILiteralNode>(nodes.OfType<ILiteralNode>().ToList());
        CheckCombinations<LiteralNode>(nodes.OfType<LiteralNode>().ToList());
    }

    [Fact]
    public void NodeCompareToLiteralNodesXsdUris()
    {
        var g = new Graph();

        var nodes = new List<INode>()
        {
            g.CreateLiteralNode("something", new Uri(XmlSpecsHelper.XmlSchemaDataTypeAnyUri)),
            g.CreateLiteralNode("thing", new Uri(XmlSpecsHelper.XmlSchemaDataTypeAnyUri)),
            g.CreateLiteralNode("1", new Uri(XmlSpecsHelper.XmlSchemaDataTypeAnyUri)),
            g.CreateLiteralNode("http://example.org", new Uri(XmlSpecsHelper.XmlSchemaDataTypeAnyUri)),
            g.CreateLiteralNode("http://example.org:8080", new Uri(XmlSpecsHelper.XmlSchemaDataTypeAnyUri)),
            g.CreateLiteralNode("http://example.org/path", new Uri(XmlSpecsHelper.XmlSchemaDataTypeAnyUri)),
            g.CreateLiteralNode("ftp://ftp.example.org", new Uri(XmlSpecsHelper.XmlSchemaDataTypeAnyUri)),
            g.CreateLiteralNode("mailto:someone@example.org", new Uri(XmlSpecsHelper.XmlSchemaDataTypeAnyUri)),
            g.CreateLiteralNode("ex:custom", new Uri(XmlSpecsHelper.XmlSchemaDataTypeAnyUri)),
            g.CreateLiteralNode("10", new Uri(XmlSpecsHelper.XmlSchemaDataTypeAnyUri)),
            g.CreateLiteralNode("-3", new Uri(XmlSpecsHelper.XmlSchemaDataTypeAnyUri)),
        };

        ShowOrdering(nodes);

        CheckCombinations(nodes);
        CheckCombinations<ILiteralNode>(nodes.OfType<ILiteralNode>().ToList());
        CheckCombinations<LiteralNode>(nodes.OfType<LiteralNode>().ToList());
    }

    [Fact]
    public void NodeCompareToLiteralNodesXsdDateTimes()
    {
        var g = new Graph();

        var nodes = new List<INode>()
        {
            g.CreateLiteralNode("something", new Uri(XmlSpecsHelper.XmlSchemaDataTypeDateTime)),
            DateTime.Now.ToLiteral(g),
            DateTime.Now.AddYears(3).AddDays(1).ToLiteral(g),
            DateTime.Now.AddYears(-25).AddMinutes(-17).ToLiteral(g),
            new DateTime(1, 2, 3, 4, 5, 6).ToLiteral(g),
            new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour,
                DateTime.Now.Minute, DateTime.Now.Second, DateTimeKind.Utc).ToLiteral(g),
            g.CreateLiteralNode("thing", new Uri(XmlSpecsHelper.XmlSchemaDataTypeDateTime)),
            g.CreateLiteralNode("1", new Uri(XmlSpecsHelper.XmlSchemaDataTypeDateTime)),
            g.CreateLiteralNode("-3", new Uri(XmlSpecsHelper.XmlSchemaDataTypeDateTime)),
        };

        ShowOrdering(nodes);

        CheckCombinations(nodes);
        CheckCombinations<ILiteralNode>(nodes.OfType<ILiteralNode>().ToList());
        CheckCombinations<LiteralNode>(nodes.OfType<LiteralNode>().ToList());
    }

    [Fact]
    public void NodeCompareToUriNodes()
    {
        var g = new Graph();

        IUriNode u = g.CreateUriNode("rdf:type");
        var nodes = new List<INode>()
        {
            u,
            g.CreateUriNode(new Uri("http://example.org")),
            g.CreateUriNode(new Uri("http://example.org:80")),
            g.CreateUriNode(new Uri("http://example.org:8080")),
            g.CreateUriNode(new Uri("http://www.dotnetrdf.org")),
            g.CreateUriNode(new Uri("http://www.dotnetrdf.org/configuration#")),
            g.CreateUriNode(new Uri("http://www.dotnetrdf.org/Configuration#")),
            g.CreateUriNode(new Uri("mailto:rvesse@dotnetrdf.org")),
            u,
            g.CreateUriNode(new Uri("ftp://ftp.example.org")),
            g.CreateUriNode(new Uri("ftp://anonymous@ftp.example.org")),
            g.CreateUriNode(new Uri("ftp://user:password@ftp.example.org"))
        };

        ShowOrdering(nodes);

        CheckCombinations(nodes);
        CheckCombinations<IUriNode>(nodes.OfType<IUriNode>().ToList());
        CheckCombinations<UriNode>(nodes.OfType<UriNode>().ToList());
    }

    [Fact]
    public void NodeCompareToTripleNodes()
    {
        var g = new Graph();
        var t = g.CreateTripleNode(
            new Triple(
                g.CreateUriNode(new Uri("urn:s")),
                g.CreateUriNode(new Uri("urn:p")),
                g.CreateUriNode(new Uri("urn:o"))));
        var t2 = g.CreateTripleNode(
            new Triple(
                g.CreateUriNode(new Uri("urn:o")),
                g.CreateUriNode(new Uri("urn:p")),
                g.CreateUriNode(new Uri("urn:s"))));
        var t3 = g.CreateTripleNode(
            new Triple(
                g.CreateUriNode(new Uri("urn:s")),
                g.CreateUriNode(new Uri("urn:o")),
                g.CreateUriNode(new Uri("urn:p"))));

        var t4 = g.CreateTripleNode(
            new Triple(
                g.CreateUriNode(new Uri("urn:s")),
                g.CreateUriNode(new Uri("urn:p")),
                g.CreateLiteralNode("foo", "en")));
        var nodes = new List<INode> { t, t, t2, t3, t4 };

        //ShowOrdering(nodes);
        // Expected comparison results:
        t.CompareTo(t).Should().Be(0);
        t.CompareTo(t2).Should().BeGreaterThan(0);
        t.CompareTo(t3).Should().BeGreaterThan(0);
        t.CompareTo(t4).Should().BeLessThan(0);

        CheckCombinations(nodes);
        CheckCombinations<ITripleNode>(nodes.OfType<ITripleNode>().ToList());
        CheckCombinations<TripleNode>(nodes.OfType<TripleNode>().ToList());
    }

    [Fact]
    public void NodeCompareToMixedNodes()
    {
        var g = new Graph();
        var h = new Graph();

        IBlankNode b = g.CreateBlankNode();
        ILiteralNode plain = g.CreateLiteralNode("plain");
        IUriNode u = g.CreateUriNode("rdf:type");
        ITripleNode t = g.CreateTripleNode(new Triple(b, u, plain));
        var nodes = new List<INode>()
        {
            b,
            g.CreateBlankNode(),
            g.CreateBlankNode("id"),
            h.CreateBlankNode(),
            h.CreateBlankNode("id"),
            b,
            plain,
            g.CreateLiteralNode("plain english", "en"),
            g.CreateLiteralNode("plain french", "fr"),
            g.CreateLiteralNode("typed", new Uri(XmlSpecsHelper.XmlSchemaDataTypeString)),
            (1234).ToLiteral(g),
            (12.34m).ToLiteral(g),
            (12.34d).ToLiteral(g),
            (false).ToLiteral(g),
            (true).ToLiteral(g),
            plain,
            u,
            g.CreateUriNode(new Uri("http://example.org")),
            g.CreateUriNode(new Uri("http://example.org:8080")),
            g.CreateUriNode(new Uri("http://www.dotnetrdf.org")),
            g.CreateUriNode(new Uri("http://www.dotnetrdf.org/configuration#")),
            g.CreateUriNode(new Uri("http://www.dotnetrdf.org/Configuration#")),
            g.CreateUriNode(new Uri("mailto:rvesse@dotnetrdf.org")),
            u,
            t,
            g.CreateTripleNode(new Triple(
                g.CreateUriNode(new Uri("urn:s")), 
                g.CreateUriNode(new Uri("urn:p")), 
                g.CreateUriNode(new Uri("urn:o")))),
            t
        };

        //ShowOrdering(nodes);

        CheckCombinations(nodes);
    }

    [Fact]
    public void NodeCompareToMixedNodes3()
    {
        var g = new Graph();
        var h = new Graph();

        IBlankNode b = g.CreateBlankNode();
        ILiteralNode plain = g.CreateLiteralNode("plain");
        IUriNode u = g.CreateUriNode("rdf:type");
        var nodes = new List<INode>()
        {
            b,
            g.CreateBlankNode(),
            g.CreateBlankNode("id"),
            g.CreateLiteralNode("1.2e3", new Uri(XmlSpecsHelper.XmlSchemaDataTypeFloat)),
            g.CreateLiteralNode("1.2", new Uri(XmlSpecsHelper.XmlSchemaDataTypeFloat)),
            g.CreateLiteralNode("1.2e1", new Uri(XmlSpecsHelper.XmlSchemaDataTypeFloat)),
            g.CreateLiteralNode("10", new Uri(XmlSpecsHelper.XmlSchemaDataTypeShort)),
            g.CreateLiteralNode("-3", new Uri(XmlSpecsHelper.XmlSchemaDataTypeShort)),
            g.CreateLiteralNode("1.2E4", new Uri(XmlSpecsHelper.XmlSchemaDataTypeFloat)),
            g.CreateLiteralNode("http://example.org:8080", new Uri(XmlSpecsHelper.XmlSchemaDataTypeAnyUri)),
            g.CreateLiteralNode("http://example.org/path", new Uri(XmlSpecsHelper.XmlSchemaDataTypeAnyUri)),
            g.CreateLiteralNode("ftp://ftp.example.org", new Uri(XmlSpecsHelper.XmlSchemaDataTypeAnyUri)),
            g.CreateLiteralNode("1.2e0", new Uri(XmlSpecsHelper.XmlSchemaDataTypeFloat)),
            DateTime.Now.ToLiteral(g),
            DateTime.Now.AddYears(3).AddDays(1).ToLiteral(g),
            DateTime.Now.AddYears(-25).AddMinutes(-17).ToLiteral(g),
            g.CreateLiteralNode("1.2e-1", new Uri(XmlSpecsHelper.XmlSchemaDataTypeFloat)),
            g.CreateLiteralNode("10", new Uri(XmlSpecsHelper.XmlSchemaDataTypeFloat)),
            g.CreateLiteralNode("10e14", new Uri(XmlSpecsHelper.XmlSchemaDataTypeFloat)),
            h.CreateBlankNode(),
            h.CreateBlankNode("id"),
            b,
            plain,
            g.CreateLiteralNode("plain english", "en"),
            g.CreateLiteralNode("1", new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger)),
            g.CreateLiteralNode("10", new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger)),
            g.CreateLiteralNode("plain french", "fr"),
            g.CreateLiteralNode("typed", new Uri(XmlSpecsHelper.XmlSchemaDataTypeString)),
            (1234).ToLiteral(g),
            (12.34m).ToLiteral(g),
            (12.34d).ToLiteral(g),
            (false).ToLiteral(g),
            g.CreateLiteralNode((1).ToString("X2"), new Uri(XmlSpecsHelper.XmlSchemaDataTypeHexBinary)),
            g.CreateLiteralNode((10).ToString("X2"), new Uri(XmlSpecsHelper.XmlSchemaDataTypeHexBinary)),
            (true).ToLiteral(g),
            plain,
            u,
            g.CreateUriNode(new Uri("http://example.org")),
            g.CreateUriNode(new Uri("http://example.org:8080")),
            g.CreateLiteralNode("1", new Uri(XmlSpecsHelper.XmlSchemaDataTypeByte)),
            g.CreateLiteralNode("10", new Uri(XmlSpecsHelper.XmlSchemaDataTypeByte)),
            g.CreateLiteralNode("-3", new Uri(XmlSpecsHelper.XmlSchemaDataTypeByte)),
            g.CreateUriNode(new Uri("http://www.dotnetrdf.org")),
            g.CreateUriNode(new Uri("http://www.dotnetrdf.org/configuration#")),
            g.CreateUriNode(new Uri("http://www.dotnetrdf.org/Configuration#")),
            g.CreateUriNode(new Uri("mailto:rvesse@dotnetrdf.org")),
            u
        };

        ShowOrdering(nodes);

        CheckCombinations(nodes);
    }

    [Fact]
    public void NodeCompareToMixedNodes2()
    {
        var g = new Graph();
        IBlankNode b = g.CreateBlankNode();
        ILiteralNode l = g.CreateLiteralNode("literal", "en");
        IUriNode u = g.CreateUriNode(new Uri("http://example.org"));
        IVariableNode v = g.CreateVariableNode("var");

        var c = b.CompareTo(l);
        Assert.Equal(c * -1, l.CompareTo(b));
        c = b.CompareTo(u);
        Assert.Equal(c * -1, u.CompareTo(b));
        c = b.CompareTo(v);
        Assert.Equal(c * -1, v.CompareTo(b));

        c = l.CompareTo(b);
        Assert.Equal(c * -1, b.CompareTo(l));
        c = l.CompareTo(u);
        Assert.Equal(c * -1, u.CompareTo(l));
        c = l.CompareTo(v);
        Assert.Equal(c * -1, v.CompareTo(l));

        c = u.CompareTo(b);
        Assert.Equal(c * -1, b.CompareTo(u));
        c = u.CompareTo(l);
        Assert.Equal(c * -1, l.CompareTo(u));
        c = u.CompareTo(v);
        Assert.Equal(c * -1, v.CompareTo(u));

        c = v.CompareTo(b);
        Assert.Equal(c * -1, b.CompareTo(v));
        c = v.CompareTo(l);
        Assert.Equal(c * -1, l.CompareTo(v));
        c = v.CompareTo(u);
        Assert.Equal(c * -1, u.CompareTo(v));
    }

    //[Fact]
    //public void NodeCompareToRdfXml()
    //{
    //    Graph g = new Graph();
    //    List<INode> nodes = new List<INode>()
    //    {
    //        g.CreateBlankNode(),
    //        g.CreateUriNode("rdf:type"),
    //        g.CreateBlankNode("node"),
    //        g.CreateUriNode("rdfs:Class"),
    //        g.CreateLiteralNode("string")
    //    };

    //    NTriplesFormatter formatter = new NTriplesFormatter();

    //    _output.WriteLine("Normal Sort Order:");
    //    nodes.Sort();
    //    foreach (INode n in nodes)
    //    {
    //        _output.WriteLine(n.ToString(formatter));
    //    }

    //    _output.WriteLine();
    //    _output.WriteLine("RDF/XML Sort Order:");
    //    nodes.Sort(new RdfXmlTripleComparer());
    //    foreach (INode n in nodes)
    //    {
    //        _output.WriteLine(n.ToString(formatter));
    //    }
    //}

    [Fact]
    public void NodeCompareToEquivalentLiterals1()
    {
        var g = new Graph();
        ILiteralNode canonical = (1).ToLiteral(g);
        ILiteralNode alternate =
            g.CreateLiteralNode("01", UriFactory.Root.Create(XmlSpecsHelper.XmlSchemaDataTypeInteger));

        var ns = new List<INode>()
        {
            canonical,
            alternate
        };

        Assert.NotEqual(canonical, alternate);
        Assert.Equal(0, canonical.CompareTo(alternate));

        ShowOrdering(ns);
        CheckCombinations(ns);
        CheckCombinations<ILiteralNode>(ns.OfType<ILiteralNode>().ToList());
        CheckCombinations(ns, new FastNodeComparer());
    }

    [Fact]
    public void NodeCompareToEquivalentLiterals2()
    {
        var g = new Graph();
        ILiteralNode canonical = (true).ToLiteral(g);
        ILiteralNode alternate =
            g.CreateLiteralNode("TRUE", UriFactory.Root.Create(XmlSpecsHelper.XmlSchemaDataTypeBoolean));

        var ns = new List<INode>()
        {
            canonical,
            alternate
        };

        Assert.NotEqual(canonical, alternate);
        Assert.Equal(0, canonical.CompareTo(alternate));

        ShowOrdering(ns);
        CheckCombinations(ns);
        CheckCombinations<ILiteralNode>(ns.OfType<ILiteralNode>().ToList());
        CheckCombinations(ns, new FastNodeComparer());
    }

    [Fact]
    public void NodeCompareToEquivalentLiterals3()
    {
        var g = new Graph();
        ILiteralNode canonical = (1d).ToLiteral(g);
        ILiteralNode alternate =
            g.CreateLiteralNode("1.00000", UriFactory.Root.Create(XmlSpecsHelper.XmlSchemaDataTypeDouble));

        var ns = new List<INode>()
        {
            canonical,
            alternate
        };

        Assert.NotEqual(canonical, alternate);
        Assert.Equal(0, canonical.CompareTo(alternate));

        ShowOrdering(ns);
        CheckCombinations(ns);
        CheckCombinations<ILiteralNode>(ns.OfType<ILiteralNode>().ToList());
        CheckCombinations(ns, new FastNodeComparer());
    }
}
