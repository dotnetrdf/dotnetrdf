using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Utilities.StoreManager.Tasks
{
    public abstract class BaseTask<TResult> : ITask<TResult> where TResult : class
    {
        private TaskState _state = TaskState.Unknown;
        private String _name, _information;
        private RunTaskInternalDelegate _delegate;
        private Exception _error;
        private TResult _result;
        private TaskCallback<TResult> _callback;
        private DateTime? _start = null, _end = null;

        public BaseTask(String name)
        {
            this._name = name;
            this._state = TaskState.NotRun;
            this._delegate = new BaseTask<TResult>.RunTaskInternalDelegate(this.RunTaskInternal);
        }

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

        public String Name
        {
            get 
            {
                return this._name;
            }
        }

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

        public TResult Result
        {
            get
            {
                return this._result;
            }
        }

        public void RunTask(TaskCallback<TResult> callback)
        {
            this._start = DateTime.Now;
            this._callback = callback;
            this.State = TaskState.Starting;
            this._delegate.BeginInvoke(new AsyncCallback(this.CompleteTask), null);
            this.State = TaskState.Running;
        }

        private delegate TResult RunTaskInternalDelegate();

        protected abstract TResult RunTaskInternal();

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

        public abstract bool IsCancellable
        {
            get;
        }

        public abstract void Cancel();

        public event TaskStateChanged StateChanged;

        protected void RaiseStateChanged()
        {
            TaskStateChanged d = this.StateChanged;
            if (d != null)
            {
                d();
            }
        }
    }

    public abstract class CancellableTask<T> : BaseTask<T> where T : class
    {
        private bool _cancelled = false;

        public CancellableTask(String name)
            : base(name) { }

        public sealed override bool IsCancellable
        {
            get 
            {
                return this.State != TaskState.Completed && this.State != TaskState.CompletedWithErrors;
            }
        }

        public sealed override void Cancel()
        {
            this._cancelled = true;
            this.State = TaskState.RunningCancelled;
            this.CancelInternal();
        }

        public bool HasBeenCancelled
        {
            get
            {
                return this._cancelled;
            }
        }

        protected virtual void CancelInternal()
        {

        }
    }

    public abstract class NonCancellableTask<T> : BaseTask<T> where T : class
    {
        public NonCancellableTask(String name)
            : base(name) { }

        public override bool IsCancellable
        {
            get
            {
                return false;
            }
        }

        public sealed override void Cancel()
        {
            //Does Nothing
        }
    }
}
