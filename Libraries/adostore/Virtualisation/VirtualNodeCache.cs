using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Storage.Virtualisation
{
    /// <summary>
    /// A Cache that maps from Virtual IDs to Materialised Values
    /// </summary>
    public class VirtualNodeCache<TNodeID, TKey>
    {
        private Dictionary<TKey, INode> _mapping = new Dictionary<TKey, INode>();
        private Func<TNodeID, TKey> _keyGenerator;

        public VirtualNodeCache(Func<TNodeID, TKey> keyGenerator)
        {
            this._keyGenerator = keyGenerator;
        }

        public INode this[TNodeID id]
        {
            get
            {
                INode temp;
                if (this._mapping.TryGetValue(this._keyGenerator(id), out temp))
                {
                    return temp;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                TKey key = this._keyGenerator(id);
                if (this._mapping.ContainsKey(key))
                {
                    this._mapping[key] = value;
                }
                else
                {
                    this._mapping.Add(key, value);
                }
            }
        }
    }

    public class SimpleVirtualNodeCache<TNodeID>
        : VirtualNodeCache<TNodeID, TNodeID>
    {
        public SimpleVirtualNodeCache()
            : base(id => id) { }
    }
}
