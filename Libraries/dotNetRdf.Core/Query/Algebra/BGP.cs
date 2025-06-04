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
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra;

/// <summary>
/// Represents a BGP which is a set of Triple Patterns.
/// </summary>
public class Bgp
    : IBgp
{
    /// <summary>
    /// The ordered list of triple patterns that are contained in this BGP.
    /// </summary>
    protected readonly List<ITriplePattern> _triplePatterns = new List<ITriplePattern>();

    /// <summary>
    /// Creates a new empty BGP.
    /// </summary>
    public Bgp() {}

    /// <summary>
    /// Creates a BGP containing a single Triple Pattern.
    /// </summary>
    /// <param name="p">Triple Pattern.</param>
    public Bgp(ITriplePattern p)
    {
        _triplePatterns.Add(p);
    }

    /// <summary>
    /// Creates a BGP containing a set of Triple Patterns.
    /// </summary>
    /// <param name="ps">Triple Patterns.</param>
    public Bgp(IEnumerable<ITriplePattern> ps)
    {
        _triplePatterns.AddRange(ps);
    }

    /// <summary>
    /// Gets the number of Triple Patterns in the BGP.
    /// </summary>
    public int PatternCount
    {
        get { return _triplePatterns.Count; }
    }

    /// <summary>
    /// Gets the Triple Patterns in the BGP.
    /// </summary>
    public IReadOnlyList<ITriplePattern> TriplePatterns
    {
        get { return _triplePatterns.AsReadOnly(); }
    }


    /// <summary>
    /// Gets the Variables used in the Algebra.
    /// </summary>
    public IEnumerable<string> Variables
    {
        get
        {
            return (from tp in _triplePatterns
                from v in tp.Variables
                select v).Distinct();
        }
    }

    /// <summary>
    /// Gets the enumeration of fixed variables in the algebra i.e. variables that are guaranteed to have a bound value.
    /// </summary>
    public IEnumerable<string> FixedVariables
    {
        get
        {
            return (from tp in _triplePatterns
                from v in tp.FixedVariables
                select v).Distinct();
        }
    }

    /// <summary>
    /// Gets the enumeration of floating variables in the algebra i.e. variables that are not guaranteed to have a bound value.
    /// </summary>
    public IEnumerable<string> FloatingVariables
    {
        get
        {
            // Floating variables are those declared as floating by triple patterns minus those that are declared as fixed by the triple patterns
            IEnumerable<string> floating = from tp in _triplePatterns
                from v in tp.FloatingVariables
                select v;
            var fixedVars = new HashSet<string>(FixedVariables);
            return floating.Where(v => !fixedVars.Contains(v)).Distinct();
        }
    }

    /// <summary>
    /// Gets whether the BGP is the empty BGP.
    /// </summary>
    public bool IsEmpty
    {
        get { return (_triplePatterns.Count == 0); }
    }

    /// <summary>
    /// Returns the String representation of the BGP.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        switch (_triplePatterns.Count)
        {
            case 0:
                return "BGP()";
            case 1:
                return "BGP(" + _triplePatterns[0] + ")";
            default:
                var builder = new StringBuilder();
                builder.Append("BGP(");
                for (var i = 0; i < _triplePatterns.Count; i++)
                {
                    builder.Append(_triplePatterns[i]);
                    if (i < _triplePatterns.Count - 1) builder.Append(", ");
                }
                builder.Append(")");
                return builder.ToString();
        }
    }

    /// <inheritdoc />
    public TResult Accept<TResult, TContext>(ISparqlQueryAlgebraProcessor<TResult, TContext> processor, TContext context)
    {
        return processor.ProcessBgp(this, context);
    }

    /// <inheritdoc />
    public T Accept<T>(ISparqlAlgebraVisitor<T> visitor)
    {
        return visitor.VisitBgp(this);
    }

    /// <summary>
    /// Converts the Algebra back to a SPARQL Query.
    /// </summary>
    /// <returns></returns>
    public virtual SparqlQuery ToQuery()
    {
        var q = new SparqlQuery
        {
            RootGraphPattern = ToGraphPattern(),
        };
        q.Optimise();
        return q;
    }

    /// <summary>
    /// Converts the BGP to a Graph Pattern.
    /// </summary>
    /// <returns></returns>
    public virtual GraphPattern ToGraphPattern()
    {
        var p = new GraphPattern();
        foreach (ITriplePattern tp in _triplePatterns)
        {
            p.AddTriplePattern(tp);
        }
        return p;
    }
}