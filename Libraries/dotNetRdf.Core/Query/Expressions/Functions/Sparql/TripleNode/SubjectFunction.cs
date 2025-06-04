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

namespace VDS.RDF.Query.Expressions.Functions.Sparql.TripleNode;

/// <summary>
/// Class representing the SPARQL-Star SUBJECT() function.
/// </summary>
public class SubjectFunction : BaseUnaryExpression
{
    /// <summary>
    /// Create a new SUBJECT() function expression.
    /// </summary>
    /// <param name="expr">Expression to apply the function to.</param>
    public SubjectFunction(ISparqlExpression expr) : base(expr) { }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"SUBJECT({InnerExpression})";
    }

    /// <inheritdoc />
    public override TResult Accept<TResult, TContext, TBinding>(ISparqlExpressionProcessor<TResult, TContext, TBinding> processor, TContext context, TBinding binding)
    {
        return processor.ProcessSubjectFunction(this, context, binding);
    }

    /// <inheritdoc />
    public override T Accept<T>(ISparqlExpressionVisitor<T> visitor)
    {
        return visitor.VisitSubjectFunction(this);
    }

    /// <inheritdoc />
    public override SparqlExpressionType Type => SparqlExpressionType.Function;

    /// <inheritdoc />
    public override string Functor => SparqlSpecsHelper.SparqlStarKeywordSubject;

    /// <inheritdoc />
    public override ISparqlExpression Transform(IExpressionTransformer transformer)
    {
        return new SubjectFunction(transformer.Transform(InnerExpression));
    }
}
