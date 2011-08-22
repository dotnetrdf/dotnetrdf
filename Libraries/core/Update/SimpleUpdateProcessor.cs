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
using VDS.RDF.Update.Commands;

namespace VDS.RDF.Update
{
    /// <summary>
    /// SPARQL Update Processor which processes updates by handing them off to the <see cref="IUpdateableTripleStore.ExecuteUpdate">ExecuteUpdate()</see> method of an <see cref="IUpdateableTripleStore">IUpdateableTripleStore</see>
    /// </summary>
    public class SimpleUpdateProcessor : ISparqlUpdateProcessor
    {
        private IUpdateableTripleStore _store;

        /// <summary>
        /// Creates a new Simple Update Processor
        /// </summary>
        /// <param name="store">Updateable Triple Store</param>
        public SimpleUpdateProcessor(IUpdateableTripleStore store)
        {
            this._store = store;
        }

        /// <summary>
        /// Discards any outstanding changes
        /// </summary>
        public virtual void Discard()
        {
            //Has no effect
        }

        /// <summary>
        /// Flushes any outstanding changes to the underlying store
        /// </summary>
        public virtual void Flush()
        {
            if (this._store is ITransactionalStore)
            {
                ((ITransactionalStore)this._store).Flush();
            }
        }

        /// <summary>
        /// Processes an ADD command
        /// </summary>
        /// <param name="cmd">Add Command</param>
        public void ProcessAddCommand(AddCommand cmd)
        {
            this.ProcessCommand(cmd);
        }

        /// <summary>
        /// Processes a CLEAR command
        /// </summary>
        /// <param name="cmd">Clear Command</param>
        public void ProcessClearCommand(ClearCommand cmd)
        {
            this.ProcessCommand(cmd);
        }

        /// <summary>
        /// Processes a COPY command
        /// </summary>
        /// <param name="cmd">Copy Command</param>
        public void ProcessCopyCommand(CopyCommand cmd)
        {
            this.ProcessCommand(cmd);
        }

        /// <summary>
        /// Processes a CREATE command
        /// </summary>
        /// <param name="cmd">Create Command</param>
        public void ProcessCreateCommand(CreateCommand cmd)
        {
            this.ProcessCommand(cmd);
        }

        /// <summary>
        /// Processes a command
        /// </summary>
        /// <param name="cmd">Command</param>
        public void ProcessCommand(SparqlUpdateCommand cmd)
        {
            this._store.ExecuteUpdate(cmd);
        }

        /// <summary>
        /// Processes a command set
        /// </summary>
        /// <param name="commands">Command Set</param>
        public void ProcessCommandSet(SparqlUpdateCommandSet commands)
        {
            DateTime start = DateTime.Now;
            commands.UpdateExecutionTime = null;
            try
            {
                this._store.ExecuteUpdate(commands);
            }
            finally
            {
                TimeSpan elapsed = (DateTime.Now - start);
                commands.UpdateExecutionTime = elapsed;
            }
        }

        /// <summary>
        /// Processes a DELETE command
        /// </summary>
        /// <param name="cmd">Delete Command</param>
        public void ProcessDeleteCommand(DeleteCommand cmd)
        {
            this.ProcessCommand(cmd);
        }

        /// <summary>
        /// Processes a DELETE DATA command
        /// </summary>
        /// <param name="cmd">DELETE Data Command</param>
        public void ProcessDeleteDataCommand(DeleteDataCommand cmd)
        {
            this.ProcessCommand(cmd);
        }

        /// <summary>
        /// Processes a DROP command
        /// </summary>
        /// <param name="cmd">Drop Command</param>
        public void ProcessDropCommand(DropCommand cmd)
        {
            this.ProcessCommand(cmd);
        }

        /// <summary>
        /// Processes an INSERT command
        /// </summary>
        /// <param name="cmd">Insert Command</param>
        public void ProcessInsertCommand(InsertCommand cmd)
        {
            this.ProcessCommand(cmd);
        }

        /// <summary>
        /// Processes an INSERT DATA command
        /// </summary>
        /// <param name="cmd">Insert Data Command</param>
        public void ProcessInsertDataCommand(InsertDataCommand cmd)
        {
            this.ProcessCommand(cmd);
        }

        /// <summary>
        /// Processes a LOAD command
        /// </summary>
        /// <param name="cmd">Load Command</param>
        public void ProcessLoadCommand(LoadCommand cmd)
        {
            this.ProcessCommand(cmd);
        }

        /// <summary>
        /// Processes an INSERT/DELETE command
        /// </summary>
        /// <param name="cmd">Insert/Delete Command</param>
        public void ProcessModifyCommand(ModifyCommand cmd)
        {
            this.ProcessCommand(cmd);
        }

        /// <summary>
        /// Processes a MOVE command
        /// </summary>
        /// <param name="cmd">Move Command</param>
        public void ProcessMoveCommand(MoveCommand cmd)
        {
            this.ProcessCommand(cmd);
        }
    }
}
