using System;
using System.Collections.Generic;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Results
{
    /// <summary>
    /// Represents a single result row in a tabular result
    /// </summary>
    public interface IResultRow
        : IEquatable<IResultRow>
    {
        /// <summary>
        /// Gets the value of the specified variable which may be null
        /// </summary>
        /// <param name="var">Variable</param>
        /// <returns>Value which may be null</returns>
        /// <exception cref="RdfException">Thrown if the given variable is not present in this row</exception>
        INode this[String var] { get; }

        /// <summary>
        /// Gets the value of the variable in the specified column which may be null
        /// </summary>
        /// <param name="index">Column Index</param>
        /// <returns>Value which may be null</returns>
        /// <exception cref="IndexOutOfRangeException">Thrown if the given column index is invalid</exception>
        /// /// <exception cref="RdfException">Thrown if the given column index maps to a variable is not present in this row</exception>
        INode this[int index] { get; }

        /// <summary>
        /// Tries to get a value for the given variable returning true if a value is present (even if it is null)
        /// </summary>
        /// <param name="var">Variable</param>
        /// <param name="value">Value which may be null</param>
        /// <returns>True if variable is present, false otherwise</returns>
        bool TryGetValue(String var, out INode value);

        /// <summary>
        /// Tries to get a value for the given variable returning true if a value is present and it is not null
        /// </summary>
        /// <param name="var">Variable</param>
        /// <param name="value">Value, guaranteed to be non-null if method returns true</param>
        /// <returns>True if variable is present and value is non-null, false otherwise</returns>
        bool TryGetBoundValue(String var, out INode value);

        /// <summary>
        /// Gets whether there is a value present for the given variable (even if the value is null)
        /// </summary>
        /// <param name="var">Variable</param>
        /// <returns>True if variable is present, false otherwise</returns>
        bool HasValue(String var);

        /// <summary>
        /// Gets whether there is a value present for the given variable and the value is not null
        /// </summary>
        /// <param name="var">Variable</param>
        /// <returns>True if variable is present and value is non-null, false otherwise</returns>
        bool HasBoundValue(String var);

        /// <summary>
        /// Gets whether a result is ground i.e. all values are neither null nor blank nodes
        /// </summary>
        bool IsGroundResult { get; }

        /// <summary>
        /// Gets the variables present in this row
        /// </summary>
        IEnumerable<String> Variables { get; }
    }
}
