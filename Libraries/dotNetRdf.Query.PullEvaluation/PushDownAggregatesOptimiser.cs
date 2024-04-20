using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Primary;
using VDS.RDF.Query.Filters;
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Update;

namespace dotNetRdf.Query.PullEvaluation;

/// <summary>
/// Transforms a query algebra for the async pull evaluation processor by ensuring that all aggregate expressions
/// in SELECT and HAVING clauses are pushed down to the GROUP BY so that they can all be evaluated together
/// and then propagated up as temporary variable bindings
/// </summary>
public class PushDownAggregatesOptimiser : IAlgebraOptimiser, IExpressionTransformer
{
    private bool _isUnsafe = false;
    private int _nextPrefixId = 0;
    private readonly List<SparqlVariable> _pushedDownAggregates = [];

    /// <summary>
    /// Gets the prefix assigned to the variables that pushed down aggreagation expressions are assigned to.
    /// </summary>
    public string AutoVarPrefix { get; private set; }

    public PushDownAggregatesOptimiser(string varPrefix)
    {
        AutoVarPrefix = varPrefix;
    }

    public ISparqlAlgebra Optimise(ISparqlAlgebra algebra)
    {
        if (AutoVarPrefix == String.Empty)
        {
            AutoVarPrefix = GetAutoVarPrefix(algebra.Variables.ToList());
        }
        switch (algebra)
        {
            case GroupBy groupBy when _pushedDownAggregates.Any():
                var aggregates = groupBy.Aggregates.Concat(_pushedDownAggregates).ToList();
                var ret = new GroupBy(groupBy.InnerAlgebra, groupBy.Grouping, aggregates);
                return ret;
            case Having having:
                ISparqlFilter transformedHavingClause = Transform(having.HavingClause);
                return new Having(Optimise(having.InnerAlgebra), transformedHavingClause);
            case Extend extend:
                ISparqlExpression transformedExpression = extend.AssignExpression.Transform(this);
                return new Extend(Optimise(extend.InnerAlgebra), transformedExpression, extend.VariableName);
            case Select select:
                if (select.SparqlVariables.Any(sv => sv.IsAggregate || sv.IsProjection))
                {
                    var transformedSelectVars = select.SparqlVariables.Select(Transform).ToList();
                    return new Select(Optimise(select.InnerAlgebra), select.IsSelectAll, transformedSelectVars);
                }
                return select;
            case Ask ask:
                return new Ask(Optimise(ask.InnerAlgebra));
        }

        return algebra;
    }

    private string GetAutoVarPrefix(IList<string> existingVars)
    {
        var prefix = "_auto";
        while (existingVars.Any(v=>v.StartsWith(prefix)))
        {
            prefix = "_" + prefix;
        }

        return prefix;
    }

    private SparqlVariable Transform(SparqlVariable sv)
    {
        if (sv.IsAggregate)
        {
            var pushedDownVar = NextAutoVar();
            _pushedDownAggregates.Add(new SparqlVariable(pushedDownVar, sv.Aggregate));
            return new SparqlVariable(sv.Name, new VariableTerm(pushedDownVar));
        }

        if (sv.IsProjection)
        {
            return new SparqlVariable(sv.Name, sv.Projection.Transform(this));
        }

        return sv;
    }
    private ISparqlFilter Transform(ISparqlFilter filter)
    {
        return filter switch
        {
            UnaryExpressionFilter u => new UnaryExpressionFilter(u.Expression.Transform(this)),
            ChainFilter chainFilter => new ChainFilter(chainFilter.Filters.Select(Transform)),
            _ => filter
        };
    }
   
    /// <inheritdoc />
    public bool IsApplicable(SparqlQuery q)
    {
        return q.IsAggregate;
    }

    /// <inheritdoc />
    public bool IsApplicable(SparqlUpdateCommandSet cmds)
    {
        return false;
    }

    /// <inheritdoc />
    public bool UnsafeOptimisation { get { return _isUnsafe;} set { _isUnsafe = value; } }

    /// <inheritdoc />
    public ISparqlExpression Transform(ISparqlExpression expr)
    {
        try
        {
            if (expr is AggregateTerm aggregateTerm)
            {
                var pushedDownVar = NextAutoVar();
                _pushedDownAggregates.Add(new SparqlVariable(pushedDownVar, aggregateTerm.Aggregate));
                return new VariableTerm(pushedDownVar);
            }
            return expr.Transform(this);
        }
        catch
        {
            return expr;
        }
    }

    private string NextAutoVar()
    {
        return AutoVarPrefix + _nextPrefixId++;
    }
}