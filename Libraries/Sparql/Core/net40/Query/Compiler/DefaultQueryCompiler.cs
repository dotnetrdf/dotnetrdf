using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Primary;
using VDS.RDF.Query.Expressions.Transforms;
using VDS.RDF.Query.Expressions.Visitors;

namespace VDS.RDF.Query.Compiler
{
    public class DefaultQueryCompiler
        : IQueryCompiler
    {
        public virtual IAlgebra Compile(IQuery query)
        {
            IAlgebra algebra;

            // Firstly visit the where clause
            if (query.WhereClause != null)
            {
                CompilingElementVisitor visitor = new CompilingElementVisitor();
                algebra = visitor.Compile(query.WhereClause);
            }
            else
            {
                // For an empty where clause start from table unit
                algebra = Table.CreateUnit();
            }

            // Then visit the modifiers in the appropriate order

            // Extract out any aggregators
            List<KeyValuePair<String, IExpression>> projections = new List<KeyValuePair<String, IExpression>>();
            ISet<IAggregateExpression> aggregates = new HashSet<IAggregateExpression>();
            IDictionary<IExpression, IExpression> aggSubstitutions = new Dictionary<IExpression, IExpression>();
            if (query.Projections != null)
            {
                // Collect the aggregates and generate the temporary variables for them
                CollectAggregatesVisitor collector = new CollectAggregatesVisitor();
                aggregates = collector.Collect(query.Projections.Where(kvp => kvp.Value != null).Select(kvp => kvp.Value));
                if (aggregates.Count > 0)
                {
                    long next = 0;
                    foreach (IAggregateExpression agg in aggregates)
                    {
                        aggSubstitutions.Add(agg, new VariableTerm("." + next));
                        next++;
                    }
                }
                ExprTransformMultipleSubstitute exprTransformer = new ExprTransformMultipleSubstitute(aggSubstitutions);

                // Build the projections substituting temporary variables for aggregates where necessary
                foreach (KeyValuePair<String, IExpression> kvp in query.Projections)
                {
                    if (kvp.Value == null || aggregates.Count == 0)
                    {
                        projections.Add(kvp);
                    }
                    else
                    {
                        // Need to substitute any aggregates for variables
                        ApplyExpressionTransformVisitor visitor = new ApplyExpressionTransformVisitor(exprTransformer);
                        projections.Add(new KeyValuePair<string, IExpression>(kvp.Key, visitor.Transform(kvp.Value)));
                    }
                }
            }

            // GROUP BY
            if (query.GroupExpressions != null || aggregates.Count > 0)
            {
                // TODO Produce GroupBy clause
                if (query.GroupExpressions != null)
                {
                    // TODO Group By Expressions
                }
                else
                {
                    // TODO Default Grouping
                }                
            }

            // Project Expressions
            if (projections.Count > 0)
            {
                algebra = Extend.Create(algebra, projections);
            }

            // HAVING
            if (query.HavingConditions != null && query.HavingConditions.Any())
            {
                algebra = Filter.Create(algebra, query.HavingConditions);
            }

            // VALUES
            if (query.ValuesClause != null)
            {
                algebra = Join.Create(algebra, new Table(CompilingElementVisitor.CompileInlineData(query.ValuesClause)));
            }

            // ORDER BY
            if (query.SortConditions != null && query.SortConditions.Any())
            {
                algebra = new OrderBy(algebra, query.SortConditions);
            }

            // PROJECT
            if (query.Projections != null && query.Projections.Any(kvp => kvp.Value == null))
            {
                algebra = new Project(algebra, query.Projections.Where(kvp => kvp.Value == null).Select(kvp => kvp.Key));
            }

            // DISTINCT/REDUCED
            switch (query.QueryType)
            {
                case QueryType.SelectAllDistinct:
                case QueryType.SelectDistinct:
                    algebra = new Distinct(algebra);
                    break;
                case QueryType.SelectAllReduced:
                case QueryType.SelectReduced:
                    algebra = new Reduced(algebra);
                    break;
            }

            // LIMIT and OFFSET
            if (query.HasLimit || query.HasOffset)
            {
                algebra = new Slice(algebra, query.Limit, query.Offset);
            }

            // Return the final algebra
            return algebra;
        }
    }
}