using System;
using System.Collections.Generic;

namespace VDS.RDF.Query.Elements
{
    /// <summary>
    /// Interface for query pattern elements
    /// </summary>
    public interface IElement
        : IEquatable<IElement>
    {
        /// <summary>
        /// Accepts an element visitor
        /// </summary>
        /// <param name="visitor">Element visitor</param>
        void Accept(IElementVisitor visitor);

        /// <summary>
        /// Gets all variables mentioned in the element
        /// </summary>
        IEnumerable<String> Variables { get; }
        
        /// <summary>
        /// Gets all variables projected and thus visible outside the element
        /// </summary>
        IEnumerable<string> ProjectedVariables { get; }
    }
}
