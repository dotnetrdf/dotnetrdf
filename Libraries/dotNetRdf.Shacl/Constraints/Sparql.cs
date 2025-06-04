/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2025 dotNetRDF Project (http://dotnetrdf.org/)
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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Expressions.Primary;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Shacl.Validation;

namespace VDS.RDF.Shacl.Constraints;

internal abstract class Sparql : Constraint
{
    [DebuggerStepThrough]
    protected Sparql(Shape shape, INode value)
        : this(shape, value, Enumerable.Empty<KeyValuePair<string, INode>>())
    {
    }

    [DebuggerStepThrough]
    protected Sparql(Shape shape, INode value, IEnumerable<KeyValuePair<string, INode>> parameters)
        : base(shape, value)
    {
        Parameters = parameters;
    }

    internal override INode ConstraintComponent
    {
        get
        {
            return Vocabulary.SparqlConstraintComponent;
        }
    }

    protected abstract string Query { get; }

    protected ILiteralNode Message
    {
        get
        {
            return (ILiteralNode)Vocabulary.Message.ObjectsOf(this).SingleOrDefault();
        }
    }

    private IEnumerable<KeyValuePair<string, INode>> Parameters { get; set; }

    private IEnumerable<PrefixDeclaration> Prefixes
    {
        get
        {
            return Vocabulary.Prefixes.ObjectsOf(this).Select(p => new Prefixes(p, Graph)).SingleOrDefault() ?? Enumerable.Empty<PrefixDeclaration>();
        }
    }

    internal override bool Validate(IGraph dataGraph, INode focusNode, IEnumerable<INode> valueNodes, Report report)
    {
        var queryString = new SparqlParameterizedString(Query);

        foreach (PrefixDeclaration item in Prefixes)
        {
            queryString.Namespaces.AddNamespace(item.Prefix, item.Namespace);
        }

        SparqlQuery query = new SparqlQueryParser().ParseFromString(queryString);

        Validate(query.RootGraphPattern);

        BindFocusNode(query.RootGraphPattern, focusNode);
        query.RootGraphPattern.TriplePatterns.Insert(0, new BindPattern("currentShape", new ConstantTerm(Shape)));

        if (Shape.Graph.Name != null)
        {
            query.RootGraphPattern.TriplePatterns.Insert(0, new BindPattern("shapesGraph", new ConstantTerm(Shape.Graph.Name)));
        }

        foreach (KeyValuePair<string, INode> parameter in Parameters)
        {
            query.RootGraphPattern.TriplePatterns.Insert(0, new BindPattern(parameter.Key, new ConstantTerm(parameter.Value)));
        }

        return ValidateInternal(dataGraph, focusNode, valueNodes, report, query);
    }

    protected abstract bool ValidateInternal(IGraph dataGraph, INode focusNode, IEnumerable<INode> valueNodes, Report report, SparqlQuery query);

    private static void BindFocusNode(GraphPattern pattern, INode focusNode)
    {
        pattern.TriplePatterns.Insert(0, new BindPattern("this", new ConstantTerm(focusNode)));

        foreach (GraphPattern subPattern in pattern.ChildGraphPatterns)
        {
            BindFocusNode(subPattern, focusNode);
        }

        foreach (SubQueryPattern subQueryPattern in pattern.TriplePatterns.OfType<SubQueryPattern>())
        {
            BindFocusNode(subQueryPattern.SubQuery.RootGraphPattern, focusNode);
        }
    }

    private void Validate(GraphPattern pattern)
    {
        if (pattern.IsMinus || pattern.InlineData != null || pattern.IsService)
        {
            throw new Exception("illegal clauses");
        }

        foreach (GraphPattern subPattern in pattern.ChildGraphPatterns)
        {
            Validate(subPattern);
        }

        foreach (SubQueryPattern subQueryPattern in pattern.TriplePatterns.OfType<SubQueryPattern>())
        {
            if (!subQueryPattern.Variables.Contains("this"))
            {
                throw new Exception("missing projection");
            }

            Validate(subQueryPattern.SubQuery.RootGraphPattern);
        }
    }
}
