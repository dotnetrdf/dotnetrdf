namespace VDS.RDF.Parsing
{
    /// <summary>
    /// Interface for Parsers that support Parser Tracing
    /// </summary>
    public interface ITraceableParser 
    {
        /// <summary>
        /// Gets/Sets whether Parser Tracing is used
        /// </summary>
        bool TraceParsing
        {
            get;
            set;
        }
    }
}