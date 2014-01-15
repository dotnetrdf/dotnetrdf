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
using System.Threading;

namespace VDS.RDF.Utilities.StoreManager.Tasks
{
    /// <summary>
    /// Possible Task States
    /// </summary>
    public enum TaskState
    {
        /// <summary>
        /// Unknown
        /// </summary>
        Unknown,
        /// <summary>
        /// Starting
        /// </summary>
        Starting,
        /// <summary>
        /// Not Run
        /// </summary>
        NotRun,
        /// <summary>
        /// Running
        /// </summary>
        Running,
        /// <summary>
        /// Running but cancellation has been requested
        /// </summary>
        RunningCancelled,
        /// <summary>
        /// Completed Successfully
        /// </summary>
        Completed,
        /// <summary>
        /// Completed with Errors
        /// </summary>
        CompletedWithErrors
    }

    /// <summary>
    /// Task Result
    /// </summary>
    public class TaskResult
    {
        /// <summary>
        /// Creates a Task Result
        /// </summary>
        /// <param name="ok">Whether the task complete successfully</param>
        public TaskResult(bool ok)
        {
            this.OK = ok;
        }

        /// <summary>
        /// Gets whether the Task completed successfully
        /// </summary>
        public bool OK { get; private set; }
    }

    /// <summary>
    /// Represents a result where the result type is a struct
    /// </summary>
    /// <typeparam name="T">Struct Type</typeparam>
    public class TaskValueResult<T>
        where T : struct
    {
        /// <summary>
        /// Creates a new result
        /// </summary>
        /// <param name="value">Value</param>
        public TaskValueResult(T? value)
        {
            this.Value = value;
        }

        /// <summary>
        /// Gets the value
        /// </summary>
        public T? Value { get; private set; }
    }

    /// <summary>
    /// Delegate fpr task callback
    /// </summary>
    /// <typeparam name="T">Task Result Type</typeparam>
    /// <param name="task">Task</param>
    public delegate void TaskCallback<T>(ITask<T> task) where T : class;

    /// <summary>
    /// Delegate for task state changed
    /// </summary>
    public delegate void TaskStateChanged();

    /// <summary>
    /// Base Interface for Tasks
    /// </summary>
    public interface ITaskBase
    {
        /// <summary>
        /// Gets the Task State
        /// </summary>
        TaskState State
        {
            get;
        }

        /// <summary>
        /// Gets the name of the Task
        /// </summary>
        String Name
        {
            get;
        }

        /// <summary>
        /// Gets current information about the Task
        /// </summary>
        String Information
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the error that occurred (if any)
        /// </summary>
        Exception Error
        {
            get;
        }

        /// <summary>
        /// Gets how long the task has been running (if applicable)
        /// </summary>
        TimeSpan? Elapsed
        {
            get;
        }

        /// <summary>
        /// Gets whether the task can be cancelled
        /// </summary>
        bool IsCancellable
        {
            get;
        }

        /// <summary>
        /// Cancels the Task
        /// </summary>
        void Cancel();

        /// <summary>
        /// Event which is raised if the task state changes
        /// </summary>
        event TaskStateChanged StateChanged;
    }

    /// <summary>
    /// Interface for Tasks
    /// </summary>
    /// <typeparam name="TResult">Task Result Type</typeparam>
    public interface ITask<TResult> : ITaskBase
        where TResult : class
    {
        /// <summary>
        /// Gets the result
        /// </summary>
        TResult Result
        {
            get;
        }

        /// <summary>
        /// Runs the task, invoking the callback when it completes
        /// </summary>
        /// <param name="callback">Callback</param>
        void RunTask(TaskCallback<TResult> callback);

    }
}
