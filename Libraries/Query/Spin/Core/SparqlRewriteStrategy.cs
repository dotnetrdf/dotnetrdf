using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Spin.Core.Runtime;
using VDS.RDF.Query.Spin.SparqlStrategies;

namespace VDS.RDF.Query.Spin.Core
{
    internal class SparqlRewriteStrategy
    {

        private LinkedList<BaseSparqlRewriteStrategy> _rewriters = new LinkedList<BaseSparqlRewriteStrategy>();

        internal SparqlRewriteStrategy()
            : base()
        {
        }

        internal SparqlRewriteStrategy(List<Type> classes)
            : this()
        {
            foreach (Type cls in classes)
            {
                if (!typeof(BaseSparqlRewriteStrategy).IsAssignableFrom(cls)) throw new ArgumentException("Invalid type for rewriting strategy");
                _rewriters.AddLast((BaseSparqlRewriteStrategy)Activator.CreateInstance(cls));
            }
        }

        internal void Add(BaseSparqlRewriteStrategy rewriter)
        {
            // TODO bind rewriter listeners and events
            _rewriters.AddLast(rewriter);
        }

        internal void Rewrite(SparqlCommandUnit command)
        {
            switch (command.CommandType)
            {
                case SparqlCommandType.SparqlQuery:
                    RewriteQuery(command);
                    break;
                case SparqlCommandType.SparqlUpdate:
                    RewriteUpdate(command);
                    break;
                default:
                    throw new ArgumentException("Unknonwn command type");
            }
        }

        #region private implementation

        private void RewriteQuery(SparqlCommandUnit queryCommand)
        {
            foreach (BaseSparqlRewriteStrategy rewriter in _rewriters) {
                rewriter.Rewrite(queryCommand);
            }
        }

        private void RewriteUpdate(SparqlCommandUnit updateCommand)
        {
        }

        #endregion
    }
}
