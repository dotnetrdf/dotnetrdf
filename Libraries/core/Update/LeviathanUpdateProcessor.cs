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

using VDS.RDF.Update.Commands;

namespace VDS.RDF.Update
{
    /// <summary>
    /// Default SPARQL Update Processor provided by the library's Leviathan SPARQL Engine
    /// </summary>
    /// <remarks>
    /// <para>
    /// The Leviathan Update Processor simply invokes the <see cref="SparqlUpdateCommand.Update">Update</see> method of the SPARQL Commands it is asked to process
    /// </para>
    /// </remarks>
    public class LeviathanUpdateProcessor : ISparqlUpdateProcessor
    {
        private SparqlUpdateEvaluationContext _context;

        /// <summary>
        /// Creates a new Leviathan Update Processor
        /// </summary>
        /// <param name="store">Triple Store</param>
        public LeviathanUpdateProcessor(IInMemoryQueryableStore store)
        {
            this._context = new SparqlUpdateEvaluationContext(store);
            if (!store.HasGraph(null))
            {
                store.Add(new Graph());
            }
        }

        /// <summary>
        /// Processes a CLEAR command
        /// </summary>
        /// <param name="cmd">Clear Command</param>
        public virtual void ProcessClearCommand(ClearCommand cmd)
        {
            cmd.Evaluate(this._context);
        }

        /// <summary>
        /// Processes a CREATE command
        /// </summary>
        /// <param name="cmd">Create Command</param>
        public virtual void ProcessCreateCommand(CreateCommand cmd)
        {
            cmd.Evaluate(this._context);;
        }

        /// <summary>
        /// Processes a command
        /// </summary>
        /// <param name="cmd">Command</param>
        /// <remarks>
        /// Invokes the type specific method for the command type
        /// </remarks>
        public virtual void ProcessCommand(SparqlUpdateCommand cmd)
        {
            switch (cmd.CommandType)
            {
                case SparqlUpdateCommandType.Clear:
                    this.ProcessClearCommand((ClearCommand)cmd);
                    break;
                case SparqlUpdateCommandType.Create:
                    this.ProcessCreateCommand((CreateCommand)cmd);
                    break;
                case SparqlUpdateCommandType.Delete:
                    this.ProcessDeleteCommand((DeleteCommand)cmd);
                    break;
                case SparqlUpdateCommandType.DeleteData:
                    this.ProcessDeleteDataCommand((DeleteDataCommand)cmd);
                    break;
                case SparqlUpdateCommandType.Drop:
                    this.ProcessDropCommand((DropCommand)cmd);
                    break;
                case SparqlUpdateCommandType.Insert:
                    this.ProcessInsertCommand((InsertCommand)cmd);
                    break;
                case SparqlUpdateCommandType.InsertData:
                    this.ProcessInsertDataCommand((InsertDataCommand)cmd);
                    break;
                case SparqlUpdateCommandType.Load:
                    this.ProcessLoadCommand((LoadCommand)cmd);
                    break;
                case SparqlUpdateCommandType.Modify:
                    this.ProcessModifyCommand((ModifyCommand)cmd);
                    break;
                default:
                    throw new SparqlUpdateException("Unknown Update Commands cannot be processed by the Leviathan Update Processor");
            }
        }

        /// <summary>
        /// Processes a command set
        /// </summary>
        /// <param name="commands">Command Set</param>
        /// <remarks>
        /// Invokes <see cref="LeviathanUpdateProcessor.ProcessCommand">ProcessCommand()</see> on each command in turn
        /// </remarks>
        public virtual void ProcessCommandSet(SparqlUpdateCommandSet commands)
        {
            for (int i = 0; i < commands.CommandCount; i++)
            {
                this.ProcessCommand(commands[i]);
            }
        }

        /// <summary>
        /// Processes a DELETE command
        /// </summary>
        /// <param name="cmd">Delete Command</param>
        public virtual void ProcessDeleteCommand(DeleteCommand cmd)
        {
            cmd.Evaluate(this._context);;
        }

        /// <summary>
        /// Processes a DELETE DATA command
        /// </summary>
        /// <param name="cmd">DELETE Data Command</param>
        public virtual void ProcessDeleteDataCommand(DeleteDataCommand cmd)
        {
            cmd.Evaluate(this._context);;
        }

        /// <summary>
        /// Processes a DROP command
        /// </summary>
        /// <param name="cmd">Drop Command</param>
        public virtual void ProcessDropCommand(DropCommand cmd)
        {
            cmd.Evaluate(this._context);;
        }

        /// <summary>
        /// Processes an INSERT command
        /// </summary>
        /// <param name="cmd">Insert Command</param>
        public virtual void ProcessInsertCommand(InsertCommand cmd)
        {
            cmd.Evaluate(this._context);;
        }

        /// <summary>
        /// Processes an INSERT DATA command
        /// </summary>
        /// <param name="cmd">Insert Data Command</param>
        public virtual void ProcessInsertDataCommand(InsertDataCommand cmd)
        {
            cmd.Evaluate(this._context);;
        }

        /// <summary>
        /// Processes a LOAD command
        /// </summary>
        /// <param name="cmd">Load Command</param>
        public virtual void ProcessLoadCommand(LoadCommand cmd)
        {
            cmd.Evaluate(this._context);;
        }

        /// <summary>
        /// Processes an INSERT/DELETE command
        /// </summary>
        /// <param name="cmd">Insert/Delete Command</param>
        public virtual void ProcessModifyCommand(ModifyCommand cmd)
        {
            cmd.Evaluate(this._context);;
        }
    }
}
