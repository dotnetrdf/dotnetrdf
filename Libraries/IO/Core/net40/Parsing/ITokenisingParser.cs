using VDS.RDF.Parsing.Tokens;

namespace VDS.RDF.Parsing
{
    /// <summary>
    /// Interface for parsers that use token based parsing
    /// </summary>
    public interface ITokenisingParser
    {
        /// <summary>
        /// Gets/Sets the token queue mode used
        /// </summary>
        TokenQueueMode TokenQueueMode
        {
            get;
            set;
        }
    }
}