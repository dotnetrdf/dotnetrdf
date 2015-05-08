using System;
using System.Collections.Generic;

namespace VDS.RDF.Nodes
{
    /// <summary>
    /// Equality comparer that compares URIs
    /// </summary>
    public class UriComparer
        : IComparer<Uri>, IEqualityComparer<Uri>
    {
        /// <summary>
        /// Compares two URIs
        /// </summary>
        /// <param name="x">URI</param>
        /// <param name="y">URI</param>
        /// <returns></returns>
        public int Compare(Uri x, Uri y)
        {
            return ComparisonHelper.CompareUris(x, y);
        }

        /// <summary>
        /// Determines whether two URIs are equal
        /// </summary>
        /// <param name="x">URI</param>
        /// <param name="y">URI</param>
        /// <returns></returns>
        public bool Equals(Uri x, Uri y)
        {
            return EqualityHelper.AreUrisEqual(x, y);
        }

        /// <summary>
        /// Gets the Hash Code for a URI
        /// </summary>
        /// <param name="obj">URI</param>
        /// <returns></returns>
        public int GetHashCode(Uri obj)
        {
            return obj == null ? 0 : obj.GetEnhancedHashCode();
        }
    }
}