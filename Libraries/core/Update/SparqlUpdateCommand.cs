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
using VDS.RDF.Query.Optimisation;

namespace VDS.RDF.Update
{
    /// <summary>
    /// Possible SPARQL Update Command Types
    /// </summary>
    public enum SparqlUpdateCommandType
    {
        /// <summary>
        /// Command inserts data
        /// </summary>
        InsertData,
        /// <summary>
        /// Command deletes data
        /// </summary>
        DeleteData,
        /// <summary>
        /// Command inserts data and may be based upon a template
        /// </summary>
        Insert,
        /// <summary>
        /// Command deletes data and may be based upon a template
        /// </summary>
        Delete,
        /// <summary>
        /// Command modifies data
        /// </summary>
        Modify,
        /// <summary>
        /// Command loads a graph into the Store
        /// </summary>
        Load,
        /// <summary>
        /// Command clears a graph in the Store
        /// </summary>
        Clear,
        /// <summary>
        /// Command creates a Graph in the Store
        /// </summary>
        Create,
        /// <summary>
        /// Command removes a Graph from the Store
        /// </summary>
        Drop,
        /// <summary>
        /// Command which merges the data from one Graph into another
        /// </summary>
        Add,
        /// <summary>
        /// Command which copies the data from one Graph into another overwriting the destination Graph
        /// </summary>
        Copy,
        /// <summary>
        /// Command which moves data from one Graph to another overwriting the destination Graph and deleting the Source Graph
        /// </summary>
        Move,
        /// <summary>
        /// Unknown
        /// </summary>
        Unknown
    }

    /// <summary>
    /// Base Class of SPARQL Update Commands
    /// </summary>
    public abstract class SparqlUpdateCommand
    {
        private SparqlUpdateCommandType _type = SparqlUpdateCommandType.Unknown;

        /// <summary>
        /// Creates a new SPARQL Update Command
        /// </summary>
        /// <param name="type">Command Type</param>
        public SparqlUpdateCommand(SparqlUpdateCommandType type)
        {
            this._type = type;
        }

        /// <summary>
        /// Gets the Type of this Command
        /// </summary>
        public SparqlUpdateCommandType CommandType
        {
            get
            {
                return this._type;
            }
        }

        /// <summary>
        /// Gets whether the Command will only affect a single Graph
        /// </summary>
        public abstract bool AffectsSingleGraph
        {
            get;
        }

        /// <summary>
        /// Gets whether the Command will potentially affect the given Graph
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        /// <remarks>
        /// A return value of <strong>true</strong> does not guarantee that the Graph will be affected.  Some Commands (e.g. DROP ALL) affect all Graphs in the Dataset but the command itself doesn't know whether a Graph with the given URI is actually present in the dataset to which it is applied
        /// </remarks>
        public abstract bool AffectsGraph(Uri graphUri);

        /// <summary>
        /// Optimises the Command
        /// </summary>
        public virtual void Optimise(IQueryOptimiser optimiser)
        {
            //Does Nothing by Default
        }

        /// <summary>
        /// Evaluates the Command in the given Context
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        public abstract void Evaluate(SparqlUpdateEvaluationContext context);

        /// <summary>
        /// Processes the Command Set using the given Update Processor
        /// </summary>
        /// <param name="processor">Update Processor</param>
        public abstract void Process(ISparqlUpdateProcessor processor);

        /// <summary>
        /// Gets the String representation of the Command
        /// </summary>
        /// <returns></returns>
        public abstract override string ToString();
    }
}
