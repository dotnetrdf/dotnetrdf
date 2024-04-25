using dotNetRdf.Query.PullEvaluation;
using System.Xml.XPath;
using VDS.RDF;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Functions.Sparql.Boolean;
using VDS.RDF.Query.Expressions.Functions.Sparql.Constructor;
using VDS.RDF.Query.Expressions.Functions.Sparql.Numeric;
using VDS.RDF.Query.Expressions.Primary;

namespace dotNetRdf.Query.PullEvaluation;

internal class PullExpressionProcessor : BaseExpressionProcessor<PullEvaluationContext, ExpressionContext>
{
    public PullExpressionProcessor(ISparqlNodeComparer nodeComparer, IUriFactory uriFactory, bool useStrictOperators) :
        base(nodeComparer, uriFactory, useStrictOperators)
    {
    }

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

        if (expressionContext.TryGetBlankNode(lit.Value, out IBlankNode mappedBlankNode))
        {
            return mappedBlankNode.AsValuedNode();
        }

        IBlankNode newBlankNode = context.NodeFactory.CreateBlankNode();
        expressionContext.MapBlankNode(lit.Value, newBlankNode);
        return newBlankNode.AsValuedNode();
    }

    public override IValuedNode ProcessIriFunction(IriFunction iri, PullEvaluationContext context, ExpressionContext expressionContext)
    {
        throw new NotImplementedException("PullExpressionProcessor.ProcessIriFunction not implemented");
    }

    public override IValuedNode ProcessRandFunction(RandFunction rand, PullEvaluationContext context, ExpressionContext expressionContext)
    {
        throw new NotImplementedException("PullExpressionProcessor.ProcessRandFunction not implemented");
    }
}