namespace VDS.RDF.Parsing
{
    /// <summary>
    /// Interface for Parsers that support Tokeniser Tracing
    /// </summary>
    public interface ITraceableTokeniser
    {
        /// <summary>
        /// Gets/Sets whether Tokeniser Tracing is used
        /// </summary>
        bool TraceTokeniser
        {
            get;
            set;
        }
    }
}