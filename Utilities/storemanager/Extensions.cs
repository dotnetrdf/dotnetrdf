using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Utilities.StoreManager
{
    /// <summary>
    /// Extension Methods
    /// </summary>
    internal static class Extensions
    {
        /// <summary>
        /// Gets the Safe string form of an object
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns>The result of calling ToString() on non-null objects and String.Empty otherwise</returns>
        internal static String ToSafeString(this Object obj)
        {
            return (obj != null ? obj.ToString() : String.Empty);
        }

        /// <summary>
        /// Gets the Safe string form of a URI
        /// </summary>
        /// <param name="u">URI</param>
        /// <returns>Either the AbolsuteUri or an empty string</returns>
        internal static String ToSafeString(this Uri u)
        {
            return (u != null ? u.AbsoluteUri : String.Empty);
        }
    }
}
