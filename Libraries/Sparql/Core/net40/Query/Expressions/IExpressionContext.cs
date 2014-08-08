using System;
using System.Collections.Generic;
using VDS.RDF.Query.Engine;

namespace VDS.RDF.Query.Expressions
{
    /// <summary>
    /// Interface to provide context to expression evaluation
    /// </summary>
    public interface IExpressionContext
    {
        // TODO Figure out what is actually needed

        /// <summary>
        /// Gets the parent execution context
        /// </summary>
        IExecutionContext ParentContext { get; }

        /// <summary>
        /// Temporary context unique to this expression context
        /// </summary>
        IDictionary<String, Object> TemporaryContext { get; }
    }
}
