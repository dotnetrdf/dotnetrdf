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
    /// Extension methods for tasks
    /// </summary>
    public static class TaskExtensions
    {
        /// <summary>
        /// Gets the human readable description for a task state
        /// </summary>
        /// <param name="state">Task State</param>
        /// <returns></returns>
        public static String GetStateDescription(this TaskState state)
        {
            switch (state)
            {
                case TaskState.Completed:
                    return "Completed OK";
                case TaskState.CompletedWithErrors:
                    return "Error(s)";
                case TaskState.NotRun:
                    return "Not Yet Run";
                case TaskState.Running:
                    return "Running";
                case TaskState.RunningCancelled:
                    return "Running (Cancelled)";
                case TaskState.Starting:
                    return "Starting";
                case TaskState.Unknown:
                default:
                    return "Unknown";
            }
        }

        /// <summary>
        /// Gets the safe string version for an object
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        internal static String ToSafeString(this Object obj)
        {
            return (obj != null ? obj.ToString() : String.Empty);
        }
    }
}
