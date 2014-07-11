using System;
using System.Collections.Generic;

namespace VDS.RDF.Query.Results
{
    /// <summary>
    /// Represents a stream of result rows
    /// </summary>
    public interface IResultStream
        : IEnumerator<IResultRow>
    {
        /// <summary>
        /// Gets the variables present in the stream
        /// </summary>
        IEnumerable<String> Variables { get; } 
    }
}
