using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Aggregates.Leviathan
{
    /// <summary>
    /// A Custom aggregate which requires the Expression to evaluate to false/error for all Sets in the Group
    /// </summary>
    public class NoneAggregate
        : BaseAggregate
    {
        /// <summary>
        /// Creates a new None Aggregate
        /// </summary>
        /// <param name="expr">Expression</param>
        public NoneAggregate(ISparqlExpression expr)
            : this(expr, false) { }

        /// <summary>
        /// Creates a new None Aggregate
        /// </summary>
        /// <param name="expr">Expression</param>
        /// <param name="distinct">Whether a DISTINCT modifer applies</param>
        public NoneAggregate(ISparqlExpression expr, bool distinct)
            : base(expr, distinct) { }

        /// <summary>
        /// Applies the Aggregate to see if the expression evaluates false/error for every member of the Group
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingIDs">Binding IDs</param>
        /// <returns></returns>
        /// <remarks>
        /// Does lazy evaluation - as soon as it encounters a true it will return false
        /// </remarks>
        public override IValuedNode Apply(SparqlEvaluationContext context, IEnumerable<int> bindingIDs)
        {
            foreach (int id in bindingIDs)
            {
                try
                {
                    if (this._expr.Evaluate(context, id).AsSafeBoolean())
                    {
                        //As soon as we see a true we can return false
                        return new BooleanNode(null, false);
                    }
                }
                catch (RdfQueryException)
                {
                    //An error is a failure so we keep going
                }
            }

            //If everything is false then we return true;
            return new BooleanNode(null, true);
        }

        /// <summary>
        /// Gets the String Representation of the Aggregate
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.Append('<');
            output.Append(LeviathanFunctionFactory.LeviathanFunctionsNamespace);
            output.Append(LeviathanFunctionFactory.None);
            output.Append(">(");
            if (this._distinct) output.Append("DISTINCT ");
            output.Append(this._expr.ToString());
            output.Append(')');
            return output.ToString();
        }

        /// <summary>
        /// Gets the Functor of the Aggregate
        /// </summary>
        public override string Functor
        {
            get
            {
                return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.None;
            }
        }
    }
}
