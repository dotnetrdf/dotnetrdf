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
using Xunit;

namespace VDS.RDF.LDF.Client;

public class TpfEnumerableTests
{
    private readonly Uri someUri = new("urn:a:b");

    [Fact(DisplayName = "Requires first page")]
    public void RequiresUri()
    {
        var constructor = () => new TpfEnumerable(null);

        constructor.Should().ThrowExactly<ArgumentNullException>("because the first page was null");
    }

    [Fact(DisplayName = "Generic returns our enumerator")]
    public void GenericEnumerator()
    {
        var e = new TpfEnumerable(someUri) as IEnumerable<Triple>;

        e.GetEnumerator().Should().BeOfType<TpfEnumerator>("because it is an TPF enumerable");
    }

    [Fact(DisplayName = "Non Generic returns our enumerator")]
    public void NonGenericEnumerator()
    {
        var e = new TpfEnumerable(someUri) as IEnumerable;

        e.GetEnumerator().Should().BeOfType<TpfEnumerator>("because it is an TPF enumerable");
    }
}
