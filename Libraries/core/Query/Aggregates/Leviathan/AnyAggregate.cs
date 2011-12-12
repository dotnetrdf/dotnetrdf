using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Nodes;

namespace VDS.RDF.Query.Aggregates.Leviathan
{
    /// <summary>
    /// A Custom aggregate which requires the Expression to evaluate to true for at least one of the Sets in the Group
    /// </summary>
    public class AnyAggregate
        : BaseAggregate
    {
        /// <summary>
        /// Creates a new Any Aggregate
        /// </summary>
        /// <param name="expr">Expression</param>
        public AnyAggregate(ISparqlExpression expr)
            : this(expr, false) { }

        /// <summary>
        /// Creates a new Any Aggregate
        /// </summary>
        /// <param name="expr">Expression</param>
        /// <param name="distinct">Whether a DISTINCT modifer applies</param>
        public AnyAggregate(ISparqlExpression expr, bool distinct)
            : base(expr, distinct) { }

        /// <summary>
        /// Applies the Aggregate to see if the expression evaluates true for any member of the Group
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingIDs">Binding IDs</param>
        /// <returns></returns>
        /// <remarks>
        /// Does lazy evaluation - as soon as it encounters a true it will return true
        /// </remarks>
        public override IValuedNode Apply(SparqlEvaluationContext context, IEnumerable<int> bindingIDs)
        {
            foreach (int id in bindingIDs)
            {
                try
                {
                    if (this._expr.Evaluate(context, id).AsSafeBoolean())
                    {
                        //As soon as we see a True we can return true
                        return new BooleanNode(null, true);
                    }
                }
                catch (RdfQueryException)
                {
                    //Ignore errors in an any
                }
            }

            //If we don't see any Trues we return false
            return new BooleanNode(null, false);
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
            output.Append(LeviathanFunctionFactory.Any);
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
                return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.Any;
            }
        }
    }
}
