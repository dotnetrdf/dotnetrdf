/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2026 dotNetRDF Project (http://dotnetrdf.org/)
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

using VDS.RDF.Nodes;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Functions.Sparql.Boolean;
using VDS.RDF.Query.Expressions.Functions.Sparql.Constructor;
using VDS.RDF.Query.Expressions.Functions.Sparql.Numeric;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Pull;

internal class PullExpressionProcessor(
    ISparqlNodeComparer nodeComparer,
    IUriFactory uriFactory,
    bool useStrictOperators)
    : BaseExpressionProcessor<PullEvaluationContext, ExpressionContext>(nodeComparer, uriFactory, useStrictOperators)
{
    private readonly Random _rnd = new();

    protected override IValuedNode GetBoundValue(string variableName, PullEvaluationContext context,
        ExpressionContext expressionContext)
    {
        INode value = expressionContext.Bindings[variableName];
        return value.AsValuedNode();
    }

    protected override IEnumerable<ISparqlCustomExpressionFactory> GetExpressionFactories(PullEvaluationContext context)
    {
        throw new NotImplementedException("PullExpressionProcessor.GetExpressionFactories not implemented");
    }

    public override IValuedNode ProcessAggregateTerm(AggregateTerm aggregate, PullEvaluationContext context,
        ExpressionContext expressionContext)
    {
        throw new NotImplementedException("PullExpressionProcessor.ProcessAggregateTerm not implemented");
    }

    public override IValuedNode ProcessExistsFunction(ExistsFunction exists, PullEvaluationContext context,
        ExpressionContext expressionContext)
    {
        var builder = new EvaluationBuilder();
        IAsyncEvaluation graphPatternEvaluation = builder.Build(exists.Pattern.ToAlgebra(), context);
        ValueTask<bool> findMatch = graphPatternEvaluation
            .Evaluate(context, expressionContext.Bindings, expressionContext.ActiveGraph).AnyAsync();
        var result = findMatch.IsCompleted ? findMatch.Result : findMatch.AsTask().GetAwaiter().GetResult();
        return exists.MustExist ? new BooleanNode(result) : new BooleanNode(!result);
    }

    public override IValuedNode ProcessBNodeFunction(BNodeFunction bNode, PullEvaluationContext context,
        ExpressionContext expressionContext)
    {
        if (bNode.InnerExpression == null)
        {
            return context.NodeFactory.CreateBlankNode().AsValuedNode();
        }

        INode temp = bNode.InnerExpression.Accept(this, context, expressionContext);
        if (temp == null)
        {
            throw new RdfQueryException("Cannot create a Blank Node when the argument Expression evaluates to null");
        }

        if (temp.NodeType != NodeType.Literal)
        {
            throw new RdfQueryException(
                "Cannot create a Blank Node when the argument Expression evaluates to a non-literal node");
        }

        var lit = (ILiteralNode)temp;

        if (lit.DataType != null && lit.DataType.AbsoluteUri != XmlSpecsHelper.XmlSchemaDataTypeString)
        {
            throw new RdfQueryException(
                "Cannot create a Blank Node when the argument Expression evaluates to a literal node typed as anything other than xsd:string");
        }

        if (expressionContext.TryGetBlankNode(lit.Value, out IBlankNode? mappedBlankNode))
        {
            return mappedBlankNode.AsValuedNode();
        }

        IBlankNode newBlankNode = context.NodeFactory.CreateBlankNode();
        expressionContext.MapBlankNode(lit.Value, newBlankNode);
        return newBlankNode.AsValuedNode();
    }

    public override IValuedNode ProcessIriFunction(IriFunction iri, PullEvaluationContext context, ExpressionContext expressionContext)
    {
        IValuedNode result = iri.InnerExpression.Accept(this, context, expressionContext);
        if (result == null)
        {
            throw new RdfQueryException("Cannot create an IRI from a null");
        }

        if (result is ILiteralNode lit)
        {
            var baseUri = string.Empty;
            if (context.BaseUri != null)
            {
                baseUri = context.BaseUri.AbsoluteUri;
            }

            if (lit.DataType == null ||
                lit.DataType.AbsoluteUri.Equals(XmlSpecsHelper.XmlSchemaDataTypeString, StringComparison.Ordinal))
            {
                var uri = Tools.ResolveUri(lit.Value, baseUri);
                return new UriNode(context.UriFactory.Create(uri));
            }

            throw new RdfQueryException("Cannot create an IRI from a non-string typed literal");
        }

        if (result is IUriNode)
        {
            return result;
        }

        throw new RdfQueryException("Cannot create an IRI from a non-URI/string literal.");
    }

    public override IValuedNode ProcessRandFunction(RandFunction rand, PullEvaluationContext context, ExpressionContext expressionContext)
    {
        // The assumption here is that this method should only ever be invoked once per RandFunction/ExpressionContext pair and so we don't need to track generated values in the expression context
        return new DoubleNode(_rnd.NextDouble());
    }
}