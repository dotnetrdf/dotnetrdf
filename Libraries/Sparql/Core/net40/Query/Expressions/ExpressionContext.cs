using System;
using System.Collections.Generic;
using VDS.RDF.Query.Engine;

namespace VDS.RDF.Query.Expressions
{
    /// <summary>
    /// Default implementation of an expression context
    /// </summary>
    public class ExpressionContext
        : IExpressionContext
    {
        /// <summary>
        /// Creates a new context
        /// </summary>
        /// <param name="parentContext">Parent context</param>
        public ExpressionContext(IExecutionContext parentContext)
        {
            if (parentContext == null) throw new ArgumentNullException("parentContext");
            this.ParentContext = parentContext;
            this.TemporaryContext = new Dictionary<string, object>();
        }

        public IExecutionContext ParentContext { get; private set; }

        public IDictionary<string, object> TemporaryContext { get; set; }
    }
}
