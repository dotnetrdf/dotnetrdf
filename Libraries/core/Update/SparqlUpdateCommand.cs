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
        private bool _optimised = false;

        /// <summary>
        /// Creates a new SPARQL Update Command
        /// </summary>
        /// <param name="type">Command Type</param>
        public SparqlUpdateCommand(SparqlUpdateCommandType type)
            : this(type, false) { }

        /// <summary>
        /// Creates a new SPARQL Update Command
        /// </summary>
        /// <param name="type">Command Type</param>
        /// <param name="optimised">Whether the Command is optimised</param>
        public SparqlUpdateCommand(SparqlUpdateCommandType type, bool optimised)
        {
            this._type = type;
            this._optimised = optimised;
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
        /// Gets whether this Command is optimised
        /// </summary>
        public bool IsOptimised
        {
            get 
            {
                return this._optimised;
            }
            internal set
            {
                this._optimised = value;
            }
        }

        /// <summary>
        /// Optimises the Command
        /// </summary>
        public virtual void Optimise()
        {
            //Does Nothing by Default except set the Optimised Flag to true if it wasn't already
            this._optimised = true;
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
