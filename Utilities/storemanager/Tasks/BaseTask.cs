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
            protected set
            {
                this._information = value;
            }
        }

        public Exception Error
        {
            get
            {
                return this._error;
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
            this._callback = callback;
            this._state = TaskState.Starting;
            this.RaiseStateChanged();
            this._delegate.BeginInvoke(new AsyncCallback(this.CompleteTask), null);
            this._state = TaskState.Running;
            this.RaiseStateChanged();
        }

        private delegate TResult RunTaskInternalDelegate();

        protected abstract TResult RunTaskInternal();

        private void CompleteTask(IAsyncResult result)
        {
            try
            {
                //End the Invoke saving the Result
                this._result = this._delegate.EndInvoke(result);
                this._state = TaskState.Completed;
                this.RaiseStateChanged();
            }
            catch (Exception ex)
            {
                //Invoke errored so save the Error
                this._state = TaskState.CompletedWithErrors;
                this._information = "Error - " + ex.Message;
                this._error = ex;
                this.RaiseStateChanged();
            }
            finally
            {
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

        public override bool IsCancellable
        {
            get 
            {
                return true;
            }
        }

        public sealed override void Cancel()
        {
            this._cancelled = true;
            this.State = TaskState.RunningCancelled;
            this.RaiseStateChanged();
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
