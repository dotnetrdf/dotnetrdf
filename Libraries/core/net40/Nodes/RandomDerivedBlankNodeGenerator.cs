/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2013 dotNetRDF Project (dotnetrdf-develop@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;

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
        private static readonly DateTime _unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private readonly int _seed;

        /// <summary>
        /// Creates a new blank node generator
        /// </summary>
        public RandomDerivedBlankNodeGenerator()
            : this(DefaultSeed()) { }

        /// <summary>
        /// Creates a new blank node generator
        /// </summary>
        /// <param name="seed">Seed</param>
        public RandomDerivedBlankNodeGenerator(int seed)
        {
            this._seed = seed;
        }

        /// <summary>
        /// Gets the default seeds which is the current epoch time
        /// </summary>
        /// <returns></returns>
        private static int DefaultSeed()
        {
            return Convert.ToInt32((DateTime.UtcNow - _unixEpoch).TotalSeconds);
        }

        /// <summary>
        /// Create a new blank node
        /// </summary>
        /// <param name="id">String ID</param>
        /// <returns>Blank Node</returns>
        public Guid GetGuid(string id)
        {
            return this.MapToGuid(id);
        }

        /// <summary>
        /// Maps a String identifier into a GUID
        /// </summary>
        /// <param name="id">ID</param>
        /// <returns>GUID</returns>
        protected Guid MapToGuid(String id)
        {
            // Initialize a new random with a seed based on the combination of the seed of the generator and the hash code
            // This means that given the same ID we should generate the same sequence of random data and thus the same GUID
            // NB - There is a small chance that this could result in the same GUID for different IDs but hopefully this chance
            //      is small enough that for most people it won't be a problem
            Random rnd = new Random(Tools.CombineHashCodes(this._seed, id));
            int r = rnd.Next();
            short b = (short) (r >> 16);
            short c = (short) r;
            // TODO May be worth using some bytes from the String ID to avoid non-identical String IDs with colliding hash codes creating identical GUIDs
            byte[] d = new byte[8];
            rnd.NextBytes(d);
            return new Guid(id.GetHashCode(), b, c, d);
        }
    }
}
