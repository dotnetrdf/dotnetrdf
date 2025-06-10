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
using System.Text;
using VDS.RDF.Query.Paths;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra;

/// <summary>
/// Represents a Negated Property Set in the SPARQL Algebra.
/// </summary>
public class NegatedPropertySet : ISparqlAlgebra
{
    private readonly List<INode> _properties = new List<INode>();

    /// <summary>
    /// Creates a new Negated Property Set.
    /// </summary>
    /// <param name="start">Path Start.</param>
    /// <param name="end">Path End.</param>
    /// <param name="properties">Negated Properties.</param>
    /// <param name="inverse">Whether this is a set of Inverse Negated Properties.</param>
    public NegatedPropertySet(PatternItem start, PatternItem end, IEnumerable<Property> properties, bool inverse)
    {
        PathStart = start;
        PathEnd = end;
        _properties.AddRange(properties.Select(p => p.Predicate));
        Inverse = inverse;
    }

    /// <summary>
    /// Creates a new Negated Property Set.
    /// </summary>
    /// <param name="start">Path Start.</param>
    /// <param name="end">Path End.</param>
    /// <param name="properties">Negated Properties.</param>
    public NegatedPropertySet(PatternItem start, PatternItem end, IEnumerable<Property> properties)
        : this(start, end, properties, false) { }

    /// <summary>
    /// Gets the Path Start.
    /// </summary>
    public PatternItem PathStart { get; }

    /// <summary>
    /// Gets the Path End.
    /// </summary>
    public PatternItem PathEnd { get; }

    /// <summary>
    /// Gets the Negated Properties.
    /// </summary>
    public IEnumerable<INode> Properties
    {
        get
        {
            return _properties;
        }
    }

    /// <summary>
    /// Gets whether this is a set of Inverse Negated Properties.
    /// </summary>
    public bool Inverse { get; }

    /// <summary>
    /// Gets the Variables used in the Algebra.
    /// </summary>
    public IEnumerable<string> Variables => PathStart.Variables.Concat(PathEnd.Variables);
    

    /// <summary>
    /// Gets the enumeration of fixed variables in the algebra i.e. variables that are guaranteed to have a bound value.
    /// </summary>
    public IEnumerable<string> FixedVariables
    {
        get { return Variables; }
    }

    /// <summary>
    /// Gets the enumeration of floating variables in the algebra i.e. variables that are not guaranteed to have a bound value.
    /// </summary>
    public IEnumerable<string> FloatingVariables
    {
        get { return Enumerable.Empty<string>(); }
    }

    /// <summary>
    /// Transforms the Algebra back into a SPARQL Query.
    /// </summary>
    /// <returns></returns>
    public SparqlQuery ToQuery()
    {
        return new SparqlQuery { RootGraphPattern = ToGraphPattern() };
    }

    /// <summary>
    /// Transforms the Algebra back into a Graph Pattern.
    /// </summary>
    /// <returns></returns>
    public GraphPattern ToGraphPattern()
    {
        var gp = new GraphPattern();
        PropertyPathPattern pp;
        if (Inverse)
        {
            pp = new PropertyPathPattern(PathStart, new NegatedSet(Enumerable.Empty<Property>(), _properties.Select(p => new Property(p))), PathEnd);
        }
        else
        {
            pp = new PropertyPathPattern(PathStart, new NegatedSet(_properties.Select(p => new Property(p)), Enumerable.Empty<Property>()), PathEnd);
        }
        gp.AddTriplePattern(pp);
        return gp;
    }

    /// <summary>
    /// Gets the String representation of the Algebra.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        var output = new StringBuilder();
        output.Append("NegatedPropertySet(");
        output.Append(PathStart);
        output.Append(", {");
        for (var i = 0; i < _properties.Count; i++)
        {
            output.Append(_properties[i].ToString());
            if (i < _properties.Count - 1)
            {
                output.Append(", ");
            }
        }
        output.Append("}, ");
        output.Append(PathEnd);
        output.Append(')');

        return output.ToString();
    }

    /// <inheritdoc />
    public TResult Accept<TResult, TContext>(ISparqlQueryAlgebraProcessor<TResult, TContext> processor, TContext context)
    {
        return processor.ProcessNegatedPropertySet(this, context);
    }

    /// <inheritdoc />
    public T Accept<T>(ISparqlAlgebraVisitor<T> visitor)
    {
        return visitor.VisitNegatedPropertySet(this);
    }
}
