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
using VDS.RDF.Query.Expressions.Primary;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Expressions.Functions.Sparql.Boolean;

/// <summary>
/// Represents an EXIST/NOT EXISTS clause used as a Function in an Expression.
/// </summary>
public class ExistsFunction 
    : ISparqlExpression
{
    /// <summary>
    /// Creates a new EXISTS/NOT EXISTS function.
    /// </summary>
    /// <param name="pattern">Graph Pattern.</param>
    /// <param name="mustExist">Whether this is an EXIST.</param>
    public ExistsFunction(GraphPattern pattern, bool mustExist)
    {
        Pattern = pattern;
        MustExist = mustExist;
    }

    /// <summary>
    /// Get whether this is an EXIST (true) or NOT EXIST (false).
    /// </summary>
    public bool MustExist { get; }

    /// <summary>
    /// Get the pattern to test for (non-)existence.
    /// </summary>
    public GraphPattern Pattern { get; }


    /// <inheritdoc />
    public TResult Accept<TResult, TContext, TBinding>(ISparqlExpressionProcessor<TResult, TContext, TBinding> processor, TContext context, TBinding binding)
    {
        return processor.ProcessExistsFunction(this, context, binding);
    }

    /// <inheritdoc />
    public T Accept<T>(ISparqlExpressionVisitor<T> visitor)
    {
        return visitor.VisitExistsFunction(this);
    }

    /// <summary>
    /// Gets the Variables used in this Expression.
    /// </summary>
    public IEnumerable<string> Variables
    {
        get 
        { 
            return (from p in Pattern.TriplePatterns
                    from v in p.Variables
                    select v).Distinct();
        }
    }

    /// <summary>
    /// Gets whether an expression can safely be evaluated in parallel.
    /// </summary>
    public virtual bool CanParallelise
    {
        get
        {
            return false;
        }
    }

    /// <summary>
    /// Gets the String representation of the Expression.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        var output = new StringBuilder();
        if (MustExist)
        {
            output.Append("EXISTS ");
        }
        else
        {
            output.Append("NOT EXISTS ");
        }
        output.Append(Pattern);
        return output.ToString();
    }

    /// <summary>
    /// Gets the Type of the Expression.
    /// </summary>
    public SparqlExpressionType Type
    {
        get
        {
            return SparqlExpressionType.GraphOperator;
        }
    }

    /// <summary>
    /// Gets the Functor of the Expression.
    /// </summary>
    public string Functor
    {
        get
        {
            if (MustExist)
            {
                return SparqlSpecsHelper.SparqlKeywordExists;
            }
            else
            {
                return SparqlSpecsHelper.SparqlKeywordNotExists;
            }
        }
    }

    /// <summary>
    /// Gets the Arguments of the Expression.
    /// </summary>
    public IEnumerable<ISparqlExpression> Arguments
    {
        get
        {
            return new ISparqlExpression[] { new GraphPatternTerm(Pattern) };
        }
    }

    /// <summary>
    /// Transforms the Expression using the given Transformer.
    /// </summary>
    /// <param name="transformer">Expression Transformer.</param>
    /// <returns></returns>
    public ISparqlExpression Transform(IExpressionTransformer transformer)
    {
        ISparqlExpression temp = transformer.Transform(new GraphPatternTerm(Pattern));
        if (temp is GraphPatternTerm)
        {
            return new ExistsFunction(((GraphPatternTerm)temp).Pattern, MustExist);
        }
        else
        {
            throw new RdfQueryException("Unable to transform an EXISTS/NOT EXISTS function since the expression transformer in use failed to transform the inner Graph Pattern Expression to another Graph Pattern Expression");
        }
    }
}
