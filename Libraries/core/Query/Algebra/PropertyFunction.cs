/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

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
