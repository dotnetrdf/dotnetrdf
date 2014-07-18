using System;
using System.Collections.Generic;

namespace VDS.RDF.Query.Results
{
    /// <summary>
    /// Represents tabular results
    /// </summary>
    public interface ITabularResults
        : IEnumerable<IResultRow>, IDisposable
    {
        /// <summary>
        /// Gets whether the table of results is streaming i.e. single use
        /// </summary>
        /// <remarks>
        /// If this returns true then the enumerator returned by these results is only guaranteed to be valid once and further attempt to obtain an enumerator may result in an error.  If this is false then the results may be enumerated as many times as desired
        /// </remarks>
        bool IsStreaming { get; }

        /// <summary>
        /// Gets the enumeration of variables present in these results
        /// </summary>
        /// <remarks>
        /// If <see cref="IsStreaming"/> returns true then the enumerator returned here is only guaranteed to be valid once and subsequent attempts to obtain and enumerate may result in an error, if this is false then this enumerator may be enumerated as many times as desired.
        /// </remarks>
        IEnumerable<String> Variables { get; } 
    }
}
