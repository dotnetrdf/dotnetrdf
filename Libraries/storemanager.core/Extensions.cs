using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Utilities.StoreManager
{
    internal static class Extensions
    {
        internal static String ToSafeString(this Object value)
        {
            return ToSafeString(value, "Unknown");
        }

        internal static String ToSafeString(this Object value, String defaultValue)
        {
            return value != null ? value.ToString() : defaultValue;
        }
    }
}
