using System;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Results
{
    /// <summary>
    /// A result row which is mutable
    /// </summary>
    public interface IMutableResultRow
        : IResultRow
    {
        /// <summary>
        /// Sets the value of the specified variable
        /// </summary>
        /// <param name="var">Variable</param>
        /// <param name="value">Value</param>
        void Set(String var, INode value);
    }
}