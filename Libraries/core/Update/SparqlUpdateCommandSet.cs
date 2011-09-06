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
using System.Diagnostics;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Optimisation;

namespace VDS.RDF.Update
{
    /// <summary>
    /// Represents a sequence of SPARQL Update Commands to be executed on a Dataset
    /// </summary>
    public class SparqlUpdateCommandSet
    {
        private List<SparqlUpdateCommand> _commands = new List<SparqlUpdateCommand>();
        private NamespaceMapper _nsmap = new NamespaceMapper(true);
        private Uri _baseUri;
        private long _timeout = 0;
        private TimeSpan? _executionTime = null;
        private IEnumerable<IAlgebraOptimiser> _optimisers = Enumerable.Empty<IAlgebraOptimiser>();

        /// <summary>
        /// Creates a new empty Command Set
        /// </summary>
        public SparqlUpdateCommandSet()
        {

        }

        /// <summary>
        /// Creates a new Command Set containing the given Command
        /// </summary>
        /// <param name="command">Command</param>
        public SparqlUpdateCommandSet(SparqlUpdateCommand command)
        {
            this._commands.Add(command);
        }

        /// <summary>
        /// Creates a new Command Set with the given Commands
        /// </summary>
        /// <param name="commands">Commands</param>
        public SparqlUpdateCommandSet(IEnumerable<SparqlUpdateCommand> commands)
        {
            this._commands.AddRange(commands);
        }

        /// <summary>
        /// Adds a new Command to the end of the sequence of Commands
        /// </summary>
        /// <param name="command">Command to add</param>
        internal void AddCommand(SparqlUpdateCommand command)
        {
            this._commands.Add(command);
        }

        /// <summary>
        /// Gets the Command at the given index
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns></returns>
        public SparqlUpdateCommand this[int index]
        {
            get
            {
                if (index < 0 || index >= this._commands.Count)
                {
                    if (this._commands.Count > 0)
                    {
                        throw new IndexOutOfRangeException(index + " is not a valid index into the Command Set, the Set contains " + this._commands.Count + " Commands so only indexes in the range 0-" + (this._commands.Count - 1) + " are valid");
                    }
                    else
                    {
                        throw new IndexOutOfRangeException(index + " is not a valid index into the Command Set since it is an empty Command Set");
                    }
                }
                else
                {
                    return this._commands[index];
                }
            }
        }

        /// <summary>
        /// Gets the number of Commands in the set
        /// </summary>
        public int CommandCount
        {
            get
            {
                return this._commands.Count;
            }
        }

        /// <summary>
        /// Gets the enumeration of Commands in the set
        /// </summary>
        public IEnumerable<SparqlUpdateCommand> Commands
        {
            get
            {
                return this._commands;
            }
        }

        /// <summary>
        /// Gets/Sets the Base URI for the Command Set
        /// </summary>
        public Uri BaseUri
        {
            get
            {
                return this._baseUri;
            }
            internal set
            {
                this._baseUri = value;
            }
        }

        /// <summary>
        /// Gets the Namespace Map for the Command Set
        /// </summary>
        public NamespaceMapper NamespaceMap
        {
            get
            {
                return this._nsmap;
            }
        }

        /// <summary>
        /// Gets/Sets the Timeout for the execution of the Updates
        /// </summary>
        public long Timeout
        {
            get
            {
                return this._timeout;
            }
            set
            {
                this._timeout = Math.Max(value, 0);
            }
        }

        /// <summary>
        /// Gets/Sets the Time the updates took to execute
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if you try to inspect the execution time before/during the execution of updates</exception>
        public TimeSpan? UpdateExecutionTime
        {
            get
            {
                if (this._executionTime == null)
                {
                    throw new InvalidOperationException("Cannot inspect the Update Time as the Command Set has not yet been processed");
                }
                else
                {
                    return this._executionTime;
                }
            }
            set
            {
                this._executionTime = value;
            }
        }

        /// <summary>
        /// Gets/Sets the Algebra Optimisers to be applied to portions of updates that require queries to be made
        /// </summary>
        public IEnumerable<IAlgebraOptimiser> AlgebraOptimisers
        {
            get
            {
                return this._optimisers;
            }
            set
            {
                if (value == null)
                {
                    this._optimisers = Enumerable.Empty<IAlgebraOptimiser>();
                }
                else
                {
                    this._optimisers = value;
                }
            }
        }

        /// <summary>
        /// Optimises the Commands in the Command Set
        /// </summary>
        /// <param name="optimiser">Optimiser to use</param>
        public void Optimise(IQueryOptimiser optimiser)
        {
            foreach (SparqlUpdateCommand c in this._commands)
            {
                c.Optimise(optimiser);
            }
        }

        /// <summary>
        /// Optimises the Commands in the Command Set
        /// </summary>
        /// <remarks>Uses the globally registered query optimiser from <see cref="SparqlOptimiser.QueryOptimiser">SparqlOptimiser.QueryOptimiser</see></remarks>
        public void Optimise()
        {
            foreach (SparqlUpdateCommand c in this._commands)
            {
                c.Optimise(SparqlOptimiser.QueryOptimiser);
            }
        }

        /// <summary>
        /// Processes the Command Set using the given Update Processor
        /// </summary>
        /// <param name="processor">Update Processor</param>
        public void Process(ISparqlUpdateProcessor processor)
        {
            processor.ProcessCommandSet(this);
        }

        internal ISparqlAlgebra ApplyAlgebraOptimisers(ISparqlAlgebra algebra)
        {
            try
            {
                //Apply Local Optimisers
                foreach (IAlgebraOptimiser opt in this._optimisers.Where(o => o.IsApplicable(this)))
                {
                    try
                    {
                        algebra = opt.Optimise(algebra);
                    }
                    catch
                    {
                        //Ignore errors - if an optimiser errors then we leave the algebra unchanged
                    }
                }
                //Apply Global Optimisers
                foreach (IAlgebraOptimiser opt in SparqlOptimiser.AlgebraOptimisers.Where(o => o.IsApplicable(this)))
                {
                    try
                    {
                        algebra = opt.Optimise(algebra);
                    }
                    catch
                    {
                        //Ignore errors - if an optimiser errors then we leave the algebra unchanged
                    }
                }
                return algebra;
            }
            catch
            {
                return algebra;
            }
        }

        /// <summary>
        /// Gets the String representation of the Command Set
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();

            if (this._baseUri != null)
            {
                output.AppendLine("BASE <" + this._baseUri.ToString().Replace(">", "\\>") + ">");
            }

            foreach (String prefix in this._nsmap.Prefixes)
            {
                output.Append("PREFIX ");
                output.Append(prefix);
                output.Append(": <");
                output.Append(this._nsmap.GetNamespaceUri(prefix).ToString().Replace(">", "\\>"));
                output.AppendLine(">");
            }

            for (int i = 0; i < this._commands.Count; i++)
            {
                output.Append(this._commands[i].ToString());
                if (i < this._commands.Count - 1)
                {
                    output.AppendLine(";");
                }
            }
            return output.ToString();
        }
    }
}
