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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;

namespace VDS.RDF.LDF.Client;

[Collection("MockServer")]
public class TpfEnumeratorTests(MockServer server)
{
    private readonly Uri someUri = new("urn:a:b");

    [Fact(DisplayName = "Requires first page")]
    public void RequiresUri()
    {
        var constructor = () => new TpfEnumerator(null);

        constructor.Should().ThrowExactly<ArgumentNullException>("because the first page was null");
    }

    [Fact(DisplayName = "IEnumerator invariant: Current element is undefined (null) before the first element")]
    public void NoCurrentBeforeFirst()
    {
        var e = new TpfEnumerator(someUri) as IEnumerator;

        e.Current.Should().BeNull("because it was called before the first element");
    }

    [Fact(DisplayName = "IEnumerator<T> invariant: Current element is undefined (null) before the first element")]
    public void GenericNoCurrentBeforeFirst()
    {
        var e = new TpfEnumerator(someUri) as IEnumerator<Triple>;

        e.Current.Should().BeNull("because it was called before the first element");
    }

    [Fact(DisplayName = "IEnumerator invariant: Cannot move beyond last element")]
    public void CannotMoveBeyondLast()
    {
        var e = new TpfEnumerator(new(server.BaseUri, MockServer.singleData)) as IEnumerator;
        e.MoveNext();

        e.MoveNext().Should().BeFalse("because it was called after the last element");
    }

    [Fact(DisplayName = "IEnumerator invariant: Current element is the same object until MoveNext is called")]
    public void SameCurrent()
    {
        var e = new TpfEnumerator(new(server.BaseUri, MockServer.singleData)) as IEnumerator;
        e.MoveNext();

        e.Current.Should().BeSameAs(e.Current, "because it was not moved");
    }

    [Fact(DisplayName = "IEnumerator<T> invariant: Current element is the same object until MoveNext is called")]
    public void GenericSameCurrent()
    {
        var e = new TpfEnumerator(new(server.BaseUri, MockServer.singleData)) as IEnumerator<Triple>;
        e.MoveNext();

        e.Current.Should().BeSameAs(e.Current, "because it was not moved");
    }

    [Fact(DisplayName = "IEnumerator invariant: Current element is undefined (null) beyond the last element")]
    public void NoCurrentBeyondLast()
    {
        var e = new TpfEnumerator(new(server.BaseUri, MockServer.singleData)) as IEnumerator;
        e.MoveNext();
        e.MoveNext();

        e.Current.Should().BeNull("because it was beyond the last element");
    }

    [Fact(DisplayName = "IEnumerator<T> invariant: Current element is undefined (null) beyond the last element")]
    public void GenericNoCurrentBeyondLast()
    {
        var e = new TpfEnumerator(new(server.BaseUri, MockServer.singleData)) as IEnumerator<Triple>;
        e.MoveNext();
        e.MoveNext();

        e.Current.Should().BeNull("because it was beyond the last element");
    }

    [Fact(DisplayName = "Same current element between generic and non-generic")]
    public void IdenticalCurrent()
    {
        var generic = new TpfEnumerator(new(server.BaseUri, MockServer.singleData)) as IEnumerator<Triple>;
        var nonGeneric = generic as IEnumerator;
        generic.MoveNext();

        nonGeneric.Current.Should().BeSameAs(generic.Current, "they are the same element");
    }

    [Fact(DisplayName = "Shows no elements when response has only controls")]
    public void ControlsOnly()
    {
        var e = new TpfEnumerator(new(server.BaseUri, MockServer.minimalControls)) as IEnumerator;

        e.MoveNext().Should().BeFalse("because it contains controls only");
    }

    [Fact(DisplayName = "Traverses next page")]
    public void TraversesNextPage()
    {
        var e = new TpfEnumerator(new(server.BaseUri, MockServer.hasNextPage)) as IEnumerator;
        e.MoveNext();

        e.MoveNext().Should().BeTrue("because it traverses next page");
    }

    [Fact(DisplayName = "Disposes underlying triples")]
    public void DisposesUnderlying()
    {
        var subject = new TpfEnumerator(someUri) as IDisposable;

        subject.Invoking(e => e.Dispose()).Should().NotThrow("because it is disposable");
    }

    [Fact(DisplayName = "Cannot be reset")]
    public void CannotBeReset()
    {
        var subject = new TpfEnumerator(someUri) as IEnumerator;

        subject.Invoking(e => e.Reset()).Should().ThrowExactly<NotSupportedException>("because it is not supported");
    }
}

[CollectionDefinition("MockServer")]
public class ServerCollection : ICollectionFixture<MockServer> { }

public sealed class MockServer : IDisposable
{
    internal static readonly string minimalControls = "minimalControls";
    internal static readonly string singleData = "singleData";
    internal static readonly string multipleData = "multipleData";
    internal static readonly string hasNextPage = "hasNextPage";
    internal static readonly string hasCount = "hasCount";
    internal static readonly string hasLargeCount = "hasLargeCount";
    internal static readonly string hasNegativeCount = "hasNegativeCount";

    private readonly WireMockServer server;

    public MockServer()
    {
        server = WireMockServer.Start();

        static IRequestBuilder path(string p) => Request.Create().WithPath($"/{p}");
        static IResponseBuilder file(string f) => Response.Create()
            .WithHeader("Content-Type", "text/turtle")
            .WithTransformer(true)
            .WithBodyFromFile(Path.Combine("resources", $"{f}.ttl"));
        void endpoint(string e) => server.Given(path(e)).RespondWith(file(e));

        endpoint(minimalControls);
        endpoint(singleData);
        endpoint(multipleData);
        endpoint(hasNextPage);
        endpoint(hasCount);
        endpoint(hasLargeCount);
        endpoint(hasNegativeCount);
    }

    internal Uri BaseUri => new(server.Url);

    void IDisposable.Dispose() => server.Stop();
}