using System;
using System.Collections.Generic;

namespace VDS.RDF.Collections
{
    /// <summary>
    /// Interface for Graph Collections, a graph collection is a RDF dictionary therefore graphs with duplicate URIs will be merged together.
    /// </summary>
    public interface IGraphCollection
        : IRdfDictionary<Uri,IGraph>
    {
        /// <summary>
        /// Event that occurs when a graph is added to the collection
        /// </summary>
        event GraphEventHandler GraphAdded;

        /// <summary>
        /// Event that occurs when a graph is removed from the collection
        /// </summary>
        event GraphEventHandler GraphRemoved;
    }
}
