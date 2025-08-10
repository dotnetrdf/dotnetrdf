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
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra;

/// <summary>
/// Represents a Service Clause.
/// </summary>
public class Service
    : ITerminalOperator
{
    /// <summary>
    /// Get whether evaluation errors are suppressed.
    /// </summary>
    public bool Silent { get; }

    /// <summary>
    /// Creates a new Service clause with the given Endpoint Specifier and Graph Pattern.
    /// </summary>
    /// <param name="endpointSpecifier">Endpoint Specifier.</param>
    /// <param name="pattern">Graph Pattern.</param>
    /// <param name="silent">Whether Evaluation Errors are suppressed.</param>
    public Service(IToken endpointSpecifier, GraphPattern pattern, bool silent)
    {
        EndpointSpecifier = endpointSpecifier;
        Pattern = pattern;
        Silent = silent;
    }

    /// <summary>
    /// Creates a new Service clause with the given Endpoint Specifier and Graph Pattern.
    /// </summary>
    /// <param name="endpointSpecifier">Endpoint Specifier.</param>
    /// <param name="pattern">Graph Pattern.</param>
    public Service(IToken endpointSpecifier, GraphPattern pattern)
        : this(endpointSpecifier, pattern, false) { }


    /// <summary>
    /// Gets the Variables used in the Algebra.
    /// </summary>
    public IEnumerable<string> Variables
    {
        get
        {
            if (EndpointSpecifier.TokenType == Token.VARIABLE)
            {
                var serviceVar = ((VariableToken)EndpointSpecifier).Value.Substring(1);
                return Pattern.Variables.Concat(serviceVar.AsEnumerable()).Distinct();
            }
            else
            {
                return Pattern.Variables.Distinct();
            }
        }
    }

    /// <summary>
    /// Gets the enumeration of floating variables in the algebra i.e. variables that are not guaranteed to have a bound value.
    /// </summary>
    public IEnumerable<string> FloatingVariables
    {
        get
        {
            // Safest to assume all variables are floating as no guarantee the remote service is fully SPARQL compliant
            return Variables;
        }
    }

    /// <summary>
    /// Gets the enumeration of fixed variables in the algebra i.e. variables that are guaranteed to have a bound value.
    /// </summary>
    public IEnumerable<string> FixedVariables { get { return Enumerable.Empty<string>(); } }

    /// <summary>
    /// Gets the Endpoint Specifier.
    /// </summary>
    public IToken EndpointSpecifier { get; }

    /// <summary>
    /// Gets the Graph Pattern.
    /// </summary>
    public GraphPattern Pattern { get; }

    /// <summary>
    /// Gets the String representation of the Algebra.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return "Service(" + EndpointSpecifier.Value + ", " + Pattern + ")";
    }

    /// <inheritdoc />
    public TResult Accept<TResult, TContext>(ISparqlQueryAlgebraProcessor<TResult, TContext> processor, TContext context)
    {
        return processor.ProcessService(this, context);
    }

    /// <inheritdoc />
    public T Accept<T>(ISparqlAlgebraVisitor<T> visitor)
    {
        return visitor.VisitService(this);
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
    /// Converts the Algebra into a Graph Pattern.
    /// </summary>
    /// <returns></returns>
    public GraphPattern ToGraphPattern()
    {
        var p = new GraphPattern(Pattern);
        if (!p.HasModifier)
        {
            p.IsService = true;
            p.GraphSpecifier = EndpointSpecifier;
            return p;
        }
        else
        {
            var parent = new GraphPattern {IsService = true, GraphSpecifier = EndpointSpecifier};
            parent.AddGraphPattern(p);
            return parent;
        }
    }
}
