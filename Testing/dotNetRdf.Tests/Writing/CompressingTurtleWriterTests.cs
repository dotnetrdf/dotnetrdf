/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2024 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

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
using System.Text;
using VDS.RDF.Parsing;
using Xunit;

namespace VDS.RDF.Writing;

public class CompressingTurtleWriterTests
{
    [Fact]
    public void CollectionItemsShouldAppearOnSeparateLinesWhenPrettyPrinted()
    {
        var g = new Graph();
        g.NamespaceMap.AddNamespace("ex", UriFactory.Create("http://example.org/"));
        INode listNode = g.AssertList([g.CreateUriNode("ex:a"), g.CreateUriNode("ex:b"), g.CreateUriNode("ex:c")]);
        g.Assert(g.CreateUriNode("ex:s"), g.CreateUriNode("ex:p"), listNode);
        var writer = new CompressingTurtleWriter(TurtleSyntax.W3C) { PrettyPrintMode = true };
        var stringWriter = new System.IO.StringWriter();
        writer.Save(g, stringWriter);
        var output = stringWriter.ToString();
        var expectedOutput = new StringBuilder();
        expectedOutput.AppendLine("ex:s ex:p (ex:a");
        expectedOutput.AppendLine("           ex:b");
        expectedOutput.AppendLine("           ex:c).");
        output.Should().Contain(expectedOutput.ToString());
    }
}