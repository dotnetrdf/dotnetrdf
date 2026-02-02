/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2026 dotNetRDF Project (http://dotnetrdf.org/)
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

using Resta.UriTemplates;
using System;
using System.Collections.Generic;
using VDS.RDF.LDF.Hydra;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.LDF.Client;

internal class TpfParameters
{
    private Uri _uri;

    internal TpfParameters(IriTemplate template, INode subject = null, INode predicate = null, INode @object = null)
    {
        Validate(template, subject, predicate, @object);
        Calculate(template, subject, predicate, @object);
    }

    private static void Validate(IriTemplate template, INode subject, INode predicate, INode @object)
    {
        if (template is null)
        {
            throw new ArgumentNullException(nameof(template));
        }

        if (subject is not null && subject.NodeType != NodeType.Uri)
        {
            throw new ArgumentOutOfRangeException(nameof(subject), subject.NodeType, "Must be IRI");
        }

        if (predicate is not null && predicate.NodeType != NodeType.Uri)
        {
            throw new ArgumentOutOfRangeException(nameof(predicate), predicate.NodeType, "Must be IRI");
        }

        if (@object is not null && @object.NodeType != NodeType.Uri && @object.NodeType != NodeType.Literal)
        {
            throw new ArgumentOutOfRangeException(nameof(@object), @object.NodeType, "Must be IRI or literal");
        }
    }

    private void Calculate(IriTemplate template, INode subject = null, INode predicate = null, INode @object = null)
    {
        var variables = new Dictionary<string, object>();
        var formatter = (INodeFormatter)new ExplicitRepresentationFormatter();

        if (subject is not null)
        {
            variables.Add(template.SubjectVariable, formatter.Format(subject));
        }

        if (predicate is not null)
        {
            variables.Add(template.PredicateVariable, formatter.Format(predicate));
        }

        if (@object is not null)
        {
            variables.Add(template.ObjectVariable, formatter.Format(@object));
        }

        _uri = new UriTemplate(template.Template).ResolveUri(variables);
    }

    public static implicit operator Uri(TpfParameters parameters) => parameters._uri;
}
