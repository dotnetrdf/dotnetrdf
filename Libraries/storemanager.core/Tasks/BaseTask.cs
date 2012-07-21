/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Utilities.StoreManager.Tasks
{
    /// <summary>
    /// Abstract Base Class for Tasks
    /// </summary>
    /// <typeparam name="TResult">Task Result Type</typeparam>
    public abstract class BaseTask<TResult>
        : ITask<TResult> where TResult : class
    {
        private TaskState _state = TaskState.Unknown;
        private String _name, _information;
        private RunTaskInternalDelegate _delegate;
        private Exception _error;
        private TResult _result;
        private TaskCallback<TResult> _callback;
        private DateTime? _start = null, _end = null;

        /// <summary>
        /// Creates a new Base Task
        /// </summary>
        /// <param name="name"></param>
        public BaseTask(String name)
        {
            this._name = name;
            this._state = TaskState.NotRun;
            this._delegate = new BaseTask<TResult>.RunTaskInternalDelegate(this.RunTaskInternal);
        }

        /// <summary>
        /// Gets/Sets the Task State
        /// </summary>
        public TaskState State
        {
            get 
            {
                return this._state;
            }
            protected set
            {
                this._state = value;
                this.RaiseStateChanged();
            }
        }

        /// <summary>
        /// Gets the Task Name
        /// </summary>
        public String Name
        {
            get 
            {
                return this._name;
            }
        }

        /// <summary>
        /// Gets/Sets the Task Information
        /// </summary>
        public String Information
        {
            get 
            {
               return this._information;
            }
            set
            {
                this._information = value;
                this.RaiseStateChanged();
            }
        }

        /// <summary>
        /// Gets/Sets the Task Error (if any)
        /// </summary>
        public Exception Error
        {
            get
            {
                return this._error;
            }
            protected set
            {
                this._error = value;
                this.RaiseStateChanged();
            }
        }

        /// <summary>
        /// Gets the Elapsed Time (if known)
        /// </summary>
        public TimeSpan? Elapsed
        {
            get
            {
                if (this._start == null)
                {
                    return null;
                }
                else if (this._end != null)
                {
                    return (this._end.Value - this._start.Value);
                }
                else
                {
                    return (DateTime.Now - this._start.Value);
                }
            }
        }

        /// <summary>
        /// Gets the Task Result
        /// </summary>
        public TResult Result
        {
            get
            {
                return this._result;
            }
        }

        /// <summary>
        /// Runs the Task
        /// </summary>
        /// <param name="callback">Callback</param>
        public void RunTask(TaskCallback<TResult> callback)
        {
            this._start = DateTime.Now;
            this._callback = callback;
            this.State = TaskState.Starting;
            this._delegate.BeginInvoke(new AsyncCallback(this.CompleteTask), null);
            this.State = TaskState.Running;
        }

        /// <summary>
        /// Delegate for running tasks in the background
        /// </summary>
        /// <returns></returns>
        private delegate TResult RunTaskInternalDelegate();

        /// <summary>
        /// Abstract method to be implemented by derived classes to implement the actual logic of the task
        /// </summary>
        /// <returns></returns>
        protected abstract TResult RunTaskInternal();

        /// <summary>
        /// Complets the task invoking the callback appropriately
        /// </summary>
        /// <param name="result">Result</param>
        private void CompleteTask(IAsyncResult result)
        {
            try
            {
                //End the Invoke saving the Result
                this._result = this._delegate.EndInvoke(result);
                this.State = TaskState.Completed;
            }
            catch (Exception ex)
            {
                //Invoke errored so save the Error
                this.State = TaskState.CompletedWithErrors;
                this.Information = "Error - " + ex.Message;
                this.Error = ex;
            }
            finally
            {
                this._end = DateTime.Now;
                //Invoke the Callback
                this._callback(this);
            }
        }

        /// <summary>
        /// Gets whether the Task may be cancelled
        /// </summary>
        public abstract bool IsCancellable
        {
            get;
        }

        /// <summary>
        /// Cancels the Task
        /// </summary>
        public abstract void Cancel();

        /// <summary>
        /// Event raised when the Task State changes
        /// </summary>
        public event TaskStateChanged StateChanged;

        /// <summary>
        /// Helper for raising the Task State changed event
        /// </summary>
        protected void RaiseStateChanged()
        {
            TaskStateChanged d = this.StateChanged;
            if (d != null)
            {
                d();
            }
        }
    }

    /// <summary>
    /// Abstract Base Class for cancellable Tasks
    /// </summary>
    /// <typeparam name="T">Task Result Type</typeparam>
    public abstract class CancellableTask<T> 
        : BaseTask<T> where T : class
    {
        private bool _cancelled = false;

        /// <summary>
        /// Creates a new Task
        /// </summary>
        /// <param name="name">Name</param>
        public CancellableTask(String name)
            : base(name) { }

        /// <summary>
        /// Gets whether the Task is cancellable, true unless the task has already completed
        /// </summary>
        public sealed override bool IsCancellable
        {
            get 
            {
                return this.State != TaskState.Completed && this.State != TaskState.CompletedWithErrors;
            }
        }

        /// <summary>
        /// Cancels the task
        /// </summary>
        public sealed override void Cancel()
        {
            this._cancelled = true;
            this.State = TaskState.RunningCancelled;
            this.CancelInternal();
        }

        /// <summary>
        /// Gets whether we've been told to cancel
        /// </summary>
        public bool HasBeenCancelled
        {
            get
            {
                return this._cancelled;
            }
        }

        /// <summary>
        /// Virtual method that derived classes may override if they wish to take action as soon as we are told to cancel
        /// </summary>
        protected virtual void CancelInternal() { }
    }

    /// <summary>
    /// Abstract Base Class for non-cancellable Tasks
    /// </summary>
    /// <typeparam name="T">Task Result Type</typeparam>
    public abstract class NonCancellableTask<T> : BaseTask<T> where T : class
    {
        /// <summary>
        /// Creates a new Task
        /// </summary>
        /// <param name="name">Name</param>
        public NonCancellableTask(String name)
            : base(name) { }

        /// <summary>
        /// Returns that the Task may not be cancelled
        /// </summary>
        public override bool IsCancellable
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Has no effect since the Task may not be cancelled
        /// </summary>
        public sealed override void Cancel()
        {
            //Does Nothing
        }
    }
}
