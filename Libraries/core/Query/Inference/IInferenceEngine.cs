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

namespace VDS.RDF.Query.Inference
{
    /// <summary>
    /// Interfaces for Inference Engines
    /// </summary>
    /// <remarks>
    /// <para>
    /// An Inference Engine is a class that given a Graph can infer extra information from that Graph based on fixed rules or rules computed from the Graphs it is performing inference on
    /// </para>
    /// <para>
    /// In general terms an implementation of an Inference Engine typically provides some form of forward chaining reasoner though implementations may do more advanced reasoning or wrap other kinds of reasoner.
    /// </para>
    /// </remarks>
    public interface IInferenceEngine
    {
        /// <summary>
        /// Applies inference to the given Graph and outputs the inferred information to that Graph
        /// </summary>
        /// <param name="g">Graph</param>
        void Apply(IGraph g);

        /// <summary>
        /// Applies inference to the Input Graph and outputs the inferred information to the Output Graph
        /// </summary>
        /// <param name="input">Graph to apply inference to</param>
        /// <param name="output">Graph inferred information is output to</param>
        void Apply(IGraph input, IGraph output);

        /// <summary>
        /// Initialises the Inference Engine using the given Graph
        /// </summary>
        /// <param name="g">Graph to initialise from</param>
        void Initialise(IGraph g);
    }
}
