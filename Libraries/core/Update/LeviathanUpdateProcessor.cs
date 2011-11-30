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
using System.Diagnostics;
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
#if !NO_RWLOCK
        private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
#endif
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
            this._dataset = data;
            if (!this._dataset.HasGraph(null))
            {
                //Create the Default unnamed Graph if it doesn't exist and then Flush() the change
                this._dataset.AddGraph(new Graph());
                this._dataset.Flush();
            }
        }

        /// <summary>
        /// Gets/Sets whether Updates are automatically committed
        /// </summary>
        public bool AutoCommit
        {
            get
            {
                return this._autoCommit;
            }
            set
            {
                this._autoCommit = value;
            }
        }

        /// <summary>
        /// Flushes any outstanding changes to the underlying dataset
        /// </summary>
        public void Flush()
        {
            if (!this._canCommit) throw new SparqlUpdateException("Unable to commit since one/more Commands executed in the current Transaction failed");
            this._dataset.Flush();
        }

        /// <summary>
        /// Discards and outstanding changes from the underlying dataset
        /// </summary>
        public void Discard()
        {
            this._dataset.Discard();
            this._canCommit = true;
        }

        /// <summary>
        /// Creates a new Evaluation Context
        /// </summary>
        /// <param name="cmds">Update Commands</param>
        /// <returns></returns>
        protected SparqlUpdateEvaluationContext GetContext(SparqlUpdateCommandSet cmds)
        {
            return new SparqlUpdateEvaluationContext(cmds, this._dataset, this.GetQueryProcessor());
        }

        /// <summary>
        /// Creates a new Evaluation Context
        /// </summary>
        /// <returns></returns>
        protected SparqlUpdateEvaluationContext GetContext()
        {
            return new SparqlUpdateEvaluationContext(this._dataset, this.GetQueryProcessor());
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
            this.ProcessAddCommandInternal(cmd, this.GetContext());
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
            this.ProcessClearCommandInternal(cmd, this.GetContext());
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
            this.ProcessCopyCommandInternal(cmd, this.GetContext());
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
            this.ProcessCreateCommandInternal(cmd, this.GetContext());
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
            this.ProcessCommandInternal(cmd, this.GetContext());
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
            //If auto-committing then Flush() any existing Transaction first
            if (this._autoCommit) this.Flush();

            //Then if possible attempt to get the lock and determine whether it needs releasing
#if !NO_RWLOCK
            ReaderWriterLockSlim currLock = (this._dataset is IThreadSafeDataset) ? ((IThreadSafeDataset)this._dataset).Lock : this._lock;
#endif
            bool mustRelease = false;
            try
            {
#if !NO_RWLOCK
                if (!currLock.IsWriteLockHeld)
                {
                    currLock.EnterWriteLock();
                    mustRelease = true;
                }
#else
                //If ReaderWriteLockSlim is not available use a Monitor instead
                Monitor.Enter(this._dataset);
                mustRelease = true;
#endif

                //Then based on the command type call the appropriate protected method
                switch (cmd.CommandType)
                {
                    case SparqlUpdateCommandType.Add:
                        this.ProcessAddCommandInternal((AddCommand)cmd, context);
                        break;
                    case SparqlUpdateCommandType.Clear:
                        this.ProcessClearCommandInternal((ClearCommand)cmd, context);
                        break;
                    case SparqlUpdateCommandType.Copy:
                        this.ProcessCopyCommandInternal((CopyCommand)cmd, context);
                        break;
                    case SparqlUpdateCommandType.Create:
                        this.ProcessCreateCommandInternal((CreateCommand)cmd, context);
                        break;
                    case SparqlUpdateCommandType.Delete:
                        this.ProcessDeleteCommandInternal((DeleteCommand)cmd, context);
                        break;
                    case SparqlUpdateCommandType.DeleteData:
                        this.ProcessDeleteDataCommandInternal((DeleteDataCommand)cmd, context);
                        break;
                    case SparqlUpdateCommandType.Drop:
                        this.ProcessDropCommandInternal((DropCommand)cmd, context);
                        break;
                    case SparqlUpdateCommandType.Insert:
                        this.ProcessInsertCommandInternal((InsertCommand)cmd, context);
                        break;
                    case SparqlUpdateCommandType.InsertData:
                        this.ProcessInsertDataCommandInternal((InsertDataCommand)cmd, context);
                        break;
                    case SparqlUpdateCommandType.Load:
                        this.ProcessLoadCommandInternal((LoadCommand)cmd, context);
                        break;
                    case SparqlUpdateCommandType.Modify:
                        this.ProcessModifyCommandInternal((ModifyCommand)cmd, context);
                        break;
                    case SparqlUpdateCommandType.Move:
                        this.ProcessMoveCommandInternal((MoveCommand)cmd, context);
                        break;
                    default:
                        throw new SparqlUpdateException("Unknown Update Commands cannot be processed by the Leviathan Update Processor");
                }

                //If auto-committing flush after every command
                if (this._autoCommit) this.Flush();
            }
            catch
            {
                //If auto-committing discard if an error occurs, if not then mark the transaction as uncomittable
                if (this._autoCommit)
                {
                    this.Discard();
                }
                else
                {
                    this._canCommit = false;
                }
                throw;
            }
            finally
            {
                //Release locks if necessary
                if (mustRelease)
                {
#if !NO_RWLOCK
                    currLock.ExitWriteLock();
#else
                    Monitor.Exit(this._dataset);
#endif
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

            //Firstly check what Transaction mode we are running in
            bool autoCommit = this._autoCommit;

            //Then create an Evaluation Context
            SparqlUpdateEvaluationContext context = this.GetContext(commands);

            //Remember to handle the Thread Safety
            //If the Dataset is Thread Safe use its own lock otherwise use our local lock
#if !NO_RWLOCK
            ReaderWriterLockSlim currLock = (this._dataset is IThreadSafeDataset) ? ((IThreadSafeDataset)this._dataset).Lock : this._lock;
#endif
            try
            {
#if !NO_RWLOCK
                currLock.EnterWriteLock();
#else
                //Have to make do with a Monitor if ReaderWriterLockSlim is not available
                Monitor.Enter(this._dataset);
#endif

                //Regardless of the Transaction Mode we turn auto-commit off for a command set so that individual commands
                //don't try and commit after each one is applied i.e. either we are in auto-commit mode and all the
                //commands must be evaluated before flushing/discarding the changes OR we are not in auto-commit mode
                //so turning it off doesn't matter as it is already turned off
                this._autoCommit = false;

                if (autoCommit)
                {
                    //Do a Flush() before we start to ensure changes from any previous commands are persisted
                    this._dataset.Flush();
                }

                //Start the operation
                context.StartExecution();
                for (int i = 0; i < commands.CommandCount; i++)
                {
                    this.ProcessCommandInternal(commands[i], context);

                    //Check for Timeout
                    context.CheckTimeout();
                }

                if (autoCommit)
                {
                    //Do a Flush() when command set completed successfully to persist the changes
                    this._dataset.Flush();
                }

                //Set Update Times
                context.EndExecution();
                commands.UpdateExecutionTime = new TimeSpan(context.UpdateTimeTicks);
            }
            catch
            {
                if (autoCommit)
                {
                    //Do a Discard() when a command set fails to discard the changes
                    this._dataset.Discard();
                }
                else
                {
                    this._canCommit = false;
                }

                //Set Update Times
                context.EndExecution();
                commands.UpdateExecutionTime = new TimeSpan(context.UpdateTimeTicks);
                throw;
            }
            finally
            {
                //Reset auto-commit setting and release our write lock
                this._autoCommit = autoCommit;
#if !NO_RWLOCK
                currLock.ExitWriteLock();
#else
                Monitor.Exit(this._dataset);
#endif
            }
        }

        /// <summary>
        /// Processes a DELETE command
        /// </summary>
        /// <param name="cmd">Delete Command</param>
        public void ProcessDeleteCommand(DeleteCommand cmd)
        {
            this.ProcessDeleteCommandInternal(cmd, this.GetContext());
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
            this.ProcessDeleteDataCommandInternal(cmd, this.GetContext());
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
            this.ProcessDropCommandInternal(cmd, this.GetContext());
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
            this.ProcessInsertCommandInternal(cmd, this.GetContext());
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
            this.ProcessInsertDataCommandInternal(cmd, this.GetContext());
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
            this.ProcessLoadCommandInternal(cmd, this.GetContext());
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
            this.ProcessModifyCommandInternal(cmd, this.GetContext());
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
            this.ProcessMoveCommandInternal(cmd, this.GetContext());
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
