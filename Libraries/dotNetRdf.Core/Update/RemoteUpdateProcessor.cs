/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2025 dotNetRDF Project (http://dotnetrdf.org/)
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

using System;
using System.Threading.Tasks;
using VDS.RDF.Update.Commands;

namespace VDS.RDF.Update;

/// <summary>
/// SPARQL Update Processor which processes updates by sending them to a remote SPARQL Update endpoint using a <see cref="SparqlUpdateClient"/> instance.
/// </summary>
public class RemoteUpdateProcessor : ISparqlUpdateProcessor
{
    [Obsolete]
    private readonly SparqlRemoteUpdateEndpoint _endpoint;
    private readonly SparqlUpdateClient _client;

    /// <summary>
    /// Creates a new Remote Update Processor.
    /// </summary>
    /// <param name="endpointUri">Endpoint URI.</param>
    [Obsolete("Replaced by RemoteUpdateProcessor(SparqlUpdateClient)")]
    public RemoteUpdateProcessor(string endpointUri)
        : this(new SparqlRemoteUpdateEndpoint(endpointUri)) { }

    /// <summary>
    /// Creates a new Remote Update Processor.
    /// </summary>
    /// <param name="endpointUri">Endpoint URI.</param>
    [Obsolete("Replaced by RemoteUpdateProcessor(SparqlUpdateClient)")]
    public RemoteUpdateProcessor(Uri endpointUri)
        : this(new SparqlRemoteUpdateEndpoint(endpointUri)) { }

    /// <summary>
    /// Creates a new Remote Update Processor.
    /// </summary>
    /// <param name="endpoint">SPARQL Remote Update Endpoint.</param>
    [Obsolete("Replaced by RemoteUpdateProcessor(SparqlUpdateClient)")]
    public RemoteUpdateProcessor(SparqlRemoteUpdateEndpoint endpoint)
    {
        _endpoint = endpoint;
    }

    /// <summary>
    /// Creates a new remote update processor.
    /// </summary>
    /// <param name="updateClient">The SPARQL update client to delegate processing of commands to.</param>
    public RemoteUpdateProcessor(SparqlUpdateClient updateClient)
    {
        _client = updateClient;
    }

    /// <summary>
    /// Discards any outstanding changes.
    /// </summary>
    public void Discard()
    {
        // No discard actions required
    }

    /// <summary>
    /// Flushes any outstanding changes to the underlying store.
    /// </summary>
    public void Flush()
    {
        // No flush actions required
    }

    /// <summary>
    /// Processes an ADD command.
    /// </summary>
    /// <param name="cmd">Add Command.</param>
    public void ProcessAddCommand(AddCommand cmd)
    {
        ProcessCommand(cmd);
    }

    /// <summary>
    /// Processes a CLEAR command.
    /// </summary>
    /// <param name="cmd">Clear Command.</param>
    public void ProcessClearCommand(ClearCommand cmd)
    {
        ProcessCommand(cmd);
    }

    /// <summary>
    /// Processes a COPY command.
    /// </summary>
    /// <param name="cmd">Copy Command.</param>
    public void ProcessCopyCommand(CopyCommand cmd)
    {
        ProcessCommand(cmd);
    }

    /// <summary>
    /// Processes a CREATE command.
    /// </summary>
    /// <param name="cmd">Create Command.</param>
    public void ProcessCreateCommand(CreateCommand cmd)
    {
        ProcessCommand(cmd);
    }

    /// <summary>
    /// Processes a command.
    /// </summary>
    /// <param name="cmd">Command.</param>
    public void ProcessCommand(SparqlUpdateCommand cmd)
    {
        if (_client != null)
        {
            Task.Run(()=>_client.UpdateAsync(cmd.ToString())).Wait();
        }
        else
        {
            _endpoint.Update(cmd.ToString());
        }
    }

    /// <summary>
    /// Processes a command set.
    /// </summary>
    /// <param name="commands">Command Set.</param>
    public void ProcessCommandSet(SparqlUpdateCommandSet commands)
    {
        DateTime start = DateTime.Now;
        commands.UpdateExecutionTime = null;
        try
        {
            if (_client != null)
            {
                Task.Run(() => _client.UpdateAsync(commands.ToString())).Wait();
            }
            else
            {
                _endpoint.Update(commands.ToString());
            }
        }
        finally
        {
            TimeSpan elapsed = (DateTime.Now - start);
            commands.UpdateExecutionTime = elapsed;
        }
    }

    /// <summary>
    /// Processes a DELETE command.
    /// </summary>
    /// <param name="cmd">Delete Command.</param>
    public void ProcessDeleteCommand(DeleteCommand cmd)
    {
        ProcessCommand(cmd);
    }

    /// <summary>
    /// Processes a DELETE DATA command.
    /// </summary>
    /// <param name="cmd">DELETE Data Command.</param>
    public void ProcessDeleteDataCommand(DeleteDataCommand cmd)
    {
        ProcessCommand(cmd);
    }

    /// <summary>
    /// Processes a DROP command.
    /// </summary>
    /// <param name="cmd">Drop Command.</param>
    public void ProcessDropCommand(DropCommand cmd)
    {
        ProcessCommand(cmd);
    }

    /// <summary>
    /// Processes an INSERT command.
    /// </summary>
    /// <param name="cmd">Insert Command.</param>
    public void ProcessInsertCommand(InsertCommand cmd)
    {
        ProcessCommand(cmd);
    }

    /// <summary>
    /// Processes an INSERT DATA command.
    /// </summary>
    /// <param name="cmd">Insert Data Command.</param>
    public void ProcessInsertDataCommand(InsertDataCommand cmd)
    {
        ProcessCommand(cmd);
    }

    /// <summary>
    /// Processes a LOAD command.
    /// </summary>
    /// <param name="cmd">Load Command.</param>
    public void ProcessLoadCommand(LoadCommand cmd)
    {
        ProcessCommand(cmd);
    }

    /// <summary>
    /// Processes an INSERT/DELETE command.
    /// </summary>
    /// <param name="cmd">Insert/Delete Command.</param>
    public void ProcessModifyCommand(ModifyCommand cmd)
    {
        ProcessCommand(cmd);
    }

    /// <summary>
    /// Processes a MOVE command.
    /// </summary>
    /// <param name="cmd">Move Command.</param>
    public void ProcessMoveCommand(MoveCommand cmd)
    {
        ProcessCommand(cmd);
    }
}
