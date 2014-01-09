/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
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
        private String _information;
        private readonly RunTaskInternalDelegate _delegate;
        private Exception _error;
        private TaskCallback<TResult> _callback;
        private DateTime? _start = null, _end = null;

        /// <summary>
        /// Creates a new Base Task
        /// </summary>
        /// <param name="name">Task Name</param>
        protected BaseTask(String name)
        {
            this.Name = name;
            this._state = TaskState.NotRun;
            this._delegate = new BaseTask<TResult>.RunTaskInternalDelegate(this.RunTaskInternal);
        }

        /// <summary>
        /// Gets/Sets the Task State
        /// </summary>
        public TaskState State
        {
            get { return this._state; }
            protected set
            {
                this._state = value;
                this.RaiseStateChanged();
            }
        }

        /// <summary>
        /// Gets the Task Name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets/Sets the Task Information
        /// </summary>
        public String Information
        {
            get { return this._information; }
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
            get { return this._error; }
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
                if (this._start == null) return null;
                if (this._end != null) return (this._end.Value - this._start.Value);
                return (DateTime.Now - this._start.Value);
            }
        }

        /// <summary>
        /// Gets the Task Result
        /// </summary>
        public TResult Result { get; private set; }

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
                this.Result = this._delegate.EndInvoke(result);
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
        public abstract bool IsCancellable { get; }

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
        /// <summary>
        /// Creates a new Task
        /// </summary>
        /// <param name="name">Task Name</param>
        protected CancellableTask(String name)
            : base(name)
        {
            HasBeenCancelled = false;
        }

        /// <summary>
        /// Gets whether the Task is cancellable, true unless the task has already completed
        /// </summary>
        public override sealed bool IsCancellable
        {
            get { return this.State != TaskState.Completed && this.State != TaskState.CompletedWithErrors; }
        }

        /// <summary>
        /// Cancels the task
        /// </summary>
        public override sealed void Cancel()
        {
            this.HasBeenCancelled = true;
            this.State = TaskState.RunningCancelled;
            this.CancelInternal();
        }

        /// <summary>
        /// Gets whether we've been told to cancel
        /// </summary>
        public bool HasBeenCancelled { get; private set; }

        /// <summary>
        /// Virtual method that derived classes may override if they wish to take action as soon as we are told to cancel
        /// </summary>
        protected virtual void CancelInternal()
        {
        }
    }

    /// <summary>
    /// Abstract Base Class for non-cancellable Tasks
    /// </summary>
    /// <typeparam name="T">Task Result Type</typeparam>
    public abstract class NonCancellableTask<T>
        : BaseTask<T> where T : class
    {
        /// <summary>
        /// Creates a new Task
        /// </summary>
        /// <param name="name">Name</param>
        protected NonCancellableTask(String name)
            : base(name)
        {
        }

        /// <summary>
        /// Returns that the Task may not be cancelled
        /// </summary>
        public override bool IsCancellable
        {
            get { return false; }
        }

        /// <summary>
        /// Has no effect since the Task may not be cancelled
        /// </summary>
        public override sealed void Cancel()
        {
            //Does Nothing
        }
    }
}