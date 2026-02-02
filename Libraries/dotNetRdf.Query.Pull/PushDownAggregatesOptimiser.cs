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

using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Primary;
using VDS.RDF.Query.Filters;
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Update;

namespace VDS.RDF.Query.Pull;

/// <summary>
/// Transforms a query algebra for the async pull evaluation processor by ensuring that all aggregate expressions
/// in SELECT and HAVING clauses are pushed down to the GROUP BY so that they can all be evaluated together
/// and then propagated up as temporary variable bindings
/// </summary>
public class PushDownAggregatesOptimiser(string varPrefix) : IAlgebraOptimiser, IExpressionTransformer
{
    private bool _isUnsafe = false;
    private int _nextPrefixId = 0;
    private readonly List<SparqlVariable> _pushedDownAggregates = [];

    /// <summary>
    /// Gets the prefix assigned to the variables that pushed down aggreagation expressions are assigned to.
    /// </summary>
    public string AutoVarPrefix { get; private set; } = varPrefix;

    /// <inheritdoc />
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
            case OrderBy orderBy:
                return new OrderBy(orderBy.Transform(this), orderBy.Ordering);
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