using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra
{
    /// <summary>
    /// Represents a part of the algebra that has been determined to not return any results in advance and so can be replaced with this operator which always returns null
    /// </summary>
    public class NullOperator : ISparqlAlgebra, ITerminalOperator
    {
        private List<String> _vars = new List<string>();

        public NullOperator(IEnumerable<String> variables)
        {
            this._vars.AddRange(variables);
        }

        public BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            return new NullMultiset();
        }

        public IEnumerable<string> Variables
        {
            get 
            {
                return this._vars;
            }
        }

        public SparqlQuery ToQuery()
        {
            throw new RdfQueryException("A NullOperator cannot be transformed back to a Query");
        }

        public GraphPattern ToGraphPattern()
        {
            throw new RdfQueryException("A NullOperator cannot be transformed into a Graph Pattern");
        }

        public override string ToString()
        {
            return "NullOperator()";
        }
    }
}
