using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Spin.Utility;

namespace VDS.RDF.Query.Spin.Core.Runtime.Registries
{
    internal static class InMemoryGraphsRegistry
    {

        private static Dictionary<Uri, int> _refCount = new Dictionary<Uri, int>(RDFHelper.uriComparer);
        private static Dictionary<Uri, IGraph> _graphs = new Dictionary<Uri, IGraph>(RDFHelper.uriComparer);

        public static IGraph Get(Uri graphUri) {
            if (_graphs.ContainsKey(graphUri)) {
                return _graphs[graphUri];
            }
            return null;
        }

        public static void Register(IGraph g)
        {
            if (g.BaseUri == null) {
                throw new InvalidOperationException("The graph's BaseUri cannot be null");
            }
            _refCount[g.BaseUri] = _refCount.ContainsKey(g.BaseUri) ? _refCount[g.BaseUri] + 1 : 1;
            _graphs[g.BaseUri] = g;
        }

        public static void UnRegister(IGraph g)
        {
            if (g.BaseUri == null)
            {
                throw new InvalidOperationException("The graph's BaseUri cannot be null");
            }
            UnRegister(g.BaseUri);
        }

        public static void UnRegister(Uri graphUri)
        {
            if (graphUri == null) return;
            if (_refCount.ContainsKey(graphUri)) {
                _refCount[graphUri]--;
                if (_refCount[graphUri]==0) {
                    _refCount.Remove(graphUri);
                    _graphs.Remove(graphUri);
                }
            }
        }

    }
}
