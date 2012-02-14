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
    public enum TaskState
    {
        Unknown,
        Starting,
        NotRun,
        Running,
        RunningCancelled,
        Completed,
        CompletedWithErrors
    }

    public class TaskResult
    {
        private bool _ok = false;

        public TaskResult(bool ok)
        {
            this._ok = ok;
        }

        public bool OK
        {
            get
            {
                return this._ok;
            }
        }
    }

    public class TaskValueResult<T>
        where T : struct
    {
        private T? _value;

        public TaskValueResult(T? value)
        {
            this._value = value;
        }

        public T? Value
        {
            get
            {
                return this._value;
            }
        }
    }

    public delegate void TaskCallback<T>(ITask<T> task) where T : class;

    public delegate void TaskStateChanged();

    public interface ITaskBase
    {
        TaskState State
        {
            get;
        }

        String Name
        {
            get;
        }

        String Information
        {
            get;
            set;
        }

        Exception Error
        {
            get;
        }

        TimeSpan? Elapsed
        {
            get;
        }

        bool IsCancellable
        {
            get;
        }

        void Cancel();

        event TaskStateChanged StateChanged;
    }

    public interface ITask<TResult> : ITaskBase
        where TResult : class
    {

        TResult Result
        {
            get;
        }

        void RunTask(TaskCallback<TResult> callback);

    }
}
