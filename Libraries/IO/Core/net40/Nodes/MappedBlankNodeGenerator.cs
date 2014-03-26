using System;
using System.Collections.Generic;

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
        private IDictionary<String, Guid> _ids = new Dictionary<string, Guid>();

        /// <summary>
        /// Create a new blank node
        /// </summary>
        /// <param name="id">String ID</param>
        /// <returns>Blank Node</returns>
        public Guid GetGuid(string id)
        {
            Guid guid;
            if (!this._ids.TryGetValue(id, out guid))
            {
                guid = Guid.NewGuid();
                this._ids.Add(id, guid);
            }
            return guid;
        }
    }
}
