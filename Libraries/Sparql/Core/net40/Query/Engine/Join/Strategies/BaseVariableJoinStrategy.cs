using System;
using System.Collections.Generic;
using System.Linq;

namespace VDS.RDF.Query.Engine.Join.Strategies
{
    public abstract class BaseVariableJoinStrategy 
        : BaseJoinStrategy 
    {
        protected BaseVariableJoinStrategy(IEnumerable<String> joinVars)
        {
            if (joinVars == null) throw new ArgumentNullException("joinVars");
            this.JoinVariables = joinVars.ToList().AsReadOnly();
        }

        /// <summary>
        /// Gets the join variables
        /// </summary>
        public IList<String> JoinVariables { get; private set; }
    }
}