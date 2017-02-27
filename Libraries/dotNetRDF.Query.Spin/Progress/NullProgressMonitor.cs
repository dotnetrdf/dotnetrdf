using System;
namespace VDS.RDF.Query.Spin.Progress
{
    /// <summary>
    /// A ProgressMonitor that doesn't "do" anything.
    /// Support for canceling is provided via <code>setCanceled</code>.
    /// </summary>
    /// <author>Holger Knublauch</author>
    public class NullProgressMonitor : IProgressMonitor
    {

        private bool canceled;

        public bool isCanceled()
        {
            return canceled;
        }

        public void beginTask(String label, int totalWork)
        {
        }

        public void done()
        {
        }

        public void setCanceled(bool value)
        {
            this.canceled = value;
        }

        public void setTaskName(String value)
        {
        }

        public void subTask(String label)
        {
        }

        public void worked(int amount)
        {
        }
    }
}