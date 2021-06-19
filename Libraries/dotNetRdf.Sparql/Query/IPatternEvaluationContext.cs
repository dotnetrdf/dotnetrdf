using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDS.RDF.Query
{
    public interface IPatternEvaluationContext
    {
        /// <summary>
        /// Gets whether pattern evaluation should use rigorous evaluation mode.
        /// </summary>
        bool RigorousEvaluation { get; }

        /// <summary>
        /// Get whether the specified variable is found in the evaluation context.
        /// </summary>
        /// <param name="varName">The name of the variable to look for.</param>
        /// <returns>True if the evaluation context contains a whose name matches <paramref name="varName"/>, false otherwise.</returns>
        bool ContainsVariable(string varName);

        /// <summary>
        /// Gets whether the evaluation context contains a binding of the specified value to the specified variable.
        /// </summary>
        /// <param name="varName">The name of the variable to look for.</param>
        /// <param name="value">The expected value.</param>
        /// <returns>True if the evaluation context contains a binding for <paramref name="varName"/> to <paramref name="value"/>, false otherwise.</returns>
        bool ContainsValue(string varName, INode value);
    }
}
