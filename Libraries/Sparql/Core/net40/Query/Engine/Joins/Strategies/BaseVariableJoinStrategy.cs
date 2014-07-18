using System;
using System.Collections.Generic;
using System.Linq;

namespace VDS.RDF.Query.Engine.Joins.Strategies
{
    public abstract class BaseVariableJoinStrategy
        : BaseJoinStrategy
    {
        protected BaseVariableJoinStrategy(IEnumerable<String> joinVars)
        {
            if (joinVars == null) throw new ArgumentNullException("joinVars");
            this.JoinVariables = joinVars.ToList().AsReadOnly();
            if (this.JoinVariables.Count == 0) throw new ArgumentException("Number of join variables must be >= 1", "joinVars");
        }

        /// <summary>
        /// Gets the join variables
        /// </summary>
        public IList<String> JoinVariables { get; private set; }
    }
}