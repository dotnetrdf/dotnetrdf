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
    /// Interface for SPARQL Aggregates which can be used to calculate aggregates over Results
    /// </summary>
    public interface ISparqlAggregate
    {
        /// <summary>
        /// Applies the Aggregate to the Result Binder and returns a single Node as a Result
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        INode Apply(SparqlEvaluationContext context);

        /// <summary>
        /// Applies the Aggregate to the Result Binder and returns a single Node as a Result
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingIDs">Enumerable of Binding IDs which the aggregate is applied over</param>
        INode Apply(SparqlEvaluationContext context, IEnumerable<int> bindingIDs);

        /// <summary>
        /// Gets the Expression that the Aggregate is applied to
        /// </summary>
        ISparqlExpression Expression
        {
            get;
        }

        /// <summary>
        /// Gets the Type of the Aggregate
        /// </summary>
        SparqlExpressionType Type
        {
            get;
        }

        /// <summary>
        /// Gets the URI/Keyword of the Aggregate
        /// </summary>
        String Functor
        {
            get;
        }

        /// <summary>
        /// Gets the Arguments of the Aggregate
        /// </summary>
        IEnumerable<ISparqlExpression> Arguments
        {
            get;
        }
    }
}
