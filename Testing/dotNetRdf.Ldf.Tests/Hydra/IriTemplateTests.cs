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

public class IriTemplateTests
{
    [Fact(DisplayName = "SubjectVariable throws without mapping")]
    public void SubjectRequiresMapping()
    {
        var g = new Graph();
        var t = g.CreateBlankNode();

        var subjectVariable = () => new IriTemplate(t, g).SubjectVariable;

        subjectVariable.Should().ThrowExactly<LdfException>("because there is no mapping");
    }

    [Fact(DisplayName = "SubjectVariable throws without property")]
    public void SubjectRequiresProperty()
    {
        var g = new Graph();
        var t = g.CreateBlankNode();
        var m = g.CreateBlankNode();
        g.Assert(t, Vocabulary.Hydra.Mapping, m);

        var subjectVariable = () => new IriTemplate(t, g).SubjectVariable;

        subjectVariable.Should().ThrowExactly<LdfException>("because there is no property");
    }

    [Fact(DisplayName = "SubjectVariable throws without variable")]
    public void SubjectRequiresVariable()
    {
        var g = new Graph();
        var t = g.CreateBlankNode();
        var m = g.CreateBlankNode();
        g.Assert(t, Vocabulary.Hydra.Mapping, m);
        g.Assert(m, Vocabulary.Hydra.Property, Vocabulary.Rdf.Subject);

        var subjectVariable = () => new IriTemplate(t, g).SubjectVariable;

        subjectVariable.Should().ThrowExactly<LdfException>("because there is no variable");
    }

    [Fact(DisplayName = "SubjectVariable returns variable of mapping with subject property")]
    public void Subject()
    {
        var g = new Graph();
        var t = g.CreateBlankNode();
        var m = g.CreateBlankNode();
        var v = g.CreateLiteralNode(Guid.NewGuid().ToString());
        g.Assert(t, Vocabulary.Hydra.Mapping, m);
        g.Assert(m, Vocabulary.Hydra.Property, Vocabulary.Rdf.Subject);
        g.Assert(m, Vocabulary.Hydra.Variable, v);

        new IriTemplate(t, g).SubjectVariable.Should().Be(v.Value, "because it is shaped correctly");
    }

    [Fact(DisplayName = "PredicateVariable throws without mapping")]
    public void PredicateRequiresMapping()
    {
        var g = new Graph();
        var t = g.CreateBlankNode();

        var predicateVariable = () => new IriTemplate(t, g).PredicateVariable;

        predicateVariable.Should().ThrowExactly<LdfException>("because there is no mapping");
    }

    [Fact(DisplayName = "PredicateVariable throws without property")]
    public void PredicateRequiresProperty()
    {
        var g = new Graph();
        var t = g.CreateBlankNode();
        var m = g.CreateBlankNode();
        g.Assert(t, Vocabulary.Hydra.Mapping, m);

        var predicateVariable = () => new IriTemplate(t, g).PredicateVariable;

        predicateVariable.Should().ThrowExactly<LdfException>("because there is no property");
    }

    [Fact(DisplayName = "PredicateVariable throws without variable")]
    public void PredicateRequiresVariable()
    {
        var g = new Graph();
        var t = g.CreateBlankNode();
        var m = g.CreateBlankNode();
        g.Assert(t, Vocabulary.Hydra.Mapping, m);
        g.Assert(m, Vocabulary.Hydra.Property, Vocabulary.Rdf.Predicate);

        var predicateVariable = () => new IriTemplate(t, g).PredicateVariable;

        predicateVariable.Should().ThrowExactly<LdfException>("because there is no variable");
    }

    [Fact(DisplayName = "PredicateVariable returns variable of mapping with predicate property")]
    public void Predicate()
    {
        var g = new Graph();
        var t = g.CreateBlankNode();
        var m = g.CreateBlankNode();
        var v = g.CreateLiteralNode(Guid.NewGuid().ToString());
        g.Assert(t, Vocabulary.Hydra.Mapping, m);
        g.Assert(m, Vocabulary.Hydra.Property, Vocabulary.Rdf.Predicate);
        g.Assert(m, Vocabulary.Hydra.Variable, v);

        new IriTemplate(t, g).PredicateVariable.Should().Be(v.Value, "because it is shaped correctly");
    }

    [Fact(DisplayName = "ObjectVariable throws without mapping")]
    public void ObjectRequiresMapping()
    {
        var g = new Graph();
        var t = g.CreateBlankNode();

        var objectVariable = () => new IriTemplate(t, g).ObjectVariable;

        objectVariable.Should().ThrowExactly<LdfException>("because there is no mapping");
    }

    [Fact(DisplayName = "ObjectVariable throws without property")]
    public void ObjectRequiresProperty()
    {
        var g = new Graph();
        var t = g.CreateBlankNode();
        var m = g.CreateBlankNode();
        g.Assert(t, Vocabulary.Hydra.Mapping, m);

        var objectVariable = () => new IriTemplate(t, g).ObjectVariable;

        objectVariable.Should().ThrowExactly<LdfException>("because there is no property");
    }

    [Fact(DisplayName = "ObjectVariable throws without variable")]
    public void ObjectRequiresVariable()
    {
        var g = new Graph();
        var t = g.CreateBlankNode();
        var m = g.CreateBlankNode();
        g.Assert(t, Vocabulary.Hydra.Mapping, m);
        g.Assert(m, Vocabulary.Hydra.Property, Vocabulary.Rdf.Object);

        var objectVariable = () => new IriTemplate(t, g).ObjectVariable;

        objectVariable.Should().ThrowExactly<LdfException>("because there is no variable");
    }

    [Fact(DisplayName = "ObjectVariable returns variable of mapping with object property")]
    public void Object()
    {
        var g = new Graph();
        var t = g.CreateBlankNode();
        var m = g.CreateBlankNode();
        var v = g.CreateLiteralNode(Guid.NewGuid().ToString());
        g.Assert(t, Vocabulary.Hydra.Mapping, m);
        g.Assert(m, Vocabulary.Hydra.Property, Vocabulary.Rdf.Object);
        g.Assert(m, Vocabulary.Hydra.Variable, v);

        new IriTemplate(t, g).ObjectVariable.Should().Be(v.Value, "because it is shaped correctly");
    }

    [Fact(DisplayName = "Template throws without template")]
    public void TemplateRequiresTemplate()
    {
        var g = new Graph();
        var t = g.CreateBlankNode();

        var template = () => new IriTemplate(t, g).Template;

        template.Should().ThrowExactly<LdfException>("because there is no template");
    }

    [Fact(DisplayName = "Template returns object of template statement")]
    public void Template()
    {
        var g = new Graph();
        var t = g.CreateBlankNode();
        var v = g.CreateLiteralNode(Guid.NewGuid().ToString());
        g.Assert(t, Vocabulary.Hydra.Template, v);

        new IriTemplate(t, g).Template.Should().Be(v.Value, "because it is shaped correctly");
    }
}
