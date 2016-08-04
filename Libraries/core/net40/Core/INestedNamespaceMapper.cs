using System;

namespace VDS.RDF
{
    /// <summary>
    /// A Namespace Mapper which has an explicit notion of Nesting
    /// </summary>
    public interface INestedNamespaceMapper : INamespaceMapper
    {
        /// <summary>
        /// Gets the Nesting Level at which the given Namespace is definition is defined
        /// </summary>
        /// <param name="prefix">Prefix</param>
        /// <returns></returns>
        int GetNestingLevel(string prefix);

        /// <summary>
        /// Increments the Nesting Level
        /// </summary>
        void IncrementNesting();

        /// <summary>
        /// Decrements the Nesting Level
        /// </summary>
        /// <remarks>
        /// When the Nesting Level is decremented any Namespaces defined at a greater Nesting Level are now out of scope and so are removed from the Mapper
        /// </remarks>
        void DecrementNesting();

        /// <summary>
        /// Gets the current Nesting Level
        /// </summary>
        int NestingLevel { get; }
    }
}