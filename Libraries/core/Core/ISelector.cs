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

namespace VDS.RDF
{
    /// <summary>
    /// Interface for defining arbitrary Selectors for Selecting Nodes and Triples from Graphs
    /// </summary>
    /// <typeparam name="T">Type to perform Selection upon</typeparam>
    public interface ISelector<T>
    {
        /// <summary>
        /// Method which determines whether an Object of the given Type is accepted by this Selector
        /// </summary>
        /// <param name="obj">Object to test</param>
        /// <returns></returns>
        bool Accepts(T obj);
    }

    /// <summary>
    /// Interface for defining arbitrary Selectors which are dependant on the Results of a previous Selector
    /// </summary>
    /// <typeparam name="T">Type to perform Selection upon</typeparam>
    public interface IDependentSelector<T> : ISelector<T>
    {
        /// <summary>
        /// Method used to initialise this Selector with the results of the previous Selector
        /// </summary>
        /// <param name="input"></param>
        void Initialise(IEnumerable<T> input);
    }
}
