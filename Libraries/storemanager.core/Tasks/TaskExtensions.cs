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
