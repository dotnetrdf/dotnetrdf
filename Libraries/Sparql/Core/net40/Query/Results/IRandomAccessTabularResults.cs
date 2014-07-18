using System;

namespace VDS.RDF.Query.Results
{
    /// <summary>
    /// Represents tabular results that may be accessed randomly, this means that they are not streamed and may be accessed in any order
    /// </summary>
    public interface IRandomAccessTabularResults
        : ITabularResults
    {
        /// <summary>
        /// Gets the number of result rows present
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Gets the row at the specified row index
        /// </summary>
        /// <param name="index">Row Index</param>
        /// <returns>Result row</returns>
        /// <exception cref="IndexOutOfRangeException">Thrown if the row index is invalid</exception>
        IResultRow this[int index] { get; }
    }
}