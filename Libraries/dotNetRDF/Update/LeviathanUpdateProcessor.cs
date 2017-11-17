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
using System.Threading;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Update.Commands;

namespace VDS.RDF.Update
{
    /// <summary>
    /// Default SPARQL Update Processor provided by the library's Leviathan SPARQL Engine
    /// </summary>
    /// <remarks>
    /// <para>
    /// The Leviathan Update Processor simply invokes the <see cref="SparqlUpdateCommand.Evaluate">Evaluate</see> method of the SPARQL Commands it is asked to process.  Derived implementations may override the relevant virtual protected methods to substitute their own evaluation of an update for our default standards compliant implementations.
    /// </para>
    /// </remarks>
    public class LeviathanUpdateProcessor 
        : ISparqlUpdateProcessor
    {
        /// <summary>
        /// Dataset over which updates are applied
        /// </summary>
        protected ISparqlDataset _dataset;
        private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private bool _autoCommit = true, _canCommit = true;

        /// <summary>
        /// Creates a new Leviathan Update Processor
        /// </summary>
        /// <param name="store">Triple Store</param>
        public LeviathanUpdateProcessor(IInMemoryQueryableStore store)
            : this(new InMemoryDataset(store)) { }

        /// <summary>
        /// Creates a new Leviathan Update Processor
        /// </summary>
        /// <param name="data">SPARQL Dataset</param>
        public LeviathanUpdateProcessor(ISparqlDataset data)
        {
            _dataset = data;
            if (!_dataset.HasGraph(null))
            {
                // Create the Default unnamed Graph if it doesn't exist and then Flush() the change
                _dataset.AddGraph(new Graph());
                _dataset.Flush();
            }
        }

        /// <summary>
        /// Gets/Sets whether Updates are automatically committed
        /// </summary>
        public bool AutoCommit
        {
            get
            {
                return _autoCommit;
            }
            set
            {
                _autoCommit = value;
            }
        }

        /// <summary>
        /// Flushes any outstanding changes to the underlying dataset
        /// </summary>
        public void Flush()
        {
            if (!_canCommit) throw new SparqlUpdateException("Unable to commit since one/more Commands executed in the current Transaction failed");
            _dataset.Flush();
        }

        /// <summary>
        /// Discards and outstanding changes from the underlying dataset
        /// </summary>
        public void Discard()
        {
            _dataset.Discard();
            _canCommit = true;
        }

        /// <summary>
        /// Creates a new Evaluation Context
        /// </summary>
        /// <param name="cmds">Update Commands</param>
        /// <returns></returns>
        protected SparqlUpdateEvaluationContext GetContext(SparqlUpdateCommandSet cmds)
        {
            return new SparqlUpdateEvaluationContext(cmds, _dataset, GetQueryProcessor());
        }

        /// <summary>
        /// Creates a new Evaluation Context
        /// </summary>
        /// <returns></returns>
        protected SparqlUpdateEvaluationContext GetContext()
        {
            return new SparqlUpdateEvaluationContext(_dataset, GetQueryProcessor());
        }

        /// <summary>
        /// Gets the Query Processor to be used
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// By default <strong>null</strong> is returned which indicates that the default query processing behaviour is used, to use a specific processor extend this class and override this method.  If you do so you will have access to the dataset in use so generally you will want to use a query processor that accepts a <see cref="ISparqlDataset">ISparqlDataset</see> instance
        /// </remarks>
        protected virtual ISparqlQueryAlgebraProcessor<BaseMultiset, SparqlEvaluationContext> GetQueryProcessor()
        {
            return null;
        }

        /// <summary>
        /// Processes an ADD command
        /// </summary>
        /// <param name="cmd">Add Command</param>
        public void ProcessAddCommand(AddCommand cmd)
        {
            ProcessAddCommandInternal(cmd, GetContext());
        }

        /// <summary>
        /// Processes an ADD command
        /// </summary>
        /// <param name="cmd">Add Command</param>
        /// <param name="context">SPARQL Update Evaluation Context</param>
        protected virtual void ProcessAddCommandInternal(AddCommand cmd, SparqlUpdateEvaluationContext context)
        {
            cmd.Evaluate(context);
        }

        /// <summary>
        /// Processes a CLEAR command
        /// </summary>
        /// <param name="cmd">Clear Command</param>
        public void ProcessClearCommand(ClearCommand cmd)
        {
            ProcessClearCommandInternal(cmd, GetContext());
        }

        /// <summary>
        /// Processes a CLEAR command
        /// </summary>
        /// <param name="cmd">Clear Command</param>
        /// <param name="context">SPARQL Update Evaluation Context</param>
        protected virtual void ProcessClearCommandInternal(ClearCommand cmd, SparqlUpdateEvaluationContext context)
        {
            cmd.Evaluate(context);
        }

        /// <summary>
        /// Processes a COPY command
        /// </summary>
        /// <param name="cmd">Copy Command</param>
        public void ProcessCopyCommand(CopyCommand cmd)
        {
            ProcessCopyCommandInternal(cmd, GetContext());
        }

        /// <summary>
        /// Processes a COPY command
        /// </summary>
        /// <param name="cmd">Copy Command</param>
        /// <param name="context">SPARQL Update Evaluation Context</param>
        protected virtual void ProcessCopyCommandInternal(CopyCommand cmd, SparqlUpdateEvaluationContext context)
        {
            cmd.Evaluate(context);
        }

        /// <summary>
        /// Processes a CREATE command
        /// </summary>
        /// <param name="cmd">Create Command</param>
        public void ProcessCreateCommand(CreateCommand cmd)
        {
            ProcessCreateCommandInternal(cmd, GetContext());
        }

        /// <summary>
        /// Processes a CREATE command
        /// </summary>
        /// <param name="cmd">Create Command</param>
        /// <param name="context">SPARQL Update Evaluation Context</param>
        protected virtual void ProcessCreateCommandInternal(CreateCommand cmd, SparqlUpdateEvaluationContext context)
        {
            cmd.Evaluate(context);
        }

        /// <summary>
        /// Processes a command
        /// </summary>
        /// <param name="cmd">Command</param>
        public void ProcessCommand(SparqlUpdateCommand cmd)
        {
            ProcessCommandInternal(cmd, GetContext());
        }

        /// <summary>
        /// Processes a command
        /// </summary>
        /// <param name="cmd">Command</param>
        /// <param name="context">SPARQL Update Evaluation Context</param>
        /// <remarks>
        /// Invokes the type specific method for the command type
        /// </remarks>
        private void ProcessCommandInternal(SparqlUpdateCommand cmd, SparqlUpdateEvaluationContext context)
        {
            // If auto-committing then Flush() any existing Transaction first
            if (_autoCommit) Flush();

            // Then if possible attempt to get the lock and determine whether it needs releasing
            ReaderWriterLockSlim currLock = (_dataset is IThreadSafeDataset) ? ((IThreadSafeDataset)_dataset).Lock : _lock;
            bool mustRelease = false;
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
                        ProcessAddCommandInternal((AddCommand)cmd, context);
                        break;
                    case SparqlUpdateCommandType.Clear:
                        ProcessClearCommandInternal((ClearCommand)cmd, context);
                        break;
                    case SparqlUpdateCommandType.Copy:
                        ProcessCopyCommandInternal((CopyCommand)cmd, context);
                        break;
                    case SparqlUpdateCommandType.Create:
                        ProcessCreateCommandInternal((CreateCommand)cmd, context);
                        break;
                    case SparqlUpdateCommandType.Delete:
                        ProcessDeleteCommandInternal((DeleteCommand)cmd, context);
                        break;
                    case SparqlUpdateCommandType.DeleteData:
                        ProcessDeleteDataCommandInternal((DeleteDataCommand)cmd, context);
                        break;
                    case SparqlUpdateCommandType.Drop:
                        ProcessDropCommandInternal((DropCommand)cmd, context);
                        break;
                    case SparqlUpdateCommandType.Insert:
                        ProcessInsertCommandInternal((InsertCommand)cmd, context);
                        break;
                    case SparqlUpdateCommandType.InsertData:
                        ProcessInsertDataCommandInternal((InsertDataCommand)cmd, context);
                        break;
                    case SparqlUpdateCommandType.Load:
                        ProcessLoadCommandInternal((LoadCommand)cmd, context);
                        break;
                    case SparqlUpdateCommandType.Modify:
                        ProcessModifyCommandInternal((ModifyCommand)cmd, context);
                        break;
                    case SparqlUpdateCommandType.Move:
                        ProcessMoveCommandInternal((MoveCommand)cmd, context);
                        break;
                    default:
                        throw new SparqlUpdateException("Unknown Update Commands cannot be processed by the Leviathan Update Processor");
                }

                // If auto-committing flush after every command
                if (_autoCommit) Flush();
            }
            catch
            {
                // If auto-committing discard if an error occurs, if not then mark the transaction as uncomittable
                if (_autoCommit)
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
        /// Processes a command set
        /// </summary>
        /// <param name="commands">Command Set</param>
        /// <remarks>
        /// Invokes <see cref="LeviathanUpdateProcessor.ProcessCommand">ProcessCommand()</see> on each command in turn
        /// </remarks>
        public void ProcessCommandSet(SparqlUpdateCommandSet commands)
        {
            commands.UpdateExecutionTime = null;

            // Firstly check what Transaction mode we are running in
            bool autoCommit = _autoCommit;

            // Then create an Evaluation Context
            SparqlUpdateEvaluationContext context = GetContext(commands);

            // Remember to handle the Thread Safety
            // If the Dataset is Thread Safe use its own lock otherwise use our local lock
            ReaderWriterLockSlim currLock = (_dataset is IThreadSafeDataset) ? ((IThreadSafeDataset)_dataset).Lock : _lock;
            try
            {
                currLock.EnterWriteLock();

                // Regardless of the Transaction Mode we turn auto-commit off for a command set so that individual commands
                // don't try and commit after each one is applied i.e. either we are in auto-commit mode and all the
                // commands must be evaluated before flushing/discarding the changes OR we are not in auto-commit mode
                // so turning it off doesn't matter as it is already turned off
                _autoCommit = false;

                if (autoCommit)
                {
                    // Do a Flush() before we start to ensure changes from any previous commands are persisted
                    _dataset.Flush();
                }

                // Start the operation
                context.StartExecution();
                for (int i = 0; i < commands.CommandCount; i++)
                {
                    ProcessCommandInternal(commands[i], context);

                    // Check for Timeout
                    context.CheckTimeout();
                }

                if (autoCommit)
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
                if (autoCommit)
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
                // Reset auto-commit setting and release our write lock
                _autoCommit = autoCommit;
                currLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Processes a DELETE command
        /// </summary>
        /// <param name="cmd">Delete Command</param>
        public void ProcessDeleteCommand(DeleteCommand cmd)
        {
            ProcessDeleteCommandInternal(cmd, GetContext());
        }

        /// <summary>
        /// Processes a DELETE command
        /// </summary>
        /// <param name="cmd">Delete Command</param>
        /// <param name="context">SPARQL Update Evaluation Context</param>
        protected virtual void ProcessDeleteCommandInternal(DeleteCommand cmd, SparqlUpdateEvaluationContext context)
        {
            cmd.Evaluate(context);
        }

        /// <summary>
        /// Processes a DELETE DATA command
        /// </summary>
        /// <param name="cmd">DELETE Data Command</param>
        public void ProcessDeleteDataCommand(DeleteDataCommand cmd)
        {
            ProcessDeleteDataCommandInternal(cmd, GetContext());
        }

        /// <summary>
        /// Processes a DELETE DATA command
        /// </summary>
        /// <param name="cmd">Delete Data Command</param>
        /// <param name="context">SPARQL Update Evaluation Context</param>
        protected virtual void ProcessDeleteDataCommandInternal(DeleteDataCommand cmd, SparqlUpdateEvaluationContext context)
        {
            cmd.Evaluate(context);
        }

        /// <summary>
        /// Processes a DROP command
        /// </summary>
        /// <param name="cmd">Drop Command</param>
        public void ProcessDropCommand(DropCommand cmd)
        {
            ProcessDropCommandInternal(cmd, GetContext());
        }

        /// <summary>
        /// Processes a DROP command
        /// </summary>
        /// <param name="cmd">Drop Command</param>
        /// <param name="context">SPARQL Update Evaluation Context</param>
        protected virtual void ProcessDropCommandInternal(DropCommand cmd, SparqlUpdateEvaluationContext context)
        {
            cmd.Evaluate(context);
        }

        /// <summary>
        /// Processes an INSERT command
        /// </summary>
        /// <param name="cmd">Insert Command</param>
        public void ProcessInsertCommand(InsertCommand cmd)
        {
            ProcessInsertCommandInternal(cmd, GetContext());
        }

        /// <summary>
        /// Processes an INSERT command
        /// </summary>
        /// <param name="cmd">Insert Command</param>
        /// <param name="context">SPARQL Update Evaluation Context</param>
        protected virtual void ProcessInsertCommandInternal(InsertCommand cmd, SparqlUpdateEvaluationContext context)
        {
            cmd.Evaluate(context);
        }

        /// <summary>
        /// Processes an INSERT DATA command
        /// </summary>
        /// <param name="cmd">Insert Data Command</param>
        public void ProcessInsertDataCommand(InsertDataCommand cmd)
        {
            ProcessInsertDataCommandInternal(cmd, GetContext());
        }

        /// <summary>
        /// Processes an INSERT DATA command
        /// </summary>
        /// <param name="cmd">Insert Data Command</param>
        /// <param name="context">SPARQL Update Evaluation Context</param>
        protected virtual void ProcessInsertDataCommandInternal(InsertDataCommand cmd, SparqlUpdateEvaluationContext context)
        {
            cmd.Evaluate(context);
        }

        /// <summary>
        /// Processes a LOAD command
        /// </summary>
        /// <param name="cmd">Load Command</param>
        public void ProcessLoadCommand(LoadCommand cmd)
        {
            ProcessLoadCommandInternal(cmd, GetContext());
        }

        /// <summary>
        /// Processes a LOAD command
        /// </summary>
        /// <param name="cmd">Load Command</param>
        /// <param name="context">SPARQL Update Evaluation Context</param>
        protected virtual void ProcessLoadCommandInternal(LoadCommand cmd, SparqlUpdateEvaluationContext context)
        {
            cmd.Evaluate(context);
        }

        /// <summary>
        /// Processes an INSERT/DELETE command
        /// </summary>
        /// <param name="cmd">Insert/Delete Command</param>
        public void ProcessModifyCommand(ModifyCommand cmd)
        {
            ProcessModifyCommandInternal(cmd, GetContext());
        }

        /// <summary>
        /// Processes an INSERT/DELETE command
        /// </summary>
        /// <param name="cmd">Insert/Delete Command</param>
        /// <param name="context">SPARQL Update Evaluation Context</param>
        protected virtual void ProcessModifyCommandInternal(ModifyCommand cmd, SparqlUpdateEvaluationContext context)
        {
            cmd.Evaluate(context);
        }

        /// <summary>
        /// Processes a MOVE command
        /// </summary>
        /// <param name="cmd">Move Command</param>
        public void ProcessMoveCommand(MoveCommand cmd)
        {
            ProcessMoveCommandInternal(cmd, GetContext());
        }

        /// <summary>
        /// Processes a MOVE command
        /// </summary>
        /// <param name="cmd">Move Command</param>
        /// <param name="context">SPARQL Update Evaluation Context</param>
        protected virtual void ProcessMoveCommandInternal(MoveCommand cmd, SparqlUpdateEvaluationContext context)
        {
            cmd.Evaluate(context);
        }
    }
}
