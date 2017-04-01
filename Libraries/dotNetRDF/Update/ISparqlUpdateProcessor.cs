/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
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
