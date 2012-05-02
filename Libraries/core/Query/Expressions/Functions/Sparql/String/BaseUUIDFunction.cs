using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Expressions.Functions.Sparql.String
{
    /// <summary>
    /// Abstract Base Class for functions that generate UUIDs
    /// </summary>
    public abstract class BaseUUIDFunction
        : ISparqlExpression
    {

        public virtual IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            Guid uuid = Guid.NewGuid();
            return this.EvaluateInternal(uuid);
        }

        protected abstract IValuedNode EvaluateInternal(Guid uuid);

        public virtual IEnumerable<string> Variables
        {
            get
            {
                return Enumerable.Empty<System.String>(); 
            }
        }

        public virtual SparqlExpressionType Type
        {
            get
            { 
                return SparqlExpressionType.Function; 
            }
        }

        public abstract string Functor
        {
            get;
        }

        public virtual IEnumerable<ISparqlExpression> Arguments
        {
            get 
            { 
                return Enumerable.Empty<ISparqlExpression>();
            }
        }

        public virtual ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return this;
        }

        public virtual bool CanParallelise
        {
            get 
            {
                return true;
            }
        }
    }
}
