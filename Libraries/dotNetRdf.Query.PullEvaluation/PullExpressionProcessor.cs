using dotNetRdf.Query.PullEvaluation;
using VDS.RDF;
using VDS.RDF.Nodes;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Functions.Sparql.Boolean;
using VDS.RDF.Query.Expressions.Functions.Sparql.Constructor;
using VDS.RDF.Query.Expressions.Functions.Sparql.Numeric;
using VDS.RDF.Query.Expressions.Primary;

internal class PullExpressionProcessor : BaseExpressionProcessor<PullEvaluationContext, ISet>
{
    public PullExpressionProcessor(ISparqlNodeComparer nodeComparer, IUriFactory uriFactory, bool useStrictOperators) : base(nodeComparer, uriFactory, useStrictOperators)
    {
    }

    protected override IValuedNode GetBoundValue(string variableName, PullEvaluationContext context, ISet binding)
    {
        INode value = binding[variableName];
        return value.AsValuedNode();
    }

    protected override IEnumerable<ISparqlCustomExpressionFactory> GetExpressionFactories(PullEvaluationContext context)
    {
        throw new NotImplementedException("PullExpressionProcessor.GetExpressionFactories not implemented");
    }

    public override IValuedNode ProcessAggregateTerm(AggregateTerm aggregate, PullEvaluationContext context, ISet binding)
    {
        throw new NotImplementedException("PullExpressionProcessor.ProcessAggregateTerm not implemented");
    }

    public override IValuedNode ProcessExistsFunction(ExistsFunction exists, PullEvaluationContext context, ISet binding)
    {
        // return exists.Accept(context.ExpressionProcessor, context, binding);
        throw new NotImplementedException("PullExpressionProcessor.ProcessExistsFunction not implemented");
    }

    public override IValuedNode ProcessBNodeFunction(BNodeFunction bNode, PullEvaluationContext context, ISet binding)
    {
        throw new NotImplementedException("PullExpressionProcessor.ProcessBNodeFunction not implemented");
    }

    public override IValuedNode ProcessIriFunction(IriFunction iri, PullEvaluationContext context, ISet binding)
    {
        throw new NotImplementedException("PullExpressionProcessor.ProcessIriFunction not implemented");
    }

    public override IValuedNode ProcessRandFunction(RandFunction rand, PullEvaluationContext context, ISet binding)
    {
        throw new NotImplementedException("PullExpressionProcessor.ProcessRandFunction not implemented");
    }
}