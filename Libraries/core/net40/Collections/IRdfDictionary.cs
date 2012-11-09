using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Collections
{
    /// <summary>
    /// Interface for RDF dictionaries, RDF dictionaries are special cases of a normal dictionary where values with duplicate keys are merged together
    /// </summary>
    /// <typeparam name="TKey">Key Type</typeparam>
    /// <typeparam name="TValue">Value Type</typeparam>
    public interface IRdfDictionary<TKey, TValue>
        : IDictionary<TKey, TValue>, IDisposable
    {
    }
}
