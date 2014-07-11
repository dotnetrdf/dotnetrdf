using System;
using System.Collections.Generic;
using VDS.RDF.Nodes;

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

    /// <summary>
    /// Represents tabular results that are mutable i.e. may be freely modified by the user
    /// </summary>
    public interface IMutableTabularResults
        : ITabularResults, IList<IMutableResultRow>
    {
        /// <summary>
        /// Adds a variable to the results, the variable is added only to the <see cref="ITabularResults.Variables"/> enumeration and not to individual result rows, use the overload <see cref="AddVariable(string, INode)"/> if you wish to add the variable to individual rows
        /// </summary>
        /// <param name="var">Variable</param>
        void AddVariable(String var);

        /// <summary>
        /// Adds a variable to the results adding it to both the <see cref="ITabularResults.Variables"/> enumeration and the individual rows assigned them the given initial value
        /// </summary>
        /// <param name="var">Variable</param>
        /// <param name="initialValue">Initial value to assign to each row</param>
        void AddVariable(String var, INode initialValue);
    }
}
