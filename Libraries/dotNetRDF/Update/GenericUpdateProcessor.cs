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

using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Tokens;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Construct;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Storage;
using VDS.RDF.Update.Commands;

namespace VDS.RDF.Update
{
    /// <summary>
    /// SPARQL Update Processor which processes commands against a generic underlying store represented by an <see cref="IStorageProvider">IStorageProvider</see> implementation
    /// </summary>
    /// <remarks>
    /// <para>
    /// If the provided manager also implements the <see cref="IUpdateableStorage">IUpdateableStorage</see> interface then the managers native SPARQL Update implementation will be used for the non-type specific calls i.e. <see cref="GenericUpdateProcessor.ProcessCommand">ProcessCommand()</see> and <see cref="GenericUpdateProcessor.ProcessCommandSet">ProcessCommandSet()</see>.  At all other times the SPARQL Update commands will be processed by approximating their behaviour through calls to <see cref="IStorageProvider.SaveGraph">SaveGraph()</see>, <see cref="IStorageProvider.LoadGraph(IGraph, Uri)">LoadGraph()</see> and <see cref="IStorageProvider.UpdateGraph(Uri, System.Collections.Generic.IEnumerable{VDS.RDF.Triple}, IEnumerable{Triple})">UpdateGraph()</see> in addition to local in-memory manipulation of the data.  Some commands such as INSERT and DELETE can only be processed when the manager is also a <see cref="IQueryableStorage">IQueryableStorage</see> since they rely on making a query and performing actions based on the results of that query.
    /// </para>
    /// <para>
    /// The performance of this processor is somewhat dependent on the underlying <see cref="IStorageProvider">IStorageProvider</see>.  If the underlying manager supports triple level updates as indicated by the <see cref="IStorageCapabilities.UpdateSupported">UpdateSupported</see> property then operations can be performed quite efficiently, if this is not the case then any operation which modifies a Graph will need to load the existing Graph from the store, make the modifications locally in-memory and then save the resulting Graph back to the Store
    /// </para>
    /// </remarks>
    public class GenericUpdateProcessor 
        : ISparqlUpdateProcessor
    {
        private IStorageProvider _manager;

        /// <summary>
        /// Creates a new Generic Update Processor
        /// </summary>
        /// <param name="manager">Generic IO Manager</param>
        public GenericUpdateProcessor(IStorageProvider manager)
        {
            if (manager.IsReadOnly) throw new ArgumentException("Cannot create a GenericUpdateProcessor for a store which is read-only", "manager");
            _manager = manager;
        }

        /// <summary>
        /// Discards any outstanding changes
        /// </summary>
        public virtual void Discard()
        {
            // Does Nothing
        }

        /// <summary>
        /// Flushes any outstanding changes to the underlying store
        /// </summary>
        public virtual void Flush()
        {
            // Does Nothing
        }

        /// <summary>
        /// Processes an ADD command
        /// </summary>
        /// <param name="cmd">Add Command</param>
        public void ProcessAddCommand(AddCommand cmd)
        {
            if (_manager is IUpdateableStorage)
            {
                ((IUpdateableStorage)_manager).Update(cmd.ToString());
            }
            else
            {
                try
                {
                    // If Source and Destination are equal this is a no-op
                    if (EqualityHelper.AreUrisEqual(cmd.SourceUri, cmd.DestinationUri)) return;

                    // Firstly check that appropriate IO Behaviour is provided
                    if (cmd.SourceUri == null || cmd.DestinationUri == null)
                    {
                        if ((_manager.IOBehaviour & IOBehaviour.HasDefaultGraph) == 0) throw new SparqlUpdateException("The underlying store does not provide support for an explicit unnamed Default Graph required to process this command");
                    }
                    IOBehaviour desired = cmd.DestinationUri == null ? IOBehaviour.OverwriteDefault : IOBehaviour.OverwriteNamed;
                    if ((_manager.IOBehaviour & desired) == 0) throw new SparqlUpdateException("The underlying store does not provide the required IO Behaviour to implement this command");

                    // Load Source Graph
                    Graph source = new Graph();
                    _manager.LoadGraph(source, cmd.SourceUri);
                    source.BaseUri = cmd.SourceUri;

                    // Load Destination Graph
                    Graph dest = new Graph();
                    _manager.LoadGraph(dest, cmd.DestinationUri);
                    dest.BaseUri = cmd.DestinationUri;

                    // Transfer the data and update the Destination Graph
                    dest.Merge(source);
                    _manager.SaveGraph(dest);
                }
                catch
                {
                    if (!cmd.Silent) throw;
                }
            }
        }

        /// <summary>
        /// Processes a CLEAR command
        /// </summary>
        /// <param name="cmd">Clear Command</param>
        /// <remarks>
        /// Implemented by replacing the Graph with an empty Graph
        /// </remarks>
        public void ProcessClearCommand(ClearCommand cmd)
        {
            if (_manager is IUpdateableStorage)
            {
                ((IUpdateableStorage)_manager).Update(cmd.ToString());
            }
            else 
            {
                try
                {
                    IGraph g;

                    switch (cmd.Mode)
                    {
                        case ClearMode.Default:
                        case ClearMode.Graph:
                            if (cmd.TargetUri == null && (_manager.IOBehaviour & IOBehaviour.HasDefaultGraph) == 0) throw new SparqlUpdateException("Unable to clear the default graph as the underlying store does not support an explicit default graph");
                            if (cmd.TargetUri != null && (_manager.IOBehaviour & IOBehaviour.HasNamedGraphs) == 0) throw new SparqlUpdateException("Unable to clear a named graph as the underlying store does not support named graphs");

                            if ((cmd.TargetUri == null && (_manager.IOBehaviour & IOBehaviour.OverwriteDefault) != 0) || (cmd.TargetUri != null && (_manager.IOBehaviour & IOBehaviour.OverwriteNamed) != 0))
                            {
                                // Can approximate by saving an empty Graph over the existing Graph
                                g = new Graph
                                {
                                    BaseUri = cmd.TargetUri,
                                };
                                _manager.SaveGraph(g);
                            }
                            else if (_manager.UpdateSupported && (_manager.IOBehaviour & IOBehaviour.CanUpdateDeleteTriples) != 0)
                            {
                                // Can approximate by loading the Graph and then deleting all Triples from it
                                g = new NonIndexedGraph();
                                _manager.LoadGraph(g, cmd.TargetUri);
                                _manager.UpdateGraph(cmd.TargetUri, null, g.Triples);
                            }
                            else
                            {
                                throw new SparqlUpdateException("Unable to evaluate a CLEAR command as the underlying store does not provide appropriate IO Behaviour to approximate this command");
                            }
                            break;

                        case ClearMode.Named:
                        case ClearMode.All:
                            if ((_manager.IOBehaviour & IOBehaviour.HasNamedGraphs) == 0) throw new SparqlUpdateException("Unable to clear named graphs as the underlying store does not support named graphs");

                            if (_manager.ListGraphsSupported)
                            {
                                List<Uri> graphs = _manager.ListGraphs().ToList();
                                foreach (var u in graphs)
                                {
                                    if ((u == null && (_manager.IOBehaviour & IOBehaviour.OverwriteDefault) != 0) || (u != null && (_manager.IOBehaviour & IOBehaviour.OverwriteNamed) != 0))
                                    {
                                        // Can approximate by saving an empty Graph over the existing Graph
                                        g = new Graph {BaseUri = u};
                                        _manager.SaveGraph(g);
                                    }
                                    else if (_manager.UpdateSupported && (_manager.IOBehaviour & IOBehaviour.CanUpdateDeleteTriples) != 0)
                                    {
                                        // Can approximate by loading the Graph and then deleting all Triples from it
                                        g = new NonIndexedGraph();
                                        _manager.LoadGraph(g, u);
                                        _manager.UpdateGraph(u, null, g.Triples);
                                    }
                                    else
                                    {
                                        throw new SparqlUpdateException("Unable to evaluate a CLEAR command as the underlying store does not provide appropriate IO Behaviour to approximate this command");
                                    }
                                }
                            }
                            else
                            {
                                throw new NotSupportedException("The Generic Update processor does not support this form of the CLEAR command");
                            }
                            break;
                    }
                }
                catch
                {
                    if (!cmd.Silent) throw;
                }
            }
        }

        /// <summary>
        /// Processes a COPY command
        /// </summary>
        /// <param name="cmd">Copy Command</param>
        public void ProcessCopyCommand(CopyCommand cmd)
        {
            if (_manager is IUpdateableStorage)
            {
                ((IUpdateableStorage)_manager).Update(cmd.ToString());
            }
            else
            {
                try
                {
                    // If Source and Destination are equal this is a no-op
                    if (EqualityHelper.AreUrisEqual(cmd.SourceUri, cmd.DestinationUri)) return;

                    // Firstly check that appropriate IO Behaviour is provided
                    if (cmd.SourceUri == null || cmd.DestinationUri == null)
                    {
                        if ((_manager.IOBehaviour & IOBehaviour.HasDefaultGraph) == 0) throw new SparqlUpdateException("The underlying store does not provide support for an explicit unnamed Default Graph required to process this command");
                    }
                    IOBehaviour desired = cmd.DestinationUri == null ? IOBehaviour.OverwriteDefault : IOBehaviour.OverwriteNamed;
                    if ((_manager.IOBehaviour & desired) == 0) throw new SparqlUpdateException("The underlying store does not provide the required IO Behaviour to implement this command");

                    Graph source = new Graph();
                    _manager.LoadGraph(source, cmd.SourceUri);
                    source.BaseUri = cmd.SourceUri;

                    // If the Manager supports delete ensure the Destination Graph is deleted
                    if (_manager.DeleteSupported)
                    {
                        try
                        {
                            _manager.DeleteGraph(cmd.DestinationUri);
                        }
                        catch (Exception ex)
                        {
                            throw new SparqlUpdateException("Unable to process a COPY command as unable to ensure that the Destination Graph was deleted prior to moving the data from the Source Graph", ex);
                        }
                    }

                    // Load Destination Graph and ensure it is empty
                    Graph dest = new Graph();
                    _manager.LoadGraph(dest, cmd.DestinationUri);
                    dest.BaseUri = cmd.DestinationUri;
                    dest.Clear();

                    // Transfer the data and update both the Graphs
                    dest.Merge(source);
                    _manager.SaveGraph(dest);
                }
                catch
                {
                    if (!cmd.Silent) throw;
                }
            }
        }

        /// <summary>
        /// Processes a CREATE command
        /// </summary>
        /// <param name="cmd">Create Command</param>
        /// <remarks>
        /// <para>
        /// Implemented by adding an empty Graph to the Store
        /// </para>
        /// <para>
        /// <strong>Warning:</strong> As the <see cref="IStorageProvider">IStorageProvider</see> interface does not allow checking whether a Graph exists processing CREATE commands can result in overwriting existing Graphs
        /// </para>
        /// </remarks>
        public void ProcessCreateCommand(CreateCommand cmd)
        {
            if (_manager is IUpdateableStorage)
            {
                ((IUpdateableStorage)_manager).Update(cmd.ToString());
            }
            else
            {
                Graph g = new Graph {BaseUri = cmd.TargetUri};

                try
                {
                    // As Khalil Ahmed pointed out the behaviour when the store doesn't support empty graphs the behaviour should be to act as if the operation succeeded
                    if ((_manager.IOBehaviour & IOBehaviour.ExplicitEmptyGraphs) == 0) return;

                    _manager.SaveGraph(g);
                }
                catch
                {
                    if (!cmd.Silent) throw;
                }
            }
        }

        /// <summary>
        /// Processes a command
        /// </summary>
        /// <param name="cmd">Command</param>
        /// <remarks>
        /// <para>
        /// If the provided manager also implements the <see cref="IUpdateableStorage">IUpdateableStorage</see> interface then the managers native SPARQL Update implementation will be used.
        /// </para>
        /// </remarks>
        public virtual void ProcessCommand(SparqlUpdateCommand cmd)
        {
            if (_manager is IUpdateableStorage)
            {
                ((IUpdateableStorage)_manager).Update(cmd.ToString());
            }
            else
            {
                switch (cmd.CommandType)
                {
                    case SparqlUpdateCommandType.Add:
                        ProcessAddCommand((AddCommand)cmd);
                        break;
                    case SparqlUpdateCommandType.Clear:
                        ProcessClearCommand((ClearCommand)cmd);
                        break;
                    case SparqlUpdateCommandType.Copy:
                        ProcessCopyCommand((CopyCommand)cmd);
                        break;
                    case SparqlUpdateCommandType.Create:
                        ProcessCreateCommand((CreateCommand)cmd);
                        break;
                    case SparqlUpdateCommandType.Delete:
                        ProcessDeleteCommand((DeleteCommand)cmd);
                        break;
                    case SparqlUpdateCommandType.DeleteData:
                        ProcessDeleteDataCommand((DeleteDataCommand)cmd);
                        break;
                    case SparqlUpdateCommandType.Drop:
                        ProcessDropCommand((DropCommand)cmd);
                        break;
                    case SparqlUpdateCommandType.Insert:
                        ProcessInsertCommand((InsertCommand)cmd);
                        break;
                    case SparqlUpdateCommandType.InsertData:
                        ProcessInsertDataCommand((InsertDataCommand)cmd);
                        break;
                    case SparqlUpdateCommandType.Load:
                        ProcessLoadCommand((LoadCommand)cmd);
                        break;
                    case SparqlUpdateCommandType.Modify:
                        ProcessModifyCommand((ModifyCommand)cmd);
                        break;
                    case SparqlUpdateCommandType.Move:
                        ProcessMoveCommand((MoveCommand)cmd);
                        break;
                    default:
                        throw new SparqlUpdateException("Unknown Update Commands cannot be processed by the Generic Update Processor");
                }
            }
        }

        /// <summary>
        /// Processes a command set
        /// </summary>
        /// <param name="commands">Command Set</param>
        /// <remarks>
        /// <para>
        /// If the provided manager also implements the <see cref="IUpdateableStorage">IUpdateableStorage</see> interface then the managers native SPARQL Update implementation will be used.
        /// </para>
        /// </remarks>
        public virtual void ProcessCommandSet(SparqlUpdateCommandSet commands)
        {
            DateTime start = DateTime.Now;
            commands.UpdateExecutionTime = null;
            try
            {
                if (_manager is IUpdateableStorage)
                {
                    ((IUpdateableStorage)_manager).Update(commands.ToString());
                }
                else
                {
                    for (int i = 0; i < commands.CommandCount; i++)
                    {
                        ProcessCommand(commands[i]);
                    }
                }
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
        /// <remarks>
        /// <para>
        /// <strong>Note:</strong> The underlying manager must implement the <see cref="IQueryableStorage">IQueryableStorage</see> interface in order for DELETE commands to be processed
        /// </para>
        /// </remarks>
        public void ProcessDeleteCommand(DeleteCommand cmd)
        {
            if (_manager is IUpdateableStorage)
            {
                ((IUpdateableStorage)_manager).Update(cmd.ToString());
            }
            else
            {
                if (_manager is IQueryableStorage)
                {
                    // Check IO Behaviour
                    // For a delete we either need the ability to Update Delete Triples or to Overwrite Graphs
                    // Firstly check behaviour persuant to default graph if applicable
                    if (cmd.DeletePattern.TriplePatterns.OfType<IConstructTriplePattern>().Any())
                    {
                        // Must support notion of default graph
                        if ((_manager.IOBehaviour & IOBehaviour.HasDefaultGraph) == 0) throw new SparqlUpdateException("The underlying store does not support the notion of an explicit unnamed Default Graph required to process this command");
                        // Must allow either OverwriteDefault or CanUpdateDeleteTriples
                        if ((_manager.IOBehaviour & IOBehaviour.CanUpdateDeleteTriples) == 0 && (_manager.IOBehaviour & IOBehaviour.OverwriteDefault) == 0) throw new SparqlUpdateException("The underlying store does not support the required IO Behaviour to implement this command");
                    }
                    // Then check behaviour persuant to named graphs if applicable
                    if (cmd.DeletePattern.HasChildGraphPatterns)
                    {
                        // Must support named graphs
                        if ((_manager.IOBehaviour & IOBehaviour.HasNamedGraphs) == 0) throw new SparqlUpdateException("The underlying store does not support the notion of named graphs required to process this command");
                        // Must allow either CanUpdateDeleteTriples or OverwriteNamed
                        if ((_manager.IOBehaviour & IOBehaviour.CanUpdateDeleteTriples) == 0 && (_manager.IOBehaviour & IOBehaviour.OverwriteNamed) == 0) throw new SparqlUpdateException("The underlying store does not support the required IO Behaviour to implement this command");
                    }

                    // First build and make the query to get a Result Set
                    String queryText = "SELECT * WHERE " + cmd.WherePattern.ToString();
                    SparqlQueryParser parser = new SparqlQueryParser();
                    SparqlQuery query = parser.ParseFromString(queryText);
                    if (cmd.GraphUri != null && !cmd.UsingUris.Any()) query.AddDefaultGraph(cmd.GraphUri);
                    foreach (Uri u in cmd.UsingUris)
                    {
                        query.AddDefaultGraph(u);
                    }
                    foreach (Uri u in cmd.UsingNamedUris)
                    {
                        query.AddNamedGraph(u);
                    }

                    Object results = ((IQueryableStorage)_manager).Query(query.ToString());
                    if (results is SparqlResultSet)
                    {
                        // Now need to transform the Result Set back to a Multiset
                        Multiset mset = new Multiset((SparqlResultSet)results);

                        // Generate the Triples for each Solution
                        List<Triple> deletedTriples = new List<Triple>();
                        Dictionary<String, List<Triple>> deletedGraphTriples = new Dictionary<string, List<Triple>>();
                        foreach (ISet s in mset.Sets)
                        {
                            List<Triple> tempDeletedTriples = new List<Triple>();
                            try
                            {
                                ConstructContext context = new ConstructContext(null, s, true);
                                foreach (IConstructTriplePattern p in cmd.DeletePattern.TriplePatterns.OfType<IConstructTriplePattern>())
                                {
                                    try
                                    {
                                        tempDeletedTriples.Add(p.Construct(context));
                                    }
                                    catch (RdfQueryException)
                                    {
                                        // If we get an error here then it means we couldn't construct a specific
                                        // triple so we continue anyway
                                    }
                                }
                                deletedTriples.AddRange(tempDeletedTriples);
                            }
                            catch (RdfQueryException)
                            {
                                // If we get an error here this means we couldn't construct for this solution so the
                                // solution is ignored for this graph
                            }

                            // Triples from GRAPH clauses
                            foreach (GraphPattern gp in cmd.DeletePattern.ChildGraphPatterns)
                            {
                                tempDeletedTriples.Clear();
                                try
                                {
                                    String graphUri;
                                    switch (gp.GraphSpecifier.TokenType)
                                    {
                                        case Token.URI:
                                            graphUri = gp.GraphSpecifier.Value;
                                            break;
                                        case Token.VARIABLE:
                                            String graphVar = gp.GraphSpecifier.Value.Substring(1);
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
                                    if (!deletedGraphTriples.ContainsKey(graphUri)) deletedGraphTriples.Add(graphUri, new List<Triple>());
                                    ConstructContext context = new ConstructContext(null, s, true);
                                    foreach (IConstructTriplePattern p in gp.TriplePatterns.OfType<IConstructTriplePattern>())
                                    {
                                        try
                                        {
                                            tempDeletedTriples.Add(p.Construct(context));
                                        }
                                        catch (RdfQueryException)
                                        {
                                            // If we get an error here then it means we couldn't construct a specific
                                            // triple so we continue anyway
                                        }
                                    }
                                    deletedGraphTriples[graphUri].AddRange(tempDeletedTriples);
                                }
                                catch (RdfQueryException)
                                {
                                    // If we throw an error this means we couldn't construct for this solution so the
                                    // solution is ignore for this graph
                                }
                            }
                        }

                        // Now decide how to apply the update
                        if (_manager.UpdateSupported)
                        {
                            _manager.UpdateGraph(cmd.GraphUri, Enumerable.Empty<Triple>(), deletedTriples);
                            foreach (KeyValuePair<String, List<Triple>> graphDeletion in deletedGraphTriples)
                            {
                                _manager.UpdateGraph(graphDeletion.Key, Enumerable.Empty<Triple>(), graphDeletion.Value);
                            }
                        }
                        else
                        {
                            Graph g = new Graph();
                            _manager.LoadGraph(g, cmd.GraphUri);
                            g.Retract(deletedTriples);
                            _manager.SaveGraph(g);

                            foreach (KeyValuePair<String, List<Triple>> graphDeletion in deletedGraphTriples)
                            {
                                g = new Graph();
                                _manager.LoadGraph(g, graphDeletion.Key);
                                g.Retract(graphDeletion.Value);
                                _manager.SaveGraph(g);
                            }
                        }
                    }
                    else
                    {
                        throw new SparqlUpdateException("Cannot evaluate an DELETE Command as the underlying Store failed to answer the query for the WHERE portion of the command as expected");
                    }
                }
                else
                {
                    throw new NotSupportedException("DELETE commands are not supported by this Update Processor as the manager for the underlying Store does not provide Query capabilities which are necessary to process this command");
                }
            }
        }

        /// <summary>
        /// Processes a DELETE DATA command
        /// </summary>
        /// <param name="cmd">DELETE Data Command</param>
        public void ProcessDeleteDataCommand(DeleteDataCommand cmd)
        {
            if (_manager is IUpdateableStorage)
            {
                ((IUpdateableStorage)_manager).Update(cmd.ToString());
            }
            else
            {
                // Check IO Behaviour
                // For a delete we either need the ability to Update Delete Triples or to Overwrite Graphs
                // Firstly check behaviour persuant to default graph if applicable
                if (cmd.DataPattern.TriplePatterns.OfType<IConstructTriplePattern>().Any())
                {
                    // Must support notion of default graph
                    if ((_manager.IOBehaviour & IOBehaviour.HasDefaultGraph) == 0) throw new SparqlUpdateException("The underlying store does not support the notion of an explicit unnamed Default Graph required to process this command");
                    // Must allow either OverwriteDefault or CanUpdateDeleteTriples
                    if ((_manager.IOBehaviour & IOBehaviour.CanUpdateDeleteTriples) == 0 && (_manager.IOBehaviour & IOBehaviour.OverwriteDefault) == 0) throw new SparqlUpdateException("The underlying store does not support the required IO Behaviour to implement this command");
                }
                // Then check behaviour persuant to named graphs if applicable
                if (cmd.DataPattern.HasChildGraphPatterns)
                {
                    // Must support named graphs
                    if ((_manager.IOBehaviour & IOBehaviour.HasNamedGraphs) == 0) throw new SparqlUpdateException("The underlying store does not support the notion of named graphs required to process this command");
                    // Must allow either CanUpdateDeleteTriples or OverwriteNamed
                    if ((_manager.IOBehaviour & IOBehaviour.CanUpdateDeleteTriples) == 0 && (_manager.IOBehaviour & IOBehaviour.OverwriteNamed) == 0) throw new SparqlUpdateException("The underlying store does not support the required IO Behaviour to implement this command");
                }

                // Split the Pattern into the set of Graph Patterns
                List<GraphPattern> patterns = new List<GraphPattern>();
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
                    if (!IsValidDataPattern(pattern, false)) throw new SparqlUpdateException("Cannot evaluate a DELETE DATA command where any of the Triple Patterns are not concrete triples - variables are not permitted");

                    Uri graphUri = null;
                    if (pattern.IsGraph)
                    {
                        switch (pattern.GraphSpecifier.TokenType)
                        {
                            case Token.QNAME:
                                throw new NotSupportedException("Graph Specifiers as QNames for DELETE DATA Commands are not supported - please specify an absolute URI instead");
                            case Token.URI:
                                graphUri = UriFactory.Create(pattern.GraphSpecifier.Value);
                                break;
                            default:
                                throw new SparqlUpdateException("Cannot evaluate an DELETE DATA Command as the Graph Specifier is not a QName/URI");
                        }
                    }

                    Graph g = new Graph();
                    if (!_manager.UpdateSupported)
                    {
                        // If the Graph to be deleted from is empty then can skip as will have no affect on the Graph
                        _manager.LoadGraph(g, graphUri);
                        if (g.IsEmpty) continue;
                    }
                    // Note that if the Manager supports Triple Level updates we won't load the Graph
                    // so we can't know whether it is empty or not and so can't skip the delete step

                    // Delete the actual Triples
                    INode subj, pred, obj;
                    ConstructContext context = new ConstructContext(g, null, false);
                    foreach (IConstructTriplePattern p in pattern.TriplePatterns.OfType<IConstructTriplePattern>())
                    {
                        subj = p.Subject.Construct(context);
                        pred = p.Predicate.Construct(context);
                        obj = p.Object.Construct(context);

                        if (!_manager.UpdateSupported)
                        {
                            // If we don't support update then we'll have loaded the existing graph
                            // so we'll use this to remove the relevant triples to get to the intended state of
                            // the graph
                            g.Retract(new Triple(subj, pred, obj));
                        }
                        else
                        {
                            // If we do support update then we'll have an empty graph which we'll use to store
                            // up the set of triples to be removed
                            g.Assert(new Triple(subj, pred, obj));
                        }
                    }

                    if (_manager.UpdateSupported)
                    {
                        _manager.UpdateGraph(graphUri, Enumerable.Empty<Triple>(), g.Triples);
                    }
                    else
                    {
                        _manager.SaveGraph(g);
                    }
                }
            }
        }

        /// <summary>
        /// Processes a DROP command
        /// </summary>
        /// <param name="cmd">Drop Command</param>
        public void ProcessDropCommand(DropCommand cmd)
        {
            if (_manager is IUpdateableStorage)
            {
                ((IUpdateableStorage)_manager).Update(cmd.ToString());
            }
            else
            {
                try
                {
                    Graph g;
                    switch (cmd.Mode)
                    {
                        case ClearMode.Graph:
                        case ClearMode.Default:
                            if (_manager.DeleteSupported)
                            {
                                // If available use DeleteGraph()
                                _manager.DeleteGraph(cmd.TargetUri);
                            }
                            else if ((cmd.TargetUri == null && (_manager.IOBehaviour & IOBehaviour.OverwriteDefault) != 0) || (cmd.TargetUri != null && (_manager.IOBehaviour & IOBehaviour.OverwriteNamed) != 0))
                            {
                                // Can approximate by saving an empty Graph over the existing Graph
                                g = new Graph {BaseUri = cmd.TargetUri};
                                _manager.SaveGraph(g);
                            }
                            else if (_manager.UpdateSupported && (_manager.IOBehaviour & IOBehaviour.CanUpdateDeleteTriples) != 0)
                            {
                                // Can approximate by loading the Graph and then deleting all Triples from it
                                g = new NonIndexedGraph();
                                _manager.LoadGraph(g, cmd.TargetUri);
                                _manager.UpdateGraph(cmd.TargetUri, null, g.Triples);
                            }
                            else
                            {
                                throw new SparqlUpdateException("Unable to evaluate a DROP command as the underlying store does not provide appropriate IO Behaviour to approximate this command");
                            }
                            break;

                        case ClearMode.All:
                        case ClearMode.Named:
                            if (_manager.ListGraphsSupported)
                            {
                                List<Uri> graphs = _manager.ListGraphs().ToList();
                                foreach (Uri u in graphs)
                                {
                                    if (_manager.DeleteSupported)
                                    {
                                        // If available use DeleteGraph()
                                        _manager.DeleteGraph(u);
                                    }
                                    else if ((u == null && (_manager.IOBehaviour & IOBehaviour.OverwriteDefault) != 0) || (u != null && (_manager.IOBehaviour & IOBehaviour.OverwriteNamed) != 0))
                                    {
                                        // Can approximate by saving an empty Graph over the existing Graph
                                        g = new Graph {BaseUri = u};
                                        _manager.SaveGraph(g);
                                    }
                                    else if (_manager.UpdateSupported && (_manager.IOBehaviour & IOBehaviour.CanUpdateDeleteTriples) != 0)
                                    {
                                        // Can approximate by loading the Graph and then deleting all Triples from it
                                        g = new NonIndexedGraph();
                                        _manager.LoadGraph(g, u);
                                        _manager.UpdateGraph(u, null, g.Triples);
                                    }
                                    else
                                    {
                                        throw new SparqlUpdateException("Unable to evaluate a DROP command as the underlying store does not provide appropriate IO Behaviour to approximate this command");
                                    }
                                }
                            }
                            else
                            {
                                throw new NotSupportedException("The Generic Update processor does not support this form of the DROP command");
                            }
                            break;
                    }
                }
                catch
                {
                    if (!cmd.Silent) throw;
                }
            }
        }

        /// <summary>
        /// Processes an INSERT command
        /// </summary>
        /// <param name="cmd">Insert Command</param>
        /// <remarks>
        /// <para>
        /// <strong>Note:</strong> The underlying manager must implement the <see cref="IQueryableStorage">IQueryableStorage</see> interface in order for INSERT commands to be processed
        /// </para>
        /// </remarks>
        public void ProcessInsertCommand(InsertCommand cmd)
        {
            if (_manager is IUpdateableStorage)
            {
                ((IUpdateableStorage)_manager).Update(cmd.ToString());
            }
            else
            {
                if (_manager is IQueryableStorage)
                {
                    // Check IO Behaviour
                    // For a insert we either need the ability to Update Add Triples or to Overwrite Graphs
                    // Firstly check behaviour persuant to default graph if applicable
                    if (cmd.InsertPattern.TriplePatterns.OfType<IConstructTriplePattern>().Any())
                    {
                        // Must support notion of default graph
                        if ((_manager.IOBehaviour & IOBehaviour.HasDefaultGraph) == 0) throw new SparqlUpdateException("The underlying store does not support the notion of an explicit unnamed Default Graph required to process this command");
                        // Must allow either OverwriteDefault or CanUpdateAddTriples
                        if ((_manager.IOBehaviour & IOBehaviour.CanUpdateAddTriples) == 0 && (_manager.IOBehaviour & IOBehaviour.OverwriteDefault) == 0) throw new SparqlUpdateException("The underlying store does not support the required IO Behaviour to implement this command");
                    }
                    // Then check behaviour persuant to named graphs if applicable
                    if (cmd.InsertPattern.HasChildGraphPatterns)
                    {
                        // Must support named graphs
                        if ((_manager.IOBehaviour & IOBehaviour.HasNamedGraphs) == 0) throw new SparqlUpdateException("The underlying store does not support the notion of named graphs required to process this command");
                        // Must allow either CanUpdateAddTriples or OverwriteNamed
                        if ((_manager.IOBehaviour & IOBehaviour.CanUpdateAddTriples) == 0 && (_manager.IOBehaviour & IOBehaviour.OverwriteNamed) == 0) throw new SparqlUpdateException("The underlying store does not support the required IO Behaviour to implement this command");
                    }

                    // First build and make the query to get a Result Set
                    String queryText = "SELECT * WHERE " + cmd.WherePattern.ToString();
                    SparqlQueryParser parser = new SparqlQueryParser();
                    SparqlQuery query = parser.ParseFromString(queryText);
                    if (cmd.GraphUri != null && !cmd.UsingUris.Any()) query.AddDefaultGraph(cmd.GraphUri);
                    foreach (Uri u in cmd.UsingUris)
                    {
                        query.AddDefaultGraph(u);
                    }
                    foreach (Uri u in cmd.UsingNamedUris)
                    {
                        query.AddNamedGraph(u);
                    }

                    Object results = ((IQueryableStorage)_manager).Query(query.ToString());
                    if (results is SparqlResultSet)
                    {
                        // Now need to transform the Result Set back to a Multiset
                        Multiset mset = new Multiset((SparqlResultSet)results);

                        // Generate the Triples for each Solution
                        List<Triple> insertedTriples = new List<Triple>();
                        Dictionary<String, List<Triple>> insertedGraphTriples = new Dictionary<string, List<Triple>>();
                        foreach (ISet s in mset.Sets)
                        {
                            List<Triple> tempInsertedTriples = new List<Triple>();
                            try
                            {
                                ConstructContext context = new ConstructContext(null, s, true);
                                foreach (IConstructTriplePattern p in cmd.InsertPattern.TriplePatterns.OfType<IConstructTriplePattern>())
                                {
                                    try
                                    {
                                        tempInsertedTriples.Add(p.Construct(context));
                                    }
                                    catch (RdfQueryException)
                                    {
                                        // If we get an error here then it means we couldn't construct a specific
                                        // triple so we continue anyway
                                    }
                                }
                                insertedTriples.AddRange(tempInsertedTriples);
                            }
                            catch (RdfQueryException)
                            {
                                // If we get an error here this means we couldn't construct for this solution so the
                                // solution is ignore for this graph
                            }

                            // Triples from GRAPH clauses
                            foreach (GraphPattern gp in cmd.InsertPattern.ChildGraphPatterns)
                            {
                                tempInsertedTriples.Clear();
                                try
                                {
                                    String graphUri;
                                    switch (gp.GraphSpecifier.TokenType)
                                    {
                                        case Token.URI:
                                            graphUri = gp.GraphSpecifier.Value;
                                            break;
                                        case Token.VARIABLE:
                                            String graphVar = gp.GraphSpecifier.Value.Substring(1);
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
                                    if (!insertedGraphTriples.ContainsKey(graphUri)) insertedGraphTriples.Add(graphUri, new List<Triple>());
                                    ConstructContext context = new ConstructContext(null, s, true);
                                    foreach (IConstructTriplePattern p in gp.TriplePatterns.OfType<IConstructTriplePattern>())
                                    {
                                        try
                                        {
                                            tempInsertedTriples.Add(p.Construct(context));
                                        }
                                        catch (RdfQueryException)
                                        {
                                            // If we get an error here then it means we couldn't construct a specific
                                            // triple so we continue anyway
                                        }
                                    }
                                    insertedGraphTriples[graphUri].AddRange(tempInsertedTriples);
                                }
                                catch (RdfQueryException)
                                {
                                    // If we get an error here this means we couldn't construct for this solution so the
                                    // solution is ignore for this graph
                                }
                            }
                        }

                        // Now decide how to apply the update
                        if (_manager.UpdateSupported)
                        {
                            _manager.UpdateGraph(cmd.GraphUri, insertedTriples, Enumerable.Empty<Triple>());
                            foreach (KeyValuePair<String, List<Triple>> graphInsertion in insertedGraphTriples)
                            {
                                _manager.UpdateGraph(graphInsertion.Key, graphInsertion.Value, Enumerable.Empty<Triple>());
                            }
                        }
                        else
                        {
                            Graph g = new Graph();
                            _manager.LoadGraph(g, cmd.GraphUri);
                            g.Assert(insertedTriples);
                            _manager.SaveGraph(g);

                            foreach (KeyValuePair<String, List<Triple>> graphInsertion in insertedGraphTriples)
                            {
                                g = new Graph();
                                _manager.LoadGraph(g, graphInsertion.Key);
                                g.Assert(graphInsertion.Value);
                                _manager.SaveGraph(g);
                            }
                        }
                    }
                    else
                    {
                        throw new SparqlUpdateException("Cannot evaluate an INSERT Command as the underlying Store failed to answer the query for the WHERE portion of the command as expected");
                    }
                }
                else
                {
                    throw new NotSupportedException("INSERT commands are not supported by this Update Processor as the manager for the underlying Store does not provide Query capabilities which are necessary to process this command");
                }
            }
        }

        /// <summary>
        /// Processes an INSERT DATA command
        /// </summary>
        /// <param name="cmd">Insert Data Command</param>
        public void ProcessInsertDataCommand(InsertDataCommand cmd)
        {
            if (_manager is IUpdateableStorage)
            {
                ((IUpdateableStorage)_manager).Update(cmd.ToString());
            }
            else
            {
                // Check IO Behaviour
                // For a insert we either need the ability to Update Delete Triples or to Overwrite Graphs
                // Firstly check behaviour persuant to default graph if applicable
                if (cmd.DataPattern.TriplePatterns.OfType<IConstructTriplePattern>().Any())
                {
                    // Must support notion of default graph
                    if ((_manager.IOBehaviour & IOBehaviour.HasDefaultGraph) == 0) throw new SparqlUpdateException("The underlying store does not support the notion of an explicit unnamed Default Graph required to process this command");
                    // Must allow either OverwriteDefault or CanUpdateAddTriples
                    if ((_manager.IOBehaviour & IOBehaviour.CanUpdateAddTriples) == 0 && (_manager.IOBehaviour & IOBehaviour.OverwriteDefault) == 0) throw new SparqlUpdateException("The underlying store does not support the required IO Behaviour to implement this command");
                }
                // Then check behaviour persuant to named graphs if applicable
                if (cmd.DataPattern.HasChildGraphPatterns)
                {
                    // Must support named graphs
                    if ((_manager.IOBehaviour & IOBehaviour.HasNamedGraphs) == 0) throw new SparqlUpdateException("The underlying store does not support the notion of named graphs required to process this command");
                    // Must allow either CanUpdateAddTriples or OverwriteNamed
                    if ((_manager.IOBehaviour & IOBehaviour.CanUpdateAddTriples) == 0 && (_manager.IOBehaviour & IOBehaviour.OverwriteNamed) == 0) throw new SparqlUpdateException("The underlying store does not support the required IO Behaviour to implement this command");
                }

                // Split the Pattern into the set of Graph Patterns
                List<GraphPattern> patterns = new List<GraphPattern>();
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
                    if (!IsValidDataPattern(pattern, false)) throw new SparqlUpdateException("Cannot evaluate an INSERT DATA command where any of the Triple Patterns are not concrete triples - variables are not permitted");

                    Uri graphUri = null;
                    if (pattern.IsGraph)
                    {
                        switch (pattern.GraphSpecifier.TokenType)
                        {
                            case Token.QNAME:
                                throw new NotSupportedException("Graph Specifiers as QNames for INSERT DATA Commands are not supported - please specify an absolute URI instead");
                            case Token.URI:
                                graphUri = UriFactory.Create(pattern.GraphSpecifier.Value);
                                break;
                            default:
                                throw new SparqlUpdateException("Cannot evaluate an INSERT DATA Command as the Graph Specifier is not a QName/URI");
                        }
                    }

                    Graph g = new Graph();
                    if (!_manager.UpdateSupported) _manager.LoadGraph(g, graphUri);

                    // Insert the actual Triples
                    INode subj, pred, obj;
                    ConstructContext context = new ConstructContext(g, null, false);
                    foreach (IConstructTriplePattern p in pattern.TriplePatterns.OfType<IConstructTriplePattern>())
                    {
                        subj = p.Subject.Construct(context);
                        pred = p.Predicate.Construct(context);
                        obj = p.Object.Construct(context);

                        g.Assert(new Triple(subj, pred, obj));
                    }

                    if (_manager.UpdateSupported)
                    {
                        _manager.UpdateGraph(graphUri, g.Triples, Enumerable.Empty<Triple>());
                    }
                    else
                    {
                        _manager.SaveGraph(g);
                    }
                }
            }
        }

        /// <summary>
        /// Processes a LOAD command
        /// </summary>
        /// <param name="cmd">Load Command</param>
        public void ProcessLoadCommand(LoadCommand cmd)
        {
            if (_manager is IUpdateableStorage)
            {
                ((IUpdateableStorage)_manager).Update(cmd.ToString());
            }
            else
            {
                try
                {
                    // Check IO Behaviour
                    // For a load which is essentially an insert we either need the ability to Update Add Triples or to Overwrite Graphs
                    if (cmd.TargetUri == null)
                    {
                        // Must support notion of default graph
                        if ((_manager.IOBehaviour & IOBehaviour.HasDefaultGraph) == 0) throw new SparqlUpdateException("The underlying store does not support the notion of an explicit unnamed Default Graph required to process this command");
                        // Must allow either OverwriteDefault or CanUpdateDeleteTriples
                        if ((_manager.IOBehaviour & IOBehaviour.CanUpdateAddTriples) == 0 && (_manager.IOBehaviour & IOBehaviour.OverwriteDefault) == 0) throw new SparqlUpdateException("The underlying store does not support the required IO Behaviour to implement this command");
                    }
                    else
                    {
                        // Must support named graphs
                        if ((_manager.IOBehaviour & IOBehaviour.HasNamedGraphs) == 0) throw new SparqlUpdateException("The underlying store does not support the notion of named graphs required to process this command");
                        // Must allow either CanUpdateDeleteTriples or OverwriteNamed
                        if ((_manager.IOBehaviour & IOBehaviour.CanUpdateAddTriples) == 0 && (_manager.IOBehaviour & IOBehaviour.OverwriteNamed) == 0) throw new SparqlUpdateException("The underlying store does not support the required IO Behaviour to implement this command");
                    }

                    Graph g = new Graph();
                    if (!_manager.UpdateSupported) _manager.LoadGraph(g, cmd.TargetUri);
                    UriLoader.Load(g, cmd.SourceUri);
                    g.BaseUri = cmd.TargetUri;
                    if (_manager.UpdateSupported)
                    {
                        _manager.UpdateGraph(cmd.TargetUri, g.Triples, Enumerable.Empty<Triple>());
                    }
                    else
                    {
                        _manager.SaveGraph(g);
                    }
                }
                catch
                {
                    if (!cmd.Silent) throw;
                }
            }
        }

        /// <summary>
        /// Processes an INSERT/DELETE command
        /// </summary>
        /// <param name="cmd">Insert/Delete Command</param>
        public void ProcessModifyCommand(ModifyCommand cmd)
        {
            if (_manager is IUpdateableStorage)
            {
                ((IUpdateableStorage)_manager).Update(cmd.ToString());
            }
            else
            {
                if (_manager is IQueryableStorage)
                {
                    // Check IO Behaviour
                    // For a delete we either need the ability to Update Delete Triples or to Overwrite Graphs
                    // Firstly check behaviour persuant to default graph if applicable
                    if (cmd.DeletePattern.TriplePatterns.OfType<IConstructTriplePattern>().Any())
                    {
                        // Must support notion of default graph
                        if ((_manager.IOBehaviour & IOBehaviour.HasDefaultGraph) == 0) throw new SparqlUpdateException("The underlying store does not support the notion of an explicit unnamed Default Graph required to process this command");
                        // Must allow either OverwriteDefault or CanUpdateDeleteTriples
                        if ((_manager.IOBehaviour & IOBehaviour.CanUpdateDeleteTriples) == 0 && (_manager.IOBehaviour & IOBehaviour.OverwriteDefault) == 0) throw new SparqlUpdateException("The underlying store does not support the required IO Behaviour to implement this command");
                    }
                    // Then check behaviour persuant to named graphs if applicable
                    if (cmd.DeletePattern.HasChildGraphPatterns)
                    {
                        // Must support named graphs
                        if ((_manager.IOBehaviour & IOBehaviour.HasNamedGraphs) == 0) throw new SparqlUpdateException("The underlying store does not support the notion of named graphs required to process this command");
                        // Must allow either CanUpdateDeleteTriples or OverwriteNamed
                        if ((_manager.IOBehaviour & IOBehaviour.CanUpdateDeleteTriples) == 0 && (_manager.IOBehaviour & IOBehaviour.OverwriteNamed) == 0) throw new SparqlUpdateException("The underlying store does not support the required IO Behaviour to implement this command");
                    }
                    // Check IO Behaviour
                    // For a insert we either need the ability to Update Add Triples or to Overwrite Graphs
                    // Firstly check behaviour persuant to default graph if applicable
                    if (cmd.InsertPattern.TriplePatterns.OfType<IConstructTriplePattern>().Any())
                    {
                        // Must support notion of default graph
                        if ((_manager.IOBehaviour & IOBehaviour.HasDefaultGraph) == 0) throw new SparqlUpdateException("The underlying store does not support the notion of an explicit unnamed Default Graph required to process this command");
                        // Must allow either OverwriteDefault or CanUpdateAddTriples
                        if ((_manager.IOBehaviour & IOBehaviour.CanUpdateAddTriples) == 0 && (_manager.IOBehaviour & IOBehaviour.OverwriteDefault) == 0) throw new SparqlUpdateException("The underlying store does not support the required IO Behaviour to implement this command");
                    }
                    // Then check behaviour persuant to named graphs if applicable
                    if (cmd.InsertPattern.HasChildGraphPatterns)
                    {
                        // Must support named graphs
                        if ((_manager.IOBehaviour & IOBehaviour.HasNamedGraphs) == 0) throw new SparqlUpdateException("The underlying store does not support the notion of named graphs required to process this command");
                        // Must allow either CanUpdateAddTriples or OverwriteNamed
                        if ((_manager.IOBehaviour & IOBehaviour.CanUpdateAddTriples) == 0 && (_manager.IOBehaviour & IOBehaviour.OverwriteNamed) == 0) throw new SparqlUpdateException("The underlying store does not support the required IO Behaviour to implement this command");
                    }

                    // First build and make the query to get a Result Set
                    String queryText = "SELECT * WHERE " + cmd.WherePattern.ToString();
                    SparqlQueryParser parser = new SparqlQueryParser();
                    SparqlQuery query = parser.ParseFromString(queryText);
                    if (cmd.GraphUri != null && !cmd.UsingUris.Any()) query.AddDefaultGraph(cmd.GraphUri);
                    foreach (Uri u in cmd.UsingUris)
                    {
                        query.AddDefaultGraph(u);
                    }
                    foreach (Uri u in cmd.UsingNamedUris)
                    {
                        query.AddNamedGraph(u);
                    }

                    Object results = ((IQueryableStorage)_manager).Query(query.ToString());
                    if (results is SparqlResultSet)
                    {
                        // Now need to transform the Result Set back to a Multiset
                        Multiset mset = new Multiset((SparqlResultSet)results);

                        // Generate the Triples for each Solution
                        List<Triple> deletedTriples = new List<Triple>();
                        Dictionary<String, List<Triple>> deletedGraphTriples = new Dictionary<string, List<Triple>>();
                        foreach (ISet s in mset.Sets)
                        {
                            List<Triple> tempDeletedTriples = new List<Triple>();
                            try
                            {
                                ConstructContext context = new ConstructContext(null, s, true);
                                foreach (IConstructTriplePattern p in cmd.DeletePattern.TriplePatterns.OfType<IConstructTriplePattern>())
                                {
                                    try
                                    {
                                        tempDeletedTriples.Add(p.Construct(context));
                                    }
                                    catch (RdfQueryException)
                                    {
                                        // If we get an error here then it means we could not construct a specific
                                        // triple so we continue anyway
                                    }
                                }
                                deletedTriples.AddRange(tempDeletedTriples);
                            }
                            catch (RdfQueryException)
                            {
                                // If we get an error here this means we couldn't construct for this solution so the
                                // solution is ignored for this graph
                            }

                            // Triples from GRAPH clauses
                            foreach (GraphPattern gp in cmd.DeletePattern.ChildGraphPatterns)
                            {
                                tempDeletedTriples.Clear();
                                try
                                {
                                    String graphUri;
                                    switch (gp.GraphSpecifier.TokenType)
                                    {
                                        case Token.URI:
                                            graphUri = gp.GraphSpecifier.Value;
                                            break;
                                        case Token.VARIABLE:
                                            String graphVar = gp.GraphSpecifier.Value.Substring(1);
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
                                    if (!deletedGraphTriples.ContainsKey(graphUri)) deletedGraphTriples.Add(graphUri, new List<Triple>());
                                    ConstructContext context = new ConstructContext(null, s, true);
                                    foreach (IConstructTriplePattern p in gp.TriplePatterns.OfType<IConstructTriplePattern>())
                                    {
                                        try
                                        {
                                            tempDeletedTriples.Add(p.Construct(context));
                                        }
                                        catch (RdfQueryException)
                                        {
                                            // If we throw an error this means we couldn't construct a specific
                                            // triple so we continue anyway
                                        }
                                    }
                                    deletedGraphTriples[graphUri].AddRange(tempDeletedTriples);
                                }
                                catch (RdfQueryException)
                                {
                                    // If we get an error here this means we couldn't construct for this solution so the
                                    // solution is ignored for this graph
                                }
                            }
                        }

                        // Generate the Triples for each Solution
                        List<Triple> insertedTriples = new List<Triple>();
                        Dictionary<String, List<Triple>> insertedGraphTriples = new Dictionary<string, List<Triple>>();
                        foreach (ISet s in mset.Sets)
                        {
                            List<Triple> tempInsertedTriples = new List<Triple>();
                            try
                            {
                                ConstructContext context = new ConstructContext(null, s, true);
                                foreach (IConstructTriplePattern p in cmd.InsertPattern.TriplePatterns.OfType<IConstructTriplePattern>())
                                {
                                    try
                                    {
                                        tempInsertedTriples.Add(p.Construct(context));
                                    }
                                    catch (RdfQueryException)
                                    {
                                        // If we get an error here then it means we couldn't construct a specific
                                        // triple so we continue anyway
                                    }
                                }
                                insertedTriples.AddRange(tempInsertedTriples);
                            }
                            catch (RdfQueryException)
                            {
                                // If we get an error here this means we couldn't construct for this solution so the
                                // solution is ignored for this graph
                            }

                            // Triples from GRAPH clauses
                            foreach (GraphPattern gp in cmd.InsertPattern.ChildGraphPatterns)
                            {
                                tempInsertedTriples.Clear();
                                try
                                {
                                    String graphUri;
                                    switch (gp.GraphSpecifier.TokenType)
                                    {
                                        case Token.URI:
                                            graphUri = gp.GraphSpecifier.Value;
                                            break;
                                        case Token.VARIABLE:
                                            String graphVar = gp.GraphSpecifier.Value.Substring(1);
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
                                    if (!insertedGraphTriples.ContainsKey(graphUri)) insertedGraphTriples.Add(graphUri, new List<Triple>());
                                    ConstructContext context = new ConstructContext(null, s, true);
                                    foreach (IConstructTriplePattern p in gp.TriplePatterns.OfType<IConstructTriplePattern>())
                                    {
                                        try
                                        {
                                            tempInsertedTriples.Add(p.Construct(context));
                                        }
                                        catch (RdfQueryException)
                                        {
                                            // If we get an error here it means we couldn't construct a specific
                                            // triple so we continue anyway
                                        }
                                    }
                                    insertedGraphTriples[graphUri].AddRange(tempInsertedTriples);
                                }
                                catch (RdfQueryException)
                                {
                                    // If we get an error here this means we couldn't construct for this solution so the
                                    // solution is ignored for this graph
                                }
                            }
                        }

                        // Now decide how to apply the update
                        if (_manager.UpdateSupported)
                        {
                            _manager.UpdateGraph(cmd.GraphUri, insertedTriples, deletedTriples);
                            // We do these two operations sequentially even if in some cases they could be combined to ensure that the underlying
                            // Manager doesn't do any optimisations which would have the result of our updates not being properly applied
                            // e.g. ignoring Triples which are both asserted and retracted in one update
                            foreach (KeyValuePair<String, List<Triple>> graphDeletion in deletedGraphTriples)
                            {
                                _manager.UpdateGraph(graphDeletion.Key, Enumerable.Empty<Triple>(), graphDeletion.Value);
                            }
                            foreach (KeyValuePair<String, List<Triple>> graphInsertion in insertedGraphTriples)
                            {
                                _manager.UpdateGraph(graphInsertion.Key, graphInsertion.Value, Enumerable.Empty<Triple>());
                            }
                        }
                        else
                        {
                            Graph g = new Graph();
                            _manager.LoadGraph(g, cmd.GraphUri);
                            g.Retract(deletedTriples);
                            _manager.SaveGraph(g);

                            foreach (String graphUri in deletedGraphTriples.Keys.Concat(insertedGraphTriples.Keys).Distinct())
                            {
                                g = new Graph();
                                _manager.LoadGraph(g, graphUri);
                                if (deletedGraphTriples.ContainsKey(graphUri)) g.Retract(deletedGraphTriples[graphUri]);
                                if (insertedGraphTriples.ContainsKey(graphUri)) g.Assert(insertedGraphTriples[graphUri]);
                                _manager.SaveGraph(g);
                            }
                        }
                    }
                    else
                    {
                        throw new SparqlUpdateException("Cannot evaluate an INSERT/DELETE Command as the underlying Store failed to answer the query for the WHERE portion of the command as expected");
                    }
                }
                else
                {
                    throw new NotSupportedException("INSERT/DELETE commands are not supported by this Update Processor as the manager for the underlying Store does not provide Query capabilities which are necessary to process this command");
                }
            }
        }

        /// <summary>
        /// Processes a MOVE command
        /// </summary>
        /// <param name="cmd">Move Command</param>
        public void ProcessMoveCommand(MoveCommand cmd)
        {
            if (_manager is IUpdateableStorage)
            {
                ((IUpdateableStorage)_manager).Update(cmd.ToString());
            }
            else
            {
                try
                {
                    // If Source and Destination are equal this is a no-op
                    if (EqualityHelper.AreUrisEqual(cmd.SourceUri, cmd.DestinationUri)) return;

                    // Firstly check that appropriate IO Behaviour is provided
                    if (cmd.SourceUri == null || cmd.DestinationUri == null)
                    {
                        if ((_manager.IOBehaviour & IOBehaviour.HasDefaultGraph) == 0) throw new SparqlUpdateException("The underlying store does not provide support for an explicit unnamed Default Graph required to process this command");
                    }
                    IOBehaviour desired = cmd.DestinationUri == null ? IOBehaviour.OverwriteDefault : IOBehaviour.OverwriteNamed;
                    if ((_manager.IOBehaviour & desired) == 0) throw new SparqlUpdateException("The underlying store does not provide the required IO Behaviour to implement this command");

                    Graph source = new Graph();
                    _manager.LoadGraph(source, cmd.SourceUri);
                    source.BaseUri = cmd.SourceUri;

                    // If the Manager supports delete ensure the Destination Graph is deleted
                    if (_manager.DeleteSupported)
                    {
                        try
                        {
                            _manager.DeleteGraph(cmd.DestinationUri);
                        }
                        catch (Exception ex)
                        {
                            throw new SparqlUpdateException("Unable to process a MOVE command as unable to ensure that the Destination Graph was deleted prior to moving the data from the Source Graph", ex);
                        }
                    }

                    // Load Destination Graph and ensure it is empty
                    Graph dest = new Graph();
                    _manager.LoadGraph(dest, cmd.DestinationUri);
                    dest.BaseUri = cmd.DestinationUri;
                    dest.Clear();

                    // Transfer the data and update both the Graphs
                    // For the Source Graph which we must delete the contents of either use DeleteGraph() if supported or
                    // just save an empty Graph in its place and hope that SaveGraph() is an overwrite operation
                    dest.Merge(source);
                    source.Clear();
                    _manager.SaveGraph(dest);
                    if (_manager.DeleteSupported)
                    {
                        try
                        {
                            _manager.DeleteGraph(cmd.SourceUri);
                        }
                        catch (Exception ex)
                        {
                            throw new SparqlUpdateException("Unable to process a MOVE command as unable to ensure that the Source Graph was deleted after the movement of data to the Destination Graph", ex);
                        }
                    }
                    else
                    {
                        _manager.SaveGraph(source);
                    }
                }
                catch
                {
                    if (!cmd.Silent) throw;
                }
            }
        }

        /// <summary>
        /// Determines whether a Graph Pattern is valid for use in an INSERT/DELETE DATA command
        /// </summary>
        /// <param name="p">Graph Pattern</param>
        /// <param name="top">Is this the top level pattern?</param>
        /// <returns></returns>
        private bool IsValidDataPattern(GraphPattern p, bool top)
        {
            if (p.IsGraph)
            {
                // If a GRAPH clause then all triple patterns must be constructable and have no Child Graph Patterns
                return !p.HasChildGraphPatterns && p.TriplePatterns.All(tp => tp is IConstructTriplePattern && ((IConstructTriplePattern)tp).HasNoExplicitVariables);
            }
            else if (p.IsExists || p.IsMinus || p.IsNotExists || p.IsOptional || p.IsService || p.IsSubQuery || p.IsUnion)
            {
                // EXISTS/MINUS/NOT EXISTS/OPTIONAL/SERVICE/Sub queries/UNIONs are not permitted
                return false;
            }
            else
            {
                // For other patterns all Triple patterns must be constructable with no explicit variables
                // If top level then any Child Graph Patterns must be valid
                // Otherwise must have no Child Graph Patterns
                return p.TriplePatterns.All(tp => tp is IConstructTriplePattern && ((IConstructTriplePattern)tp).HasNoExplicitVariables) && ((top && p.ChildGraphPatterns.All(gp => IsValidDataPattern(gp, false))) || !p.HasChildGraphPatterns);
            }
        }
    }
}