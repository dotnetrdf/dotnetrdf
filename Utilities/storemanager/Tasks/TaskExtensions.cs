using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Utilities.StoreManager.Tasks
{
    static class TaskExtensions
    {
        public static String GetStateDescription(this TaskState state)
        {
            switch (state)
            {
                case TaskState.Completed:
                    return "Completed OK";
                case TaskState.CompletedWithErrors:
                    return "Completed with Error(s)";
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
    }
}
