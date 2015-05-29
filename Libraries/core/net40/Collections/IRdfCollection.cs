/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-develop@lists.sf.net)

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
using System.Collections.Generic;
using System.Collections.Specialized;

namespace VDS.RDF.Collections
{
    /// <summary>
    /// Interface for RDF Collections, RDF collections are a kind of set and therfore are required to ignore duplicates
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    /// <remarks>
    /// This simple interface is provided rather than using <see cref="ISet{T}"/> because it requires a large range of complex set operations to be defined which are unecessary for our use cases.  Also we add special bulk add operations that implementations can use to optimise bulk insertions and deletions.
    /// </remarks>
    public interface IRdfCollection<T>
        : IEnumerable<T>, IDisposable, INotifyCollectionChanged
    {
        /// <summary>
        /// Adds an item to the collection
        /// </summary>
        /// <param name="item">Item</param>
        /// <returns>True if the item was added, false otherwise</returns>
        bool Add(T item);

        /// <summary>
        /// Adds a range of items to the collection
        /// </summary>
        /// <param name="items">Items</param>
        /// <returns>True if any items were added, false otherwise</returns>
        bool AddRange(IEnumerable<T> items);

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
        bool Remove(T item);

        /// <summary>
        /// Removes a range of items from the collection
        /// </summary>
        /// <param name="items">Items</param>
        /// <returns>True if any item was removed, false otherwise</returns>
        bool RemoveRange(IEnumerable<T> items);

        /// <summary>
        /// Clears the collection
        /// </summary>
        void Clear();

        /// <summary>
        /// Gets the count of the items in the collection
        /// </summary>
        long Count 
        { 
            get;
        }

        /// <summary>
        /// Gets if the collection is ready only
        /// </summary>
        bool IsReadOnly { get; }
    }
}
