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

using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Construct;

namespace VDS.RDF.Query.Patterns;

/// <summary>
/// Class for representing quoted triple patterns in SPARQL-Star.
/// </summary>
public class QuotedTriplePattern : PatternItem
{
    /// <summary>
    /// Get the pattern for the quoted triple.
    /// </summary>
    public TriplePattern QuotedTriple { get; }

    /// <summary>
    /// Create a new quoted triple pattern.
    /// </summary>
    /// <param name="qtPattern">The pattern for matching quoted triples.</param>
    public QuotedTriplePattern(TriplePattern qtPattern)
    {
        QuotedTriple = qtPattern;
    }

    /// <inheritdoc />
    public override bool Accepts(IPatternEvaluationContext context, INode obj, ISet s)
    {
        if (obj is ITripleNode tripleNode)
        {
            return QuotedTriple.Subject.Accepts(context, tripleNode.Triple.Subject, s) &&
                   QuotedTriple.Predicate.Accepts(context, tripleNode.Triple.Predicate, s) &&
                   QuotedTriple.Object.Accepts(context, tripleNode.Triple.Object, s);
        }

        return false;
    }

    /// <inheritdoc />
    public override INode Construct(ConstructContext context)
    {
        INode subjectNode = QuotedTriple.Subject.Construct(context);
        INode predicateNode = QuotedTriple.Predicate.Construct(context);
        INode objectNode = QuotedTriple.Object.Construct(context);
        return new TripleNode(new Triple(subjectNode, predicateNode, objectNode));
    }

    /// <inheritdoc />
    public override INode Bind(ISet variableBindings)
    {
        return new TripleNode(new Triple(QuotedTriple.Subject.Bind(variableBindings),
            QuotedTriple.Predicate.Bind(variableBindings),
            QuotedTriple.Object.Bind(variableBindings)));
    }

    /// <inheritdoc />
    public override void AddBindings(INode forNode, ISet toSet)
    {
        if (forNode is not ITripleNode tn)
        {
            return;
        }

        QuotedTriple.Subject.AddBindings(tn.Triple.Subject, toSet);
        QuotedTriple.Predicate.AddBindings(tn.Triple.Predicate, toSet);
        QuotedTriple.Object.AddBindings(tn.Triple.Object, toSet);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"<< {QuotedTriple.Subject} {QuotedTriple.Predicate} {QuotedTriple.Object} >>";
    }

    /// <inheritdoc />
    public override IEnumerable<string> Variables => QuotedTriple.Subject.Variables
        .Concat(QuotedTriple.Predicate.Variables).Concat(QuotedTriple.Object.Variables).Distinct();

    /// <summary>
    /// Returns true if the quoted triple pattern does not contain any <see cref="VariablePattern"/> pattern items.
    /// </summary>
    public bool HasNoExplicitVariables => QuotedTriple.HasNoExplicitVariables;
    
    /// <summary>
    /// Returns true if the quoted triple pattern dost not contain any <see cref="BlankNodePattern"/> pattern items.
    /// </summary>
    public bool HasNoBlankVariables => QuotedTriple.HasNoBlankVariables;

    /// <inheritdoc />
    public override bool IsFixed => QuotedTriple.Variables.Count == 0;


    /// <summary>
    /// Create a set of bindings by matching <paramref name="node"/> to this pattern.
    /// </summary>
    /// <param name="node">The node to match.</param>
    /// <returns>A set of result bindings, which may be empty if <paramref name="node"/> does not match this pattern.</returns>
    public ISet CreateResults(INode node)
    {
        if (node is ITripleNode tn)
        {
            return QuotedTriple.CreateResult(tn.Triple);
        }

        return new Set();
    }
}
