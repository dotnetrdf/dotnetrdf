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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using VDS.Common.Collections;
using VDS.RDF.Parsing.Tokens;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Builder;
using VDS.RDF.Query.Construct;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Update.Commands;

namespace VDS.RDF.Update;

/// <summary>
/// Default SPARQL Update Processor provided by the library's Leviathan SPARQL Engine.
/// </summary>
/// <remarks>
/// <para>
/// Derived implementations may override the relevant virtual protected methods to substitute their own evaluation of an update for our default standards compliant implementations.
/// </para>
/// </remarks>
public class LeviathanUpdateProcessor 
    : ISparqlUpdateProcessor
{
    /// <summary>
    /// Dataset over which updates are applied.
    /// </summary>
    protected ISparqlDataset _dataset;
    private readonly ReaderWriterLockSlim _lock = new();
    private bool _canCommit = true;
    private readonly LeviathanUpdateOptions _options = new();

    /// <summary>
    /// Creates a new Leviathan Update Processor.
    /// </summary>
    /// <param name="store">Triple Store.</param>
    /// <param name="options">An optional callback invoked to set the options to be used by the update processor.</param>
    public LeviathanUpdateProcessor(IInMemoryQueryableStore store, Action<LeviathanUpdateOptions> options = null)
        : this(new InMemoryDataset(store), options) { }

    /// <summary>
    /// Creates a new Leviathan Update Processor.
    /// </summary>
    /// <param name="data">SPARQL Dataset.</param>
    /// <param name="options">An optional callback invoked to set the options to be used by the update processor.</param>
    public LeviathanUpdateProcessor(ISparqlDataset data, Action<LeviathanUpdateOptions> options = null)
    {
        _dataset = data;
        options?.Invoke(_options);
        if (!_dataset.HasGraph((IRefNode)null))
        {
            // Create the Default unnamed Graph if it doesn't exist and then Flush() the change
            _dataset.AddGraph(new Graph());
            _dataset.Flush();
        }
    }

    /// <summary>
    /// Gets/Sets whether Updates are automatically committed.
    /// </summary>
    public bool AutoCommit => _options.AutoCommit;

    /// <summary>
    /// Flushes any outstanding changes to the underlying dataset.
    /// </summary>
    public void Flush()
    {
        if (!_canCommit) throw new SparqlUpdateException("Unable to commit since one/more Commands executed in the current Transaction failed");
        _dataset.Flush();
    }

    /// <summary>
    /// Discards and outstanding changes from the underlying dataset.
    /// </summary>
    public void Discard()
    {
        _dataset.Discard();
        _canCommit = true;
    }

    /// <summary>
    /// Creates a new Evaluation Context.
    /// </summary>
    /// <param name="cmds">Update Commands.</param>
    /// <returns></returns>
    protected SparqlUpdateEvaluationContext GetContext(SparqlUpdateCommandSet cmds)
    {
        return new SparqlUpdateEvaluationContext(cmds, _dataset, GetQueryProcessor(), _options);
    }

    /// <summary>
    /// Creates a new Evaluation Context.
    /// </summary>
    /// <returns></returns>
    protected SparqlUpdateEvaluationContext GetContext()
    {
        return new SparqlUpdateEvaluationContext(_dataset, GetQueryProcessor(), _options);
    }

    /// <summary>
    /// Gets the Query Processor to be used.
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// By default <strong>null</strong> is returned which indicates that the default query processing behaviour is used, to use a specific processor extend this class and override this method.  If you do so you will have access to the dataset in use so generally you will want to use a query processor that accepts a <see cref="ISparqlDataset">ISparqlDataset</see> instance.
    /// </remarks>
    protected virtual ISparqlQueryAlgebraProcessor<BaseMultiset, SparqlEvaluationContext> GetQueryProcessor()
    {
        return null;
    }

    /// <summary>
    /// Processes an ADD command.
    /// </summary>
    /// <param name="cmd">Add Command.</param>
    public void ProcessAddCommand(AddCommand cmd)
    {
        ProcessAddCommandInternal(cmd, GetContext());
    }

    /// <summary>
    /// Processes an ADD command.
    /// </summary>
    /// <param name="cmd">Add Command.</param>
    /// <param name="context">SPARQL Update Evaluation Context.</param>
    protected virtual void ProcessAddCommandInternal(AddCommand cmd, SparqlUpdateEvaluationContext context)
    {
        try
        {
            if (context.Data.HasGraph(cmd.SourceGraphName))
            {
                // Get the Source Graph
                IGraph source = context.Data.GetModifiableGraph(cmd.SourceGraphName);

                // Get the Destination Graph
                IGraph dest;
                if (!context.Data.HasGraph(cmd.DestinationGraphName))
                {
                    dest = new Graph(cmd.DestinationGraphName);
                    context.Data.AddGraph(dest);
                }
                dest = context.Data.GetModifiableGraph(cmd.DestinationGraphName);

                // Move data from the Source into the Destination
                dest.Merge(source);
            }
            else
            {
                // Only show error if not Silent
                if (!cmd.Silent)
                {
                    if (cmd.SourceGraphName != null)
                    {
                        throw new SparqlUpdateException("Cannot ADD from Graph " + cmd.SourceGraphName + " as it does not exist");
                    }
                    else
                    {
                        // This would imply a more fundamental issue with the Dataset not understanding that null means default graph
                        throw new SparqlUpdateException("Cannot ADD from the Default Graph as it does not exist");
                    }
                }
            }
        }
        catch
        {
            // If not silent throw the exception upwards
            if (!cmd.Silent) throw;
        }
    }

    /// <summary>
    /// Processes a CLEAR command.
    /// </summary>
    /// <param name="cmd">Clear Command.</param>
    public void ProcessClearCommand(ClearCommand cmd)
    {
        ProcessClearCommandInternal(cmd, GetContext());
    }

    /// <summary>
    /// Processes a CLEAR command.
    /// </summary>
    /// <param name="cmd">Clear Command.</param>
    /// <param name="context">SPARQL Update Evaluation Context.</param>
    protected virtual void ProcessClearCommandInternal(ClearCommand cmd, SparqlUpdateEvaluationContext context)
    {
        try
        {
            switch (cmd.Mode)
            {
                case ClearMode.Graph:
                case ClearMode.Default:
                    if (context.Data.HasGraph(cmd.TargetGraphName))
                    {
                        context.Data.GetModifiableGraph(cmd.TargetGraphName).Clear();
                    }
                    break;
                case ClearMode.Named:
                    foreach (IRefNode u in context.Data.GraphNames)
                    {
                        if (u != null)
                        {
                            context.Data.GetModifiableGraph(u).Clear();
                        }
                    }
                    break;
                case ClearMode.All:
                    foreach (IRefNode u in context.Data.GraphNames)
                    {
                        context.Data.GetModifiableGraph(u).Clear();
                    }
                    break;
            }
        }
        catch
        {
            if (!cmd.Silent) throw;
        }
    }

    /// <summary>
    /// Processes a COPY command.
    /// </summary>
    /// <param name="cmd">Copy Command.</param>
    public void ProcessCopyCommand(CopyCommand cmd)
    {
        ProcessCopyCommandInternal(cmd, GetContext());
    }

    /// <summary>
    /// Processes a COPY command.
    /// </summary>
    /// <param name="cmd">Copy Command.</param>
    /// <param name="context">SPARQL Update Evaluation Context.</param>
    protected virtual void ProcessCopyCommandInternal(CopyCommand cmd, SparqlUpdateEvaluationContext context)
    {
        try
        {
            if (context.Data.HasGraph(cmd.SourceGraphName))
            {
                // If Source and Destination are same this is a no-op
                if (EqualityHelper.AreRefNodesEqual(cmd.SourceGraphName, cmd.DestinationGraphName)) return;

                // Get the Source Graph
                IGraph source = context.Data.GetModifiableGraph(cmd.SourceGraphName);

                // Create/Delete/Clear the Destination Graph
                IGraph dest;
                if (context.Data.HasGraph(cmd.DestinationGraphName))
                {
                    if (cmd.DestinationGraphName == null)
                    {
                        dest = context.Data.GetModifiableGraph(cmd.DestinationGraphName);
                        dest.Clear();
                    }
                    else
                    {
                        context.Data.RemoveGraph(cmd.DestinationGraphName);
                        dest = new Graph(cmd.DestinationGraphName);
                        context.Data.AddGraph(dest);
                        dest = context.Data.GetModifiableGraph(cmd.DestinationGraphName);
                    }
                }
                else
                {
                    dest = new Graph(cmd.DestinationGraphName);
                    context.Data.AddGraph(dest);
                }

                // Move data from the Source into the Destination
                dest.Merge(source);
            }
            else
            {
                // Only show error if not Silent
                if (!cmd.Silent)
                {
                    if (cmd.SourceGraphName != null)
                    {
                        throw new SparqlUpdateException("Cannot COPY from Graph " + cmd.SourceGraphName + " as it does not exist");
                    }
                    else
                    {
                        // This would imply a more fundamental issue with the Dataset not understanding that null means default graph
                        throw new SparqlUpdateException("Cannot COPY from the Default Graph as it does not exist");
                    }
                }
            }
        }
        catch
        {
            // If not silent throw the exception upwards
            if (!cmd.Silent) throw;
        }
    }

    /// <summary>
    /// Processes a CREATE command.
    /// </summary>
    /// <param name="cmd">Create Command.</param>
    public void ProcessCreateCommand(CreateCommand cmd)
    {
        ProcessCreateCommandInternal(cmd, GetContext());
    }

    /// <summary>
    /// Processes a CREATE command.
    /// </summary>
    /// <param name="cmd">Create Command.</param>
    /// <param name="context">SPARQL Update Evaluation Context.</param>
    protected virtual void ProcessCreateCommandInternal(CreateCommand cmd, SparqlUpdateEvaluationContext context)
    {
        if (context.Data.HasGraph(cmd.TargetGraphName))
        {
            if (!cmd.Silent)
            {
                throw new SparqlUpdateException("Cannot create a Named Graph with name '" + cmd.TargetGraphName + "' since a Graph with this URI already exists in the Store");
            }
        }
        else
        {
            var g = new Graph(cmd.TargetGraphName);
            context.Data.AddGraph(g);
        }
    }

    /// <summary>
    /// Processes a command.
    /// </summary>
    /// <param name="cmd">Command.</param>
    public void ProcessCommand(SparqlUpdateCommand cmd)
    {
        ProcessCommandInternal(cmd, GetContext(), _options.AutoCommit);
    }

    /// <summary>
    /// Processes a command.
    /// </summary>
    /// <param name="cmd">Command.</param>
    /// <param name="context">SPARQL Update Evaluation Context.</param>
    /// <param name="autoCommit">Whether to flush dataset changes after update.</param>
    /// <remarks>
    /// Invokes the type specific method for the command type.
    /// </remarks>
    private void ProcessCommandInternal(SparqlUpdateCommand cmd, SparqlUpdateEvaluationContext context,
        bool autoCommit)
    {
        // If auto-committing then Flush() any existing Transaction first
        if (autoCommit) Flush();

        // Then if possible attempt to get the lock and determine whether it needs releasing
        ReaderWriterLockSlim currLock =
            (_dataset is IThreadSafeDataset) ? ((IThreadSafeDataset) _dataset).Lock : _lock;
        var mustRelease = false;
        try
        {
            if (!currLock.IsWriteLockHeld)
            {
                currLock.EnterWriteLock();
                mustRelease = true;
            }

            // Then based on the command type call the appropriate protected method
            switch (cmd.CommandType)
            {
                case SparqlUpdateCommandType.Add:
                    ProcessAddCommandInternal((AddCommand) cmd, context);
                    break;
                case SparqlUpdateCommandType.Clear:
                    ProcessClearCommandInternal((ClearCommand) cmd, context);
                    break;
                case SparqlUpdateCommandType.Copy:
                    ProcessCopyCommandInternal((CopyCommand) cmd, context);
                    break;
                case SparqlUpdateCommandType.Create:
                    ProcessCreateCommandInternal((CreateCommand) cmd, context);
                    break;
                case SparqlUpdateCommandType.Delete:
                    ProcessDeleteCommandInternal((DeleteCommand) cmd, context);
                    break;
                case SparqlUpdateCommandType.DeleteData:
                    ProcessDeleteDataCommandInternal((DeleteDataCommand) cmd, context);
                    break;
                case SparqlUpdateCommandType.Drop:
                    ProcessDropCommandInternal((DropCommand) cmd, context);
                    break;
                case SparqlUpdateCommandType.Insert:
                    ProcessInsertCommandInternal((InsertCommand) cmd, context);
                    break;
                case SparqlUpdateCommandType.InsertData:
                    ProcessInsertDataCommandInternal((InsertDataCommand) cmd, context);
                    break;
                case SparqlUpdateCommandType.Load:
                    ProcessLoadCommandInternal((LoadCommand) cmd, context);
                    break;
                case SparqlUpdateCommandType.Modify:
                    ProcessModifyCommandInternal((ModifyCommand) cmd, context);
                    break;
                case SparqlUpdateCommandType.Move:
                    ProcessMoveCommandInternal((MoveCommand) cmd, context);
                    break;
                default:
                    throw new SparqlUpdateException(
                        "Unknown Update Commands cannot be processed by the Leviathan Update Processor");
            }

            // If auto-committing flush after every command
            if (autoCommit) Flush();
        }
        catch
        {
            // If auto-committing discard if an error occurs, if not then mark the transaction as uncomittable
            if (autoCommit)
            {
                Discard();
            }
            else
            {
                _canCommit = false;
            }

            throw;
        }
        finally
        {
            // Release locks if necessary
            if (mustRelease)
            {
                currLock.ExitWriteLock();
            }
        }
    }

    /// <summary>
    /// Processes a command set.
    /// </summary>
    /// <param name="commands">Command Set.</param>
    public void ProcessCommandSet(SparqlUpdateCommandSet commands)
    {
        commands.UpdateExecutionTime = null;

        // Then create an Evaluation Context
        SparqlUpdateEvaluationContext context = GetContext(commands);

        // Remember to handle the Thread Safety
        // If the Dataset is Thread Safe use its own lock otherwise use our local lock
        ReaderWriterLockSlim currLock = (_dataset is IThreadSafeDataset) ? ((IThreadSafeDataset)_dataset).Lock : _lock;
        try
        {
            currLock.EnterWriteLock();

            if (_options.AutoCommit)
            {
                // Do a Flush() before we start to ensure changes from any previous commands are persisted
                _dataset.Flush();
            }

            // Start the operation
            context.StartExecution();
            for (var i = 0; i < commands.CommandCount; i++)
            {
                // Process each individual command with auto-commit disabled so that individual commands
                // don't try and commit after each one is applied. When the processor is in auto-commit
                // mode, all of the commands must be evaluated before flushing/discarding the changes.
                ProcessCommandInternal(commands[i], context, false);

                // Check for Timeout
                context.CheckTimeout();
            }

            if (_options.AutoCommit)
            {
                // Do a Flush() when command set completed successfully to persist the changes
                _dataset.Flush();
            }

            // Set Update Times
            context.EndExecution();
            commands.UpdateExecutionTime = new TimeSpan(context.UpdateTimeTicks);
        }
        catch
        {
            if (_options.AutoCommit)
            {
                // Do a Discard() when a command set fails to discard the changes
                _dataset.Discard();
            }
            else
            {
                _canCommit = false;
            }

            // Set Update Times
            context.EndExecution();
            commands.UpdateExecutionTime = new TimeSpan(context.UpdateTimeTicks);
            throw;
        }
        finally
        {
            currLock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Processes a DELETE command.
    /// </summary>
    /// <param name="cmd">Delete Command.</param>
    public void ProcessDeleteCommand(DeleteCommand cmd)
    {
        ProcessDeleteCommandInternal(cmd, GetContext());
    }

    /// <summary>
    /// Processes a DELETE command.
    /// </summary>
    /// <param name="cmd">Delete Command.</param>
    /// <param name="context">SPARQL Update Evaluation Context.</param>
    protected virtual void ProcessDeleteCommandInternal(DeleteCommand cmd, SparqlUpdateEvaluationContext context)
    {
        var defGraphOk = false;
        var datasetOk = false;

        try
        {
            // If there is a WITH clause and no matching graph, and the delete pattern doesn't contain child graph patterns then there is nothing to do
            if (cmd.WithGraphName != null && !context.Data.HasGraph(cmd.WithGraphName) && !cmd.DeletePattern.HasChildGraphPatterns)
            {
                return;
            }

            // First evaluate the WHERE pattern to get the affected bindings
            ISparqlAlgebra where = cmd.WherePattern.ToAlgebra();
            if (context.Commands != null)
            {
                where = context.Commands.ApplyAlgebraOptimisers(where);
            }

            // Set Active Graph for the WHERE based upon the WITH clause
            // Don't bother if there are USING URIs as these would override any Active Graph we set here
            // so we can save ourselves the effort of doing this
            if (!cmd.UsingUris.Any())
            {
                if (cmd.WithGraphName != null)
                {
                    context.Data.SetActiveGraph(cmd.WithGraphName);
                    defGraphOk = true;
                }
                else
                {
                    context.Data.SetActiveGraph((IRefNode)null);
                    defGraphOk = true;
                }
            }

            // We need to make a dummy SparqlQuery object since if the Command has used any 
            // USING/USING NAMEDs along with GRAPH clauses then the algebra needs to have the
            // URIs available to it which it gets from the Query property of the Context
            // object


            // var query = new SparqlQuery();
            SparqlQuery query = QueryBuilder.SelectAll().BuildQuery();
            foreach (Uri u in cmd.UsingUris)
            {
                query.AddDefaultGraph(new UriNode(u));
            }
            foreach (Uri u in cmd.UsingNamedUris)
            {
                query.AddNamedGraph(new UriNode(u));
            }
            
            var queryContext = new SparqlEvaluationContext(query, context.Data, context.QueryProcessor, context.Options);
            if (cmd.UsingUris.Any())
            {
                // If there are USING URIs set the Active Graph to be formed of the Graphs with those URIs
                IList<IRefNode> activeGraphs = cmd.UsingUris.Select<Uri, IRefNode>(u => new UriNode(u)).ToList();
                context.Data.SetActiveGraph(activeGraphs);
                datasetOk = true;
            }
            BaseMultiset results = queryContext.Evaluate(where);
            if (results is IdentityMultiset) results = new SingletonMultiset(results.Variables);
            if (cmd.UsingUris.Any())
            {
                // If there are USING URIs reset the Active Graph afterwards
                // Also flag the dataset as no longer being OK as this flag is used in the finally 
                // block to determine whether the Active Graph needs resetting which it may do if the
                // evaluation of the 
                context.Data.ResetActiveGraph();
                datasetOk = false;
            }

            // Reset Active Graph for the WHERE
            if (defGraphOk)
            {
                context.Data.ResetActiveGraph();
                defGraphOk = false;
            }

            // Get the Graph from which we are deleting
            IGraph g = context.Data.HasGraph(cmd.WithGraphName) ? context.Data.GetModifiableGraph(cmd.WithGraphName) : null;

            // Delete the Triples for each Solution
            foreach (ISet s in results.Sets)
            {
                var deletedTriples = new List<Triple>();

                if (g != null)
                {
                    // Triples from raw Triple Patterns
                    try
                    {
                        var constructContext = new ConstructContext(g, true){Set = s};
                        foreach (IConstructTriplePattern p in cmd.DeletePattern.TriplePatterns
                            .OfType<IConstructTriplePattern>())
                        {
                            try
                            {
                                deletedTriples.Add(p.Construct(constructContext));
                            }
                            catch (RdfQueryException)
                            {
                                // If we get an error here then we couldn't construct a specific Triple
                                // so we continue anyway
                            }
                        }

                        g.Retract(deletedTriples);
                    }
                    catch (RdfQueryException)
                    {
                        // If we throw an error this means we couldn't construct for this solution so the
                        // solution is ignored this graph
                    }
                }

                // Triples from GRAPH clauses
                foreach (GraphPattern gp in cmd.DeletePattern.ChildGraphPatterns)
                {
                    deletedTriples.Clear();
                    try
                    {
                        IRefNode graphName;
                        switch (gp.GraphSpecifier.TokenType)
                        {
                            case Token.URI:
                                graphName = new UriNode(UriFactory.Root.Create(gp.GraphSpecifier.Value));
                                break;
                            case Token.VARIABLE:
                                var graphVar = gp.GraphSpecifier.Value.Substring(1);
                                if (s.ContainsVariable(graphVar))
                                {
                                    INode temp = s[graphVar];
                                    if (temp == null)
                                    {
                                        // If the Variable is not bound then skip
                                        continue;
                                    }
                                    else if (temp.NodeType == NodeType.Uri)
                                    {
                                        graphName = temp as IUriNode;
                                    }
                                    else if (temp.NodeType == NodeType.Blank)
                                    {
                                        graphName = temp as IBlankNode;
                                    }
                                    else
                                    {
                                        // If the Variable is not bound to a URI then skip
                                        continue;
                                    }
                                }
                                else
                                {
                                    // If the Variable is not bound for this solution then skip
                                    continue;
                                }
                                break;
                            default:
                                // Any other Graph Specifier we have to ignore this solution
                                continue;
                        }

                        // If the Dataset doesn't contain the Graph then no need to do the Deletions
                        if (!context.Data.HasGraph(graphName)) continue;

                        // Do the actual Deletions
                        IGraph h = context.Data.GetModifiableGraph(graphName);
                        var constructContext = new ConstructContext(h, true){Set = s};
                        foreach (IConstructTriplePattern p in gp.TriplePatterns.OfType<IConstructTriplePattern>())
                        {
                            try
                            {
                                deletedTriples.Add(p.Construct(constructContext));
                            }
                            catch (RdfQueryException)
                            {
                                // If we get an error here then we couldn't construct a specific
                                // triple so we continue anyway
                            }
                        }
                        h.Retract(deletedTriples);
                    }
                    catch (RdfQueryException)
                    {
                        // If we get an error here this means we couldn't construct for this solution so the
                        // solution is ignored for this graph
                    }
                }
            }
        }
        finally
        {
            // If the Dataset was set and an error occurred in doing the WHERE clause then
            // we'll need to Reset the Active Graph
            if (datasetOk) context.Data.ResetActiveGraph();
            if (defGraphOk) context.Data.ResetActiveGraph();
        }
    }

    /// <summary>
    /// Processes a DELETE DATA command.
    /// </summary>
    /// <param name="cmd">DELETE Data Command.</param>
    public void ProcessDeleteDataCommand(DeleteDataCommand cmd)
    {
        ProcessDeleteDataCommandInternal(cmd, GetContext());
    }

    /// <summary>
    /// Processes a DELETE DATA command.
    /// </summary>
    /// <param name="cmd">Delete Data Command.</param>
    /// <param name="context">SPARQL Update Evaluation Context.</param>
    protected virtual void ProcessDeleteDataCommandInternal(DeleteDataCommand cmd, SparqlUpdateEvaluationContext context)
    {
        // Split the Pattern into the set of Graph Patterns
        var patterns = new List<GraphPattern>();
        if (cmd.DataPattern.IsGraph)
        {
            patterns.Add(cmd.DataPattern);
        }
        else if (cmd.DataPattern.TriplePatterns.Count > 0 || cmd.DataPattern.HasChildGraphPatterns)
        {
            if (cmd.DataPattern.TriplePatterns.Count > 0)
            {
                patterns.Add(new GraphPattern());
                cmd.DataPattern.TriplePatterns.ForEach(tp => patterns[0].AddTriplePattern(tp));
            }
            cmd.DataPattern.ChildGraphPatterns.ForEach(gp => patterns.Add(gp));
        }
        else
        {
            // If no Triple Patterns and No Child Graph Patterns nothing to do
            return;
        }

        foreach (GraphPattern pattern in patterns)
        {
            if (!DeleteDataCommand.IsValidDataPattern(pattern, false))
            {
                throw new SparqlUpdateException("Cannot evaluate a DELETE DATA command where any of the Triple Patterns are not concrete triples (variables are not permitted) or any of the GRAPH clauses have nested Graph Patterns");
            }

            // Get the Target Graph
            IGraph target;
            IRefNode graphName;
            if (pattern.IsGraph)
            {
                switch (pattern.GraphSpecifier.TokenType)
                {
                    case Token.QNAME:
                        throw new NotSupportedException("Graph Specifiers as QNames for DELETE DATA Commands are not supported - please specify an absolute URI instead");
                    case Token.URI:
                        graphName = new UriNode(UriFactory.Root.Create(pattern.GraphSpecifier.Value));
                        break;
                    default:
                        throw new SparqlUpdateException("Cannot evaluate an DELETE DATA Command as the Graph Specifier is not a QName/URI");
                }
            }
            else
            {
                graphName = null;
            }

            // If the Pattern affects a non-existent Graph then nothing to DELETE
            if (!context.Data.HasGraph(graphName)) continue;
            target = context.Data.GetModifiableGraph(graphName);

            // Delete the actual Triples
            INode subj, pred, obj;

            var constructContext = new ConstructContext(target, false);
            foreach (IConstructTriplePattern p in pattern.TriplePatterns.OfType<IConstructTriplePattern>())
            {
                subj = p.Subject.Construct(constructContext);
                pred = p.Predicate.Construct(constructContext);
                obj = p.Object.Construct(constructContext);

                target.Retract(new Triple(subj, pred, obj));
            }
        }
    }

    /// <summary>
    /// Processes a DROP command.
    /// </summary>
    /// <param name="cmd">Drop Command.</param>
    public void ProcessDropCommand(DropCommand cmd)
    {
        ProcessDropCommandInternal(cmd, GetContext());
    }

    /// <summary>
    /// Processes a DROP command.
    /// </summary>
    /// <param name="cmd">Drop Command.</param>
    /// <param name="context">SPARQL Update Evaluation Context.</param>
    protected virtual void ProcessDropCommandInternal(DropCommand cmd, SparqlUpdateEvaluationContext context)
    {
        try
        {
            switch (cmd.Mode)
            {
                case ClearMode.Default:
                case ClearMode.Graph:
                    if (!context.Data.HasGraph(cmd.TargetGraphName))
                    {
                        if (!cmd.Silent) throw new SparqlUpdateException(
                            $"Cannot remove a Named Graph with name {cmd.TargetGraphName} since a Graph with this URI does not exist in the Store");
                    }
                    else
                    {
                        if (cmd.Mode == ClearMode.Graph)
                        {
                            context.Data.RemoveGraph(cmd.TargetGraphName);
                        }
                        else
                        {
                            // DROPing the DEFAULT graph only results in clearing it
                            // This is because removing the default graph may cause errors in later commands/queries
                            // which rely on it existing
                            context.Data.GetModifiableGraph(cmd.TargetGraphName).Clear();
                        }
                    }
                    break;

                case ClearMode.Named:
                    foreach (IRefNode u in context.Data.GraphNames.ToList())
                    {
                        if (u != null)
                        {
                            context.Data.RemoveGraph(u);
                        }
                    }
                    break;
                case ClearMode.All:
                    foreach (IRefNode u in context.Data.GraphNames.ToList())
                    {
                        if (u != null)
                        {
                            context.Data.RemoveGraph(u);
                        }
                        else
                        {
                            // DROPing the DEFAULT graph only results in clearing it
                            // This is because removing the default graph may cause errors in later commands/queries
                            // which rely on it existing
                            context.Data.GetModifiableGraph(u).Clear();
                        }
                    }
                    break;
            }
        }
        catch
        {
            if (!cmd.Silent) throw;
        }
    }

    /// <summary>
    /// Processes an INSERT command.
    /// </summary>
    /// <param name="cmd">Insert Command.</param>
    public void ProcessInsertCommand(InsertCommand cmd)
    {
        ProcessInsertCommandInternal(cmd, GetContext());
    }

    /// <summary>
    /// Processes an INSERT command.
    /// </summary>
    /// <param name="cmd">Insert Command.</param>
    /// <param name="context">SPARQL Update Evaluation Context.</param>
    protected virtual void ProcessInsertCommandInternal(InsertCommand cmd, SparqlUpdateEvaluationContext context)
    {
        var datasetOk = false;
        var defGraphOk = false;

        try
        {
            // First evaluate the WHERE pattern to get the affected bindings
            ISparqlAlgebra where = cmd.WherePattern.ToAlgebra();
            if (context.Commands != null)
            {
                where = context.Commands.ApplyAlgebraOptimisers(where);
            }

            // Set Active Graph for the WHERE
            // Don't bother if there are USING URIs as these would override any Active Graph we set here
            // so we can save ourselves the effort of doing this
            if (!cmd.UsingUris.Any())
            {
                if (cmd.WithGraphName != null)
                {
                    context.Data.SetActiveGraph(cmd.WithGraphName);
                    defGraphOk = true;
                }
                else
                {
                    context.Data.SetActiveGraph((IRefNode)null);
                    defGraphOk = true;
                }
            }

            // We need to make a dummy SparqlQuery object since if the Command has used any 
            // USING NAMEDs along with GRAPH clauses then the algebra needs to have the
            // URIs available to it which it gets from the Query property of the Context
            // object
            //var query = new SparqlQuery();
            SparqlQuery query = QueryBuilder.SelectAll().BuildQuery();

            foreach (Uri u in cmd.UsingUris)
            {
                query.AddDefaultGraph(new UriNode(u));
            }
            foreach (Uri u in cmd.UsingNamedUris)
            {
                query.AddNamedGraph(new UriNode(u));
            }
            var queryContext = new SparqlEvaluationContext(query, context.Data, context.QueryProcessor, context.Options);
            if (cmd.UsingUris.Any())
            {
                // If there are USING URIs set the Active Graph to be formed of the Graphs with those URIs
                context.Data.SetActiveGraph(cmd.UsingUris.Select<Uri, IRefNode>(u => new UriNode(u)).ToList());
                datasetOk = true;
            }
            queryContext.StartExecution(context.RemainingTimeout);
            BaseMultiset results = queryContext.Evaluate(where);
            queryContext.EndExecution();
            if (results is IdentityMultiset) results = new SingletonMultiset(results.Variables);
            if (cmd.UsingUris.Any())
            {
                // If there are USING URIs reset the Active Graph afterwards
                // Also flag the dataset as no longer being OK as this flag is used in the finally 
                // block to determine whether the Active Graph needs resetting which it may do if the
                // evaluation of the query fails for any reason
                context.Data.ResetActiveGraph();
                datasetOk = false;
            }

            // Reset Active Graph for the WHERE
            if (defGraphOk)
            {
                context.Data.ResetActiveGraph();
                defGraphOk = false;
            }

            // TODO: Need to detect when we create a Graph for Insertion but then fail to insert anything since in this case the Inserted Graph should be removed

            // Get the Graph to which we are inserting Triples with no explicit Graph clause
            IGraph g = null;
            if (cmd.InsertPattern.TriplePatterns.Count > 0)
            {
                if (context.Data.HasGraph(cmd.WithGraphName))
                {
                    g = context.Data.GetModifiableGraph(cmd.WithGraphName);
                }
                else
                {
                    // insertedGraphs.Add(this._graphUri);
                    g = new Graph(cmd.WithGraphName);
                    context.Data.AddGraph(g);
                    g = context.Data.GetModifiableGraph(cmd.WithGraphName);
                }
            }

            // Keep a record of graphs to which we insert
            var graphs = new MultiDictionary<Uri, IGraph>(u => (u != null ? u.GetEnhancedHashCode() : 0), true, new UriComparer(), MultiDictionaryMode.Avl);

            // Insert the Triples for each Solution
            foreach (ISet s in results.Sets)
            {
                var insertedTriples = new List<Triple>();

                try
                {
                    // Create a new Construct Context for each Solution
                    var constructContext = new ConstructContext(s, true);

                    // Triples from raw Triple Patterns
                    if (cmd.InsertPattern.TriplePatterns.Count > 0)
                    {
                        foreach (IConstructTriplePattern p in cmd.InsertPattern.TriplePatterns.OfType<IConstructTriplePattern>())
                        {
                            try
                            {
                                insertedTriples.Add(p.Construct(constructContext));
                            }
                            catch (RdfQueryException)
                            {
                                // If we throw an error this means we couldn't construct a specific Triple
                                // so we continue anyway
                            }
                        }
                        g.Assert(insertedTriples);
                    }

                    // Triples from GRAPH clauses
                    foreach (GraphPattern gp in cmd.InsertPattern.ChildGraphPatterns)
                    {
                        insertedTriples.Clear();
                        try
                        {
                            string graphUri;
                            switch (gp.GraphSpecifier.TokenType)
                            {
                                case Token.URI:
                                    graphUri = gp.GraphSpecifier.Value;
                                    break;
                                case Token.VARIABLE:
                                    var graphVar = gp.GraphSpecifier.Value.Substring(1);
                                    if (s.ContainsVariable(graphVar))
                                    {
                                        INode temp = s[graphVar];
                                        if (temp == null)
                                        {
                                            // If the Variable is not bound then skip
                                            continue;
                                        }
                                        if (temp.NodeType == NodeType.Uri)
                                        {
                                            graphUri = temp.ToSafeString();
                                        }
                                        else
                                        {
                                            // If the Variable is not bound to a URI then skip
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        // If the Variable is not bound for this solution then skip
                                        continue;
                                    }
                                    break;
                                default:
                                    // Any other Graph Specifier we have to ignore this solution
                                    continue;
                            }

                            // Ensure the Graph we're inserting to exists in the dataset creating it if necessary
                            IGraph h;
                            Uri destUri = UriFactory.Root.Create(graphUri);
                            IRefNode destName = new UriNode(destUri);
                            if (graphs.ContainsKey(destUri))
                            {
                                h = graphs[destUri];
                            }
                            else
                            {
                                if (context.Data.HasGraph(destName))
                                {
                                    h = context.Data.GetModifiableGraph(destName);
                                }
                                else
                                {
                                    // insertedGraphs.Add(destUri);
                                    h = new Graph(destName);
                                    context.Data.AddGraph(h);
                                    h = context.Data.GetModifiableGraph(destName);
                                }
                                graphs.Add(destUri, h);
                            }

                            // Do the actual Insertions
                            foreach (IConstructTriplePattern p in gp.TriplePatterns.OfType<IConstructTriplePattern>())
                            {
                                try
                                {
                                    Triple t = p.Construct(constructContext);
                                    t = new Triple(t.Subject, t.Predicate, t.Object);
                                    insertedTriples.Add(t);
                                }
                                catch (RdfQueryException)
                                {
                                    // If we throw an error this means we couldn't construct a specific Triple
                                    // so we continue anyway
                                }
                            }
                            h.Assert(insertedTriples);
                        }
                        catch (RdfQueryException)
                        {
                            // If we throw an error this means we couldn't construct for this solution so the
                            // solution is ignored for this Graph
                        }
                    }
                }
                catch (RdfQueryException)
                {
                    // If we throw an error this means we couldn't construct for this solution so the
                    // solution is ignored for this graph
                }
            }
        }
        finally
        {
            // If the Dataset was set and an error occurred in doing the WHERE clause then
            // we'll need to Reset the Active Graph
            if (datasetOk) context.Data.ResetActiveGraph();
            if (defGraphOk) context.Data.ResetActiveGraph();
        }
    }

    /// <summary>
    /// Processes an INSERT DATA command.
    /// </summary>
    /// <param name="cmd">Insert Data Command.</param>
    public void ProcessInsertDataCommand(InsertDataCommand cmd)
    {
        ProcessInsertDataCommandInternal(cmd, GetContext());
    }

    /// <summary>
    /// Processes an INSERT DATA command.
    /// </summary>
    /// <param name="cmd">Insert Data Command.</param>
    /// <param name="context">SPARQL Update Evaluation Context.</param>
    protected virtual void ProcessInsertDataCommandInternal(InsertDataCommand cmd, SparqlUpdateEvaluationContext context)
    {
        // Split the Pattern into the set of Graph Patterns
        var patterns = new List<GraphPattern>();
        if (cmd.DataPattern.IsGraph)
        {
            patterns.Add(cmd.DataPattern);
        }
        else if (cmd.DataPattern.TriplePatterns.Count > 0 || cmd.DataPattern.HasChildGraphPatterns)
        {
            if (cmd.DataPattern.TriplePatterns.Count > 0)
            {
                patterns.Add(new GraphPattern());
                cmd.DataPattern.TriplePatterns.ForEach(tp => patterns[0].AddTriplePattern(tp));
            }
            cmd.DataPattern.ChildGraphPatterns.ForEach(gp => patterns.Add(gp));
        }
        else
        {
            // If no Triple Patterns and No Child Graph Patterns nothing to do
            return;
        }

        var constructContext = new ConstructContext(false);
        foreach (GraphPattern pattern in patterns)
        {
            if (!InsertDataCommand.IsValidDataPattern(pattern, false)) throw new SparqlUpdateException("Cannot evaluate a INSERT DATA command where any of the Triple Patterns are not concrete triples (variables are not permitted) or any of the GRAPH clauses have nested Graph Patterns");

            // Get the Target Graph
            IGraph target;
            IRefNode graphName;
            if (pattern.IsGraph)
            {
                switch (pattern.GraphSpecifier.TokenType)
                {
                    case Token.QNAME:
                        throw new NotSupportedException("Graph Specifiers as QNames for INSERT DATA Commands are not supported - please specify an absolute URI instead");
                    case Token.URI:
                        graphName = new UriNode(UriFactory.Root.Create(pattern.GraphSpecifier.Value));
                        break;
                    default:
                        throw new SparqlUpdateException("Cannot evaluate an INSERT DATA Command as the Graph Specifier is not a QName/URI");
                }
            }
            else
            {
                graphName = null;
            }
            if (context.Data.HasGraph(graphName))
            {
                target = context.Data.GetModifiableGraph(graphName);
            }
            else
            {
                // If the Graph does not exist then it must be created
                target = new Graph(graphName);
                context.Data.AddGraph(target);
            }

            // Insert the actual Triples
            foreach (IConstructTriplePattern p in pattern.TriplePatterns.OfType<IConstructTriplePattern>())
            {
                INode subj = p.Subject.Construct(constructContext);
                INode pred = p.Predicate.Construct(constructContext);
                INode obj = p.Object.Construct(constructContext);

                target.Assert(new Triple(subj, pred, obj));
            }
        }
    }

    /// <summary>
    /// Processes a LOAD command.
    /// </summary>
    /// <param name="cmd">Load Command.</param>
    public void ProcessLoadCommand(LoadCommand cmd)
    {
        ProcessLoadCommandInternal(cmd, GetContext());
    }

    /// <summary>
    /// Processes a LOAD command.
    /// </summary>
    /// <param name="cmd">Load Command.</param>
    /// <param name="context">SPARQL Update Evaluation Context.</param>
    protected virtual void ProcessLoadCommandInternal(LoadCommand cmd, SparqlUpdateEvaluationContext context)
    {
        // Q: Does LOAD into a named Graph require that Graph to be pre-existing?
        // if (this._graphUri != null)
        // {
        //    //When adding to specific Graph need to ensure that Graph exists
        //    //In the case when we're adding to the default graph we'll create it if it doesn't exist
        //    if (!context.Data.HasGraph(this._graphUri))
        //    {
        //        throw new RdfUpdateException("Cannot LOAD into a Graph that does not exist in the Store");
        //    }
        // }

        try
        {
            // Load from the URI
            var g = new Graph(cmd.TargetGraphName);
            cmd.Loader.LoadGraph(g, cmd.SourceUri);
            if (context.Data.HasGraph(cmd.TargetGraphName))
            {
                // Merge the Data into the existing Graph
                context.Data.GetModifiableGraph(cmd.TargetGraphName).Merge(g);
            }
            else
            {
                // Add New Graph to the Dataset
                context.Data.AddGraph(g);
            }
        }
        catch
        {
            if (!cmd.Silent) throw;
        }
    }

    /// <summary>
    /// Processes an INSERT/DELETE command.
    /// </summary>
    /// <param name="cmd">Insert/Delete Command.</param>
    public void ProcessModifyCommand(ModifyCommand cmd)
    {
        ProcessModifyCommandInternal(cmd, GetContext());
    }

    /// <summary>
    /// Processes an INSERT/DELETE command.
    /// </summary>
    /// <param name="cmd">Insert/Delete Command.</param>
    /// <param name="context">SPARQL Update Evaluation Context.</param>
    protected virtual void ProcessModifyCommandInternal(ModifyCommand cmd, SparqlUpdateEvaluationContext context)
    {
        var datasetOk = false;
        var defGraphOk = false;

        try
        {
            // First evaluate the WHERE pattern to get the affected bindings
            ISparqlAlgebra where = cmd.WherePattern.ToAlgebra();
            if (context.Commands != null)
            {
                where = context.Commands.ApplyAlgebraOptimisers(where);
            }

            // Set Active Graph for the WHERE
            // Don't bother if there are USING URIs as these would override any Active Graph we set here
            // so we can save ourselves the effort of doing this
            if (!cmd.UsingUris.Any())
            {
                if (cmd.WithGraphName != null)
                {
                    context.Data.SetActiveGraph(cmd.WithGraphName);
                    defGraphOk = true;
                }
                else
                {
                    context.Data.SetActiveGraph((IRefNode)null);
                    defGraphOk = true;
                }
            }

            // We need to make a dummy SparqlQuery object since if the Command has used any 
            // USING NAMEDs along with GRAPH clauses then the algebra needs to have the
            // URIs available to it which it gets from the Query property of the Context
            // object
            //var query = new SparqlQuery();
            SparqlQuery query = QueryBuilder.SelectAll().BuildQuery();

            foreach (Uri u in cmd.UsingUris)
            {
                query.AddDefaultGraph(new UriNode(u));
            }
            foreach (Uri u in cmd.UsingNamedUris)
            {
                query.AddNamedGraph(new UriNode(u));
            }
            var queryContext = new SparqlEvaluationContext(query, context.Data, context.QueryProcessor, context.Options);
            if (cmd.UsingUris.Any())
            {
                // If there are USING URIs set the Active Graph to be formed of the Graphs with those URIs
                context.Data.SetActiveGraph(cmd.UsingUris.Select<Uri, IRefNode>(u => new UriNode(u)).ToList());
                datasetOk = true;
            }
            BaseMultiset results = queryContext.Evaluate(where);
            if (results is IdentityMultiset) results = new SingletonMultiset(results.Variables);
            if (cmd.UsingUris.Any())
            {
                // If there are USING URIs reset the Active Graph afterwards
                // Also flag the dataset as no longer being OK as this flag is used in the finally 
                // block to determine whether the Active Graph needs resetting which it may do if the
                // evaluation of the 
                context.Data.ResetActiveGraph();
                datasetOk = false;
            }

            // Reset Active Graph for the WHERE
            if (defGraphOk)
            {
                context.Data.ResetActiveGraph();
                defGraphOk = false;
            }

            // Get the Graph to which we are deleting and inserting
            IGraph g;
            var newGraph = false;
            if (context.Data.HasGraph(cmd.WithGraphName))
            {
                g = context.Data.GetModifiableGraph(cmd.WithGraphName);
            }
            else
            {
                // Inserting into a new graph. This will raise an exception if the dataset is immutable
                context.Data.AddGraph(new Graph(cmd.WithGraphName));
                g = context.Data.GetModifiableGraph(cmd.WithGraphName);
                newGraph = true;
            }

            // Delete the Triples for each Solution
            var deletedTriples = new List<Triple>();
            foreach (ISet s in results.Sets)
            {
                try
                {
                    // If the Default Graph is non-existent then Deletions have no effect on it
                    if (g != null)
                    {
                        var constructContext = new ConstructContext(g, true) {Set = s};
                        foreach (IConstructTriplePattern p in cmd.DeletePattern.TriplePatterns.OfType<IConstructTriplePattern>())
                        {
                            try
                            {
                                deletedTriples.Add(p.Construct(constructContext));
                            }
                            catch (RdfQueryException)
                            {
                                // If we get an error here then we couldn't construct a specific
                                // triple so we continue anyway
                            }
                        }
                        g.Retract(deletedTriples);
                    }
                }
                catch (RdfQueryException)
                {
                    // If we get an error here this means we couldn't construct for this solution so the
                    // solution is ignored for this graph
                }

                // Triples from GRAPH clauses
                foreach (GraphPattern gp in cmd.DeletePattern.ChildGraphPatterns)
                {
                    deletedTriples.Clear();
                    try
                    {
                        string graphUri;
                        switch (gp.GraphSpecifier.TokenType)
                        {
                            case Token.URI:
                                graphUri = gp.GraphSpecifier.Value;
                                break;
                            case Token.VARIABLE:
                                var graphVar = gp.GraphSpecifier.Value.Substring(1);
                                if (s.ContainsVariable(graphVar))
                                {
                                    INode temp = s[graphVar];
                                    if (temp == null)
                                    {
                                        // If the Variable is not bound then skip
                                        continue;
                                    }
                                    else if (temp.NodeType == NodeType.Uri)
                                    {
                                        graphUri = temp.ToSafeString();
                                    }
                                    else
                                    {
                                        // If the Variable is not bound to a URI then skip
                                        continue;
                                    }
                                }
                                else
                                {
                                    // If the Variable is not bound for this solution then skip
                                    continue;
                                }
                                break;
                            default:
                                // Any other Graph Specifier we have to ignore this solution
                                continue;
                        }

                        var graphName = new UriNode(UriFactory.Root.Create(graphUri));
                        // If the Dataset doesn't contain the Graph then no need to do the Deletions
                        if (!context.Data.HasGraph(graphName)) continue;

                        // Do the actual Deletions
                        IGraph h = context.Data.GetModifiableGraph(graphName);
                        var constructContext = new ConstructContext(h, true) {Set = s};
                        foreach (IConstructTriplePattern p in gp.TriplePatterns.OfType<IConstructTriplePattern>())
                        {
                            try
                            {
                                deletedTriples.Add(p.Construct(constructContext));
                            }
                            catch (RdfQueryException)
                            {
                                // If we get an error here then we couldn't construct a specific triple
                                // so we continue anyway
                            }
                        }
                        h.Retract(deletedTriples);
                    }
                    catch (RdfQueryException)
                    {
                        // If we get an error here this means we couldn't construct for this solution so the
                        // solution is ignore for this graph
                    }
                }
            }

            // Insert the Triples for each Solution
            foreach (ISet s in results.Sets)
            {
                var insertedTriples = new List<Triple>();
                try
                {
                    var constructContext = new ConstructContext(g, true) { Set = s };
                    foreach (IConstructTriplePattern p in cmd.InsertPattern.TriplePatterns.OfType<IConstructTriplePattern>())
                    {
                        try
                        {
                            insertedTriples.Add(p.Construct(constructContext));
                        }
                        catch (RdfQueryException)
                        {
                            // If we get an error here then we couldn't construct a specific triple
                            // so we continue anyway
                        }
                    }
                    //g.Assert(insertedTriples.Select(t => t.IsGroundTriple ? t : t.CopyTriple(g)));
                    g.Assert(insertedTriples);
                }
                catch (RdfQueryException)
                {
                    // If we get an error here this means we couldn't construct for this solution so the
                    // solution is ignored for this graph
                }

                if (insertedTriples.Count == 0 && newGraph && cmd.WithGraphName != null)
                {
                    // Remove the named graph we added as we did not insert any triples
                    context.Data.RemoveGraph(cmd.WithGraphName);
                }

                // Triples from GRAPH clauses
                foreach (GraphPattern gp in cmd.InsertPattern.ChildGraphPatterns)
                {
                    insertedTriples.Clear();
                    try
                    {
                        string graphUri;
                        switch (gp.GraphSpecifier.TokenType)
                        {
                            case Token.URI:
                                graphUri = gp.GraphSpecifier.Value;
                                break;
                            case Token.VARIABLE:
                                var graphVar = gp.GraphSpecifier.Value.Substring(1);
                                if (s.ContainsVariable(graphVar))
                                {
                                    INode temp = s[graphVar];
                                    if (temp == null)
                                    {
                                        // If the Variable is not bound then skip
                                        continue;
                                    }
                                    else if (temp.NodeType == NodeType.Uri)
                                    {
                                        graphUri = temp.ToSafeString();
                                    }
                                    else
                                    {
                                        // If the Variable is not bound to a URI then skip
                                        continue;
                                    }
                                }
                                else
                                {
                                    // If the Variable is not bound for this solution then skip
                                    continue;
                                }
                                break;
                            default:
                                // Any other Graph Specifier we have to ignore this solution
                                continue;
                        }

                        // Ensure the Graph we're inserting to exists in the dataset creating it if necessary
                        IGraph h;
                        IRefNode destGraph = new UriNode(UriFactory.Root.Create(graphUri));
                        if (context.Data.HasGraph(destGraph))
                        {
                            h = context.Data.GetModifiableGraph(destGraph);
                        }
                        else
                        {
                            h = new Graph(destGraph);
                            context.Data.AddGraph(h);
                            h = context.Data.GetModifiableGraph(destGraph);
                        }

                        // Do the actual Insertions
                        var constructContext = new ConstructContext(h, true) { Set = s };
                        foreach (IConstructTriplePattern p in gp.TriplePatterns.OfType<IConstructTriplePattern>())
                        {
                            try
                            {
                                Triple t = p.Construct(constructContext);
                                t = new Triple(t.Subject, t.Predicate, t.Object);
                                insertedTriples.Add(t);
                            }
                            catch (RdfQueryException)
                            {
                                // If we get an error here this means we couldn't construct a specific
                                // triple so we continue anyway
                            }
                        }
                        //h.Assert(insertedTriples.Select(t => t.IsGroundTriple ? t : t.CopyTriple(h)));
                        h.Assert(insertedTriples);
                    }
                    catch (RdfQueryException)
                    {
                        // If we get an error here this means we couldn't construct for this solution so the
                        // solution is ignored for this graph
                    }
                }
            }
        }
        finally
        {
            // If the Dataset was set and an error occurred in doing the WHERE clause then
            // we'll need to Reset the Active Graph
            if (datasetOk) context.Data.ResetActiveGraph();
            if (defGraphOk) context.Data.ResetActiveGraph();
        }
    }

    /// <summary>
    /// Processes a MOVE command.
    /// </summary>
    /// <param name="cmd">Move Command.</param>
    public void ProcessMoveCommand(MoveCommand cmd)
    {
        ProcessMoveCommandInternal(cmd, GetContext());
    }

    /// <summary>
    /// Processes a MOVE command.
    /// </summary>
    /// <param name="cmd">Move Command.</param>
    /// <param name="context">SPARQL Update Evaluation Context.</param>
    protected virtual void ProcessMoveCommandInternal(MoveCommand cmd, SparqlUpdateEvaluationContext context)
    {
        try
        {
            // If Source and Destination are same this is a no-op
            if (EqualityHelper.AreRefNodesEqual(cmd.SourceGraphName, cmd.DestinationGraphName)) return;

            if (context.Data.HasGraph(cmd.SourceGraphName))
            {
                // Get the Source Graph
                IGraph source = context.Data.GetModifiableGraph(cmd.SourceGraphName);

                // Create/Delete/Clear the Destination Graph
                IGraph dest;
                if (context.Data.HasGraph(cmd.DestinationGraphName))
                {
                    if (cmd.DestinationGraphName == null)
                    {
                        dest = context.Data.GetModifiableGraph(cmd.DestinationGraphName);
                        dest.Clear();
                    }
                    else
                    {
                        context.Data.RemoveGraph(cmd.DestinationGraphName);
                        dest = new Graph(cmd.DestinationGraphName);
                        context.Data.AddGraph(dest);
                        dest = context.Data.GetModifiableGraph(cmd.DestinationGraphName);
                    }
                }
                else
                {
                    dest = new Graph(cmd.DestinationGraphName);
                    context.Data.AddGraph(dest);
                }

                // Move data from the Source into the Destination
                dest.Merge(source);

                // Delete/Clear the Source Graph
                if (cmd.SourceGraphName == null)
                {
                    source.Clear();
                }
                else
                {
                    context.Data.RemoveGraph(cmd.SourceGraphName);
                }
            }
            else
            {
                // Only show error if not Silent
                if (!cmd.Silent)
                {
                    if (cmd.SourceGraphName != null)
                    {
                        throw new SparqlUpdateException("Cannot MOVE from Graph " + cmd.SourceGraphName + " as it does not exist");
                    }
                    else
                    {
                        // This would imply a more fundamental issue with the Dataset not understanding that null means default graph
                        throw new SparqlUpdateException("Cannot MOVE from the Default Graph as it does not exist");
                    }
                }
            }
        }
        catch
        {
            // If not silent throw the exception upwards
            if (!cmd.Silent) throw;
        }
    }

}
