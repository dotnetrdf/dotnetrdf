using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query
{
    public interface ISparqlQueryPatternProcessor<TResult, TContext>
    {
        TResult ProcessTriplePattern(ITriplePattern pattern, TContext context);

        TResult ProcessMatchPattern(IMatchTriplePattern match, TContext context);

        TResult ProcessFilterPattern(IFilterPattern filter, TContext context);

        TResult ProcessAssignmentPattern(IAssignmentPattern assignment, TContext context);

        TResult ProcessPathPattern(IPropertyPathPattern path, TContext context);

        TResult ProcessFunctionPattern(IPropertyFunctionPattern function, TContext context);

        TResult ProcessSubQueryPattern(ISubQueryPattern subquery, TContext context);

    }
}
