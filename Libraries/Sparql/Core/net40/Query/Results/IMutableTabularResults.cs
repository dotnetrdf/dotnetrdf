using System;
using System.Collections.Generic;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Results
{
    /// <summary>
    /// Represents tabular results that are mutable i.e. may be freely modified by the user
    /// </summary>
    public interface IMutableTabularResults
        : ITabularResults, IList<IMutableResultRow>, IEquatable<IMutableTabularResults>
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