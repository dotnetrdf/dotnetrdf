/*

Copyright Robert Vesse 2009-12
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
        private bool _ok = false;

        /// <summary>
        /// Creates a Task Result
        /// </summary>
        /// <param name="ok">Whether the task complete successfully</param>
        public TaskResult(bool ok)
        {
            this._ok = ok;
        }

        /// <summary>
        /// Gets whether the Task completed successfully
        /// </summary>
        public bool OK
        {
            get
            {
                return this._ok;
            }
        }
    }

    /// <summary>
    /// Represents a result where the result type is a struct
    /// </summary>
    /// <typeparam name="T">Struct Type</typeparam>
    public class TaskValueResult<T>
        where T : struct
    {
        private T? _value;

        /// <summary>
        /// Creates a new result
        /// </summary>
        /// <param name="value">Value</param>
        public TaskValueResult(T? value)
        {
            this._value = value;
        }

        /// <summary>
        /// Gets the value
        /// </summary>
        public T? Value
        {
            get
            {
                return this._value;
            }
        }
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
