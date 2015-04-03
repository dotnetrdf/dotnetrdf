using System;
using System.Collections.Generic;
using VDS.RDF.Query.Spin.SparqlStrategies;

namespace VDS.RDF.Query.Spin.Core
{
    internal class SparqlRewriteStrategyChain
    {

        private List<BaseSparqlRewriteStrategy> _rewriters = new List<BaseSparqlRewriteStrategy>();

        internal SparqlRewriteStrategyChain()
            : base()
        {
        }

        internal SparqlRewriteStrategyChain(List<Type> classes)
            : this()
        {
            foreach (Type cls in classes)
            {
                if (!typeof(BaseSparqlRewriteStrategy).IsAssignableFrom(cls)) throw new ArgumentException("Invalid type for rewriting strategy");
                _rewriters.Add((BaseSparqlRewriteStrategy)Activator.CreateInstance(cls));
            }
        }

        internal void Add(BaseSparqlRewriteStrategy rewriter)
        {
            // TODO bind rewriter listeners and events
            _rewriters.Add(rewriter);
        }

        internal void Rewrite(SparqlCommandUnit command)
        {
            foreach (BaseSparqlRewriteStrategy rewriter in _rewriters)
            {
                rewriter.Rewrite(command);
            }
        }

        #region private implementation

        #endregion
    }
}
