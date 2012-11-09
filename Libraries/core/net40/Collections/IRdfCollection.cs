using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Collections
{
    /// <summary>
    /// Interface for RDF Collections, RDF collections are sets which are required to ignore duplicates.
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    public interface IRdfCollection<T>
        : IEnumerable<T>, IDisposable
    {
        /// <summary>
        /// Adds an item to the collection
        /// </summary>
        /// <param name="item">Item</param>
        /// <returns>True if the item was added, false otherwise</returns>
        bool Add(T item);

        /// <summary>
        /// Is the given item contained in the collection?
        /// </summary>
        /// <param name="item">Item</param>
        /// <returns>True if the item exists, false otherwise</returns>
        bool Contains(T item);

        /// <summary>
        /// Removes an item from the collection
        /// </summary>
        /// <param name="item">Item</param>
        /// <returns>True if the item was removed, false otherwise</returns>
        bool Remove(T t);

        /// <summary>
        /// Gets the count of the items in the collection
        /// </summary>
        long Count 
        { 
            get;
        }
    }
}
