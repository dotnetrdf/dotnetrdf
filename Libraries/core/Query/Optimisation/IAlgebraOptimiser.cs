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
    /// An Algebra Optimiser is a class that can transform a SPARQL algebra from one form to another typically for optimisation purposes
    /// </summary>
    public interface IAlgebraOptimiser
    {
        /// <summary>
        /// Optimises the given Algebra
        /// </summary>
        /// <param name="algebra">Algebra to optimise</param>
        /// <returns></returns>
        /// <remarks>
        /// <strong>Important:</strong> An Algebra Optimiser must guarantee to return an equivalent algebra to the given algebra.  In the event of any error the optimiser <em>should</em> still return a valid algebra (or at least the original algebra)
        /// </remarks>
        ISparqlAlgebra Optimise(ISparqlAlgebra algebra);

        /// <summary>
        /// Determines whether an Optimiser is applicable based on the Query whose Algebra is being optimised
        /// </summary>
        /// <param name="q">SPARQL Query</param>
        /// <returns></returns>
        bool IsApplicable(SparqlQuery q);

        /// <summary>
        /// Determines whether an Optimiser is applicable based on the Update Command Set being optimised
        /// </summary>
        /// <param name="cmds">Update Command Set</param>
        /// <returns></returns>
        bool IsApplicable(SparqlUpdateCommandSet cmds);
    }
}
