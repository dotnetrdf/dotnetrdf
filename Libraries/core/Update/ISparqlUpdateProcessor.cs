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
    /// Interface for SPARQL Update Processors
    /// </summary>
    /// <remarks>
    /// <para>
    /// A SPARQL Update Processor is a class that knows how apply SPARQL Update Commands to some data source to which the processor has access
    /// </para>
    /// <para>
    /// The point of this interface is to allow for end users to implement custom update processors or to extend and modify the behaviour of the default Leviathan engine as required.
    /// </para>
    /// </remarks>
    public interface ISparqlUpdateProcessor
    {
        /// <summary>
        /// Processes an ADD command
        /// </summary>
        /// <param name="cmd">Add Command</param>
        void ProcessAddCommand(AddCommand cmd);

        /// <summary>
        /// Processes a CLEAR command
        /// </summary>
        /// <param name="cmd">Clear Command</param>
        void ProcessClearCommand(ClearCommand cmd);

        /// <summary>
        /// Processes a COPY command
        /// </summary>
        /// <param name="cmd">Copy Command</param>
        void ProcessCopyCommand(CopyCommand cmd);

        /// <summary>
        /// Processes a CREATE command
        /// </summary>
        /// <param name="cmd">Create Command</param>
        void ProcessCreateCommand(CreateCommand cmd);

        /// <summary>
        /// Processes a command
        /// </summary>
        /// <param name="cmd">Command</param>
        void ProcessCommand(SparqlUpdateCommand cmd);

        /// <summary>
        /// Processes a command set
        /// </summary>
        /// <param name="commands">Command Set</param>
        void ProcessCommandSet(SparqlUpdateCommandSet commands);

        /// <summary>
        /// Processes a DELETE command
        /// </summary>
        /// <param name="cmd">Delete Command</param>
        void ProcessDeleteCommand(DeleteCommand cmd);

        /// <summary>
        /// Processes a DELETE DATA command
        /// </summary>
        /// <param name="cmd">DELETE Data Command</param>
        void ProcessDeleteDataCommand(DeleteDataCommand cmd);

        /// <summary>
        /// Processes a DROP command
        /// </summary>
        /// <param name="cmd">Drop Command</param>
        void ProcessDropCommand(DropCommand cmd);

        /// <summary>
        /// Processes an INSERT command
        /// </summary>
        /// <param name="cmd">Insert Command</param>
        void ProcessInsertCommand(InsertCommand cmd);

        /// <summary>
        /// Processes an INSERT DATA command
        /// </summary>
        /// <param name="cmd">Insert Data Command</param>
        void ProcessInsertDataCommand(InsertDataCommand cmd);

        /// <summary>
        /// Processes a LOAD command
        /// </summary>
        /// <param name="cmd">Load Command</param>
        void ProcessLoadCommand(LoadCommand cmd);

        /// <summary>
        /// Processes an INSERT/DELETE command
        /// </summary>
        /// <param name="cmd">Insert/Delete Command</param>
        void ProcessModifyCommand(ModifyCommand cmd);

        /// <summary>
        /// Processes a MOVE command
        /// </summary>
        /// <param name="cmd">Move Command</param>
        void ProcessMoveCommand(MoveCommand cmd);

        /// <summary>
        /// Causes any outstanding changes to be discarded
        /// </summary>
        void Discard();

        /// <summary>
        /// Causes any outstanding changes to be flushed to the underlying storage
        /// </summary>
        void Flush();
    }
}
