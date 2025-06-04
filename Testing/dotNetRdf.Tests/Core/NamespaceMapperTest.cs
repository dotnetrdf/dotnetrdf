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
using FluentAssertions;
using Xunit;

namespace VDS.RDF;


public class NamespaceMapperTest : BaseTest
{
    public NamespaceMapperTest(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void NamespaceMapperEvent()
    {
        var eventRaised = false;

        NamespaceChanged added = delegate(String prefix, Uri u) { eventRaised = true; };
        NamespaceChanged changed = delegate(String prefix, Uri u) { eventRaised = true; };
        NamespaceChanged removed = delegate(String prefix, Uri u) { eventRaised = true; };

        var nsmap = new NamespaceMapper();
        nsmap.NamespaceAdded += added;
        nsmap.NamespaceModified += changed;
        nsmap.NamespaceRemoved += removed;

        _output.WriteLine("Trying to add the RDF Namespace, this should already be defined");
        nsmap.AddNamespace("rdf", new Uri(NamespaceMapper.RDF));
        Assert.False(eventRaised);
        eventRaised = false;
        _output.WriteLine(string.Empty);

        _output.WriteLine("Trying to add an example Namespace which isn't defined");
        nsmap.AddNamespace("ex", new Uri("http://example.org/"));
        Assert.True(eventRaised);
        eventRaised = false;
        _output.WriteLine(nsmap.GetNamespaceUri("ex").AbsoluteUri);
        _output.WriteLine(string.Empty);

        _output.WriteLine("Trying to modify the example Namespace");
        nsmap.AddNamespace("ex", new Uri("http://example.org/test/"));
        Assert.True(eventRaised);
        eventRaised = false;
        _output.WriteLine(nsmap.GetNamespaceUri("ex").AbsoluteUri);
        _output.WriteLine(string.Empty);

        _output.WriteLine("Trying to remove the example Namespace");
        nsmap.RemoveNamespace("ex");
        Assert.True(eventRaised);
        eventRaised = false;
        _output.WriteLine(string.Empty);

        _output.WriteLine("Trying to remove a non-existent Namespace");
        nsmap.RemoveNamespace("ex");
        Assert.False(eventRaised);
        eventRaised = false;
        _output.WriteLine(string.Empty);

        _output.WriteLine("Adding some example Namespace back in again for an import test");
        nsmap.AddNamespace("ex", new Uri("http://example.org/"));
        nsmap.AddNamespace("ns0", new Uri("http://example.org/clashes/"));

        _output.WriteLine("Creating another Namespace Mapper with the ex prefix mapped to a different URI");
        var nsmap2 = new NamespaceMapper();
        nsmap2.AddNamespace("ex", new Uri("http://example.org/test/"));

        _output.WriteLine("Importing the new NamespaceMapper into the original");
        nsmap.Import(nsmap2);
        _output.WriteLine("NamespaceMapper now contains the following Namespaces:");
        foreach (var prefix in nsmap.Prefixes)
        {
            _output.WriteLine("\t" + prefix + " <" + nsmap.GetNamespaceUri(prefix).AbsoluteUri + ">");
        }
        Assert.Equal(nsmap.GetNamespaceUri("ex"), new Uri("http://example.org/"));
        Assert.Equal(nsmap.GetNamespaceUri("ns1"), new Uri("http://example.org/test/"));
    }

    [Fact]
    public void WhenTwoPrefixesMapToTheSameUriTheUriToPrefixMapIsExtended()
    {
        var nsmap = new NamespaceMapper();
        nsmap.AddNamespace("foo", new Uri("http://example.org/"));
        nsmap.AddNamespace("bar", new Uri("http://example.org/"));
        nsmap.GetPrefix(new Uri("http://example.org/")).Should().BeOneOf("foo", "bar");
        nsmap.RemoveNamespace("bar");
        nsmap.GetPrefix(new Uri("http://example.org/")).Should().Be("foo");
    }
}
