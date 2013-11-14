using System;
using System.IO;
using VDS.RDF.Graphs;

namespace VDS.RDF.Writing
{
    public abstract class BaseGraphStoreWriter
        : IRdfWriter
    {
        public void Save(IGraph g, TextWriter output)
        {
            if (g == null) throw new ArgumentNullException("g", "Cannot write RDF from a null graph");
            if (output == null) throw new ArgumentNullException("output", "Cannot write RDF to a null writer");

            IGraphStore graphStore = new GraphStore();
            graphStore.Add(g);
            this.Save(graphStore, output);
        }

        public abstract void Save(IGraphStore graphStore, TextWriter output);

        /// <summary>
        /// Helper method for generating Parser Warning Events
        /// </summary>
        /// <param name="message">Warning Message</param>
        protected void RaiseWarning(String message)
        {
            if (this.Warning != null)
            {
                this.Warning(message);
            }
        }

        /// <summary>
        /// Event which is raised when there is a non-fatal issue with the RDF being written
        /// </summary>
        public event RdfWriterWarning Warning;
    }
}
