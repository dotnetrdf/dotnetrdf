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
using VDS.RDF.Query.PropertyFunctions;

namespace VDS.RDF.Query.Patterns;

/// <summary>
/// Class for representing property function patterns in SPARQL Query.
/// </summary>
public class PropertyFunctionPattern
    : BaseTriplePattern, IPropertyFunctionPattern, IComparable<PropertyFunctionPattern>
{
    private readonly List<ITriplePattern> _patterns;
    private readonly List<PatternItem> _lhsArgs, _rhsArgs;

    /// <summary>
    /// Creates a new Property Function pattern.
    /// </summary>
    /// <param name="info">Function information.</param>
    /// <param name="propertyFunction">Property Function.</param>
    public PropertyFunctionPattern(PropertyFunctionInfo info, ISparqlPropertyFunction propertyFunction)
        : this(info.Patterns.OfType<ITriplePattern>(), info.SubjectArgs, info.ObjectArgs, propertyFunction) { }

    /// <summary>
    /// Creates a new Property Function pattern.
    /// </summary>
    /// <param name="origPatterns">Original Triple Patterns.</param>
    /// <param name="lhsArgs">Subject Arguments.</param>
    /// <param name="rhsArgs">Object Arguments.</param>
    /// <param name="propertyFunction">Property Function.</param>
    public PropertyFunctionPattern(IEnumerable<ITriplePattern> origPatterns, IEnumerable<PatternItem> lhsArgs, IEnumerable<PatternItem> rhsArgs, ISparqlPropertyFunction propertyFunction)
    {
        _patterns = origPatterns.ToList();
        _lhsArgs = lhsArgs.ToList();
        _rhsArgs = rhsArgs.ToList();
        PropertyFunction = propertyFunction;

        foreach (PatternItem item in _lhsArgs.Concat(_rhsArgs))
        {
            foreach (var variableName in item.Variables)
            {
                if (!_vars.Contains(variableName)) _vars.Add(variableName);
            }
        }
    }

    /// <summary>
    /// Gets the Pattern Type.
    /// </summary>
    public override TriplePatternType PatternType
    {
        get
        {
            return TriplePatternType.PropertyFunction;
        }
    }

    /// <summary>
    /// Gets the Subject arguments.
    /// </summary>
    public IEnumerable<PatternItem> SubjectArgs
    {
        get
        {
            return _lhsArgs;
        }
    }

    /// <summary>
    /// Gets the Object arguments.
    /// </summary>
    public IEnumerable<PatternItem> ObjectArgs
    {
        get
        {
            return _rhsArgs;
        }
    }

    /// <summary>
    /// Gets the original triple patterns.
    /// </summary>
    public IEnumerable<ITriplePattern> OriginalPatterns
    {
        get
        {
            return _patterns;
        }
    }

    /// <summary>
    /// Gets the property function.
    /// </summary>
    public ISparqlPropertyFunction PropertyFunction { get; }

    /// <summary>
    /// Returns the empty enumerable as cannot guarantee any variables are bound.
    /// </summary>
    public override IEnumerable<string> FixedVariables
    {
        get { return Enumerable.Empty<string>(); }
    }

    /// <summary>
    /// Returns all variables mentioned in the property function as we can't guarantee they are bound.
    /// </summary>
    public override IEnumerable<string> FloatingVariables { get { return _vars; } }

    /// <inheritdoc />
    public override TResult Accept<TResult, TContext>(ISparqlQueryAlgebraProcessor<TResult, TContext> processor, TContext context)
    {
        return processor.ProcessPropertyFunctionPattern(this, context);
    }

    /// <inheritdoc />
    public override T Accept<T>(ISparqlAlgebraVisitor<T> visitor)
    {
        return visitor.VisitPropertyFunctionPattern(this);
    }

    /// <summary>
    /// Returns false because property functions are not accept-alls.
    /// </summary>
    public override bool IsAcceptAll
    {
        get 
        {
            return false;
        }
    }

    /// <summary>
    /// Returns true if none of the. 
    /// </summary>
    public override bool HasNoBlankVariables
    {
        get 
        {
            return !_vars.Any(v => v.StartsWith("_:"));
        }
    }

    /// <summary>
    /// Compares a property function pattern to another.
    /// </summary>
    /// <param name="other">Pattern.</param>
    /// <returns></returns>
    public int CompareTo(PropertyFunctionPattern other)
    {
        return CompareTo((IPropertyFunctionPattern)other);
    }

    /// <summary>
    /// Compares a property function pattern to another.
    /// </summary>
    /// <param name="other">Pattern.</param>
    /// <returns></returns>
    public int CompareTo(IPropertyFunctionPattern other)
    {
        return base.CompareTo(other);
    }

    /// <summary>
    /// Gets the string representation of the pattern.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        var output = new StringBuilder();
        if (_lhsArgs.Count > 1)
        {
            output.Append("( ");
            foreach (PatternItem arg in _lhsArgs)
            {
                output.Append(arg);
                output.Append(' ');
            }
            output.Append(')');
        }
        else
        {
            output.Append(_lhsArgs.First());
        }
        output.Append(" <");
        output.Append(PropertyFunction.FunctionUri);
        output.Append("> ");
        if (_rhsArgs.Count > 1)
        {
            output.Append("( ");
            foreach (PatternItem arg in _rhsArgs)
            {
                output.Append(arg);
                output.Append(' ');
            }
            output.Append(')');
        }
        else
        {
            output.Append(_rhsArgs.First());
        }
        output.Append(" .");
        return output.ToString();
    }
}
