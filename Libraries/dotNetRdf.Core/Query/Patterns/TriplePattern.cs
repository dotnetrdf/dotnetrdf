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
using System.Linq;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Construct;

namespace VDS.RDF.Query.Patterns;


/// <summary>
/// Class for representing Triple Patterns in SPARQL Queries.
/// </summary>
public class TriplePattern
    : BaseTriplePattern, IMatchTriplePattern, IConstructTriplePattern, IComparable<TriplePattern>
{
    /// <summary>
    /// Creates a new Triple Pattern.
    /// </summary>
    /// <param name="subj">Subject Pattern.</param>
    /// <param name="pred">Predicate Pattern.</param>
    /// <param name="obj">Object Pattern.</param>
    public TriplePattern(PatternItem subj, PatternItem pred, PatternItem obj)
    {
        Subject = subj;
        switch (pred)
        {
            case BlankNodePattern:
                throw new RdfParseException("Cannot use a Triple Pattern with a Blank Node Predicate in a SPARQL Query");
            case NodeMatchPattern { Node: ITripleNode }:
                throw new RdfParseException(
                    "Cannot use a triple pattern with a quoted triple predicate in a SPARQL query.");
            case QuotedTriplePattern:
                throw new RdfParseException(
                    "Cannot use a Triple Pattern with a Triple Node predicate in a SPARQL Query");
        }

        Predicate = pred;
        Object = obj;

        // Decide on the Index Type
        if (Subject.IsFixed)
        {
            if (Predicate.IsFixed)
            {
                IndexType = TripleIndexType.SubjectPredicate;
            }
            else if (Object.IsFixed)
            {
                IndexType = TripleIndexType.SubjectObject;
            }
            else
            {
                IndexType = TripleIndexType.Subject;
            }
        }
        else if (Predicate.IsFixed)
        {
            IndexType = Object.IsFixed ? TripleIndexType.PredicateObject : TripleIndexType.Predicate;
        }
        else if (Object.IsFixed)
        {
            IndexType = TripleIndexType.Object;
        }

        // Determine variables used
        if (!Subject.IsFixed)
        {
            _vars.AddRange(Subject.Variables);
        }
        if (!Predicate.IsFixed)
        {
            foreach (var pv in Predicate.Variables)
            {
                if (!_vars.Contains(pv)) _vars.Add(pv);
            }
            Predicate.Repeated = Subject.Equals(Predicate);
        }
        if (!Object.IsFixed)
        {
            foreach (var ov in Object.Variables)
            {
                if (!_vars.Contains(ov)) _vars.Add(ov);
            }

            Object.Repeated = Object.Equals(Subject) || Object.Equals(Predicate);
        }
        _vars.Sort();
        if (_vars.Count == 0) IndexType = TripleIndexType.NoVariables;
    }

    /// <summary>
    /// Evaluates a triple match pattern against the given triple.
    /// </summary>
    /// <param name="context">Evaluation Context.</param>
    /// <param name="obj">Triple to test.</param>
    /// <returns>A set of variable bindings if the <paramref name="obj"/> matches, null otherwise.</returns>
    public ISet Evaluate(IPatternEvaluationContext context, Triple obj)
    {
        ISet set = new Set();
        return Subject.Accepts(context, obj.Subject, set) &&
               Predicate.Accepts(context, obj.Predicate, set) &&
               Object.Accepts(context, obj.Object, set)
            ? set
            : null;
    }

    /// <summary>
    /// Gets the pattern type.
    /// </summary>
    public override TriplePatternType PatternType
    {
        get 
        {
            return TriplePatternType.Match;
        }
    }

    /// <summary>
    /// Gets the Index Type we will use for this Pattern.
    /// </summary>
    public TripleIndexType IndexType { get; } = TripleIndexType.None;

    /// <summary>
    /// Subject Pattern.
    /// </summary>
    public PatternItem Subject { get; }

    /// <summary>
    /// Predicate Pattern.
    /// </summary>
    public PatternItem Predicate { get; }

    /// <summary>
    /// Object Pattern.
    /// </summary>
    public PatternItem Object { get; }

    /// <summary>
    /// Returns all variables mentioned as a match guarantees all variables are bound.
    /// </summary>
    public override IEnumerable<string> FixedVariables
    {
        get { return Variables; }
    }

    /// <summary>
    /// Returns an empty enumeration as a match guarantees all variables are bound.
    /// </summary>
    public override IEnumerable<string> FloatingVariables { get { return Enumerable.Empty<string>(); } }

    /// <inheritdoc />
    public override TResult Accept<TResult, TContext>(ISparqlQueryAlgebraProcessor<TResult, TContext> processor, TContext context)
    {
        return processor.ProcessTriplePattern(this, context);
    }

    /// <inheritdoc />
    public override T Accept<T>(ISparqlAlgebraVisitor<T> visitor)
    {
        return visitor.VisitTriplePattern(this);
    }

    /// <summary>
    /// Returns whether the Triple Pattern is an accept all.
    /// </summary>
    /// <remarks>
    /// True if all three Pattern Items are <see cref="VariablePattern">VariablePattern</see> and all the Variables names are distinct.
    /// </remarks>
    public override bool IsAcceptAll
    {
        get
        {
            return Subject is VariablePattern svp  && Predicate is VariablePattern pvp && Object is VariablePattern ovp &&
                   svp.VariableName != pvp.VariableName &&
                   pvp.VariableName != ovp.VariableName &&
                   svp.VariableName != ovp.VariableName;
        }

    }

    /// <summary>
    /// Generates a Result Set for a Triple that matches the Pattern.
    /// </summary>
    /// <param name="t">Triple.</param>
    /// <returns></returns>
    public ISet CreateResult(Triple t)
    {
        ISet s = new Set();
        if (!Subject.IsFixed)
        {
            Subject.AddBindings(t.Subject, s);
        }

        if (!Predicate.IsFixed && !Predicate.Repeated)
        {
            Predicate.AddBindings(t.Predicate, s);
        }

        if (!Object.IsFixed && !Object.Repeated)
        {
            Object.AddBindings(t.Object, s);
        }
        return s;
    }

    /// <summary>
    /// Constructs a Triple from a Set based on this Triple Pattern.
    /// </summary>
    /// <param name="context">Construct Context.</param>
    /// <returns></returns>
    public Triple Construct(ConstructContext context)
    {
        return new Triple(Subject.Construct(context), Predicate.Construct(context), Object.Construct(context));
    }

    /// <summary>
    /// Gets whether the Pattern contains no Variables of any kind.
    /// </summary>
    public bool HasNoVariables
    {
        get
        {
            return (IndexType == TripleIndexType.NoVariables);
        }
    }

    /// <summary>
    /// Gets whether the Pattern contains no Explicit Variables (i.e. Blank Node Variables are ignored).
    /// </summary>
    public bool HasNoExplicitVariables
    {
        get
        {
            return Subject is NodeMatchPattern or BlankNodePattern or FixedBlankNodePattern or QuotedTriplePattern { HasNoExplicitVariables: true } &&
                   Predicate is NodeMatchPattern or BlankNodePattern or FixedBlankNodePattern &&
                   Object is NodeMatchPattern or BlankNodePattern or FixedBlankNodePattern or QuotedTriplePattern { HasNoExplicitVariables: true };
        }
    }

    /// <summary>
    /// Gets whether the Pattern contains no Explicit Variables (i.e. Blank Node Variables are ignored).
    /// </summary>
    public override bool HasNoBlankVariables
    {
        get
        {
            return Subject is NodeMatchPattern or VariablePattern or FixedBlankNodePattern or QuotedTriplePattern{HasNoBlankVariables:true} &&
                   Predicate is NodeMatchPattern or VariablePattern or FixedBlankNodePattern &&
                   Object is NodeMatchPattern or VariablePattern or FixedBlankNodePattern or QuotedTriplePattern{HasNoBlankVariables:true};
        }
    }

    /// <summary>
    /// Compares a triple pattern to another.
    /// </summary>
    /// <param name="other">Pattern.</param>
    /// <returns></returns>
    public int CompareTo(TriplePattern other)
    {
        return base.CompareTo(other);
    }

    /// <summary>
    /// Compares a triple pattern to another.
    /// </summary>
    /// <param name="other">Pattern.</param>
    /// <returns></returns>
    public int CompareTo(IMatchTriplePattern other)
    {
        return base.CompareTo(other);
    }

    /// <summary>
    /// Gets the String representation of this Pattern.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return Subject + " " + Predicate + " " + Object;
    }

    /// <summary>
    /// Wrap this triple pattern as a quoted triple match pattern.
    /// </summary>
    /// <returns>A <see cref="NodeMatchPattern"/> if the subject, predicate and object of this pattern are not variables,
    /// A <see cref="QuotedTriplePattern"/> that wraps this pattern otherwise.</returns>
    public PatternItem AsQuotedPatternItem()
    {
        if (Subject is NodeMatchPattern subjectNodeMatchPattern &&
            Predicate is NodeMatchPattern predicateNodeMatchPattern &&
            Object is NodeMatchPattern objectNodeMatchPattern)
        {
            return new NodeMatchPattern(new TripleNode(new Triple(subjectNodeMatchPattern.Node,
                predicateNodeMatchPattern.Node, objectNodeMatchPattern.Node)));
        }

        return new QuotedTriplePattern(this);
    }
}