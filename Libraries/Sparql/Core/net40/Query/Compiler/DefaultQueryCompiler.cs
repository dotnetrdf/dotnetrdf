using System.Linq;
using VDS.RDF.Query.Algebra;

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

            // GROUP BY

            // Project Expressions
            if (query.Projections != null && query.Projections.Any(kvp => kvp.Value != null))
            {
                // TODO Must handle replacing aggregates with their temporary variables
                algebra = Extend.Create(algebra, query.Projections.Where(kvp => kvp.Value != null));
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