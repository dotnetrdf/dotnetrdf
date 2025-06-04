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
using VDS.RDF.Parsing.Tokens;
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra;

/// <summary>
/// Represents a GRAPH clause.
/// </summary>
public class Graph
    : IUnaryOperator
{
    private readonly ISparqlAlgebra _pattern;
    private readonly IToken _graphSpecifier;

    /// <summary>
    /// Creates a new Graph clause.
    /// </summary>
    /// <param name="pattern">Pattern.</param>
    /// <param name="graphSpecifier">Graph Specifier.</param>
    public Graph(ISparqlAlgebra pattern, IToken graphSpecifier)
    {
        _pattern = pattern;
        _graphSpecifier = graphSpecifier;
    }

    /// <summary>
    /// Gets the Variables used in the Algebra.
    /// </summary>
    public IEnumerable<string> Variables
    {
        get
        {
            if (_graphSpecifier.TokenType != Token.VARIABLE) return _pattern.Variables.Distinct();

            // Include graph variable
            var graphVar = ((VariableToken) _graphSpecifier).Value.Substring(1);
            return _pattern.Variables.Concat(graphVar.AsEnumerable()).Distinct();
        }
    }

    /// <inheritdoc />
    public IEnumerable<string> FloatingVariables
    {
        get
        {
            if (_graphSpecifier.TokenType != Token.VARIABLE) return _pattern.FloatingVariables;

            // May need to add graph variable to floating variables if it isn't fixed
            // Strictly speaking the graph variable should always be fixed but non-standard implementations may treat the default graph as a named graph with a null URI
            var graphVar = ((VariableToken) _graphSpecifier).Value.Substring(1);
            var fixedVars = new HashSet<string>(FixedVariables);
            return fixedVars.Contains(graphVar) ? _pattern.FloatingVariables : _pattern.FloatingVariables.Concat(graphVar.AsEnumerable()).Distinct();
        }
    }

    /// <summary>
    /// Gets the enumeration of fixed variables in the algebra i.e. variables that are guaranteed to have a bound value.
    /// </summary>
    public IEnumerable<string> FixedVariables
    {
        get { return _pattern.FixedVariables; }
    }

    /// <summary>
    /// Gets the Graph Specifier.
    /// </summary>
    public IToken GraphSpecifier
    {
        get { return _graphSpecifier; }
    }

    /// <summary>
    /// Gets the Inner Algebra.
    /// </summary>
    public ISparqlAlgebra InnerAlgebra
    {
        get { return _pattern; }
    }

    /// <summary>
    /// Gets the String representation of the Algebra.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"Graph({_graphSpecifier.Value}, {_pattern})";
    }

    /// <inheritdoc />
    public TResult Accept<TResult, TContext>(ISparqlQueryAlgebraProcessor<TResult, TContext> processor, TContext context)
    {
        return processor.ProcessGraph(this, context);
    }

    /// <inheritdoc />
    public T Accept<T>(ISparqlAlgebraVisitor<T> visitor)
    {
        return visitor.VisitGraph(this);
    }

    /// <summary>
    /// Converts the Algebra back to a SPARQL Query.
    /// </summary>
    /// <returns></returns>
    public SparqlQuery ToQuery()
    {
        var q = new SparqlQuery {RootGraphPattern = ToGraphPattern()};
        q.Optimise();
        return q;
    }

    /// <summary>
    /// Converts the Algebra back to a Graph Pattern.
    /// </summary>
    /// <returns></returns>
    public GraphPattern ToGraphPattern()
    {
        var p = _pattern.ToGraphPattern();
        if (!p.IsGraph)
        {
            p.IsGraph = true;
            p.GraphSpecifier = _graphSpecifier;
        }
        return p;
    }

    /// <summary>
    /// Transforms the Inner Algebra using the given Optimiser.
    /// </summary>
    /// <param name="optimiser">Optimiser.</param>
    /// <returns></returns>
    public ISparqlAlgebra Transform(IAlgebraOptimiser optimiser)
    {
        return new Graph(optimiser.Optimise(_pattern), _graphSpecifier);
    }

    /// <summary>
    /// Creates a Graph instance by applying a graph specifier to an algebra.
    /// </summary>
    /// <param name="algebra">The algebra to be constrained.</param>
    /// <param name="graphSpecifier">A token specifying the graph constraint.</param>
    /// <returns>A Graph instance representing the application of the graph constraint to the algebra.</returns>
    public static ISparqlAlgebra ApplyGraph(ISparqlAlgebra algebra, IToken graphSpecifier)
    {
        if (!(algebra is Graph)) return new Graph(algebra, graphSpecifier);

        var other = (Graph) algebra;
        if (other.GraphSpecifier.TokenType == graphSpecifier.TokenType && other.GraphSpecifier.Value.Equals(graphSpecifier.Value))
        {
            // We already have the appropriate graph specifier applied to us so reapplying it is unnecessary
            return algebra;
        }
        return new Graph(algebra, graphSpecifier);
    }
}