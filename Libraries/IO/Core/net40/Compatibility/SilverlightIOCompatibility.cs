using System;
using VDS.RDF.Specifications;

namespace VDS.RDF.Compatibility
{
#if SILVERLIGHT
    public static class SilverlightIOCompatibility
    {
        public static bool IsHexEncoding(String value, int index)
        {
            if (index + 2 >= value.Length) return false;
            return value[0] == '%' && TurtleSpecsHelper.IsHex(value[1]) && TurtleSpecsHelper.IsHex(value[2]);
        }

        public static char HexUnescape(String value, ref int index)
        {
            if (index + 2 >= value.Length) throw new ArgumentException("Malformed Percent Encoded Escape");
            if (value[index] != '%') throw new ArgumentException("Malformed Percent Encoded Escape");
            index = index + 3;
            return UnicodeSpecsHelper.ConvertToChar(value.Substring(index + 1, 2));
        }
    }
#endif
}
