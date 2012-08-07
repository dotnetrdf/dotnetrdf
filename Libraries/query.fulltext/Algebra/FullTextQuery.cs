using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.FullText.Search;
using VDS.RDF.Query.PropertyFunctions;

namespace VDS.RDF.Query.Algebra
{
    /// <summary>
    /// Algebra Operator which provides full text query capabilities for a query
    /// </summary>
    /// <remarks>
    /// The evaluation of this operator simply registers the search provider with the Evaluation Context such that any <see cref="FullTextMatchPropertyFunction"/> instances are honoured
    /// </remarks>
    public class FullTextQuery
        : IUnaryOperator
    {
        private IFullTextSearchProvider _provider;

        public FullTextQuery(IFullTextSearchProvider searchProvider, ISparqlAlgebra algebra)
        {
            this._provider = searchProvider;
            this.InnerAlgebra = algebra;
        }

        public ISparqlAlgebra InnerAlgebra
        {
            get;
            private set;
        }

        public ISparqlAlgebra Transform(Optimisation.IAlgebraOptimiser optimiser)
        {
            return new FullTextQuery(this._provider, optimiser.Optimise(this.InnerAlgebra));
        }

        public BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            context[FullTextHelper.ContextKey] = this._provider;
            return context.Evaluate(this.InnerAlgebra);
        }

        public IEnumerable<string> Variables
        {
            get 
            {
                return this.InnerAlgebra.Variables;
            }
        }

        public SparqlQuery ToQuery()
        {
            return this.InnerAlgebra.ToQuery();
        }

        public Patterns.GraphPattern ToGraphPattern()
        {
            return this.InnerAlgebra.ToGraphPattern();
        }

        public override string ToString()
        {
            return "FullTextQuery(" + this.InnerAlgebra.ToString() + ")";
        }
    }
}
