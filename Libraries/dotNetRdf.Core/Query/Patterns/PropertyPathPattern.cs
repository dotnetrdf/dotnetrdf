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
using System.Text;
using VDS.RDF.Query.Paths;

namespace VDS.RDF.Query.Patterns;

/// <summary>
/// Class for representing property patterns in SPARQL Queries.
/// </summary>
public class PropertyPathPattern
    : BaseTriplePattern, IPropertyPathPattern, IComparable<PropertyPathPattern>
{
    /// <summary>
    /// Creates a new Property Path Pattern.
    /// </summary>
    /// <param name="subj">Subject.</param>
    /// <param name="path">Property Path.</param>
    /// <param name="obj">Object.</param>
    public PropertyPathPattern(PatternItem subj, ISparqlPath path, PatternItem obj)
    {
        Subject = subj;
        Path = path;
        Object = obj;
        Subject.RigorousEvaluation = true;
        Object.RigorousEvaluation = true;

        // Build our list of Variables
        foreach (var variableName in Subject.Variables.Concat(Object.Variables))
        {
            if (!_vars.Contains(variableName)) _vars.Add(variableName);
        }
        _vars.Sort();
    }

    /// <summary>
    /// Gets the pattern type.
    /// </summary>
    public override TriplePatternType PatternType => TriplePatternType.Path;

    /// <summary>
    /// Gets the Subject of the Property Path.
    /// </summary>
    public PatternItem Subject { get; }

    /// <summary>
    /// Gets the Property Path.
    /// </summary>
    public ISparqlPath Path { get; }

    /// <summary>
    /// Gets the Object of the Property Path.
    /// </summary>
    public PatternItem Object { get; }

    /// <summary>
    /// Gets the enumeration of fixed variables in the pattern i.e. variables that are guaranteed to have a bound value.
    /// </summary>
    public override IEnumerable<string> FixedVariables => _vars;

    /// <summary>
    /// Gets the enumeration of floating variables in the pattern i.e. variables that are not guaranteed to have a bound value.
    /// </summary>
    public override IEnumerable<string> FloatingVariables { get; } = Enumerable.Empty<string>();

    /// <inheritdoc />
    public override TResult Accept<TResult, TContext>(ISparqlQueryAlgebraProcessor<TResult, TContext> processor, TContext context)
    {
        return processor.ProcessPropertyPathPattern(this, context);
    }

    /// <inheritdoc />
    public override T Accept<T>(ISparqlAlgebraVisitor<T> visitor)
    {
        return visitor.VisitPropertyPathPattern(this);
    }

    /// <summary>
    /// Gets whether the Pattern accepts all Triple Patterns.
    /// </summary>
    public override bool IsAcceptAll => false;

    /// <summary>
    /// Returns false a property path may always contain implicit blank variables.
    /// </summary>
    public override bool HasNoBlankVariables => false;

    /// <summary>
    /// Compares a property path pattern to another.
    /// </summary>
    /// <param name="other">Pattern.</param>
    /// <returns></returns>
    public int CompareTo(PropertyPathPattern other)
    {
        return CompareTo((IPropertyPathPattern)other);
    }

    /// <summary>
    /// Compares a property path pattern to another.
    /// </summary>
    /// <param name="other">Pattern.</param>
    /// <returns></returns>
    public int CompareTo(IPropertyPathPattern other)
    {
        return base.CompareTo(other);
    }

    /// <summary>
    /// Gets the String representation of the Pattern.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        var output = new StringBuilder();
        output.Append(Subject);
        output.Append(' ');
        output.Append(Path.ToString());
        output.Append(' ');
        output.Append(Object);
        return output.ToString();
    }
}
