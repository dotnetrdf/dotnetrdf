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

#if !SILVERLIGHT

using System;
using VDS.RDF.Update.Commands;

namespace VDS.RDF.Update
{
    /// <summary>
    /// SPARQL Update Processor which processes updates by sending them to a remote SPARQL Update endpoint represented by a <see cref="SparqlRemoteUpdateEndpoint">SparqlRemoteUpdateEndpoint</see> instance
    /// </summary>
    public class RemoteUpdateProcessor : ISparqlUpdateProcessor
    {
        private SparqlRemoteUpdateEndpoint _endpoint;

        /// <summary>
        /// Creates a new Remote Update Processor
        /// </summary>
        /// <param name="endpointUri">Endpoint URI</param>
        public RemoteUpdateProcessor(string endpointUri)
            : this(new SparqlRemoteUpdateEndpoint(endpointUri)) { }

        /// <summary>
        /// Creates a new Remote Update Processor
        /// </summary>
        /// <param name="endpointUri">Endpoint URI</param>
        public RemoteUpdateProcessor(Uri endpointUri)
            : this(new SparqlRemoteUpdateEndpoint(endpointUri)) { }

        /// <summary>
        /// Creates a new Remote Update Processor
        /// </summary>
        /// <param name="endpoint">SPARQL Remote Update Endpoint</param>
        public RemoteUpdateProcessor(SparqlRemoteUpdateEndpoint endpoint)
        {
            this._endpoint = endpoint;
        }

        /// <summary>
        /// Discards any outstanding changes
        /// </summary>
        public void Discard()
        {
            //No discard actions required
        }

        /// <summary>
        /// Flushes any outstanding changes to the underlying store
        /// </summary>
        public void Flush()
        {
            //No flush actions requied
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
            this._endpoint.Update(cmd.ToString());
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
                this._endpoint.Update(commands.ToString());
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

#endif
