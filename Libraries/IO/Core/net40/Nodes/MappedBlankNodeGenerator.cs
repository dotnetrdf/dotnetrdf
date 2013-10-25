using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Nodes
{
    /// <summary>
    /// A blank node generator that keeps a mapping of string IDs to blank nodes
    /// </summary>
    /// <remarks>
    /// As this implementation keeps a mapping its memory usage will grow over time and for data with large amounts of blank nodes this may exhaust memory
    /// </remarks>
    public class MappedBlankNodeGenerator
        : IBlankNodeGenerator
    {
        private readonly IRdfHandler _handler;
        private IDictionary<String, INode> _nodes = new Dictionary<string, INode>();

        public MappedBlankNodeGenerator(IRdfHandler handler, int seed)
        {
            if (ReferenceEquals(handler, null)) throw new ArgumentNullException("handler");
            this._handler = handler;
        }

        /// <summary>
        /// Create a new blank node
        /// </summary>
        /// <param name="id">String ID</param>
        /// <returns>Blank Node</returns>
        public INode CreateBlankNode(string id)
        {
            INode n;
            if (!this._nodes.TryGetValue(id, out n))
            {
                n = this._handler.CreateBlankNode();
                this._nodes.Add(id, n);
            }
            return n;
        }
    }
}
