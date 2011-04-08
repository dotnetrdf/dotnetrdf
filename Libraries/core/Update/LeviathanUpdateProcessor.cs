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

using System.Diagnostics;
using System.Threading;
using VDS.RDF.Query.Datasets;
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
        private ISparqlDataset _dataset;
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
            this._canCommit = true;
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
        /// Processes an ADD command
        /// </summary>
        /// <param name="cmd">Add Command</param>
        public void ProcessAddCommand(AddCommand cmd)
        {
            if (this._autoCommit) this.Flush();
            ReaderWriterLockSlim currLock = (this._dataset is IThreadSafeDataset) ? ((IThreadSafeDataset)this._dataset).Lock : this._lock;
            bool mustRelease = false;
            try
            {
                if (!currLock.IsWriteLockHeld)
                {
                    currLock.EnterWriteLock();
                    mustRelease = true;
                }
                cmd.Evaluate(new SparqlUpdateEvaluationContext(this._dataset));
                if (this._autoCommit) this.Flush();
            }
            catch
            {
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
                if (mustRelease)
                {
                    currLock.ExitWriteLock();
                }
            }
        }

        /// <summary>
        /// Processes a CLEAR command
        /// </summary>
        /// <param name="cmd">Clear Command</param>
        public void ProcessClearCommand(ClearCommand cmd)
        {
            if (this._autoCommit) this.Flush();
            ReaderWriterLockSlim currLock = (this._dataset is IThreadSafeDataset) ? ((IThreadSafeDataset)this._dataset).Lock : this._lock;
            bool mustRelease = false;
            try
            {
                if (!currLock.IsWriteLockHeld)
                {
                    currLock.EnterWriteLock();
                    mustRelease = true;
                }
                cmd.Evaluate(new SparqlUpdateEvaluationContext(this._dataset));
                if (this._autoCommit) this.Flush();
            }
            catch
            {
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
                if (mustRelease)
                {
                    currLock.ExitWriteLock();
                }
            }
        }

        /// <summary>
        /// Processes a COPY command
        /// </summary>
        /// <param name="cmd">Copy Command</param>
        public void ProcessCopyCommand(CopyCommand cmd)
        {
            if (this._autoCommit) this.Flush();
            ReaderWriterLockSlim currLock = (this._dataset is IThreadSafeDataset) ? ((IThreadSafeDataset)this._dataset).Lock : this._lock;
            bool mustRelease = false;
            try
            {
                if (!currLock.IsWriteLockHeld)
                {
                    currLock.EnterWriteLock();
                    mustRelease = true;
                }
                cmd.Evaluate(new SparqlUpdateEvaluationContext(this._dataset));
                if (this._autoCommit) this.Flush();
            }
            catch
            {
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
                if (mustRelease)
                {
                    currLock.ExitWriteLock();
                }
            }
        }

        /// <summary>
        /// Processes a CREATE command
        /// </summary>
        /// <param name="cmd">Create Command</param>
        public void ProcessCreateCommand(CreateCommand cmd)
        {
            if (this._autoCommit) this.Flush();
            ReaderWriterLockSlim currLock = (this._dataset is IThreadSafeDataset) ? ((IThreadSafeDataset)this._dataset).Lock : this._lock;
            bool mustRelease = false;
            try
            {
                if (!currLock.IsWriteLockHeld)
                {
                    currLock.EnterWriteLock();
                    mustRelease = true;
                }
                cmd.Evaluate(new SparqlUpdateEvaluationContext(this._dataset));
                if (this._autoCommit) this.Flush();
            }
            catch
            {
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
                if (mustRelease)
                {
                    currLock.ExitWriteLock();
                }
            }
        }

        /// <summary>
        /// Processes a command
        /// </summary>
        /// <param name="cmd">Command</param>
        /// <remarks>
        /// Invokes the type specific method for the command type
        /// </remarks>
        public void ProcessCommand(SparqlUpdateCommand cmd)
        {
            switch (cmd.CommandType)
            {
                case SparqlUpdateCommandType.Add:
                    this.ProcessAddCommand((AddCommand)cmd);
                    break;
                case SparqlUpdateCommandType.Clear:
                    this.ProcessClearCommand((ClearCommand)cmd);
                    break;
                case SparqlUpdateCommandType.Copy:
                    this.ProcessCopyCommand((CopyCommand)cmd);
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
                case SparqlUpdateCommandType.Move:
                    this.ProcessMoveCommand((MoveCommand)cmd);
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
        public void ProcessCommandSet(SparqlUpdateCommandSet commands)
        {
            //Stuff for checking Timeouts
#if !NO_STOPWATCH
            Stopwatch timer = new Stopwatch();
#else
            DateTime start, end;
#endif
            commands.UpdateExecutionTime = null;

            //Firstly check what Transaction mode we are running in
            bool autoCommit = this._autoCommit;

            //Remember to handle the Thread Safety
            //If the Dataset is Thread Safe use its own lock otherwise use our local lock
            ReaderWriterLockSlim currLock = (this._dataset is IThreadSafeDataset) ? ((IThreadSafeDataset)this._dataset).Lock : this._lock;
            try
            {
                currLock.EnterWriteLock();

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

                //Start timing the operation
#if !NO_STOPWATCH
                timer.Start();
#else 
                DateTime start = DateTime.Now;
#endif
                for (int i = 0; i < commands.CommandCount; i++)
                {
                    this.ProcessCommand(commands[i]);

                    //Check for Timeout
                    if (commands.Timeout > 0)
                    {
                        if (i < commands.CommandCount - 1)
                        {
#if !NO_STOPWATCH
                            if (timer.ElapsedMilliseconds >= commands.Timeout)
                            {
                                timer.Stop();
                                throw new SparqlUpdateTimeoutException("Update Execution Time exceeded the Timeout of " + commands.Timeout + "ms, update aborted after " + timer.ElapsedMilliseconds + "ms");
                            }
#else
                            end = DateTime.Now;
                            TimeSpan elapsed = (end - start);
                            if (elapsed.Milliseconds >= commands.UpdateTimeout)
                            {
                                throw new SparqlUpdateTimeoutException("Update Execution Time exceeded the Timeout of " + commands.UpdateTimeout + "ms, update aborted after " + elapsed.Milliseconds + "ms");
                            }
#endif
                        }
                    }
                }

                if (autoCommit)
                {
                    //Do a Flush() when command set completed successfully to persist the changes
                    this._dataset.Flush();
                }

#if !NO_STOPWATCH
                timer.Stop();
                commands.UpdateExecutionTime = timer.Elapsed;
#else
                commands.UpdateExecutionTime = (DateTime.Now - start);
#endif
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
#if !NO_STOPWATCH
                timer.Stop();
                commands.UpdateExecutionTime = timer.Elapsed;
#else
                commands.UpdateExecutionTime = (DateTime.Now - start);
#endif
                throw;
            }
            finally
            {
                //Reset auto-commit setting and release our write lock
                this._autoCommit = autoCommit;
                currLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Processes a DELETE command
        /// </summary>
        /// <param name="cmd">Delete Command</param>
        public void ProcessDeleteCommand(DeleteCommand cmd)
        {
            if (this._autoCommit) this.Flush();
            ReaderWriterLockSlim currLock = (this._dataset is IThreadSafeDataset) ? ((IThreadSafeDataset)this._dataset).Lock : this._lock;
            bool mustRelease = false;
            try
            {
                if (!currLock.IsWriteLockHeld)
                {
                    currLock.EnterWriteLock();
                    mustRelease = true;
                }
                cmd.Evaluate(new SparqlUpdateEvaluationContext(this._dataset));
                if (this._autoCommit) this.Flush();
            }
            catch
            {
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
                if (mustRelease)
                {
                    currLock.ExitWriteLock();
                }
            }
        }

        /// <summary>
        /// Processes a DELETE DATA command
        /// </summary>
        /// <param name="cmd">DELETE Data Command</param>
        public void ProcessDeleteDataCommand(DeleteDataCommand cmd)
        {
            if (this._autoCommit) this.Flush();
            ReaderWriterLockSlim currLock = (this._dataset is IThreadSafeDataset) ? ((IThreadSafeDataset)this._dataset).Lock : this._lock;
            bool mustRelease = false;
            try
            {
                if (!currLock.IsWriteLockHeld)
                {
                    currLock.EnterWriteLock();
                    mustRelease = true;
                }
                cmd.Evaluate(new SparqlUpdateEvaluationContext(this._dataset));
                if (this._autoCommit) this.Flush();
            }
            catch
            {
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
                if (mustRelease)
                {
                    currLock.ExitWriteLock();
                }
            }
        }

        /// <summary>
        /// Processes a DROP command
        /// </summary>
        /// <param name="cmd">Drop Command</param>
        public void ProcessDropCommand(DropCommand cmd)
        {
            if (this._autoCommit) this.Flush();
            ReaderWriterLockSlim currLock = (this._dataset is IThreadSafeDataset) ? ((IThreadSafeDataset)this._dataset).Lock : this._lock;
            bool mustRelease = false;
            try
            {
                if (!currLock.IsWriteLockHeld)
                {
                    currLock.EnterWriteLock();
                    mustRelease = true;
                }
                cmd.Evaluate(new SparqlUpdateEvaluationContext(this._dataset));
                if (this._autoCommit) this.Flush();
            }
            catch
            {
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
                if (mustRelease)
                {
                    currLock.ExitWriteLock();
                }
            }
        }

        /// <summary>
        /// Processes an INSERT command
        /// </summary>
        /// <param name="cmd">Insert Command</param>
        public void ProcessInsertCommand(InsertCommand cmd)
        {
            if (this._autoCommit) this.Flush();
            ReaderWriterLockSlim currLock = (this._dataset is IThreadSafeDataset) ? ((IThreadSafeDataset)this._dataset).Lock : this._lock;
            bool mustRelease = false;
            try
            {
                if (!currLock.IsWriteLockHeld)
                {
                    currLock.EnterWriteLock();
                    mustRelease = true;
                }
                cmd.Evaluate(new SparqlUpdateEvaluationContext(this._dataset));
                if (this._autoCommit) this.Flush();
            }
            catch
            {
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
                if (mustRelease)
                {
                    currLock.ExitWriteLock();
                }
            }
        }

        /// <summary>
        /// Processes an INSERT DATA command
        /// </summary>
        /// <param name="cmd">Insert Data Command</param>
        public void ProcessInsertDataCommand(InsertDataCommand cmd)
        {
            if (this._autoCommit) this.Flush();
            ReaderWriterLockSlim currLock = (this._dataset is IThreadSafeDataset) ? ((IThreadSafeDataset)this._dataset).Lock : this._lock;
            bool mustRelease = false;
            try
            {
                if (!currLock.IsWriteLockHeld)
                {
                    currLock.EnterWriteLock();
                    mustRelease = true;
                }
                cmd.Evaluate(new SparqlUpdateEvaluationContext(this._dataset));
                if (this._autoCommit) this.Flush();
            }
            catch
            {
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
                if (mustRelease)
                {
                    currLock.ExitWriteLock();
                }
            }
        }

        /// <summary>
        /// Processes a LOAD command
        /// </summary>
        /// <param name="cmd">Load Command</param>
        public void ProcessLoadCommand(LoadCommand cmd)
        {
            if (this._autoCommit) this.Flush();
            ReaderWriterLockSlim currLock = (this._dataset is IThreadSafeDataset) ? ((IThreadSafeDataset)this._dataset).Lock : this._lock;
            bool mustRelease = false;
            try
            {
                if (!currLock.IsWriteLockHeld)
                {
                    currLock.EnterWriteLock();
                    mustRelease = true;
                }
                cmd.Evaluate(new SparqlUpdateEvaluationContext(this._dataset));
                if (this._autoCommit) this.Flush();
            }
            catch
            {
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
                if (mustRelease)
                {
                    currLock.ExitWriteLock();
                }
            }
        }

        /// <summary>
        /// Processes an INSERT/DELETE command
        /// </summary>
        /// <param name="cmd">Insert/Delete Command</param>
        public void ProcessModifyCommand(ModifyCommand cmd)
        {
            if (this._autoCommit) this.Flush();
            ReaderWriterLockSlim currLock = (this._dataset is IThreadSafeDataset) ? ((IThreadSafeDataset)this._dataset).Lock : this._lock;
            bool mustRelease = false;
            try
            {
                if (!currLock.IsWriteLockHeld)
                {
                    currLock.EnterWriteLock();
                    mustRelease = true;
                }
                cmd.Evaluate(new SparqlUpdateEvaluationContext(this._dataset));
                if (this._autoCommit) this.Flush();
            }
            catch
            {
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
                if (mustRelease)
                {
                    currLock.ExitWriteLock();
                }
            }
        }

        /// <summary>
        /// Processes a MOVE command
        /// </summary>
        /// <param name="cmd">Move Command</param>
        public void ProcessMoveCommand(MoveCommand cmd)
        {
            if (this._autoCommit) this.Flush();
            ReaderWriterLockSlim currLock = (this._dataset is IThreadSafeDataset) ? ((IThreadSafeDataset)this._dataset).Lock : this._lock;
            bool mustRelease = false;
            try
            {
                if (!currLock.IsWriteLockHeld)
                {
                    currLock.EnterWriteLock();
                    mustRelease = true;
                }
                cmd.Evaluate(new SparqlUpdateEvaluationContext(this._dataset));
                if (this._autoCommit) this.Flush();
            }
            catch
            {
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
                if (mustRelease)
                {
                    currLock.ExitWriteLock();
                }
            }
        }
    }
}
