using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Nodes
{
    /// <summary>
    /// A blank node generator that uses a random number based derivation method to map string IDs to create blank nodes
    /// </summary>
    /// <remarks>
    /// Since this implementation doesn't keep any record of previously generated blank nodes it is extremely memory efficient, however in a few rare cases where two different IDs have colliding hash codes it may generate the same blank node for different IDs.  This can be avoided by using the <see cref="MappedBlankNodeGenerator"/> instead but this implementation is generally preferred because of its low memory usage.
    /// </remarks>
    public class RandomDerivedBlankNodeGenerator 
        : IBlankNodeGenerator
    {
        private readonly IRdfHandler _handler;
        private readonly int _seed;

        public RandomDerivedBlankNodeGenerator(IRdfHandler handler, int seed)
        {
            if (ReferenceEquals(handler, null)) throw new ArgumentNullException("handler");
            this._handler = handler;
            this._seed = seed;
        }

        /// <summary>
        /// Create a new blank node
        /// </summary>
        /// <param name="id">String ID</param>
        /// <returns>Blank Node</returns>
        public INode CreateBlankNode(String id)
        {
            return _handler.CreateBlankNode(this.MapToGuid(id));
        }

        protected Guid MapToGuid(String id)
        {
            // Initialize a new random with a seed based on the combination of the seed of the generator and the hash code
            // This means that given the same ID we should generate the same sequence of random data and thus the same GUID
            Random rnd = new Random(Tools.CombineHashCodes(this._seed, id));
            int r = rnd.Next();
            short b = (short) (r >> 16);
            short c = (short) r;
            // TODO May be worth using some bytes from the ID to avoid non-identical IDs with colliding hash codes creating identical GUIDs
            byte[] d = new byte[8];
            rnd.NextBytes(d);
            Guid guid = new Guid(id.GetHashCode(), b, c, d);
        }
    }
}
