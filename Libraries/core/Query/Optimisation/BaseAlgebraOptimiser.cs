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
using VDS.RDF.Query.Algebra;
using VDS.RDF.Update;

namespace VDS.RDF.Query.Optimisation
{

    /// <summary>
    /// Abstract Base Class for Algebra Transformers where the Transformer may care about the depth of the Algebra in the Algebra Tree
    /// </summary>
    public abstract class BaseAlgebraOptimiser : IAlgebraOptimiser
    {
        /// <summary>
        /// Attempts to optimise an Algebra to another more optimal form
        /// </summary>
        /// <param name="algebra">Algebra</param>
        /// <returns></returns>
        public virtual ISparqlAlgebra Optimise(ISparqlAlgebra algebra)
        {
            return this.OptimiseInternal(algebra, 0);
        }

        /// <summary>
        /// Transforms the Algebra to another form tracking the depth in the Algebra tree
        /// </summary>
        /// <param name="algebra">Algebra</param>
        /// <param name="depth">Depth</param>
        /// <returns></returns>
        protected abstract ISparqlAlgebra OptimiseInternal(ISparqlAlgebra algebra, int depth);

        /// <summary>
        /// Determines whether the Optimiser can be applied to a given Query
        /// </summary>
        /// <param name="q">Query</param>
        /// <returns></returns>
        public abstract bool IsApplicable(SparqlQuery q);

        /// <summary>
        /// Determines whether the Optimiser can be applied to a given Update Command Set
        /// </summary>
        /// <param name="cmds">Command Set</param>
        /// <returns></returns>
        public abstract bool IsApplicable(SparqlUpdateCommandSet cmds);
    }
}
