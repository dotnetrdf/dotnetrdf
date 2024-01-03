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
using Xunit;

namespace VDS.RDF.LDF.Hydra;

public class IriTemplateMappingTests
{
    [Fact(DisplayName = "Variable throws without variable")]
    public void RequiresVariable()
    {
        var g = new Graph();
        var m = g.CreateBlankNode();

        var variable = () => new IriTemplateMapping(m, g).Variable;

        variable.Should().ThrowExactly<LdfException>("because there is no variable");
    }

    [Fact(DisplayName = "Variable wraps variable")]
    public void Variable()
    {
        var g = new Graph();
        var m = g.CreateBlankNode();
        var v = g.CreateLiteralNode(Guid.NewGuid().ToString());
        g.Assert(m, Vocabulary.Hydra.Variable, v);

        new IriTemplateMapping(m, g).Variable.Should().Be(v.Value, "because it is shaped correctly");
    }

    [Fact(DisplayName = "Property throws without property")]
    public void RequiresProperty()
    {
        var g = new Graph();
        var t = g.CreateBlankNode();

        var property = () => new IriTemplateMapping(t, g).Property;

        property.Should().ThrowExactly<LdfException>("because there is no property");
    }


    [Fact(DisplayName = "Property wraps property")]
    public void Property()
    {
        var g = new Graph();
        var m = g.CreateBlankNode();
        var v = g.CreateBlankNode();
        g.Assert(m, Vocabulary.Hydra.Property, v);

        new IriTemplateMapping(m, g).Property.Should().Be(v, "because it is shaped correctly");
    }
}
