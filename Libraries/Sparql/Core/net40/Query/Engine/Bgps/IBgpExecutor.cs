using System.Collections.Generic;
using VDS.RDF.Graphs;

namespace VDS.RDF.Query.Engine.Bgps
{
    /// <summary>
    /// Interface for BGP executors
    /// </summary>
    public interface IBgpExecutor
    {
        /// <summary>
        /// Matches a single triple pattern against the active graph as defined by the given context
        /// </summary>
        /// <param name="t">Triple pattern</param>
        /// <param name="context">Execution Context</param>
        /// <returns>Set for each distinct match</returns>
        /// <remarks>
        /// The active graph may be formed of multiple graphs, please see the remarks on <see cref="IExecutionContext.ActiveGraph"/> to understand how it should be interpreted
        /// </remarks>
        IEnumerable<ISolution> Match(Triple t, IExecutionContext context);

        /// <summary>
        /// Matches a single triple pattern with relevant variables from the given input set substituted into it against the active graph as defined by the given context
        /// </summary>
        /// <param name="t">Triple pattern</param>
        /// <param name="input">Input Set</param>
        /// <param name="context">Execution Context</param>
        /// <returns>Set for each distinct match</returns>
        /// <remarks>
        /// The active graph may be formed of multiple graphs, please see the remarks on <see cref="IExecutionContext.ActiveGraph"/> to understand how it should be interpreted
        /// </remarks>
        IEnumerable<ISolution> Match(Triple t, ISolution input, IExecutionContext context);
    }
}