using System.IO;
using VDS.RDF.Graphs;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Writing.Contexts
{
    /// <summary>
    /// Interface for Writer Contexts
    /// </summary>
    public interface IWriterContext
    {
        /// <summary>
        /// Gets the TextWriter being written to
        /// </summary>
        TextWriter Output
        {
            get;
        }

        /// <summary>
        /// Gets/Sets the Pretty Printing Mode used
        /// </summary>
        bool PrettyPrint
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the High Speed Mode used
        /// </summary>
        bool HighSpeedModePermitted
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the Compression Level used
        /// </summary>
        int CompressionLevel
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the Node Formatter used
        /// </summary>
        INodeFormatter NodeFormatter
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the URI Formatter used
        /// </summary>
        IUriFormatter UriFormatter
        {
            get;
            set;
        }
    }
}