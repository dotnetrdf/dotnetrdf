using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.Alexandria
{
    public static class Extensions
    {
        /// <summary>
        /// Gets either the String representation of the Object or the Empty String if the object is null
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        internal static String ToSafeString(this Object obj)
        {
            return (obj == null) ? String.Empty : obj.ToString();
        }
    }
}
