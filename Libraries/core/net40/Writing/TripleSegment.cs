using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Writing
{
    /// <summary>
    /// Indicates which Segment of a Triple Node Output is being generated for
    /// </summary>
    /// <remarks>
    /// Used by Writers and Formatters to ensure restrictions on which Nodes can appear where in the syntax are enforced
    /// </remarks>
    public enum TripleSegment
    {
        /// <summary>
        /// Subject of the Triple
        /// </summary>
        Subject,
        /// <summary>
        /// Predicate of the Triple
        /// </summary>
        Predicate,
        /// <summary>
        /// Object of the Triple
        /// </summary>
        Object
    }
}
