/*

Copyright Robert Vesse 2009-10
rvesse@vdesign-studios.com

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Aggregates
{
    /// <summary>
    /// Abstract Base Class for Aggregate Functions
    /// </summary>
    public abstract class BaseAggregate : ISparqlAggregate
    {
        /// <summary>
        /// Expression that the aggregate operates over
        /// </summary>
        protected ISparqlExpression _expr;
        /// <summary>
        /// Whether a DISTINCT modifer is applied
        /// </summary>
        protected bool _distinct = false;

        /// <summary>
        /// Base Constructor for Aggregates
        /// </summary>
        /// <param name="expr">Expression that the aggregate is over</param>
        public BaseAggregate(ISparqlExpression expr)
        {
            this._expr = expr;
        }

        /// <summary>
        /// Base Constructor for Aggregates
        /// </summary>
        /// <param name="expr">Expression that the aggregate is over</param>
        /// <param name="distinct">Whether a Distinct modifer is applied</param>
        public BaseAggregate(ISparqlExpression expr, bool distinct)
            : this(expr)
        {
            this._distinct = distinct;
        }

        /// <summary>
        /// Applies the Aggregate to the Result Binder
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        public INode Apply(SparqlEvaluationContext context)
        {
            return this.Apply(context, context.Binder.BindingIDs);
        }

        /// <summary>
        /// Applies the Aggregate to the Result Binder
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingIDs">Enumerable of Binding IDs over which the Aggregate applies</param>
        /// <returns></returns>
        public abstract INode Apply(SparqlEvaluationContext context, IEnumerable<int> bindingIDs);

        /// <summary>
        /// Expression that the Aggregate executes over
        /// </summary>
        public ISparqlExpression Expression
        {
            get
            {
                return this._expr;
            }
        }

        /// <summary>
        /// Gets the String representation of the Aggregate
        /// </summary>
        /// <returns></returns>
        public abstract override string ToString();

        /// <summary>
        /// Gets the Type of the Expression
        /// </summary>
        public SparqlExpressionType Type
        {
            get
            {
                return SparqlExpressionType.Aggregate;
            }
        }

        /// <summary>
        /// Gets the Functor of the Aggregate
        /// </summary>
        public abstract String Functor
        {
            get;
        }

        /// <summary>
        /// Gets the Arguments of the Expression
        /// </summary>
        public virtual IEnumerable<ISparqlExpression> Arguments
        {
            get
            {
                return this._expr.AsEnumerable();
            }
        }
    }
}
