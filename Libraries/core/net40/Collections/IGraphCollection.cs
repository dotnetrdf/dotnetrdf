using System;
using System.Collections.Generic;

namespace VDS.RDF.Collections
{
    /// <summary>
    /// Interface for Graph Collections, a graph collection is a collection where graphs with duplicate URIs will be merged together.
    /// </summary>
    public interface IGraphCollection
        : IRdfDictionary<Uri,IGraph>
    {
        event GraphEventHandler GraphAdded;

        event GraphEventHandler GraphRemoved;
    }
}
