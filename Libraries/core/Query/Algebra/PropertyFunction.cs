using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Query.PropertyFunctions;

namespace VDS.RDF.Query.Algebra
{
    /// <summary>
    /// Algebra that represents the application of a Property Function
    /// </summary>
    public class PropertyFunction
        : IUnaryOperator
    {
        private ISparqlPropertyFunction _function;
        private ISparqlAlgebra _algebra;

        public PropertyFunction(ISparqlAlgebra algebra, ISparqlPropertyFunction function)
        {
            this._function = function;
            this._algebra = algebra;
        }

        public ISparqlAlgebra InnerAlgebra
        {
            get 
            {
                return this._algebra;
            }
        }

        public ISparqlAlgebra Transform(IAlgebraOptimiser optimiser)
        {
            return new PropertyFunction(optimiser.Optimise(this._algebra), this._function);
        }

        public BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            context.InputMultiset = context.Evaluate(this._algebra);
            return this._function.Evaluate(context);
        }

        public IEnumerable<string> Variables
        {
            get 
            {
                return this._algebra.Variables.Concat(this._function.Variables).Distinct();
            }
        }

        public SparqlQuery ToQuery()
        {
            throw new NotImplementedException();
        }

        public Patterns.GraphPattern ToGraphPattern()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return "PropertyFunction(" + this._algebra.ToString() + "," + this._function.FunctionUri + ")";
        }
    }
}
